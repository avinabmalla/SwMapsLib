using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using SwMapsLib.Data;
using SwMapsLib.Utils;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using SwMapsLib.Extensions;

namespace SwMapsLib.Conversions.GPKG
{
	public class SwMapsGpkgWriter
	{
		SQLiteConnection conn;
		SQLiteTransaction sqlTrans;
		SwMapsProject Project;

		string EPSG4326WKT = "GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]";

		public bool ExportPhotos { get; set; } = true;
		public bool ExportTracks { get; set; } = true;
		public bool ExportAllLayers { get; set; } = true;
		public bool ExportLineVertices { get; set; } = true;
		public bool IncludeMediaFilesAsBlob { get; set; } = true;
		public List<string> LayersToExport { get; set; } = new List<string>();


		public SwMapsGpkgWriter(SwMapsProject project)
		{
			Project = project;
		}


		public void Export(string path)
		{
			try
			{
				conn = new SQLiteConnection($"Data Source={path};Version=3;");
				conn.Open();
				sqlTrans = conn.BeginTransaction();
				CreateTables();

				foreach (var l in Project.FeatureLayers)
				{
					if (LayersToExport.Contains(l.Name) || ExportAllLayers) AddLayer(l);
				}

				if (ExportPhotos) AddPhotos();
				if (ExportTracks) AddTracks();
				sqlTrans.Commit();
			}
			finally
			{
				try
				{
					sqlTrans.Dispose();
					conn.CloseConnection();
				}
				catch { }
			}
		}

		private void CreateTables()
		{
			conn.ExecuteSQL("CREATE TABLE IF NOT EXISTS gpkg_contents(" +
							"table_name	TEXT NOT NULL," +
							"data_type	TEXT NOT NULL," +
							"identifier	TEXT UNIQUE," +
							"description	TEXT DEFAULT ''," +
							"last_change	DATETIME NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now'))," +
							"min_x	DOUBLE," +
							"min_y	DOUBLE," +
							"max_x	DOUBLE," +
							"max_y	DOUBLE," +
							"srs_id	INTEGER," +
							"PRIMARY KEY(table_name));");

			conn.ExecuteSQL("CREATE TABLE IF NOT EXISTS gpkg_geometry_columns (" +
							"table_name	TEXT NOT NULL," +
							"column_name	TEXT NOT NULL," +
							"geometry_type_name	TEXT NOT NULL," +
							"srs_id	INTEGER NOT NULL," +
							"z	TINYINT NOT NULL," +
							"m	TINYINT NOT NULL," +
							"CONSTRAINT pk_geom_cols PRIMARY KEY(table_name,column_name)," +
							"CONSTRAINT uk_gc_table_name UNIQUE(table_name));");

			conn.ExecuteSQL("CREATE TABLE IF NOT EXISTS gpkg_spatial_ref_sys (" +
							"srs_name	TEXT NOT NULL," +
							"srs_id	INTEGER NOT NULL," +
							"organization	TEXT NOT NULL," +
							"organization_coordsys_id	INTEGER NOT NULL," +
							"definition	TEXT NOT NULL," +
							"description	TEXT," +
							"PRIMARY KEY(srs_id));");

			conn.ExecuteSQL("CREATE VIEW geometry_columns AS " +
				"SELECT table_name AS f_table_name, column_name AS f_geometry_column, " +
				"(CASE geometry_type_name WHEN 'GEOMETRY' THEN 0 WHEN 'POINT' THEN 1 WHEN 'LINESTRING' THEN 2 WHEN 'POLYGON' THEN 3 WHEN 'MULTIPOINT' THEN 4 " +
				"WHEN 'MULTILINESTRING' THEN 5 WHEN 'MULTIPOLYGON' THEN 6 WHEN 'GEOMETRYCOLLECTION' THEN 7 WHEN 'CIRCULARSTRING' THEN 8 WHEN 'COMPOUNDCURVE' THEN 9 " +
				"WHEN 'CURVEPOLYGON' THEN 10 WHEN 'MULTICURVE' THEN 11 WHEN 'MULTISURFACE' THEN 12 WHEN 'CURVE' THEN 13 WHEN 'SURFACE' THEN 14 " +
				"WHEN 'POLYHEDRALSURFACE' THEN 15 WHEN 'TIN' THEN 16 WHEN 'TRIANGLE' THEN 17 ELSE 0 END) AS geometry_type, " +
				"2 + (CASE z WHEN 1 THEN 1 WHEN 2 THEN 1 ELSE 0 END) + (CASE m WHEN 1 THEN 1 WHEN 2 THEN 1 ELSE 0 END) AS coord_dimension, " +
				"srs_id AS srid FROM gpkg_geometry_columns;");

			conn.ExecuteSQL("CREATE VIEW spatial_ref_sys AS SELECT srs_id AS srid, organization AS auth_name, organization_coordsys_id AS auth_srid, definition AS srtext FROM gpkg_spatial_ref_sys;");

			conn.ExecuteSQL("CREATE VIEW st_geometry_columns AS SELECT table_name, column_name, 'ST_' || geometry_type_name AS geometry_type_name, " +
				"g.srs_id, srs_name FROM gpkg_geometry_columns as g JOIN gpkg_spatial_ref_sys AS s WHERE g.srs_id = s.srs_id;");


			conn.ExecuteSQL("CREATE VIEW st_spatial_ref_sys AS SELECT srs_name, srs_id, organization, organization_coordsys_id, definition, description FROM gpkg_spatial_ref_sys;");

			AddSrs("WGS 84 geodetic", 4326, "EPSG", 4326, EPSG4326WKT, "");
		}

		private void AddSrs(string name, int id, string organization, int code, string definition, string description)
		{
			var cv = new Dictionary<string, object>();
			cv.Add("srs_name", name);
			cv.Add("srs_id", id);
			cv.Add("organization", organization);
			cv.Add("organization_coordsys_id", code);
			cv.Add("definition", definition);
			cv.Add("description", description);
			conn.Insert("gpkg_spatial_ref_sys", cv, sqlTrans);
		}

		private void AddContents(string tableName, string dataType, string identifier,
			string description, DateTime lastChange, double minX, double minY, double maxX, double maxY, int srsId)
		{
			var cv = new Dictionary<string, object>();
			cv.Add("table_name", tableName);
			cv.Add("data_type", dataType);
			cv.Add("identifier", identifier); ;
			cv.Add("description", description);
			cv.Add("last_change", lastChange.ToString("yyyy-MM-ddTHH:mm:ssZ"));
			cv.Add("min_x", minX);
			cv.Add("min_y", minY);
			cv.Add("max_x", maxX);
			cv.Add("max_y", maxY);
			cv.Add("srs_id", srsId);

			conn.Insert("gpkg_contents", cv, sqlTrans);
		}

		private void AddGeometryColumn(string tableName, string columnName, string geomType, int srsId, byte z, byte m)
		{
			var cv = new Dictionary<string, object>();
			cv.Add("table_name", tableName);
			cv.Add("column_name", columnName);
			cv.Add("geometry_type_name", geomType);
			cv.Add("srs_id", srsId);
			cv.Add("z", z);
			cv.Add("m", m);
			conn.Insert("gpkg_geometry_columns", cv, sqlTrans);
		}

		private void AddTracks()
		{
			var tracks = Project.Tracks;
			if (tracks.Count == 0) return;

			var minLat = 90.0;
			var maxLat = -90.0;
			var minLon = 180.0;
			var maxLon = -180.0;

			foreach (var t in tracks)
			{
				foreach (var p in t.Vertices)
				{
					minLat = Math.Min(minLat, p.Latitude);
					maxLat = Math.Max(maxLat, p.Latitude);
					minLon = Math.Min(minLon, p.Longitude);
					maxLon = Math.Max(maxLon, p.Longitude);
				}
			}

			var columns = new List<string>();
			columns.Add("ID INTEGER PRIMARY KEY AUTOINCREMENT");
			columns.Add("UUID TEXT");
			columns.Add("geom LINESTRING");
			columns.Add("length NUMBER");
			columns.Add("remarks TEXT");
			columns.Add("start_time TEXT");
			columns.Add("end_time TEXT");

			var tableSql = $"CREATE TABLE Tracks({string.Join(",", columns)});";
			conn.ExecuteSQL(tableSql);

			AddContents("Tracks", "features", "Tracks", "From SW Maps", DateTime.UtcNow, minLon, minLat, maxLon, maxLat, 4326);
			AddGeometryColumn("Tracks", "geom", "LINESTRING", 4326, 1, 0);

			foreach (var t in tracks)
			{
				var cv = new Dictionary<string, object>();
				var geom = GpkgGeometryConverter.LinestringToGpkg(t.Vertices);

				cv.Add("UUID", t.UUID);
				cv.Add("geom", geom);
				cv.Add("length", t.Length);
				cv.Add("start_time", Formatter.GetTimeLabel(t.Vertices.First().Time));
				cv.Add("end_time", Formatter.GetTimeLabel(t.Vertices.Last().Time));
				cv.Add("remarks", t.Remarks.Trim());

				conn.Insert("Tracks", cv, sqlTrans);
			}

		}
		private void AddPhotos()
		{
			var photos = Project.PhotoPoints;
			if (photos.Count <= 0) return;

			var minLat = 90.0;
			var maxLat = -90.0;
			var minLon = 180.0;
			var maxLon = -180.0;

			foreach (var p in photos)
			{
				if (File.Exists(p.FileName) == false) continue;

				minLat = Math.Min(minLat, p.Location.Latitude);
				maxLat = Math.Max(maxLat, p.Location.Latitude);
				minLon = Math.Min(minLon, p.Location.Longitude);
				maxLon = Math.Max(maxLon, p.Location.Longitude);
			}

			var columns = new List<string>();
			columns.Add("ID INTEGER PRIMARY KEY AUTOINCREMENT");
			columns.Add("UUID TEXT");
			columns.Add("geom POINT");
			columns.Add("latitude REAL");
			columns.Add("longitude REAL");
			columns.Add("elevation REAL");
			columns.Add("time TEXT");
			columns.Add("remarks TEXT");
			columns.Add("photo BLOB");


			var tableSql = $"CREATE TABLE Photos({string.Join(",", columns)});";
			conn.ExecuteSQL(tableSql);

			AddContents("Photos", "features", "Photos", "From SW Maps", DateTime.UtcNow, minLon, minLat, maxLon, maxLat, 4326);
			AddGeometryColumn("Photos", "geom", "POINT", 4326, 1, 0);

			foreach (var p in photos)
			{
				if (File.Exists(p.FileName) == false) continue;

				var geom = GpkgGeometryConverter.PointToGpkg(p.Location);
				var cv = new Dictionary<string, object>();
				cv.Add("UUID", p.ID);
				cv.Add("geom", geom);
				cv.Add("latitude", p.Location.Latitude);
				cv.Add("longitude", p.Location.Longitude);
				cv.Add("elevation", p.Location.Elevation);
				cv.Add("time", Formatter.GetTimeLabel(p.Location.Time));
				cv.Add("remarks", p.Remarks.Trim());

				if (IncludeMediaFilesAsBlob)
				{
					cv.Add("photo", File.ReadAllBytes(p.FileName));
				}
				conn.Insert("Photos", cv, sqlTrans);
			}
		}

		private void AddLayer(SwMapsFeatureLayer layer)
		{
			var features = Project.GetAllFeatures(layer);
			if (features.Count == 0) return;

			var tableName = GetTableName(layer.Name);
			AddedTables.Add(tableName.ToLower());

			var geomType = "POINT";
			if (layer.GeometryType == SwMapsGeometryType.Line) geomType = "LINESTRING";
			if (layer.GeometryType == SwMapsGeometryType.Polygon) geomType = "POLYGON";

			var columns = new List<string>();
			columns.Add("ID INTEGER PRIMARY KEY AUTOINCREMENT");
			columns.Add("UUID TEXT");
			columns.Add("FID TEXT");
			columns.Add($"geom {geomType}");
			columns.Add("_description TEXT");

			if (layer.GeometryType == SwMapsGeometryType.Point)
			{
				columns.Add("latitude REAL");
				columns.Add("longitude REAL");
				columns.Add("elevation REAL");
				columns.Add("ortho_ht REAL");
				columns.Add("time TEXT");
				columns.Add("fix_id INTEGER");
				columns.Add("additional_data TEXT");
			}
			else if (layer.GeometryType == SwMapsGeometryType.Line)
			{
				columns.Add("_length REAL");
			}
			else if (layer.GeometryType == SwMapsGeometryType.Polygon)
			{
				columns.Add("_area REAL");
				columns.Add("_perimeter REAL");
			}

			var fields = new Dictionary<string, string>();
			foreach (var attr in layer.AttributeFields)
			{
				var dataType = "TEXT";
				if (attr.DataType == SwMapsAttributeType.Text) dataType = "TEXT";
				if (attr.DataType == SwMapsAttributeType.Numeric) dataType = "REAL";
				if (attr.DataType == SwMapsAttributeType.Options) dataType = "TEXT";
				if (attr.DataType == SwMapsAttributeType.Photo) dataType = IncludeMediaFilesAsBlob ? "BLOB" : "TEXT";
				if (attr.DataType == SwMapsAttributeType.Audio) dataType = IncludeMediaFilesAsBlob ? "BLOB" : "TEXT";
				if (attr.DataType == SwMapsAttributeType.Video) dataType = IncludeMediaFilesAsBlob ? "BLOB" : "TEXT";

				var NewName = attr.GetExportFieldName();
				int count = 0;
				foreach (var v in fields.Values)
				{
					if (v.ToUpper().StartsWith(NewName.ToUpper()))
					{
						count++;
					}
				}

				if (count > 0) NewName += $"_{count}";
				columns.Add($"{NewName} {dataType}");
				fields[attr.FieldName] = NewName;
			}

			var tableSql = $"CREATE TABLE {tableName}({string.Join(",", columns)});";
			conn.ExecuteSQL(tableSql);
			var minLat = 90.0;
			var maxLat = -90.0;
			var minLon = 180.0;
			var maxLon = -180.0;

			foreach (var f in features)
			{
				foreach (var p in f.Points)
				{
					minLat = Math.Min(minLat, p.Latitude);
					maxLat = Math.Max(maxLat, p.Latitude);
					minLon = Math.Min(minLon, p.Longitude);
					maxLon = Math.Max(maxLon, p.Longitude);
				}
			}

			AddContents(tableName, "features", tableName, "From SW Maps", DateTime.UtcNow, minLon, minLat, maxLon, maxLat, 4326);
			AddGeometryColumn(tableName, "geom", geomType, 4326, 1, 0);

			foreach (var f in features)
			{
				if (f.GeometryType == SwMapsGeometryType.Point && f.Points.Count < 1) continue;
				if (f.GeometryType == SwMapsGeometryType.Line && f.Points.Count < 2) continue;
				if (f.GeometryType == SwMapsGeometryType.Polygon && f.Points.Count < 3) continue;

				byte[] geom = null;
				if (f.GeometryType == SwMapsGeometryType.Point) geom = GpkgGeometryConverter.PointToGpkg(f.Points.Last());
				if (f.GeometryType == SwMapsGeometryType.Line) geom = GpkgGeometryConverter.LinestringToGpkg(f.Points);
				if (f.GeometryType == SwMapsGeometryType.Polygon) geom = GpkgGeometryConverter.PolygonToGpkg(f.Points);


				var cv = new Dictionary<string, object>();
				cv.Add("UUID", f.UUID);
				cv.Add("FID", f.FeatureID);
				cv.Add("geom", geom);
				cv.Add("_description", f.Remarks?.Trim() ?? "");

				if (layer.GeometryType == SwMapsGeometryType.Point)
				{
					var pt = f.Points.Last();
					cv.Add("latitude", pt.Latitude);
					cv.Add("longitude", pt.Longitude);
					cv.Add("elevation", pt.Elevation);
					cv.Add("ortho_ht", pt.OrthoHeight);
					cv.Add("time", Formatter.GetTimeLabel(pt.Time));
					cv.Add("additional_data", pt.AdditionalData);
					if (!layer.Drawn)
					{
						cv.Add("fix_id", pt.FixID);
					}
				}
				else if (layer.GeometryType == SwMapsGeometryType.Line)
				{
					cv.Add("_length", f.Length);
				}
				else if (layer.GeometryType == SwMapsGeometryType.Polygon)
				{
					cv.Add("_area", f.Area);
					cv.Add("_perimeter", f.Length);
				}

				var attrValues = f.AttributeValues;
				foreach (var attr in attrValues)
				{
					var colName = fields[attr.FieldName];
					object value = attr.Value;

					if (attr.DataType == SwMapsAttributeType.Numeric) value = Convert.ToDouble(attr.Value.Trim());
					else if (SwMapsTypes.IsMediaAttribute(attr.DataType))
					{
						if (IncludeMediaFilesAsBlob)
						{
							var filePath = attr.Value;
							if (File.Exists(filePath) == false)
							{
								filePath = Path.Combine(Project.MediaFolderPath, attr.Value);
							}

							if (File.Exists(filePath))
							{
								value = File.ReadAllBytes(filePath);
							}
							else
							{
								value = null;
							}
						}
					}
					else
					{
						value = attr.Value.Trim();
					}

					cv.Add(colName, value);
				}

				conn.Insert(tableName, cv, sqlTrans);

			}
		}

		List<string> AddedTables = new List<string>();
		string GetTableName(string layerName)
		{
			var tableName = Regex.Replace(layerName, "[^A-Za-z0-9]", "_");
			if (Char.IsDigit(tableName[0]))
				tableName = $"_{tableName}";

			if (tableName.ToLower() == "photos") tableName = $"_{tableName}";
			if (tableName.ToLower() == "tracks") tableName = $"_{tableName}";

			var tblName = tableName.Trim();
			var i = 1;
			while (AddedTables.Contains(tblName.ToLower()))
			{
				tblName = tableName + "_" + i;
				i++;
			}
			tableName = tblName;

			return tableName;

		}
	}
}

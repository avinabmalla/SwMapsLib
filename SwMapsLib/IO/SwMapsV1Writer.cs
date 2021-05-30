using SwMapsLib.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using SwMapsLib.Utils;
using System.IO;

namespace SwMapsLib.IO
{
	public class SwMapsV1Writer
	{
		SwMapsProject Project;

		SQLiteConnection conn;
		SQLiteTransaction sqlTrans;


		public SwMapsV1Writer(SwMapsProject project)
		{
			Project = project;
		}

		public void WriteSwmapsDb(string path)
		{
			if (File.Exists(path)) File.Delete(path);

			conn = new SQLiteConnection($"Data Source={path};Version=3;");
			conn.Open();
			sqlTrans = conn.BeginTransaction();

			CreateTables();

			WriteProjectAttributes();
			
			WriteFeatureLayers();
			WriteAttributeFields();
			
			WriteFeatures();
			WriteFeatureAttributes();

			WritePhotos();
			WriteTracks();

			sqlTrans.Commit();
			conn.Close();
		}


		void CreateTables()
		{
			conn.ExecuteSQL("CREATE TABLE tracks (id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT, color INTEGER, description TEXT);");
			conn.ExecuteSQL("CREATE TABLE track_points (line_id INTEGER, seq INTEGER, lat NUMBER, lon NUMBER, elv NUMBER, time NUMBER);");
			conn.ExecuteSQL("CREATE TABLE shp_style (layername TEXT, field TEXT, value TEXT, point_shape TEXT, point_color INTEGER, line_color INTEGER, polygon_color INTEGER, line_width INTEGER, label_field TEXT);");
			conn.ExecuteSQL("CREATE TABLE project_info (id INTEGER PRIMARY KEY AUTOINCREMENT, attr TEXT, value TEXT);");
			conn.ExecuteSQL("CREATE TABLE project_attributes (id INTEGER PRIMARY KEY AUTOINCREMENT, attr TEXT, value TEXT);");
			conn.ExecuteSQL("CREATE TABLE polylines (id INTEGER PRIMARY KEY AUTOINCREMENT, uuid TEXT, name TEXT, layer TEXT, description TEXT, closed INTEGER);");
			conn.ExecuteSQL("CREATE TABLE polyline_points (line_id INTEGER, seq INTEGER, lat NUMBER, lon NUMBER, elv NUMBER, time NUMBER, start_time NUMBER);");
			conn.ExecuteSQL("CREATE TABLE points (id INTEGER PRIMARY KEY AUTOINCREMENT, uuid TEXT, lat NUMBER, lon NUMBER, elv NUMBER, layer TEXT, description TEXT, time NUMBER, start_time NUMBER);");
			conn.ExecuteSQL("CREATE TABLE photos (id INTEGER PRIMARY KEY AUTOINCREMENT, lat NUMBER, lon NUMBER, elv NUMBER, description TEXT, filename TEXT);");
			conn.ExecuteSQL("CREATE TABLE layers (name TEXT, srcType TEXT, filename TEXT, z_index NUMBER, active NUMBER, path TEXT, cache NUMBER, sw_origin NUMBER);");
			conn.ExecuteSQL("CREATE TABLE data_layers (name TEXT, data_type TEXT, point_shape TEXT, point_color INTEGER, line_color INTEGER, polygon_color INTEGER, line_width INTEGER, active INTEGER, drawn INTEGER, label_field TEXT);");
			conn.ExecuteSQL("CREATE TABLE attribute_fields (item_layer TEXT, field TEXT, data_type TEXT, field_choices TEXT);");
			conn.ExecuteSQL("CREATE TABLE attribute_data (item_id INTEGER, field TEXT, data_type TEXT, value TEXT, item_layer TEXT);");
			conn.ExecuteSQL("CREATE TABLE android_metadata (locale TEXT);");
		}

		void WriteProjectAttributes()
		{
			foreach (var attr in Project.ProjectAttributes)
			{
				var cv = new Dictionary<string, object>();
				cv["attr"] = attr.Name;
				cv["value"] = attr.Value;
				conn.Insert("project_attributes", cv);
			}
		}

		void WriteFeatureLayers()
		{
			foreach (var lyr in Project.FeatureLayers)
			{
				var cv = new Dictionary<string, object>();

				cv["name"] = lyr.Name;
				cv["data_type"] = GeometryTypeToString(lyr.GeometryType);
				cv["point_shape"] = PointShapeToString(lyr.PointShape);
				cv["point_color"] = lyr.Color;
				cv["line_color"] = lyr.Color;
				cv["polygon_color"] = lyr.FillColor;
				cv["line_width"] = lyr.LineWidth;
				cv["active"] = lyr.Active ? 1 : 0;
				cv["drawn"] = lyr.Drawn ? 1 : 0;
				cv["label_field"] = lyr.LabelFieldName;

				conn.Insert("data_layers", cv);
			}
		}

		void WriteAttributeFields()
		{
			foreach (var l in Project.FeatureLayers)
			{
				foreach (var attr in l.AttributeFields)
				{
					var cv = new Dictionary<string, object>();
					cv["item_layer"] = l.Name;
					cv["field"] = attr.FieldName;
					cv["data_type"] = AttributeTypeToString(attr.DataType);
					cv["field_choices"] = string.Join("||", attr.Choices);

					conn.Insert("attribute_fields", cv);
				}
			}
		}

		void WriteFeatures()
		{
			foreach (var f in Project.Features)
			{
				if (f.GeometryType == SwMapsGeometryType.Point)
				{
					var cv = new Dictionary<string, object>();
					cv["uuid"] = f.UUID;
					cv["lat"] = f.Points[0].Latitude;
					cv["lon"] = f.Points[0].Longitude;
					cv["elv"] = f.Points[0].Elevation;
					cv["layer"] = Project.GetLayer(f.LayerID).Name;
					cv["description"] = f.Remarks;
					cv["time"] = f.Points[0].Time;
					cv["start_time"] = f.Points[0].StartTime;

					f.FeatureID = (int)conn.Insert("points", cv);
				}
				else
				{
					var cv = new Dictionary<string, object>();
					cv["uuid"] = f.UUID;
					cv["name"] = f.Name;
					cv["layer"] = Project.GetLayer(f.LayerID).Name;
					cv["description"] = f.Remarks;
					cv["closed"] = (f.GeometryType == SwMapsGeometryType.Polygon) ? 1 : 0;
					f.FeatureID = (int)conn.Insert("polylines", cv);

					foreach (var pt in f.Points)
					{
						var cv1 = new Dictionary<string, object>();
						cv1["line_id"] = f.FeatureID;
						cv1["seq"] = pt.Seq;
						cv1["lat"] = pt.Latitude;
						cv1["lon"] = pt.Longitude;
						cv1["elv"] = pt.Elevation;
						cv1["time"] = pt.Time;
						cv1["start_time"] = pt.StartTime;
						conn.Insert("polyline_points", cv1);
					}
				}
			}
		}
		
		void WriteFeatureAttributes()
		{
			foreach (var f in Project.Features)
			{
				var layerName = Project.GetLayer(f.LayerID).Name;
				foreach (var attr in f.AttributeValues)
				{
					var cv = new Dictionary<string, object>();
					cv["item_id"] = f.FeatureID;
					cv["field"] = attr.FieldName;
					cv["data_type"] = AttributeTypeToString(attr.DataType);
					cv["value"] = attr.Value;
					cv["item_layer"] = layerName;
					conn.Insert("attribute_data", cv);
				}
			}
		}
		
		void WritePhotos()
		{
			foreach(var ph in Project.PhotoPoints)
			{
				var cv = new Dictionary<string, object>();
				cv["lat"] = ph.Location.Latitude;
				cv["lon"] = ph.Location.Longitude;
				cv["elv"] = ph.Location.Elevation;
				cv["description"] = ph.Remarks;
				cv["filename"] = ph.FileName;
				conn.Insert("photos", cv);
			}
		}
		
		void WriteTracks()
		{
			foreach(var track in Project.Tracks)
			{
				var cv = new Dictionary<string, object>();
				cv["name"] = track.Name;
				cv["color"] = track.Color;
				cv["description"] = track.Remarks;

				int trackID = (int)conn.Insert("tracks", cv);
				foreach(var pt in track.Vertices)
				{
					var cv1 = new Dictionary<string, object>();
					cv1["line_id"] = trackID;
					cv1["seq"] = pt.Seq;
					cv1["lat"] = pt.Latitude;
					cv1["lon"] = pt.Longitude;
					cv1["elv"] = pt.Elevation;
					cv1["time"] = pt.Time;

					conn.Insert("track_points", cv1);
				}
			}
		}

		string PointShapeToString(SwMapsPointShape pt)
		{
			if (pt == SwMapsPointShape.Circle) return "CIRCLE";
			if (pt == SwMapsPointShape.Triangle) return "TRIANGLE";
			if (pt == SwMapsPointShape.Square) return "SQUARE";
			if (pt == SwMapsPointShape.FilledCircle) return "CIRCLE_FILL";
			return "CIRCLE";
		}

		string AttributeTypeToString(SwMapsAttributeType a)
		{
			if (a == SwMapsAttributeType.Text) return "TEXT";
			if (a == SwMapsAttributeType.Numeric) return "NUMERIC";
			if (a == SwMapsAttributeType.Options) return "OPTIONS";
			if (a == SwMapsAttributeType.Photo) return "PHOTO";
			if (a == SwMapsAttributeType.Audio) return "AUDIO";
			if (a == SwMapsAttributeType.Video) return "VIDEO";
			return "TEXT";
		}

		string GeometryTypeToString(SwMapsGeometryType gt)
		{
			if (gt == SwMapsGeometryType.Point) return "POINT";
			if (gt == SwMapsGeometryType.Line) return "LINE";
			if (gt == SwMapsGeometryType.Polygon) return "POLYGON";
			return "POINT";
		}
	}
}

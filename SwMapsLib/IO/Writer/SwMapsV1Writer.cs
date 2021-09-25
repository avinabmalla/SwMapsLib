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
	/// <summary>
	/// Writes a SW Maps V1 Database (swmaps) File
	/// 
	/// Avinab Malla
	/// avinabmalla@yahoo.com
	/// 28 May 2021
	/// </summary>
	public class SwMapsV1Writer
	{
		public const int Version = 20;
		SwMapsProject Project;


		public event EventHandler<SQLiteConnection> OnDbWrite;

		public SwMapsV1Writer(SwMapsProject project)
		{
			Project = project;
		}

		public void WriteSwmapsDb(string path)
		{
			if (File.Exists(path)) File.Delete(path);

			Project.ResequenceAll();

			using (var conn = new SQLiteConnection($"Data Source={path};Version=3;"))
			{
				conn.Open();
				conn.ExecuteSQL(String.Format("pragma user_version = {0};", Version));


				using (var sqlTrans = conn.BeginTransaction())
				{

					CreateTables(conn, sqlTrans);
					WriteProjectInfo(conn, sqlTrans);
					WriteProjectAttributes(conn, sqlTrans);

					WriteFeatureLayers(conn, sqlTrans);
					WriteAttributeFields(conn, sqlTrans);

					WriteFeatures(conn, sqlTrans);
					WriteFeatureAttributes(conn, sqlTrans);

					WritePhotos(conn, sqlTrans);
					WriteTracks(conn, sqlTrans);

					sqlTrans.Commit();
				}
				OnDbWrite?.Invoke(this, conn);
				conn.Close();

			}

			//https://stackoverflow.com/questions/8511901/system-data-sqlite-close-not-releasing-database-file
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}



		void CreateTables(SQLiteConnection conn, SQLiteTransaction sqlTrans)
		{
			conn.ExecuteSQL("CREATE TABLE tracks (id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT, color INTEGER, description TEXT);", sqlTrans);
			conn.ExecuteSQL("CREATE TABLE track_points (line_id INTEGER, seq INTEGER, lat NUMBER, lon NUMBER, elv NUMBER, time NUMBER);", sqlTrans);

			conn.ExecuteSQL("CREATE TABLE shp_style (" +
				"layername TEXT," +
				"field TEXT," +
				"value TEXT," +
				"point_shape TEXT," +
				"point_color INTEGER," +
				"line_color INTEGER," +
				"polygon_color INTEGER," +
				"line_width INTEGER," +
				"label_field TEXT);", sqlTrans);

			conn.ExecuteSQL("CREATE TABLE project_info (id INTEGER PRIMARY KEY AUTOINCREMENT, attr TEXT, value TEXT);", sqlTrans);
			conn.ExecuteSQL("CREATE TABLE project_attributes (id INTEGER PRIMARY KEY AUTOINCREMENT, attr TEXT, value TEXT);", sqlTrans);

			conn.ExecuteSQL("CREATE TABLE polylines (" +
				"id INTEGER PRIMARY KEY AUTOINCREMENT," +
				"uuid TEXT," +
				"name TEXT," +
				"layer TEXT," +
				"description TEXT," +
				"closed INTEGER);", sqlTrans);

			conn.ExecuteSQL("CREATE TABLE polyline_points (" +
				"line_id INTEGER," +
				"seq INTEGER," +
				"lat NUMBER," +
				"lon NUMBER," +
				"elv NUMBER," +
				"time NUMBER," +
				"start_time NUMBER);", sqlTrans);


			conn.ExecuteSQL("CREATE TABLE points (" +
				"id INTEGER PRIMARY KEY AUTOINCREMENT," +
				"uuid TEXT," +
				"lat NUMBER," +
				"lon NUMBER," +
				"elv NUMBER," +
				"layer TEXT," +
				"description TEXT," +
				"time NUMBER," +
				"start_time NUMBER);", sqlTrans);


			conn.ExecuteSQL("CREATE TABLE photos (" +
				"id INTEGER PRIMARY KEY AUTOINCREMENT," +
				"lat NUMBER," +
				"lon NUMBER," +
				"elv NUMBER," +
				"description TEXT," +
				"filename TEXT);", sqlTrans);


			conn.ExecuteSQL("CREATE TABLE layers (" +
				"name TEXT," +
				"srcType TEXT," +
				"filename TEXT," +
				"z_index NUMBER," +
				"active NUMBER," +
				"path TEXT," +
				"cache NUMBER," +
				"sw_origin NUMBER);", sqlTrans);


			conn.ExecuteSQL("CREATE TABLE data_layers (" +
				"name TEXT," +
				"data_type TEXT," +
				"point_shape TEXT," +
				"point_color INTEGER," +
				"line_color INTEGER," +
				"polygon_color INTEGER," +
				"line_width INTEGER," +
				"active INTEGER," +
				"drawn INTEGER," +
				"label_field TEXT);", sqlTrans);


			conn.ExecuteSQL("CREATE TABLE attribute_fields (item_layer TEXT, field TEXT, data_type TEXT, field_choices TEXT);", sqlTrans);
			conn.ExecuteSQL("CREATE TABLE attribute_data (item_id INTEGER, field TEXT, data_type TEXT, value TEXT, item_layer TEXT);", sqlTrans);
			conn.ExecuteSQL("CREATE TABLE android_metadata (locale TEXT);", sqlTrans);
		}

		void WriteProjectInfo(SQLiteConnection conn, SQLiteTransaction sqlTrans)
		{
			foreach (var key in Project.ProjectInfo.Keys)
			{
				var cv = new Dictionary<string, object>();
				cv["attr"] = key;
				cv["value"] = Project.ProjectInfo[key];
				conn.Insert("project_info", cv, sqlTrans);
			}
		}
		void WriteProjectAttributes(SQLiteConnection conn, SQLiteTransaction sqlTrans)
		{
			foreach (var attr in Project.ProjectAttributes)
			{
				var cv = new Dictionary<string, object>();
				cv["attr"] = attr.Name;
				cv["value"] = attr.Value;
				conn.Insert("project_attributes", cv, sqlTrans);
			}
		}

		void WriteFeatureLayers(SQLiteConnection conn, SQLiteTransaction sqlTrans)
		{
			foreach (var lyr in Project.FeatureLayers)
			{
				var cv = new Dictionary<string, object>();

				cv["name"] = lyr.Name;
				cv["data_type"] = SwMapsTypes.GeometryTypeToString(lyr.GeometryType);
				cv["point_shape"] = SwMapsTypes.PointShapeToString(lyr.PointShape);
				cv["point_color"] = lyr.Color;
				cv["line_color"] = lyr.Color;
				cv["polygon_color"] = lyr.FillColor;
				cv["line_width"] = lyr.LineWidth;
				cv["active"] = lyr.Active ? 1 : 0;
				cv["drawn"] = lyr.Drawn ? 1 : 0;
				cv["label_field"] = lyr.LabelFieldName;

				conn.Insert("data_layers", cv, sqlTrans);
			}
		}

		void WriteAttributeFields(SQLiteConnection conn, SQLiteTransaction sqlTrans)
		{
			foreach (var l in Project.FeatureLayers)
			{
				foreach (var attr in l.AttributeFields)
				{
					var cv = new Dictionary<string, object>();
					cv["item_layer"] = l.Name;
					cv["field"] = attr.FieldName;
					cv["data_type"] = SwMapsTypes.AttributeTypeToString(attr.DataType);
					cv["field_choices"] = string.Join("||", attr.Choices);

					conn.Insert("attribute_fields", cv, sqlTrans);
				}
			}
		}

		void WriteFeatures(SQLiteConnection conn, SQLiteTransaction sqlTrans)
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

					f.FeatureID = (int)conn.Insert("points", cv, sqlTrans);
				}
				else
				{
					var cv = new Dictionary<string, object>();
					cv["uuid"] = f.UUID;
					cv["name"] = f.Name;
					cv["layer"] = Project.GetLayer(f.LayerID).Name;
					cv["description"] = f.Remarks;
					cv["closed"] = (f.GeometryType == SwMapsGeometryType.Polygon) ? 1 : 0;
					f.FeatureID = (int)conn.Insert("polylines", cv, sqlTrans);

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
						conn.Insert("polyline_points", cv1, sqlTrans);
					}
				}
			}
		}

		void WriteFeatureAttributes(SQLiteConnection conn, SQLiteTransaction sqlTrans)
		{
			foreach (var f in Project.Features)
			{
				var layerName = Project.GetLayer(f.LayerID).Name;
				foreach (var attr in f.AttributeValues)
				{
					var cv = new Dictionary<string, object>();
					cv["item_id"] = f.FeatureID;
					cv["field"] = attr.FieldName;
					cv["data_type"] = SwMapsTypes.AttributeTypeToString(attr.DataType);

					if (SwMapsTypes.IsMediaAttribute(attr.DataType))
					{
						cv["value"] = Path.GetFileName(attr.Value);
					}
					else
					{
						cv["value"] = attr.Value;
					}
					cv["item_layer"] = layerName;
					conn.Insert("attribute_data", cv, sqlTrans);
				}
			}
		}

		void WritePhotos(SQLiteConnection conn, SQLiteTransaction sqlTrans)
		{
			foreach (var ph in Project.PhotoPoints)
			{
				var cv = new Dictionary<string, object>();
				cv["lat"] = ph.Location.Latitude;
				cv["lon"] = ph.Location.Longitude;
				cv["elv"] = ph.Location.Elevation;
				cv["description"] = ph.Remarks;
				cv["filename"] = Path.GetFileName(ph.FileName);
				conn.Insert("photos", cv, sqlTrans);
			}
		}

		void WriteTracks(SQLiteConnection conn, SQLiteTransaction sqlTrans)
		{
			foreach (var track in Project.Tracks)
			{
				var cv = new Dictionary<string, object>();
				cv["name"] = track.Name;
				cv["color"] = track.Color;
				cv["description"] = track.Remarks;

				int trackID = (int)conn.Insert("tracks", cv, sqlTrans);
				foreach (var pt in track.Vertices)
				{
					var cv1 = new Dictionary<string, object>();
					cv1["line_id"] = trackID;
					cv1["seq"] = pt.Seq;
					cv1["lat"] = pt.Latitude;
					cv1["lon"] = pt.Longitude;
					cv1["elv"] = pt.Elevation;
					cv1["time"] = pt.Time;

					conn.Insert("track_points", cv1, sqlTrans);
				}
			}
		}

	}
}

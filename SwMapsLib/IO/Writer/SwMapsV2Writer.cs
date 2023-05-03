using SwMapsLib.Data;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwMapsLib.Utils;
using System.IO;
using SwMapsLib.Extensions;

namespace SwMapsLib.IO
{
	/// <summary>
	/// Writes a SW Maps V2 Database (swm2) File
	/// 
	/// Avinab Malla
	/// avinabmalla@yahoo.com
	/// 29 May 2021
	/// </summary>
	public class SwMapsV2Writer
	{
		public const int Version = 114;
		SwMapsProject Project;
		SQLiteConnection conn;
		SQLiteTransaction sqlTrans;

		public event EventHandler<SQLiteConnection> OnDbWrite;
		public SwMapsV2Writer(SwMapsProject project)
		{
			Project = project;
		}

		public void WriteSwmapsDb(string path)
		{
			if (File.Exists(path)) File.Delete(path);

			Project.ResequenceAll();

			conn = new SQLiteConnection($"Data Source={path};Version=3;");
			try
			{
				conn.Open();
				conn.ExecuteSQL(String.Format("pragma user_version = {0};", Version));

				sqlTrans = conn.BeginTransaction();

				CreateTables();
				WriteProjectInfo();
				WriteProjectAttributes();

				WriteFeatureLayers();
				WriteAttributeFields();

				WriteFeatures();
				WriteFeatureAttributes();

				WritePhotos();
				WriteTracks();

				sqlTrans.Commit();

				OnDbWrite?.Invoke(this, conn);
			}
			catch(Exception ex)
			{
				sqlTrans.Rollback();
				throw ex;
			}
			finally
			{
				conn.CloseConnection();
			}
		}

		void CreateTables()
		{
			conn.ExecuteSQL("CREATE TABLE project_info(attr TEXT UNIQUE, value TEXT);", sqlTrans);

			conn.ExecuteSQL("CREATE TABLE project_attributes(" +
				"attr TEXT UNIQUE NOT NULL," +
				"value TEXT," +
				"data_type TEXT," +
				"field_choices TEXT," +
				"required_field INTEGER," +
				"field_length INTEGER);", sqlTrans);

			conn.ExecuteSQL("CREATE TABLE external_layers(" +
				"uuid TEXT UNIQUE NOT NULL," +
				"name TEXT," +
				"source_type TEXT," +
				"full_path TEXT," +
				"z_index NUMBER," +
				"active NUMBER," +
				"wms_layer_name TEXT," +
				"gpkg_layers TEXT," +
				"cache NUMBER);", sqlTrans);

			conn.ExecuteSQL("CREATE TABLE feature_layers(" +
				"uuid TEXT UNIQUE NOT NULL," +
				"name TEXT UNIQUE NOT NULL," +
				"group_name TEXT," +
				"geom_type TEXT," +
				"point_symbol TEXT," +
				"color INTEGER," +
				"fill_color INTEGER," +
				"line_width INTEGER," +
				"label_field_id TEXT," +
				"active INTEGER," +
				"drawn INTEGER," +
				"png_symbol BLOB," +
				"z_index INTEGER)", sqlTrans);

			conn.ExecuteSQL("CREATE TABLE attribute_fields(" +
				"uuid TEXT UNIQUE NOT NULL," +
				"layer_id TEXT," +
				"field_name TEXT," +
				"data_type TEXT," +
				"field_choices TEXT);", sqlTrans);

			conn.ExecuteSQL("CREATE TABLE attribute_values(" +
				"item_id TEXT," +
				"field_id TEXT," +
				"data_type TEXT," +
				"value TEXT);", sqlTrans);

			conn.ExecuteSQL("CREATE TABLE features(" +
				"uuid TEXT UNIQUE NOT NULL," +
				"layer_id TEXT," +
				"name TEXT," +
				"remarks TEXT)", sqlTrans);

			conn.ExecuteSQL("CREATE TABLE points(" +
				"uuid TEXT UNIQUE NOT NULL," +
				"fid TEXT," +
				"seq NUMBER," +
				"lat NUMBER," +
				"lon NUMBER," +
				"elv  NUMBER," +
				"ortho_ht  NUMBER," +
				"time NUMBER," +
				"start_time NUMBER," +
				"instrument_ht NUMBER," +
				"fix_quality NUMBER," +
				"speed NUMBER," +
				"snap_id TEXT," +
				"additional_data TEXT)", sqlTrans);

			conn.ExecuteSQL("CREATE TABLE photos(" +
				"uuid TEXT UNIQUE NOT NULL," +
				"remarks TEXT," +
				"photo_path TEXT)", sqlTrans);

			conn.ExecuteSQL("CREATE TABLE layer_style(" +
				"layer_id TEXT," +
				"table_name TEXT," +
				"field TEXT," +
				"value TEXT," +
				"point_shape TEXT," +
				"color INTEGER," +
				"fill_color INTEGER," +
				"line_width INTEGER," +
				"label_field TEXT)", sqlTrans);

			conn.ExecuteSQL("CREATE TABLE tracks(" +
				"uuid TEXT UNIQUE NOT NULL," +
				"name TEXT UNIQUE NOT NULL," +
				"color TEXT," +
				"description TEXT);", sqlTrans);

			conn.ExecuteSQL("CREATE INDEX IF NOT EXISTS points_fid_index on points(fid);", sqlTrans);
			conn.ExecuteSQL("CREATE INDEX IF NOT EXISTS points_uuid_index on points(uuid);", sqlTrans);
			conn.ExecuteSQL("CREATE INDEX IF NOT EXISTS field_layerid_index on attribute_fields(layer_id);", sqlTrans);
			conn.ExecuteSQL("CREATE INDEX IF NOT EXISTS attrvalue_fid_index on attribute_values(item_id);", sqlTrans);
			conn.ExecuteSQL("CREATE INDEX IF NOT EXISTS feature_id_index on features(uuid);", sqlTrans);
			conn.ExecuteSQL("CREATE INDEX IF NOT EXISTS feature_layerid_index on features(layer_id);", sqlTrans);

		}

		void WriteProjectInfo()
		{
			foreach (var key in Project.ProjectInfo.Keys)
			{
				var cv = new Dictionary<string, object>();
				cv["attr"] = key;
				cv["value"] = Project.ProjectInfo[key];
				conn.Insert("project_info", cv, sqlTrans);
			}
		}

		void WriteProjectAttributes()
		{
			foreach (var attr in Project.ProjectAttributes)
			{
				var cv = new Dictionary<string, object>();
				cv["attr"] = attr.Name;
				cv["value"] = attr.Value;
				cv["data_type"] = SwMapsTypes.ProjectAttributeTypeToString(attr.DataType);
				cv["field_choices"] = string.Join("||", attr.Choices);
				cv["required_field"] = attr.IsRequired ? 1 : 0;
				cv["field_length"] = attr.FieldLength;
				conn.Insert("project_attributes", cv, sqlTrans);
			}
		}

		void WriteFeatureLayers()
		{
			foreach (var lyr in Project.FeatureLayers)
			{
				var cv = new Dictionary<string, object>();

				cv["uuid"] = lyr.UUID;
				cv["name"] = lyr.Name;
				cv["group_name"] = lyr.GroupName;
				cv["geom_type"] = SwMapsTypes.GeometryTypeToString(lyr.GeometryType);
				cv["point_symbol"] = SwMapsTypes.PointShapeToString(lyr.PointShape);
				cv["color"] = lyr.Color;
				cv["fill_color"] = lyr.FillColor;
				cv["line_width"] = lyr.LineWidth;
				cv["label_field_id"] = lyr.LabelFieldID;
				cv["active"] = lyr.Active ? 1 : 0;
				cv["drawn"] = lyr.Drawn ? 1 : 0;
				cv["png_symbol"] = lyr.PngSymbol;
				cv["z_index"] = lyr.ZIndex;
				conn.Insert("feature_layers", cv, sqlTrans);
			}
		}

		void WriteAttributeFields()
		{
			foreach (var lyr in Project.FeatureLayers)
			{
				foreach (var attr in lyr.AttributeFields)
				{
					var cv = new Dictionary<string, object>();

					cv["uuid"] = attr.UUID;
					cv["layer_id"] = attr.LayerID;
					cv["field_name"] = attr.FieldName;
					cv["data_type"] = SwMapsTypes.AttributeTypeToString(attr.DataType);
					cv["field_choices"] = string.Join("||", attr.Choices);

					conn.Insert("attribute_fields", cv, sqlTrans);
				}
			}
		}

		void WriteFeatures()
		{
			foreach (var f in Project.Features)
			{
				var cv = new Dictionary<string, object>();

				cv["uuid"] = f.UUID;
				cv["layer_id"] = f.LayerID;
				cv["name"] = f.Name;
				cv["remarks"] = f.Remarks;
				conn.Insert("features", cv, sqlTrans);

				foreach (var pt in f.Points)
				{
					var cv1 = new Dictionary<string, object>();

					cv1["uuid"] = pt.ID;
					cv1["fid"] = pt.FeatureID;
					cv1["seq"] = pt.Seq;
					cv1["lat"] = pt.Latitude;
					cv1["lon"] = pt.Longitude;
					cv1["elv"] = pt.Elevation;
					cv1["ortho_ht"] = pt.OrthoHeight;
					cv1["time"] = pt.Time;
					cv1["start_time"] = pt.StartTime;
					cv1["instrument_ht"] = pt.InstrumentHeight;
					cv1["fix_quality"] = pt.FixID;
					cv1["speed"] = pt.Speed;
					cv1["snap_id"] = pt.SnapID;
					cv1["additional_data"] = pt.AdditionalData;

					conn.Insert("points", cv1, sqlTrans);
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
					cv["item_id"] = attr.FeatureID;
					cv["field_id"] = attr.FieldID;
					cv["data_type"] = SwMapsTypes.AttributeTypeToString(attr.DataType);

					if (SwMapsTypes.IsMediaAttribute(attr.DataType))
					{
						cv["value"] = Path.GetFileName(attr.Value);
					}
					else
					{
						cv["value"] = attr.Value;
					}

					conn.Insert("attribute_values", cv, sqlTrans);
				}
			}
		}

		void WritePhotos()
		{
			foreach (var ph in Project.PhotoPoints)
			{
				var cv = new Dictionary<string, object>();

				cv["uuid"] = ph.ID;
				cv["remarks"] = ph.Remarks;
				cv["photo_path"] = Path.GetFileName(ph.FileName);
				conn.Insert("photos", cv, sqlTrans);

				var pt = ph.Location;

				var cv1 = new Dictionary<string, object>();
				cv1["uuid"] = pt.ID;
				cv1["fid"] = pt.FeatureID;
				cv1["seq"] = pt.Seq;
				cv1["lat"] = pt.Latitude;
				cv1["lon"] = pt.Longitude;
				cv1["elv"] = pt.Elevation;
				cv1["ortho_ht"] = pt.OrthoHeight;
				cv1["time"] = pt.Time;
				cv1["start_time"] = pt.StartTime;
				cv1["instrument_ht"] = pt.InstrumentHeight;
				cv1["fix_quality"] = pt.FixID;
				cv1["speed"] = pt.Speed;
				cv1["snap_id"] = pt.SnapID;
				cv1["additional_data"] = pt.AdditionalData;

				conn.Insert("points", cv1, sqlTrans);
			}
		}

		void WriteTracks()
		{
			foreach (var tr in Project.Tracks)
			{
				var cv = new Dictionary<string, object>();
				cv["uuid"] = tr.UUID;
				cv["name"] = tr.Name;
				cv["color"] = tr.Color;
				cv["description"] = tr.Remarks;

				conn.Insert("tracks", cv, sqlTrans);

				foreach (var pt in tr.Vertices)
				{
					var cv1 = new Dictionary<string, object>();
					cv1["uuid"] = pt.ID;
					cv1["fid"] = pt.FeatureID;
					cv1["seq"] = pt.Seq;
					cv1["lat"] = pt.Latitude;
					cv1["lon"] = pt.Longitude;
					cv1["elv"] = pt.Elevation;
					cv1["ortho_ht"] = pt.OrthoHeight;
					cv1["time"] = pt.Time;
					cv1["start_time"] = pt.StartTime;
					cv1["instrument_ht"] = pt.InstrumentHeight;
					cv1["fix_quality"] = pt.FixID;
					cv1["speed"] = pt.Speed;
					cv1["snap_id"] = pt.SnapID;
					cv1["additional_data"] = pt.AdditionalData;

					conn.Insert("points", cv1, sqlTrans);
				}
			}
		}


	}
}

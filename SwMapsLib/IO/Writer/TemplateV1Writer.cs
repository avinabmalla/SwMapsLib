using SwMapsLib.Data;
using SwMapsLib.Utils;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace SwMapsLib.IO
{
	public class TemplateV1Writer
	{
		public const int Version = 4;
		SwMapsTemplate Template;

		public TemplateV1Writer(SwMapsTemplate template)
		{
			Template = template;
		}

		public void WriteTemplate(string path)
		{
			if (File.Exists(path)) File.Delete(path);

			using (var conn = new SQLiteConnection($"Data Source={path};Version=3;"))
			{
				conn.Open();
				conn.ExecuteSQL(String.Format("pragma user_version = {0};", Version));


				using (var sqlTrans = conn.BeginTransaction())
				{

					CreateTables(conn, sqlTrans);
					WriteProjectAttributes(conn, sqlTrans);

					WriteFeatureLayers(conn, sqlTrans);
					WriteAttributeFields(conn, sqlTrans);
					sqlTrans.Commit();
				}
				conn.Close();

			}

			//https://stackoverflow.com/questions/8511901/system-data-sqlite-close-not-releasing-database-file
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		void CreateTables(SQLiteConnection conn, SQLiteTransaction sqlTrans)
		{
			conn.ExecuteSQL("CREATE TABLE project_attributes (id INTEGER PRIMARY KEY AUTOINCREMENT, attr TEXT, value TEXT);", sqlTrans);

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
		}

		void WriteProjectAttributes(SQLiteConnection conn, SQLiteTransaction sqlTrans)
		{
			foreach (var attr in Template.ProjectAttributes)
			{
				var cv = new Dictionary<string, object>();
				cv["attr"] = attr.Name;
				cv["value"] = attr.Value;
				conn.Insert("project_attributes", cv, sqlTrans);
			}
		}

		void WriteFeatureLayers(SQLiteConnection conn, SQLiteTransaction sqlTrans)
		{
			foreach (var lyr in Template.Layers)
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
			foreach (var l in Template.Layers)
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

	}
}

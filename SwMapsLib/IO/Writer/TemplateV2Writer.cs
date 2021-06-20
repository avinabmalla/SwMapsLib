using SwMapsLib.Data;
using SwMapsLib.Utils;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace SwMapsLib.IO.Writer
{
	public class TemplateV2Writer
	{
		public const int Version = 104;
		SwMapsTemplate Template;

		public TemplateV2Writer(SwMapsTemplate template)
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

					WriteTemplateInfo(conn, sqlTrans);
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
			conn.ExecuteSQL("CREATE TABLE template_info(attr TEXT, value TEXT);", sqlTrans);

			conn.ExecuteSQL("CREATE TABLE project_attributes(" +
				"attr TEXT," +
				"value TEXT," +
				"data_type TEXT," +
				"field_choices TEXT," +
				"required_field INTEGER," +
				"field_length INTEGER);", sqlTrans);

			conn.ExecuteSQL("CREATE TABLE feature_layers(" +
				"uuid TEXT," +
				"name TEXT," +
				"group_name TEXT," +
				"geom_type TEXT," +
				"point_symbol TEXT," +
				"color INTEGER," +
				"fill_color INTEGER," +
				"line_width INTEGER," +
				"label_field_id TEXT," +
				"active INTEGER," +
				"drawn INTEGER," +
				"png_symbol BLOB)", sqlTrans);

			conn.ExecuteSQL("CREATE TABLE attribute_fields(" +
				"uuid TEXT," +
				"layer_id TEXT," +
				"field_name TEXT," +
				"data_type TEXT," +
				"field_choices TEXT);", sqlTrans);
		}

		void WriteTemplateInfo(SQLiteConnection conn, SQLiteTransaction sqlTrans)
		{
			var cv = new Dictionary<string, object>();
			cv["attr"] = "template_name";
			cv["value"] = Template.TemplateName;
			conn.Insert("template_info", cv, sqlTrans);

			cv["attr"] = "template_author";
			cv["value"] = Template.TemplateAuthor;
			conn.Insert("template_info", cv, sqlTrans);
		}

		void WriteProjectAttributes(SQLiteConnection conn, SQLiteTransaction sqlTrans)
		{
			foreach (var attr in Template.ProjectAttributes)
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

		void WriteFeatureLayers(SQLiteConnection conn, SQLiteTransaction sqlTrans)
		{
			foreach (var lyr in Template.Layers)
			{
				var cv = new Dictionary<string, object>();

				cv["uuid"] = lyr.UUID;
				cv["name"] = lyr.Name;
				cv["group_name"] = lyr.GroupName;
				cv["geom_type"] = SwMapsTypes.GeometryTypeToString(lyr.GeometryType);
				cv["point_symbol"] = lyr.PointShape;
				cv["color"] = lyr.Color;
				cv["fill_color"] = lyr.FillColor;
				cv["line_width"] = lyr.LineWidth;
				cv["label_field_id"] = lyr.LabelFieldID;
				cv["active"] = lyr.Active ? 1 : 0;
				cv["drawn"] = lyr.Drawn ? 1 : 0;
				cv["png_symbol"] = lyr.PngSymbol;
				conn.Insert("feature_layers", cv, sqlTrans);
			}
		}

		void WriteAttributeFields(SQLiteConnection conn, SQLiteTransaction sqlTrans)
		{
			foreach (var lyr in Template.Layers)
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

	}
}

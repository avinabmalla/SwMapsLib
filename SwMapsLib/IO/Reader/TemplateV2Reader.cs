using SwMapsLib.Data;
using SwMapsLib.Utils;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace SwMapsLib.IO.Reader
{
	class TemplateV2Reader
	{
		public SwMapsTemplate Read(string TemplatePath, SQLiteConnection conn)
		{
			var template = new SwMapsTemplate(TemplatePath);
			template.Layers = ReadAllFeatureLayers(conn);
			template.ProjectAttributes = ReadProjectAttributes(conn);
			ReadTemplateInfo(conn, template);

			return template;
		}

		public void ReadTemplateInfo(SQLiteConnection conn, SwMapsTemplate template)
		{
			var sql = "SELECT * FROM template_info";
			using (var cmd = new SQLiteCommand(sql, conn))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
				{
					var attr = reader.ReadString("attr");
					var value = reader.ReadString("value");
					if (attr == "template_author") template.TemplateAuthor = value;
					if (attr == "template_name") template.TemplateName = value;
				}
		}

		public List<SwMapsProjectAttribute> ReadProjectAttributes(SQLiteConnection conn)
		{
			var ret = new List<SwMapsProjectAttribute>();
			var sql = "SELECT * FROM project_attributes";

			using (var cmd = new SQLiteCommand(sql, conn))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var a = new SwMapsProjectAttribute();
						a.Name = reader.ReadString("attr");
						a.Value = reader.ReadString("value");

						a.IsRequired = reader.ReadInt32("required_field") == 1;

						var dataType = reader.ReadString("data_type");
						a.DataType = SwMapsTypes.ProjectAttributeTypeFromString(dataType);

						var choices = reader.ReadString("field_choices");
						a.Choices = choices.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries).ToList();

						ret.Add(a);
					}
				}
			}

			return ret;
		}


		public List<SwMapsFeatureLayer> ReadAllFeatureLayers(SQLiteConnection conn)
		{
			var ret = new List<SwMapsFeatureLayer>();
			var sql = "SELECT * FROM feature_layers;";
			using (var cmd = new SQLiteCommand(sql, conn))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
				{
					var layer = new SwMapsFeatureLayer();
					layer.UUID = reader.ReadString("uuid");
					layer.Name = reader.ReadString("name");
					layer.GroupName = reader.ReadString("group_name");

					var geomType = reader.ReadString("geom_type");
					layer.GeometryType = SwMapsTypes.GeometryTypeFromString(geomType);

					var pointSymbol = reader.ReadString("point_symbol");
					layer.PointShape = SwMapsTypes.PointShapeFromString(pointSymbol);

					layer.Color = reader.ReadInt32("color");
					layer.FillColor = reader.ReadInt32("fill_color");
					layer.LineWidth = reader.ReadSingle("line_width");
					layer.LabelFieldID = reader.ReadString("label_field_id");
					layer.Active = reader.ReadInt32("active") == 1;
					layer.Drawn = reader.ReadInt32("drawn") == 1;
					layer.PngSymbol = reader.ReadBlob("png_symbol");
					layer.AttributeFields = ReadAttributeFields(conn, layer.UUID);
					ret.Add(layer);
				}
			return ret;
		}


		public List<SwMapsAttributeField> ReadAttributeFields(SQLiteConnection conn, string layerID)
		{
			var ret = new List<SwMapsAttributeField>();
			var sql = $"SELECT * FROM attribute_fields WHERE layer_id = '{layerID}';";

			using (var cmd = new SQLiteCommand(sql, conn))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
				{
					var a = new SwMapsAttributeField();
					a.UUID = reader.ReadString("uuid");
					a.LayerID = reader.ReadString("layer_id");
					a.FieldName = reader.ReadString("field_name");

					var dataType = reader.ReadString("data_type");
					a.DataType = SwMapsTypes.AttributeTypeFromString(dataType);


					a.Choices = reader.ReadString("field_choices").Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries).ToList();
					if (ret.Any(at => at.UUID == a.UUID)) continue;
					ret.Add(a);


				}
			return ret;
		}

	}
}

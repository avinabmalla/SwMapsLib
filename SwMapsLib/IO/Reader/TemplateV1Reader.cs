using SwMapsLib.Data;
using SwMapsLib.Utils;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace SwMapsLib.IO
{
	class TemplateV1Reader
	{

		public Dictionary<string, string> LayerIDs = new Dictionary<string, string>(); //name, uuid
		public Dictionary<string, string> AttributeFieldIDs = new Dictionary<string, string>();

		public SwMapsTemplate Read(string TemplatePath, SQLiteConnection conn)
		{
			var template = new SwMapsTemplate(TemplatePath);
			template.ProjectAttributes = ReadAllProjectAttributes(conn);
			template.Layers = ReadAllFeatureLayers(conn);
			return template;
		}

		private List<SwMapsProjectAttribute> ReadAllProjectAttributes(SQLiteConnection conn)
		{
			var ret = new List<SwMapsProjectAttribute>();
			var sql = $"SELECT * FROM project_attributes;";

			using (var cmd = new SQLiteCommand(sql, conn))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
				{
					var a = new SwMapsProjectAttribute();
					a.DataType = SwMapsProjectAttributeType.Text;
					a.FieldLength = 0;
					a.IsRequired = false;

					a.Name = reader.ReadString("attr");
					a.Value = reader.ReadString("value");

					ret.Add(a);
				}

			return ret;
		}

		private List<SwMapsAttributeField> ReadAttributeFields(SQLiteConnection conn, string layer)
		{
			var ret = new List<SwMapsAttributeField>();
			var sql = $"SELECT * FROM attribute_fields WHERE item_layer='{layer}';";

			using (var cmd = new SQLiteCommand(sql, conn))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
				{
					var a = new SwMapsAttributeField();
					a.LayerID = layer;
					a.FieldName = reader.ReadString("field");

					a.UUID = Guid.NewGuid().ToString();
					AttributeFieldIDs[layer + "||" + a.FieldName] = a.UUID;

					var dataType = reader.ReadString("data_type");
					a.DataType = SwMapsTypes.AttributeTypeFromString(dataType);


					a.Choices = reader.ReadString("field_choices").Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries).ToList();
					ret.Add(a);
				}
			return ret;
		}


		private List<SwMapsFeatureLayer> ReadAllFeatureLayers(SQLiteConnection conn)
		{
			var ret = new List<SwMapsFeatureLayer>();
			using (var cmd = new SQLiteCommand("SELECT *,rowid FROM data_layers;", conn))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
				{
					var layer = new SwMapsFeatureLayer();

					layer.Name = reader.ReadString("name");

					layer.UUID = Guid.NewGuid().ToString();
					LayerIDs[layer.Name] = layer.UUID;

					layer.Color = reader.ReadInt32("point_color");
					layer.LineWidth = reader.ReadSingle("line_width") / 2f;
					layer.Active = true;
					layer.Drawn = reader.ReadInt32("drawn") == 1;

					var geomType = reader.ReadString("data_type");
					layer.GeometryType = SwMapsTypes.GeometryTypeFromString(geomType);

					var pointSymbol = reader.ReadString("point_shape");
					layer.PointShape = SwMapsTypes.PointShapeFromString(pointSymbol);

					if (layer.GeometryType == SwMapsGeometryType.Point)
						layer.Color = reader.ReadInt32("point_color");
					else
						layer.Color = reader.ReadInt32("line_color");

					layer.FillColor = reader.ReadInt32("polygon_color");

					layer.LabelFieldID = reader.ReadString("label_field");

					if (layer.LabelFieldID == "(NO LABEL)")
						layer.LabelFieldID = "";

					layer.AttributeFields = ReadAttributeFields(conn, layer.Name);
					ret.Add(layer);
				}
			return ret;
		}

	}
}

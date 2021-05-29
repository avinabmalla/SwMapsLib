using SwMapsLib.Data;
using SwMapsLib.Utils;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.IO
{
	public class SwMapsV2Reader : ISwMapsDbReader
	{
		public readonly string Swm2Path;

		SQLiteConnection conn;
		
		public SwMapsV2Reader(string swm2Path)
		{
			Swm2Path = swm2Path;
		}

		public SwMapsProject Read()
		{
			conn = new SQLiteConnection($"Data Source={Swm2Path};Version=3;");
			conn.Open();

			var mediaPath = Directory.GetParent(Path.GetDirectoryName(Swm2Path)).FullName;
			mediaPath = Path.Combine(mediaPath, "Photos");

			var project = new SwMapsProject(Swm2Path, mediaPath);

			project.FeatureLayers = ReadAllFeatureLayers();
			project.Features = ReadAllFeatures();
			project.Tracks = ReadAllTracks();
			project.PhotoPoints = ReadAllPhotoPoints();
			project.ProjectAttributes = ReadProjectAttributes();

			foreach (var f in project.Features)
			{
				var layer = project.GetLayer(f.LayerID);
				foreach (var a in f.AttributeValues)
				{
					a.FieldName = layer.AttributeFields.FirstOrDefault(e => e.UUID == a.FieldID)?.FieldName ?? "";
				}
			}
			conn.Close();
			return project;
		}

		public List<SwMapsProjectAttribute> ReadProjectAttributes()
		{
			var ret = new List<SwMapsProjectAttribute>();
			var sql = "SELECT * FROM project_attributes";
			using var cmd = new SQLiteCommand(sql, conn);
			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				var a = new SwMapsProjectAttribute();
				a.Name = reader.ReadString("attr");
				a.Value = reader.ReadString("value");
				a.IsRequired = reader.ReadInt32("required_field") == 1;
				ret.Add(a);
			}
			return ret;
		}

		public List<SwMapsAttributeField> ReadAttributeFields(string layerID)
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
					var dataType = reader.ReadString("data_type").ToUpper();
					if (dataType == "TEXT")
						a.DataType = SwMapsAttributeType.Text;
					else if (dataType == "NUMERIC")
						a.DataType = SwMapsAttributeType.Numeric;
					else if (dataType == "OPTIONS")
						a.DataType = SwMapsAttributeType.Options;
					else if (dataType == "PHOTO")
						a.DataType = SwMapsAttributeType.Photo;
					else if (dataType == "AUDIO")
						a.DataType = SwMapsAttributeType.Audio;
					else if (dataType == "VIDEO")
						a.DataType = SwMapsAttributeType.Video;

					a.Choices = reader.ReadString("field_choices").Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries).ToList();
					if (ret.Any(at => at.UUID == a.UUID)) continue;
					ret.Add(a);


				}
			return ret;
		}

		public List<SwMapsAttributeValue> ReadAttributeValues(string fid)
		{
			var ret = new List<SwMapsAttributeValue>();
			var sql = $"SELECT * FROM attribute_values WHERE item_id='{fid}'";
			using (var cmd = new SQLiteCommand(sql, conn))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
				{
					var a = new SwMapsAttributeValue();
					a.FeatureID = fid;
					a.FieldID = reader.ReadString("field_id");
					a.Value = reader.ReadString("value");

					var dataType = reader.ReadString("data_type").ToUpper();
					if (dataType == "TEXT")
						a.DataType = SwMapsAttributeType.Text;
					else if (dataType == "NUMERIC")
						a.DataType = SwMapsAttributeType.Numeric;
					else if (dataType == "OPTIONS")
						a.DataType = SwMapsAttributeType.Options;
					else if (dataType == "PHOTO")
						a.DataType = SwMapsAttributeType.Photo;
					else if (dataType == "AUDIO")
						a.DataType = SwMapsAttributeType.Audio;
					else if (dataType == "VIDEO")
						a.DataType = SwMapsAttributeType.Video;
					ret.Add(a);
				}

			return ret;
		}

		public List<SwMapsFeature> ReadAllFeatures()
		{
			var ret = new List<SwMapsFeature>();
			var sql = "SELECT * FROM features";
			using (var cmd = new SQLiteCommand(sql, conn))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
				{
					var feature = new SwMapsFeature();
					feature.UUID = reader.ReadString("uuid");
					feature.LayerID = reader.ReadString("layer_id");
					feature.Name = reader.ReadString("name");
					feature.Remarks = reader.ReadString("remarks");
					feature.Points = ReadPoints(feature.UUID);
					feature.AttributeValues = ReadAttributeValues(feature.UUID);


					ret.Add(feature);
				}
			return ret;
		}

		public List<SwMapsFeatureLayer> ReadAllFeatureLayers()
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
					var geomType = reader.ReadString("geom_type").ToUpper();

					if (geomType == "POINT")
						layer.GeometryType = SwMapsGeometryType.Point;
					if (geomType == "LINE")
						layer.GeometryType = SwMapsGeometryType.Line;
					if (geomType == "POLYGON")
						layer.GeometryType = SwMapsGeometryType.Polygon;

					var pointSymbol = reader.ReadString("point_symbol").ToUpper();
					if (pointSymbol == "CIRCLE")
						layer.PointShape = SwMapsPointShape.Circle;
					else if (pointSymbol == "CIRCLE_FILL")
						layer.PointShape = SwMapsPointShape.FilledCircle;
					else if (pointSymbol == "TRIANGLE")
						layer.PointShape = SwMapsPointShape.Triangle;
					else if (pointSymbol == "SQUARE")
						layer.PointShape = SwMapsPointShape.Square;

					layer.Color = reader.ReadInt32("color");
					layer.FillColor = reader.ReadInt32("fill_color");
					layer.LineWidth = reader.ReadSingle("line_width");
					layer.LabelFieldID = reader.ReadString("label_field_id");
					layer.Active = reader.ReadInt32("active") == 1;
					layer.Drawn = reader.ReadInt32("drawn") == 1;
					layer.PngSymbol = reader.ReadBlob("png_symbol");
					layer.AttributeFields = ReadAttributeFields(layer.UUID);
					ret.Add(layer);
				}
			return ret;
		}

		public List<SwMapsPoint> ReadPoints(string fid)
		{
			var ret = new List<SwMapsPoint>();
			var sql = $"SELECT * FROM points WHERE fid='{fid}' ORDER BY seq";
			using (var pointCmd = new SQLiteCommand(sql, conn))
			using (var pointReader = pointCmd.ExecuteReader())
				while (pointReader.Read())
				{
					SwMapsPoint vertex = new SwMapsPoint();
					vertex.Latitude = pointReader.ReadDouble("lat");
					vertex.Longitude = pointReader.ReadDouble("lon");
					vertex.Elevation = pointReader.ReadDouble("elv");
					vertex.Time = pointReader.ReadInt64("time");
					ret.Add(vertex);
				}
			return ret;
		}

		public List<SwMapsTrack> ReadAllTracks()
		{
			List<SwMapsTrack> ret = new List<SwMapsTrack>();

			var sql = "SELECT * FROM tracks";
			using (var cmd = new SQLiteCommand(sql, conn))
			using (var trackReader = cmd.ExecuteReader())
				while (trackReader.Read())
				{
					SwMapsTrack tr = new SwMapsTrack();
					tr.UUID = trackReader.ReadString("uuid");
					tr.Name = trackReader.ReadString("name");
					tr.Color = trackReader.ReadInt32("color");
					tr.Remarks = trackReader.ReadString("description");
					ret.Add(tr);
				}

			foreach (var tr in ret)
			{
				tr.Vertices = ReadPoints(tr.UUID);
			}
			return ret;
		}

		public List<SwMapsPhotoPoint> ReadAllPhotoPoints()
		{
			var ret = new List<SwMapsPhotoPoint>();
			var sql = "SELECT * FROM photos";
			using (var cmd = new SQLiteCommand(sql, conn))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
				{
					var ph = new SwMapsPhotoPoint();
					ph.ID = reader.ReadString("uuid");
					ph.Remarks = reader.ReadString("remarks");
					ph.FileName = reader.ReadString("photo_path");
					ph.Location = ReadPoints(ph.ID).FirstOrDefault();
					ret.Add(ph);
				}
			return ret;
		}
	}
}

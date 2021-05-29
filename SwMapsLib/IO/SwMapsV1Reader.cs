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
	public class SwMapsV1Reader : ISwMapsDbReader
	{
		public readonly string SwMapsPath;
		SQLiteConnection conn;

		public Dictionary<string, string> LayerIDs = new Dictionary<string, string>();
		public Dictionary<int, string> PointIDs = new Dictionary<int, string>();
		public Dictionary<string, string> LinePolygonIDs = new Dictionary<string, string>();
		public Dictionary<string, string> AttributeFieldIDs = new Dictionary<string, string>();

		public SwMapsV1Reader(string swmapsPath)
		{
			SwMapsPath = swmapsPath;
		}

		public SwMapsProject Read()
		{
			conn = new SQLiteConnection($"Data Source={SwMapsPath};Version=3;");
			conn.Open();

			var mediaPath = Directory.GetParent(Path.GetDirectoryName(SwMapsPath)).FullName;
			mediaPath = Path.Combine(mediaPath, "Photos");

			var project = new SwMapsProject(SwMapsPath, mediaPath);

			project.FeatureLayers = ReadAllFeatureLayers();
			project.Features = ReadAllFeatures();
			project.Tracks = ReadAllTracks();
			project.PhotoPoints = ReadAllPhotoPoints();
			conn.Close();
			return project;
		}

		private List<SwMapsAttributeField> ReadAttributeFields(string layer)
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
					a.UUID = layer + "||" + a.FieldName;

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
					ret.Add(a);
				}
			return ret;
		}

		private List<SwMapsPoint> ReadPolylinePoints(int lineId)
		{
			var ret = new List<SwMapsPoint>();
			using (var cmd = new SQLiteCommand($"SELECT * FROM polyline_points WHERE line_id='{lineId}';", conn))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
				{
					var pt = new SwMapsPoint();
					pt.Latitude = reader.ReadDouble("lat");
					pt.Longitude = reader.ReadDouble("lon");
					pt.Elevation = reader.ReadDouble("elv");
					pt.Time = reader.ReadInt64("time");
					pt.StartTime = reader.ReadInt64("start_time");
					pt.FeatureID = lineId.ToString();
					pt.Seq = reader.ReadInt32("seq");
					ret.Add(pt);
				}
			return ret;
		}

		private List<SwMapsPoint> ReadTrackPoints(int lineId)
		{
			var ret = new List<SwMapsPoint>();
			using (var cmd = new SQLiteCommand($"SELECT * FROM track_points WHERE line_id='{lineId}';", conn))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
				{
					var pt = new SwMapsPoint();
					pt.Latitude = reader.ReadDouble("lat");
					pt.Longitude = reader.ReadDouble("lon");
					pt.Elevation = reader.ReadDouble("elv");
					pt.Time = reader.ReadInt64("time");
					pt.StartTime = pt.Time;
					pt.FeatureID = lineId.ToString();
					pt.Seq = reader.ReadInt32("seq");
					ret.Add(pt);
				}
			return ret;
		}

		private List<SwMapsAttributeValue> ReadAllAttributeValues(string fid, int itemID, string itemLayer)
		{
			var sql = $"SELECT * FROM attribute_data WHERE item_id='{itemID}' AND item_layer='{itemLayer}';";
			var ret = new List<SwMapsAttributeValue>();

			using (var cmd = new SQLiteCommand(sql, conn))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
				{
					var attr = new SwMapsAttributeValue();
					attr.Value = reader.ReadString("value");
					attr.FieldName = reader.ReadString("field");
					attr.FieldID = itemLayer + "||" + attr.FieldName;
					attr.FeatureID = fid;

					var dataType = reader.ReadString("data_type").ToUpper();
					if (dataType == "TEXT")
						attr.DataType = SwMapsAttributeType.Text;
					else if (dataType == "NUMERIC")
						attr.DataType = SwMapsAttributeType.Numeric;
					else if (dataType == "OPTIONS")
						attr.DataType = SwMapsAttributeType.Options;
					else if (dataType == "PHOTO")
						attr.DataType = SwMapsAttributeType.Photo;
					else if (dataType == "AUDIO")
						attr.DataType = SwMapsAttributeType.Audio;
					else if (dataType == "VIDEO")
						attr.DataType = SwMapsAttributeType.Video;

					ret.Add(attr);

				}
			return ret;
		}

		private List<SwMapsPoint> ReadPolylinePoints(string fid, int lineId)
		{
			var ret = new List<SwMapsPoint>();

			using (var cmd = new SQLiteCommand($"SELECT * FROM polyline_points WHERE line_id='{lineId}';", conn))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
				{
					var pt = new SwMapsPoint();
					pt.ID = Guid.NewGuid().ToString();
					pt.FeatureID = fid;
					pt.Seq = reader.ReadInt32("seq");
					pt.Latitude = reader.ReadDouble("lat");
					pt.Longitude = reader.ReadDouble("lon");
					pt.Elevation = reader.ReadDouble("elv");
					pt.Speed = 0;
					pt.Time = reader.ReadInt64("time");
					pt.StartTime = reader.ReadInt64("start_time");
					ret.Add(pt);
				}
			return ret;
		}

		private List<SwMapsFeature> ReadAllPointFeatures()
		{
			var ret = new List<SwMapsFeature>();
			using (var cmd = new SQLiteCommand($"SELECT * FROM points;", conn))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
				{
					var f = new SwMapsFeature();
					f.FeatureID = reader.ReadInt32("id");
					f.UUID = reader.ReadString("uuid");
					f.Name = reader.ReadInt32("id").ToString();
					f.Remarks = reader.ReadString("description");
					f.LayerID = reader.ReadString("layer");
					f.GeometryType = SwMapsGeometryType.Point;

					var pt = new SwMapsPoint();
					pt.ID = Guid.NewGuid().ToString();
					pt.FeatureID = f.UUID;
					pt.Seq = 0;
					pt.Latitude = reader.ReadDouble("lat");
					pt.Longitude = reader.ReadDouble("lon");
					pt.Elevation = reader.ReadDouble("elv");
					pt.Speed = 0;
					pt.Time = reader.ReadInt64("time");
					pt.StartTime = reader.ReadInt64("start_time");

					f.Points = new List<SwMapsPoint>();
					f.Points.Add(pt);

					f.AttributeValues = ReadAllAttributeValues(f.UUID, f.FeatureID, f.LayerID);
					ret.Add(f);
				}
			return ret;
		}

		private List<SwMapsFeature> ReadAllLinePolygonFeatures()
		{
			var ret = new List<SwMapsFeature>();
			using (var cmd = new SQLiteCommand($"SELECT * FROM polylines;", conn))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
				{
					var f = new SwMapsFeature();
					f.FeatureID = reader.ReadInt32("id");
					f.UUID = reader.ReadString("uuid");
					f.Name = reader.ReadString("name");
					f.LayerID = reader.ReadString("layer");
					f.Remarks = reader.ReadString("description");
					var closed = reader.ReadInt32("closed") != 0;
					f.GeometryType = closed ? SwMapsGeometryType.Polygon : SwMapsGeometryType.Line;
					f.Points = ReadPolylinePoints(f.UUID, f.FeatureID);
					f.AttributeValues = ReadAllAttributeValues(f.UUID, f.FeatureID, f.LayerID);
					ret.Add(f);
				}
			return ret;
		}

		private List<SwMapsFeature> ReadAllFeatures()
		{
			var ret = new List<SwMapsFeature>();
			ret.AddRange(ReadAllPointFeatures());
			ret.AddRange(ReadAllLinePolygonFeatures());
			return ret;
		}

		private List<SwMapsFeatureLayer> ReadAllFeatureLayers()
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
					layer.Active = reader.ReadInt32("active") == 1;
					layer.Drawn = reader.ReadInt32("drawn") == 1;

					var geomType = reader.ReadString("data_type").ToUpper();

					if (geomType == "POINT")
						layer.GeometryType = SwMapsGeometryType.Point;
					if (geomType == "LINE")
						layer.GeometryType = SwMapsGeometryType.Line;
					if (geomType == "POLYGON")
						layer.GeometryType = SwMapsGeometryType.Polygon;

					var pointSymbol = reader.ReadString("point_shape").ToUpper();
					if (pointSymbol == "CIRCLE")
						layer.PointShape = SwMapsPointShape.Circle;
					else if (pointSymbol == "CIRCLE_FILL")
						layer.PointShape = SwMapsPointShape.FilledCircle;
					else if (pointSymbol == "TRIANGLE")
						layer.PointShape = SwMapsPointShape.Triangle;
					else if (pointSymbol == "SQUARE")
						layer.PointShape = SwMapsPointShape.Square;

					if (layer.GeometryType == SwMapsGeometryType.Point)
						layer.Color = reader.ReadInt32("point_color");
					else
						layer.Color = reader.ReadInt32("line_color");

					layer.FillColor = reader.ReadInt32("polygon_color");

					layer.LabelFieldID = reader.ReadString("label_field");

					if (layer.LabelFieldID == "(NO LABEL)")
						layer.LabelFieldID = "";

					layer.AttributeFields = ReadAttributeFields(layer.Name);
					ret.Add(layer);
				}
			return ret;
		}

		private List<SwMapsTrack> ReadAllTracks()
		{
			List<SwMapsTrack> ret = new List<SwMapsTrack>();

			var sql = "SELECT * FROM tracks";
			using (var cmd = new SQLiteCommand(sql, conn))
			using (var trackReader = cmd.ExecuteReader())
				while (trackReader.Read())
				{
					SwMapsTrack tr = new SwMapsTrack();
					tr.UUID = trackReader.ReadInt32("id").ToString();
					tr.Name = trackReader.ReadString("name");
					tr.Color = trackReader.ReadInt32("color");
					tr.Remarks = trackReader.ReadString("description");
					ret.Add(tr);
				}

			foreach (var tr in ret)
			{
				tr.Vertices = ReadTrackPoints(Convert.ToInt32(tr.UUID));
			}
			return ret;
		}

		private List<SwMapsPhotoPoint> ReadAllPhotoPoints()
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
					ph.Location = new SwMapsPoint();
					ph.Location.Latitude = reader.ReadDouble("lat");
					ph.Location.Longitude = reader.ReadDouble("lon");
					ph.Location.Elevation = reader.ReadDouble("elv");
					ret.Add(ph);
				}
			return ret;
		}
	}
}

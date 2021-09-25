using SwMapsLib.Data;
using SwMapsLib.Primitives;
using SwMapsLib.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SwMapsLib
{
	public sealed class SwMapsProjectBuilder
	{
		public SwMapsProject Project { get; private set; }

		public SwMapsProjectBuilder()
		{
			Project = new SwMapsProject();
		}

		public void ImportTemplate(SwMapsTemplate template)
		{
			Project.FeatureLayers.AddRange(template.Layers);
			Project.ProjectAttributes.AddRange(template.ProjectAttributes);
		}


		public SwMapsProjectAttribute AddProjectAttribute(string name, SwMapsProjectAttributeType dataType,
			string value = "", bool required = false, int fieldLength = 0, IEnumerable<string> options = null)
		{
			var ret = new SwMapsProjectAttribute
			{
				Name = name,
				Value = value,
				DataType = dataType,
				IsRequired = required,
				FieldLength = fieldLength,
				Choices = (options != null) ? options.ToList() : new List<string>()
			};

			Project.ProjectAttributes.Add(ret);
			return ret;
		}

		SwMapsFeatureLayer GetLayer(string layerName)
		{
			foreach (var l in Project.FeatureLayers)
			{
				if (l.Name.Equals(layerName, StringComparison.OrdinalIgnoreCase))
				{
					return l;
				}
			}
			return null;
		}
		public SwMapsFeatureLayer AddFeatureLayer(
			string layerName,
			SwMapsGeometryType geomType,
			bool isDrawn)
		{

			if (GetLayer(layerName) != null)
			{
				throw new Exception($"Layer having name {layerName} already exists!");
			}


			var lyr = new SwMapsFeatureLayer
			{
				UUID = Guid.NewGuid().ToString(),
				Name = layerName,
				GeometryType = geomType,
				Drawn = isDrawn,
				Active = true
			};
			Project.FeatureLayers.Add(lyr);
			return lyr;
		}

		public void AddAttributeField(string layerName, string attrName,
			SwMapsAttributeType attrType, IEnumerable<string> choices = null)
		{
			SwMapsFeatureLayer layer = GetLayer(layerName);

			if (layer == null)
				throw new Exception($"Cannot find layer having name {layerName}!");

			var attr = new SwMapsAttributeField
			{
				LayerID = layer.UUID,
				FieldName = attrName,
				DataType = attrType,
				Choices = (choices != null) ? choices.ToList() : new List<string>()
			};


			layer.AttributeFields.Add(attr);
		}

		public SwMapsFeature AddPointFeature(string layerName, double lat,
			double lon, double elv, DateTime? time = null)
		{
			DateTime time1 = time ?? DateTime.UtcNow;

			SwMapsFeatureLayer layer = GetLayer(layerName);

			if (layer == null)
				throw new Exception($"Cannot find layer having name {layerName}!");

			if (layer.GeometryType != SwMapsGeometryType.Point)
				throw new Exception($"Layer {layerName} is not a point layer!");

			var f = new SwMapsFeature
			{
				LayerID = layer.UUID,
				UUID = Guid.NewGuid().ToString(),
				GeometryType = SwMapsGeometryType.Point
			};

			var pt = new SwMapsPoint
			{
				FeatureID = f.UUID,
				Seq = 0,
				Latitude = lat,
				Longitude = lon,
				Elevation = elv,
				FixID = 0,
				Time = TimeHelper.DateTimeToJavaTimeStamp(time1),
				StartTime = TimeHelper.DateTimeToJavaTimeStamp(time1)
			};
			f.Points.Add(pt);

			Project.Features.Add(f);

			return f;
		}

		public SwMapsFeature AddLineFeature(string layerName,
			string name, List<LatLng> points, DateTime? time = null)
		{
			DateTime time1 = time ?? DateTime.UtcNow;

			SwMapsFeatureLayer layer = GetLayer(layerName);

			if (layer == null)
				throw new Exception($"Cannot find layer having name {layerName}!");

			if (layer.GeometryType != SwMapsGeometryType.Line)
				throw new Exception($"Layer {layerName} is not a line layer!");

			var f = new SwMapsFeature
			{
				Name = name,
				LayerID = layer.UUID,
				UUID = Guid.NewGuid().ToString(),
				GeometryType = SwMapsGeometryType.Line
			};

			foreach (var p in points)
			{
				var pt = new SwMapsPoint
				{
					FeatureID = f.UUID,
					Seq = f.Points.Count,
					Latitude = p.Latitude,
					Longitude = p.Longitude,
					Elevation = p.Elevation,
					FixID = 0,
					Time = TimeHelper.DateTimeToJavaTimeStamp(time1),
					StartTime = TimeHelper.DateTimeToJavaTimeStamp(time1)
				};
				f.Points.Add(pt);
			}

			Project.Features.Add(f);
			return f;
		}

		public SwMapsFeature AddPolygonFeature(string layerName, string name,
			List<LatLng> points, DateTime? time = null)
		{
			DateTime time1 = time ?? DateTime.UtcNow;

			SwMapsFeatureLayer layer = GetLayer(layerName);

			if (layer == null)
				throw new Exception($"Cannot find layer having name {layerName}!");

			if (layer.GeometryType != SwMapsGeometryType.Polygon)
				throw new Exception($"Layer {layerName} is not a polygon layer!");

			var f = new SwMapsFeature
			{
				Name = name,
				LayerID = layer.UUID,
				UUID = Guid.NewGuid().ToString(),
				GeometryType = SwMapsGeometryType.Polygon
			};

			foreach (var p in points)
			{
				var pt = new SwMapsPoint
				{
					FeatureID = f.UUID,
					Seq = f.Points.Count,
					Latitude = p.Latitude,
					Longitude = p.Longitude,
					Elevation = p.Elevation,
					FixID = 0,
					Time = TimeHelper.DateTimeToJavaTimeStamp(time1),
					StartTime = TimeHelper.DateTimeToJavaTimeStamp(time1)
				};
				f.Points.Add(pt);
			}

			Project.Features.Add(f);
			return f;
		}

		public void SetAttributeValue(SwMapsFeature feature, string fieldName, string value)
		{
			var layer = Project.GetLayer(feature.LayerID);
			if (layer == null)
				throw new Exception($"Cannot find layer of the feature!");

			var attrField = layer.GetAttributeField(fieldName);
			if (attrField == null)
				throw new Exception($"Layer {layer.Name} does not have attribute field {fieldName}!");


			if (attrField.DataType == SwMapsAttributeType.Options)
			{
				if (attrField.Choices.Contains(value) == false)
					throw new Exception($"Option attribute does not have the option {value}!");
			}
			else if (attrField.DataType == SwMapsAttributeType.Photo
				|| attrField.DataType == SwMapsAttributeType.Audio
				|| attrField.DataType == SwMapsAttributeType.Video)
			{
				if (System.IO.File.Exists(value) == false)
				{
					throw new Exception($"Media attribute file does not exist!");
				}
			}


			foreach (var a in feature.AttributeValues)
			{
				if (a.FieldID == attrField.UUID)
				{
					a.Value = value;
					return;
				}
			}

			var attr = new SwMapsAttributeValue
			{
				FieldID = attrField.UUID,
				FeatureID = feature.UUID,
				FieldName = attrField.FieldName,
				DataType = attrField.DataType,
				Value = value
			};

			feature.AttributeValues.Add(attr);
		}

		public void SetLayerStyle(string layerName, SwMapsPointShape pointShape, Color color, float lineWidth)
		{
			var layer = GetLayer(layerName);
			if (layer == null)
				throw new Exception($"Cannot find layer having name {layerName}!");

			layer.PointShape = pointShape;
			var ptColor = Color.FromArgb(color.R, color.G, color.B);
			var fillColor = Color.FromArgb(80, color.R, color.G, color.B);
			layer.Color = ColorUtils.GetColorInt(ptColor);
			layer.FillColor = ColorUtils.GetColorInt(fillColor);
			layer.LineWidth = lineWidth;
		}
	}
}

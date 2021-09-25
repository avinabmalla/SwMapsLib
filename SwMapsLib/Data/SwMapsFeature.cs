using SwMapsLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.Data
{
	public class SwMapsFeature
	{
		public int FeatureID { get; set; }
		public string UUID { get; set; } = Guid.NewGuid().ToString();
		public string Name { get; set; }
		public string Remarks { get; set; }
		public string LayerID { get; set; }
		public SwMapsGeometryType GeometryType { get; set; }

		public List<SwMapsPoint> Points { get; set; } = new List<SwMapsPoint>();

		public List<SwMapsAttributeValue> AttributeValues { get; set; } = new List<SwMapsAttributeValue>();


		public SwMapsAttributeValue GetAttributeByFieldname(string fieldName)
		{
			return AttributeValues.FirstOrDefault(iterator => iterator.FieldName == fieldName);
		}


		public string GetLabel(SwMapsFeatureLayer layer)
		{
			if (layer.LabelFieldID == "") return "";
			return AttributeValues.FirstOrDefault(av => av.FieldID == layer.LabelFieldID)?.Value ?? "";
		}

		public double Length
		{
			get
			{
				if (GeometryType == SwMapsGeometryType.Point) return 0;
				var pts = Points.Select(pt => pt.ToLatLng()).ToList();
				double l = 0;
				for (int i = 0; i < pts.Count-1; i++)
				{
					l += SphericalUtil.computeDistanceBetween(pts[i], pts[i + 1]);
				}
				if (GeometryType == SwMapsGeometryType.Polygon)
				{
					l += SphericalUtil.computeDistanceBetween(pts.Last(), pts.First());
				}
				return Math.Round(l,4);
			}
		}
		public object Area
		{
			get
			{
				if (GeometryType != SwMapsGeometryType.Polygon) return 0;
				var pts = Points.Select(pt => pt.ToLatLng()).ToList();
				return Math.Round(SphericalUtil.computeArea(pts),4);
			}
		}

		public long GetLastModifiedTime()
		{
			long maxTime = 0;
			foreach (var pt in Points) maxTime = Math.Max(pt.Time, maxTime);
			return maxTime;
		}

	}
}

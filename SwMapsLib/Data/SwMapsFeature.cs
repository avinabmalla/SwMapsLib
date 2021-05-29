using NetTopologySuite.Geometries;
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
		public string UUID { get; set; }
		public string Name { get; set; }
		public string Remarks { get; set; }
		public string LayerID { get; set; }
		public SwMapsGeometryType GeometryType { get; set; }

		public List<SwMapsPoint> Points { get; set; }

		public List<SwMapsAttributeValue> AttributeValues;


		public SwMapsAttributeValue GetAttributeByFieldname(string fieldName)
		{
			return AttributeValues.FirstOrDefault(iterator => iterator.FieldName == fieldName);
		}
		public string ToWKT()
		{
			var points = Points.Select(pt => new CoordinateZ(pt.Longitude, pt.Latitude, pt.Elevation)).ToArray();
			if (GeometryType == SwMapsGeometryType.Point)
			{
				var geom = new NetTopologySuite.Geometries.Point(points.First());
				return geom.ToString();
			}
			if (GeometryType == SwMapsGeometryType.Line)
			{
				if (points.Count() == 1)
				{
					var pts = new List<CoordinateZ>(points);
					pts.Add(points[0]);
					points = pts.ToArray();
				}
				var geom = new NetTopologySuite.Geometries.LineString(points);
				return geom.ToString();
			}
			if (GeometryType == SwMapsGeometryType.Polygon)
			{
				var pts = new List<CoordinateZ>(points);

				if (points.Count() == 2)
				{
					pts.Add(points[0]);
				}
				else if (points.Count() == 1)
				{
					pts.Add(points[0]);
					pts.Add(points[0]);
				}

				points = pts.ToArray();

				var geom = new NetTopologySuite.Geometries.Polygon(new LinearRing(points));
				return geom.ToString();
			}
			return "";
		}
	}
}

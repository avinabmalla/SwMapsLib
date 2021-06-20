using SwMapsLib.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.Utils
{
	public static class SwMapsTypes
	{
		public static SwMapsPointShape PointShapeFromString(string ptShape)
		{
			if (ptShape == null) return SwMapsPointShape.Circle;
			var pu = ptShape.ToUpper().Trim();


			var ret = SwMapsPointShape.Circle;
			if (pu == "CIRCLE")
				ret = SwMapsPointShape.Circle;
			else if (pu == "CIRCLE_FILL")
				ret = SwMapsPointShape.FilledCircle;
			else if (pu == "TRIANGLE")
				ret = SwMapsPointShape.Triangle;
			else if (pu == "SQUARE")
				ret = SwMapsPointShape.Square;

			return ret;
		}

		public static SwMapsAttributeType AttributeTypeFromString(string dataType)
		{
			if (dataType == null) return SwMapsAttributeType.Text;
			var dt = dataType.ToUpper().Trim();
			var ret = SwMapsAttributeType.Text;

			if (dt == "TEXT")
				ret = SwMapsAttributeType.Text;
			else if (dt == "NUMERIC")
				ret = SwMapsAttributeType.Numeric;
			else if (dt == "OPTIONS")
				ret = SwMapsAttributeType.Options;
			else if (dt == "PHOTO")
				ret = SwMapsAttributeType.Photo;
			else if (dt == "AUDIO")
				ret = SwMapsAttributeType.Audio;
			else if (dt == "VIDEO")
				ret = SwMapsAttributeType.Video;

			return ret;
		}

		public static SwMapsProjectAttributeType ProjectAttributeTypeFromString(string dataType)
		{
			if (dataType == null) return SwMapsProjectAttributeType.Text;
			var dt = dataType.ToUpper().Trim();

			var ret = SwMapsProjectAttributeType.Text;

			if (dt == "TEXT")
				ret = SwMapsProjectAttributeType.Text;
			else if (dt == "NUMERIC")
				ret = SwMapsProjectAttributeType.Numeric;
			else if (dt == "OPTIONS")
				ret = SwMapsProjectAttributeType.Options;

			return ret;
		}

		public static SwMapsGeometryType GeometryTypeFromString(string geomType)
		{
			if (geomType == null) return SwMapsGeometryType.Point;
			var gt = geomType.ToUpper().Trim();

			var ret = SwMapsGeometryType.Point;

			if (gt == "POINT")
				ret = SwMapsGeometryType.Point;
			if (gt == "LINE")
				ret = SwMapsGeometryType.Line;
			if (gt == "POLYGON")
				ret = SwMapsGeometryType.Polygon;

			return ret;
		}

		public static string PointShapeToString(SwMapsPointShape pt)
		{
			if (pt == SwMapsPointShape.Circle) return "CIRCLE";
			if (pt == SwMapsPointShape.Triangle) return "TRIANGLE";
			if (pt == SwMapsPointShape.Square) return "SQUARE";
			if (pt == SwMapsPointShape.FilledCircle) return "CIRCLE_FILL";
			return "CIRCLE";
		}

		public static string AttributeTypeToString(SwMapsAttributeType a)
		{
			if (a == SwMapsAttributeType.Text) return "TEXT";
			if (a == SwMapsAttributeType.Numeric) return "NUMERIC";
			if (a == SwMapsAttributeType.Options) return "OPTIONS";
			if (a == SwMapsAttributeType.Photo) return "PHOTO";
			if (a == SwMapsAttributeType.Audio) return "AUDIO";
			if (a == SwMapsAttributeType.Video) return "VIDEO";
			return "TEXT";
		}
		public static string ProjectAttributeTypeToString(SwMapsProjectAttributeType a)
		{
			if (a == SwMapsProjectAttributeType.Text) return "TEXT";
			if (a == SwMapsProjectAttributeType.Numeric) return "NUMERIC";
			if (a == SwMapsProjectAttributeType.Options) return "OPTIONS";
			return "TEXT";
		}

		public static string GeometryTypeToString(SwMapsGeometryType gt)
		{
			if (gt == SwMapsGeometryType.Point) return "POINT";
			if (gt == SwMapsGeometryType.Line) return "LINE";
			if (gt == SwMapsGeometryType.Polygon) return "POLYGON";
			return "POINT";
		}
	}
}

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
			var ret = SwMapsPointShape.Circle;

			if (ptShape == "CIRCLE")
				ret = SwMapsPointShape.Circle;
			else if (ptShape == "CIRCLE_FILL")
				ret = SwMapsPointShape.FilledCircle;
			else if (ptShape == "TRIANGLE")
				ret = SwMapsPointShape.Triangle;
			else if (ptShape == "SQUARE")
				ret = SwMapsPointShape.Square;

			return ret;
		}

		public static SwMapsAttributeType AttributeTypeFromString(string dataType)
		{
			var ret = SwMapsAttributeType.Text;

			if (dataType == "TEXT")
				ret = SwMapsAttributeType.Text;
			else if (dataType == "NUMERIC")
				ret = SwMapsAttributeType.Numeric;
			else if (dataType == "OPTIONS")
				ret = SwMapsAttributeType.Options;
			else if (dataType == "PHOTO")
				ret = SwMapsAttributeType.Photo;
			else if (dataType == "AUDIO")
				ret = SwMapsAttributeType.Audio;
			else if (dataType == "VIDEO")
				ret = SwMapsAttributeType.Video;

			return ret;
		}

		public static SwMapsProjectAttributeType ProjectAttributeTypeFromString(string dataType)
		{
			var ret = SwMapsProjectAttributeType.Text;

			if (dataType == "TEXT")
				ret = SwMapsProjectAttributeType.Text;
			else if (dataType == "NUMERIC")
				ret = SwMapsProjectAttributeType.Numeric;
			else if (dataType == "OPTIONS")
				ret = SwMapsProjectAttributeType.Options;

			return ret;
		}

		public static SwMapsGeometryType GeometryTypeFromString(string geomType)
		{
			var ret = SwMapsGeometryType.Point;

			if (geomType == "POINT")
				ret = SwMapsGeometryType.Point;
			if (geomType == "LINE")
				ret = SwMapsGeometryType.Line;
			if (geomType == "POLYGON")
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

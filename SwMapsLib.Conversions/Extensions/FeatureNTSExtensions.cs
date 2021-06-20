using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Geometries;
using SwMapsLib;
using SwMapsLib.Data;
using System.Linq;

namespace SwMapsLib.Conversions.Extensions
{
	public static class FeatureNTSExtensions
	{
		public static Geometry ToNtsGeometry2D(this SwMapsFeature feature)
		{
			var coordinates = feature.Points.Select(pt => new Coordinate(pt.Longitude, pt.Latitude)).ToList();
			Geometry geom = null;

			if (feature.GeometryType == SwMapsGeometryType.Point)
			{
				if (coordinates.Count() <= 1) return null;
				geom = new Point(coordinates.FirstOrDefault());
			}
			else if (feature.GeometryType == SwMapsGeometryType.Line)
			{
				if (coordinates.Count() <= 2) return null;
				geom = new LineString(coordinates.ToArray());
			}
			else if (feature.GeometryType == SwMapsGeometryType.Polygon)
			{
				if (coordinates.Count() <= 3) return null;
				coordinates.Add(coordinates[0]);
				geom = new Polygon(new LinearRing(coordinates.ToArray()));
			}

			geom.SRID = 4326;
			return geom;
		}

		public static Geometry ToNtsGeometry3D(this SwMapsFeature feature, bool useOrthoHt = false)
		{
			var coordinates = feature.Points.Select(pt => new CoordinateZ(pt.Longitude, pt.Latitude, useOrthoHt ? pt.OrthoHeight : pt.Elevation)).ToList();
			Geometry geom = null;

			if (feature.GeometryType == SwMapsGeometryType.Point)
			{
				if (coordinates.Count() <= 1) return null;
				geom = new Point(coordinates.FirstOrDefault());
			}
			else if (feature.GeometryType == SwMapsGeometryType.Line)
			{
				if (coordinates.Count() <= 2) return null;
				geom = new LineString(coordinates.ToArray());
			}
			else if (feature.GeometryType == SwMapsGeometryType.Polygon)
			{
				if (coordinates.Count() <= 3) return null;
				coordinates.Add(coordinates[0]);
				geom = new Polygon(new LinearRing(coordinates.ToArray()));
			}

			geom.SRID = 4326;
			return geom;
		}
	}
}

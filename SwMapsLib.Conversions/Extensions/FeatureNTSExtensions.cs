using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Geometries;
using SwMapsLib;
using SwMapsLib.Data;
using System.Linq;
using SwMapsLib.Primitives;

namespace SwMapsLib.Conversions.Extensions
{
	public static class FeatureNTSExtensions
	{
		public static Coordinate ToCoordinate(this LatLng ll)
		{
			return new Coordinate(ll.Longitude, ll.Latitude);
		}
		public static Coordinate ToCoordinate(this SwMapsPoint pt)
		{
			return new Coordinate(pt.Longitude, pt.Latitude);
		}

		public static CoordinateZ ToCoordinateZ(this SwMapsPoint pt, bool useOrthoHt = false)
		{
			return new CoordinateZ(pt.Longitude, pt.Latitude, useOrthoHt ? pt.OrthoHeight : pt.Elevation);
		}

		public static Geometry ToNtsGeometry2D(this SwMapsFeature feature)
		{
			var coordinates = feature.Points.Select(pt => pt.ToCoordinate()).ToList();
			Geometry geom = null;

			if (feature.GeometryType == SwMapsGeometryType.Point)
			{
				if (coordinates.Count() < 1) return null;
				geom = new Point(coordinates.FirstOrDefault());
			}
			else if (feature.GeometryType == SwMapsGeometryType.Line)
			{
				if (coordinates.Count() < 2) return null;
				geom = new LineString(coordinates.ToArray());
			}
			else if (feature.GeometryType == SwMapsGeometryType.Polygon)
			{
				if (coordinates.Count() < 3) return null;
				coordinates.Add(coordinates[0]);
				geom = new Polygon(new LinearRing(coordinates.ToArray()));
			}

			geom.SRID = 4326;
			return geom;
		}

		public static Geometry ToNtsGeometry3D(this SwMapsFeature feature, bool useOrthoHt = false)
		{
			var coordinates = feature.Points.Select(pt => pt.ToCoordinateZ(useOrthoHt)).ToList();
			Geometry geom = null;

			if (feature.GeometryType == SwMapsGeometryType.Point)
			{
				if (coordinates.Count() < 1) return null;
				geom = new Point(coordinates.FirstOrDefault());
			}
			else if (feature.GeometryType == SwMapsGeometryType.Line)
			{
				if (coordinates.Count() < 2) return null;
				geom = new LineString(coordinates.ToArray());
			}
			else if (feature.GeometryType == SwMapsGeometryType.Polygon)
			{
				if (coordinates.Count() < 3) return null;
				coordinates.Add(coordinates[0]);
				geom = new Polygon(new LinearRing(coordinates.ToArray()));
			}

			geom.SRID = 4326;
			return geom;
		}

		public static Geometry ToNtsGeometry3D(this SwMapsPoint point, bool useOrthoHt = false)
		{
			var coordinates = point.ToCoordinateZ(useOrthoHt);
			Geometry geom = new Point(coordinates);
			geom.SRID = 4326;
			return geom;
		}

		public static Geometry ToNtsGeometry2D(this SwMapsPoint point, bool useOrthoHt = false)
		{
			var coordinates = point.ToCoordinate();
			Geometry geom = new Point(coordinates);
			geom.SRID = 4326;
			return geom;
		}

		public static Geometry ToNtsGeometry3D(this SwMapsTrack track, bool useOrthoHt = false)
		{

			var coordinates = track.Vertices.Select(pt => pt.ToCoordinateZ()).ToList();
			if (coordinates.Count() < 2) return null;
			var geom = new LineString(coordinates.ToArray());
			geom.SRID = 4326;
			return geom;
		}

		public static Geometry ToNtsGeometry2D(this SwMapsTrack track, bool useOrthoHt = false)
		{
			var coordinates = track.Vertices.Select(pt => pt.ToCoordinate()).ToList();
			if (coordinates.Count() < 2) return null;
			var geom = new LineString(coordinates.ToArray());
			geom.SRID = 4326;
			return geom;
		}



	}
}

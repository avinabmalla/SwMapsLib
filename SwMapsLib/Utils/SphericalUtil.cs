using SwMapsLib.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwMapsLib.Utils
{
	/// <summary>
	/// SphericalUtil Library, from the Google Maps Android Utils
	/// </summary>
	public static class SphericalUtil
	{
		const double EARTH_RADIUS = 6371009;
		static double toRadians(double deg) => deg * Math.PI / 180;
		static double toDegrees(double rad) => rad * 180 / Math.PI;
		static double atan2(double x, double y) => Math.Atan2(x, y);
		static double sin(double x) => Math.Sin(x);
		static double asin(double x) => Math.Asin(x);
		static double cos(double x) => Math.Cos(x);
		static double acos(double x) => Math.Acos(x);
		static double tan(double x) => Math.Tan(x);
		static double sqrt(double x) => Math.Sqrt(x);
		static double abs(double x) => Math.Abs(x);
		static double mod(double x, double m) => ((x % m) + m) % m;
		static double PI = Math.PI;

		public static double wrap(double n, double min, double max)
		{
			return (n >= min && n < max) ? n : (mod(n - min, max - min) + min);
		}

		public static double wrapAngle(double n)
		{
			return wrap(n, -180, 180);
		}

		static double arcHav(double x)
		{
			return 2 * asin(sqrt(x));
		}

		static double hav(double x)
		{
			double sinHalf = sin(x * 0.5);
			return sinHalf * sinHalf;
		}

		static double havDistance(double lat1, double lat2, double dLng)
		{
			return hav(lat1 - lat2) + hav(dLng) * cos(lat1) * cos(lat2);
		}


		/**
         * Returns the heading from one LatLng to another LatLng. Headings are
         * expressed in degrees clockwise from North within the range [-180,180).
         * @return The heading in degrees clockwise from north.
         */
		public static double computeHeading(LatLng from, LatLng to)
		{
			// http://williams.best.vwh.net/avform.htm#Crs
			double fromLat = toRadians(from.Latitude);
			double fromLng = toRadians(from.Longitude);
			double toLat = toRadians(to.Latitude);
			double toLng = toRadians(to.Longitude);
			double dLng = toLng - fromLng;
			double heading = atan2(
					sin(dLng) * cos(toLat),
					cos(fromLat) * sin(toLat) - sin(fromLat) * cos(toLat) * cos(dLng));
			return wrap(toDegrees(heading), -180, 180);
		}

		/**
         * Returns the LatLng resulting from moving a distance from an origin
         * in the specified heading (expressed in degrees clockwise from north).
         * @param from     The LatLng from which to start.
         * @param distance The distance to travel.
         * @param heading  The heading in degrees clockwise from north.
         */
		public static LatLng computeOffset(LatLng from, double distance, double heading)
		{
			distance /= EARTH_RADIUS;
			heading = toRadians(heading);
			// http://williams.best.vwh.net/avform.htm#LL
			double fromLat = toRadians(from.Latitude);
			double fromLng = toRadians(from.Longitude);
			double cosDistance = cos(distance);
			double sinDistance = sin(distance);
			double sinFromLat = sin(fromLat);
			double cosFromLat = cos(fromLat);
			double sinLat = cosDistance * sinFromLat + sinDistance * cosFromLat * cos(heading);
			double dLng = atan2(
					sinDistance * cosFromLat * sin(heading),
					cosDistance - sinFromLat * sinLat);
			return new LatLng(toDegrees(asin(sinLat)), toDegrees(fromLng + dLng));
		}

		/**
         * Returns the location of origin when provided with a LatLng destination,
         * meters travelled and original heading. Headings are expressed in degrees
         * clockwise from North. This function returns null when no solution is
         * available.
         * @param to       The destination LatLng.
         * @param distance The distance travelled, in meters.
         * @param heading  The heading in degrees clockwise from north.
         */
		public static LatLng computeOffsetOrigin(LatLng to, double distance, double heading)
		{
			heading = toRadians(heading);
			distance /= EARTH_RADIUS;
			// http://lists.maptools.org/pipermail/proj/2008-October/003939.html
			double n1 = cos(distance);
			double n2 = sin(distance) * cos(heading);
			double n3 = sin(distance) * sin(heading);
			double n4 = sin(toRadians(to.Latitude));
			// There are two solutions for b. b = n2 * n4 +/- sqrt(), one solution results
			// in the Latitude outside the [-90, 90] range. We first try one solution and
			// back off to the other if we are outside that range.
			double n12 = n1 * n1;
			double discriminant = n2 * n2 * n12 + n12 * n12 - n12 * n4 * n4;
			if (discriminant < 0)
			{
				// No real solution which would make sense in LatLng-space.
				return null;
			}
			double b = n2 * n4 + sqrt(discriminant);
			b /= n1 * n1 + n2 * n2;
			double a = (n4 - n2 * b) / n1;
			double fromLatRadians = atan2(a, b);
			if (fromLatRadians < -PI / 2 || fromLatRadians > PI / 2)
			{
				b = n2 * n4 - sqrt(discriminant);
				b /= n1 * n1 + n2 * n2;
				fromLatRadians = atan2(a, b);
			}
			if (fromLatRadians < -PI / 2 || fromLatRadians > PI / 2)
			{
				// No solution which would make sense in LatLng-space.
				return null;
			}
			double fromLngRadians = toRadians(to.Longitude) -
					atan2(n3, n1 * cos(fromLatRadians) - n2 * sin(fromLatRadians));
			return new LatLng(toDegrees(fromLatRadians), toDegrees(fromLngRadians));
		}

		/**
         * Returns the LatLng which lies the given fraction of the way between the
         * origin LatLng and the destination LatLng.
         * @param from     The LatLng from which to start.
         * @param to       The LatLng toward which to travel.
         * @param fraction A fraction of the distance to travel.
         * @return The interpolated LatLng.
         */
		public static LatLng interpolate(LatLng from, LatLng to, double fraction)
		{
			// http://en.wikipedia.org/wiki/Slerp
			double fromLat = toRadians(from.Latitude);
			double fromLng = toRadians(from.Longitude);
			double toLat = toRadians(to.Latitude);
			double toLng = toRadians(to.Longitude);
			double cosFromLat = cos(fromLat);
			double cosToLat = cos(toLat);

			// Computes Spherical interpolation coefficients.
			double angle = computeAngleBetween(from, to);
			double sinAngle = sin(angle);
			if (sinAngle < 1E-6)
			{
				return from;
			}
			double a = sin((1 - fraction) * angle) / sinAngle;
			double b = sin(fraction * angle) / sinAngle;

			// Converts from polar to vector and interpolate.
			double x = a * cosFromLat * cos(fromLng) + b * cosToLat * cos(toLng);
			double y = a * cosFromLat * sin(fromLng) + b * cosToLat * sin(toLng);
			double z = a * sin(fromLat) + b * sin(toLat);

			// Converts interpolated vector back to polar.
			double lat = atan2(z, sqrt(x * x + y * y));
			double lng = atan2(y, x);
			return new LatLng(toDegrees(lat), toDegrees(lng));
		}


		public static LatLng Interpolate(List<LatLng> vertices, double DistanceM)
		{
			var totalLength = computeLength(vertices);
			if (DistanceM >= totalLength) return vertices.Last();
			if (DistanceM <= 0) return vertices.First();

			var distance = 0.0;
			for (int i = 0; i < vertices.Count - 1; i++)
			{
				var pt1 = vertices[i];
				var pt2 = vertices[i + 1];
				var segmentLength = computeDistanceBetween(pt1, pt2);
				if (DistanceM > distance && DistanceM <= distance + segmentLength)
				{
					var fraction = (DistanceM - distance) / segmentLength;
					return interpolate(pt1, pt2, fraction);
				}
				distance += segmentLength;
			}
			return vertices.Last();
		}

		/**
         * Returns distance on the unit sphere; the arguments are in radians.
         */
		private static double distanceRadians(double lat1, double lng1, double lat2, double lng2)
		{
			return arcHav(havDistance(lat1, lat2, lng1 - lng2));
		}

		/**
         * Returns the angle between two LatLngs, in radians. This is the same as the distance
         * on the unit sphere.
         */
		static double computeAngleBetween(LatLng from, LatLng to)
		{
			return distanceRadians(toRadians(from.Latitude), toRadians(from.Longitude),
								   toRadians(to.Latitude), toRadians(to.Longitude));
		}

		/**
         * Returns the distance between two LatLngs, in meters.
         */
		public static double computeDistanceBetween(LatLng from, LatLng to)
		{
			return computeAngleBetween(from, to) * EARTH_RADIUS;
		}

		/**
         * Returns the length of the given path, in meters, on Earth.
         */
		public static double computeLength(List<LatLng> path)
		{
			if (path.Count < 2)
			{
				return 0;
			}
			double length = 0;
			LatLng prev = path[0];
			double prevLat = toRadians(prev.Latitude);
			double prevLng = toRadians(prev.Longitude);
			foreach (LatLng point in path)
			{
				double lat = toRadians(point.Latitude);
				double lng = toRadians(point.Longitude);
				length += distanceRadians(prevLat, prevLng, lat, lng);
				prevLat = lat;
				prevLng = lng;
			}
			return length * EARTH_RADIUS;
		}

		/**
         * Returns the area of a closed path on Earth.
         * @param path A closed path.
         * @return The path's area in square meters.
         */
		public static double computeArea(List<LatLng> path)
		{
			return abs(computeSignedArea(path));
		}

		/**
         * Returns the signed area of a closed path on Earth. The sign of the area may be used to
         * determine the orientation of the path.
         * "inside" is the surface that does not contain the South Pole.
         * @param path A closed path.
         * @return The loop's area in square meters.
         */
		public static double computeSignedArea(List<LatLng> path)
		{
			return computeSignedArea(path, EARTH_RADIUS);
		}

		/**
         * Returns the signed area of a closed path on a sphere of given radius.
         * The computed area uses the same units as the radius squared.
         * Used by SphericalUtilTest.
         */
		static double computeSignedArea(List<LatLng> path, double radius)
		{
			int size = path.Count;
			if (size < 3) { return 0; }
			double total = 0;
			LatLng prev = path[size - 1];
			double prevTanLat = tan((PI / 2 - toRadians(prev.Latitude)) / 2);
			double prevLng = toRadians(prev.Longitude);
			// For each edge, accumulate the signed area of the triangle formed by the North Pole
			// and that edge ("polar triangle").
			foreach (LatLng point in path)
			{
				double tanLat = tan((PI / 2 - toRadians(point.Latitude)) / 2);
				double lng = toRadians(point.Longitude);
				total += polarTriangleArea(tanLat, lng, prevTanLat, prevLng);
				prevTanLat = tanLat;
				prevLng = lng;
			}
			return total * (radius * radius);
		}

		/**
         * Returns the signed area of a triangle which has North Pole as a vertex.
         * Formula derived from "Area of a spherical triangle given two edges and the included angle"
         * as per "Spherical Trigonometry" by Todhunter, page 71, section 103, point 2.
         * See http://books.google.com/books?id=3uBHAAAAIAAJ&pg=PA71
         * The arguments named "tan" are tan((pi/2 - Latitude)/2).
         */
		private static double polarTriangleArea(double tan1, double lng1, double tan2, double lng2)
		{
			double deltaLng = lng1 - lng2;
			double t = tan1 * tan2;
			return 2 * atan2(t * sin(deltaLng), 1 + t * cos(deltaLng));
		}
	}
}

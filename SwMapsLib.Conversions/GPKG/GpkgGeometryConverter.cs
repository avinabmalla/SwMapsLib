using SwMapsLib.Data;
using SwMapsLib.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwMapsLib.Conversions.GPKG
{
	class GpkgGeometryConverter
	{
		public static byte[] PointToGpkg(LatLng pt)
		{
			int srid = 4326;
			var ret = new List<byte>();
			ret.Add(0x47);
			ret.Add(0x50);
			ret.Add(0x0);
			if (BitConverter.IsLittleEndian)
			{
				ret.Add(1);
			}
			else
			{
				ret.Add(0);
			}

			ret.AddRange(BitConverter.GetBytes(srid));
			if (BitConverter.IsLittleEndian)
			{
				ret.Add(1);
			}
			else
			{
				ret.Add(0);
			}

			ret.AddRange(BitConverter.GetBytes(1001));
			ret.AddRange(BitConverter.GetBytes(pt.Longitude));
			ret.AddRange(BitConverter.GetBytes(pt.Latitude));
			ret.AddRange(BitConverter.GetBytes(pt.Elevation));
			return ret.ToArray();
		}

		public static byte[] LinestringToGpkg(List<LatLng> points)
		{
			int srid = 4326;
			var ret = new List<byte>();
			ret.Add(0x47);
			ret.Add(0x50);
			ret.Add(0x0);
			if (BitConverter.IsLittleEndian)
			{
				ret.Add(1);
			}
			else
			{
				ret.Add(0);
			}

			ret.AddRange(BitConverter.GetBytes(srid));
			if (BitConverter.IsLittleEndian)
			{
				ret.Add(1);
			}
			else
			{
				ret.Add(0);
			}

			ret.AddRange(BitConverter.GetBytes(1002));
			ret.AddRange(BitConverter.GetBytes(points.Count));
			foreach (var pt in points)
			{
				ret.AddRange(BitConverter.GetBytes(pt.Longitude));
				ret.AddRange(BitConverter.GetBytes(pt.Latitude));
				ret.AddRange(BitConverter.GetBytes(pt.Elevation));
			}

			return ret.ToArray();
		}

		public static byte[] PolygonToGpkg(List<LatLng> points)
		{
			int srid = 4326;
			var ret = new List<byte>();
			ret.Add(0x47);
			ret.Add(0x50);
			ret.Add(0x0);
			if (BitConverter.IsLittleEndian)
			{
				ret.Add(1);
			}
			else
			{
				ret.Add(0);
			}

			ret.AddRange(BitConverter.GetBytes(srid));
			if (BitConverter.IsLittleEndian)
			{
				ret.Add(1);
			}
			else
			{
				ret.Add(0);
			}

			ret.AddRange(BitConverter.GetBytes(1003)); // Polygon Z
			ret.AddRange(BitConverter.GetBytes(1)); // 1 Ring
			if (points.Last().Longitude != points.First().Longitude || points.Last().Latitude != points.First().Latitude)
			{
				points.Add(new LatLng(points.First().Latitude, points.First().Longitude, points.First().Elevation));
			}

			ret.AddRange(BitConverter.GetBytes(points.Count));
			foreach (var pt in points)
			{
				ret.AddRange(BitConverter.GetBytes(pt.Longitude));
				ret.AddRange(BitConverter.GetBytes(pt.Latitude));
				ret.AddRange(BitConverter.GetBytes(pt.Elevation));
			}

			return ret.ToArray();
		}

		public static byte[] PointToGpkg(SwMapsPoint point)
		{
			return PointToGpkg(point.ToLatLng());
		}
		public static byte[] LinestringToGpkg(List<SwMapsPoint> points)
		{
			var pts = points.Select(pt => pt.ToLatLng()).ToList();
			return LinestringToGpkg(pts);
		}

		public static byte[] PolygonToGpkg(List<SwMapsPoint> points)
		{
			var pts = points.Select(pt => pt.ToLatLng()).ToList();
			return PolygonToGpkg(pts);
		}

	}
}

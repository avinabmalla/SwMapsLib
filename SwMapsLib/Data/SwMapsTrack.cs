using SwMapsLib.Primitives;
using SwMapsLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.Data
{
	public class SwMapsTrack
	{
		public string UUID { get; set; }
		public string Name { get; set; }
		public int Color { get; set; }
		public string Remarks { get; set; }
		public List<SwMapsPoint> Vertices { get; set; } = new List<SwMapsPoint>();
		public List<LatLng> PointsLL => Vertices.Select(it => new LatLng(it.Latitude, it.Longitude)).ToList();
		public double Length
		{
			get
			{
				var pts = Vertices.Select(pt => pt.ToLatLng()).ToList();
				double l = 0;
				for (int i = 0; i < pts.Count; i++)
				{
					l += SphericalUtil.computeDistanceBetween(pts[i], pts[i + 1]);
				}
				return l;
			}
		}

		public long GetLastModifiedTime()
		{
			long maxTime = 0;
			foreach (var pt in Vertices) maxTime = Math.Max(pt.Time, maxTime);
			return maxTime;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}

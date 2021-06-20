using SwMapsLib.Primitives;
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
		public override string ToString()
		{
			return Name;
		}
	}
}

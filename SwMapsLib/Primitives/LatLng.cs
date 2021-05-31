using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.Primitives
{
	public class LatLng
	{
		public double Latitude { get; set; }
		public double Longitude { get; set; }

		public double Elevation { get; set; }
		public LatLng(double lat, double lon, double elv= 0)
		{
			Latitude = lat;
			Longitude = lon;
			Elevation = elv;
		}
	}
}

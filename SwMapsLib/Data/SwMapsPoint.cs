using SwMapsLib.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.Data
{
	public class SwMapsPoint
	{
		public string ID { get; set; } = Guid.NewGuid().ToString();
		public string FeatureID { get; set; }
		public int Seq { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public double Elevation { get; set; }
		public double OrthoHeight { get; set; }
		public double Speed { get; set; }
		public long Time { get; set; }
		public long StartTime { get; set; }
		public double InstrumentHeight { get; set; }
		public int FixID { get; set; }
		public string SnapID { get; set; }
		public string AdditionalData { get; set; }
		public double Bearing { get; set; }
		public double AccuracyH { get; set; }
		public double AccuracyV { get; set; }
		public string PositionData { get; set; }

		

		public override string ToString()
		{
			return $"{Latitude:0.0000000}, {Longitude:0.0000000}";
		}

		public LatLng ToLatLng()
		{
			return new LatLng(Latitude, Longitude, Elevation);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.Data
{
	public class SwMapsPoint
	{
		public string ID;
		public string FeatureID;
		public int Seq;
		public double Latitude;
		public double Longitude;
		public double Elevation;
		public double OrthoHeight;
		public double Speed;
		public long Time;
		public long StartTime;
		public double InstrumentHeight;
		public int FixID;
		public string SnapID;
		public string AdditionalData;
		public override string ToString()
		{
			return $"{Latitude:0.0000000}, {Longitude:0.0000000}";
		}
	}
}

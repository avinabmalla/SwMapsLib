using System;
using System.Collections.Generic;
using System.Text;

namespace SwMapsLib.Conversions
{
	enum NmeaFixQuality
	{
		Invalid = 0,
		Single = 1,
		DGPS = 2,
		PPP = 3,
		RtkFix = 4,
		RtkFloat = 5,
		Estimated = 6,
		ManualInput = 7,
		Simulation = 8,
		SBAS = 9
	}
}

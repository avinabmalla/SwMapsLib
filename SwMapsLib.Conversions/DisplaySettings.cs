using System;
using System.Collections.Generic;
using System.Text;

namespace SwMapsLib.Conversions
{
	public class DisplaySettings
	{
		public LengthUnit LengthUnit = LengthUnit.Meters;
		public AreaUnit AreaUnit = AreaUnit.SquareMeters;
		public LengthUnit ElevationUnit = LengthUnit.Meters;
		public LatLngUnit LatLngUnit = LatLngUnit.Degrees;
		public SpeedUnit SpeedUnit = SpeedUnit.KilometersPerHour;
	}

	public enum LatLngUnit
	{
		Degrees,
		DMS,
		DM
	}

	public enum AreaUnit {
		SquareMeters,
		Hectare,
		SquareKilometers,
		SquareMiles,
		Acre
	}

	public enum LengthUnit
	{
		Meters,
		Feet,
	}

	public enum SpeedUnit
	{
		MetersPerSecond,
		KilometersPerHour,
		MilesPerHour
	}
	
}

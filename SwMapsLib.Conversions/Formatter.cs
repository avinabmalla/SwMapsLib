using SwMapsLib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace SwMapsLib.Conversions
{
	public class Formatter
	{
		public DisplaySettings Settings { get; set; }
		public Formatter(DisplaySettings settings)
		{
			Settings = settings;
		}

		public static string GetTimeLabel(long time)
		{
			return TimeHelper.JavaTimeStampToDateTime(time).ToString("MM/dd/yyyy HH:mm:ss zzz");
		}

		internal string FormatLatLng(double l)
		{
			if (Settings.LatLngUnit == LatLngUnit.Degrees)
				return l.ToString("F8");
			else if (Settings.LatLngUnit == LatLngUnit.DMS)
			{
				var d = (int)Math.Floor(l);

				var md = (l - d) * 60;
				var m = (int)Math.Floor(md);

				var s = Math.Round((md - m) * 60);
				return d + "° " + m.ToString("00") + "'" + s.ToString("F3") + "\"";
			}
			else
			{
				var d = (int)Math.Floor(l);
				var md = (l - d) * 60;
				return d + "° " + md.ToString("F6") + "'";

			}
		}

		internal string GetElevationLabel(double elevation)
		{
			if (Settings.ElevationUnit == LengthUnit.Meters)
				return elevation.ToString("F3") + "m";
			else if (Settings.ElevationUnit == LengthUnit.Feet)
				return (elevation / 0.3048).ToString("F3") + "ft";

			return elevation.ToString("F3") + "m";
		}

		internal string GetElevationLabel1DP(double elevation)
		{
			if (Settings.ElevationUnit == LengthUnit.Meters)
				return elevation.ToString("F1") + "m";
			else if (Settings.ElevationUnit == LengthUnit.Feet)
				return (elevation / 0.3048).ToString("F1") + "ft";

			return elevation.ToString("F1") + "m";
		}
	}
}

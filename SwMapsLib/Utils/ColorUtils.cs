using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SwMapsLib.Utils
{
	public static class ColorUtils
	{
		public static Color GetColor(int argb)
		{
			byte A = (byte)((argb >> 24) & 0xFF);
			byte R = (byte)((argb >> 16) & 0xFF);
			byte G = (byte)((argb >> 8) & 0xFF);
			byte B = (byte)((argb >> 0) & 0xFF);
			return Color.FromArgb(A, R, G, B);
		}

		public static int GetColorInt(Color color)
		{
			byte A = color.A;
			byte R = color.R;
			byte G = color.G;
			byte B = color.B;

			return  (A & 0xff) << 24 | (R & 0xff) << 16 | (G & 0xff) << 8 | (B & 0xff);
		}
	}
}

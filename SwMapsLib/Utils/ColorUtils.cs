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
			int a = color.A;
			int r = color.R;
			int g = color.G;
			int b = color.B;
			return ((a << 24) | 0xFF) + ((r << 16) | 0xFF) + ((g << 8) | 0xFF) + (b | 0xFF);
		}
	}
}

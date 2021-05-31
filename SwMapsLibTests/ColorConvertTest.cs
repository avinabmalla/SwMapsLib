using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwMapsLib.Utils;
namespace SwMapsLibTests
{
	[TestClass]
	public class ColorConvertTest
	{
		[TestMethod]
		public void ColorToInt()
		{
			var c = Color.FromArgb(128, 255, 0);
			var ci = ColorUtils.GetColorInt(c);
			Assert.AreEqual(-8323328, ci);
		}

		[TestMethod]
		public void IntToColor()
		{
			var c = -8323328;
			var ci = ColorUtils.GetColor(c);

			Assert.AreEqual(128, ci.R);
			Assert.AreEqual(255, ci.G);
			Assert.AreEqual(0, ci.B);
		}
	}
}

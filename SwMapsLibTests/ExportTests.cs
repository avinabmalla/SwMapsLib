using Microsoft.VisualStudio.TestTools.UnitTesting;
using SwMapsLib.Conversions.KMZ;
using SwMapsLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLibTests
{
	[TestClass]
	public class ExportTests
	{
		[TestMethod]
		public void ExportKmz()
		{
			var path = @"Data\\TestSwmz.swmz";
			var project = new SwmzReader(path).Read();
			var exporter = new SwMapsKmzWriter(project);
			exporter.WriteKml(@"Data\\TestSwmz.kml");
			exporter.WriteKmz(@"Data\\TestSwmz.kmz");
		}
	}
}

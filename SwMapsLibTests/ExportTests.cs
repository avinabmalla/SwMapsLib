using Microsoft.VisualStudio.TestTools.UnitTesting;
using SwMapsLib.Conversions.GPKG;
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
        [TestMethod]
        public void ReadSwmzAndExportGeopackage()
        {
            //30101001_con.swmz is depricated version of Swmz. No longer maintained
            var path = @"Data\TestSwmz.swmz";
            var reader = new SwmzReader(path);
            var project = reader.Read();

            var outPath = @"Data\TestSwmz.gpkg";
            System.IO.File.Delete(outPath);
            var writer = new SwMapsGpkgWriter(project);
            writer.Export(outPath);
        }
    }
}

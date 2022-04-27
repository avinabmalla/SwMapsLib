using Microsoft.VisualStudio.TestTools.UnitTesting;
using SwMapsLib.IO;
using System;
using System.Linq;

namespace SwMapsLibTests
{
	[TestClass]
	public class SwMapsReadTests
	{
		[TestMethod]
		public void ReadSwMapsV1()
		{
			var path = @"Data\30101001_con.swmaps";
			var reader = new SwMapsV1Reader(path);
			var project = reader.Read();
		}

		[TestMethod]
		public void ReadSwmz()
		{
			var path = @"Data\339865.swmz";
			var reader = new SwmzReader(path);
			var project = reader.Read();
			var pointFeatures = project.Features.Where(f => f.GeometryType == SwMapsLib.Data.SwMapsGeometryType.Point).ToList();

			var multiPointFeatures = pointFeatures.Where(p => p.Points.Count > 1);


		}


	}
}

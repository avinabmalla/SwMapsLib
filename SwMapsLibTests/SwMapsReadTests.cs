using Microsoft.VisualStudio.TestTools.UnitTesting;
using SwMapsLib.IO;
using System;


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

		
	}
}

using SwMapsLib.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.IO.Writer
{
	public class SwmzWriter
	{
		public SwMapsProject Project { get; private set; }
		public int Version { get; private set; }
		public SwmzWriter(SwMapsProject project, int version)
		{
			Project = project;
			if(version < 1 || version > 2)
			{
				throw new Exception("Version must be 1 or 2");
			}

			Version = version;
		}

		public void Write(string path)
		{

		}
	}
}

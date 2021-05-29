using SwMapsLib.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.IO
{
	interface ISwMapsDbReader
	{
		SwMapsProject Read();
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.Data
{
	public class SwMapsPhotoPoint
	{
		public string ID { get; set; }
		public string Remarks { get; set; }
		public string FileName { get; set; }
		public SwMapsPoint Location { get; set; }
	}
}

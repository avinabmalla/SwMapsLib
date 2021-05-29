using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.Data
{
	public class SwMapsAttributeField
	{
		public string UUID { get; set; }
		public string LayerID { get; set; }
		public string FieldName { get; set; }
		public SwMapsAttributeType DataType { get; set; }
		public List<string> Choices { get; set; }
	}

	
}

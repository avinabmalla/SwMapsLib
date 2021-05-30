using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.Data
{
	public class SwMapsProjectAttribute
	{
		public string Name { get; set; }
		public string Value { get; set; }
		public SwMapsProjectAttributeType DataType { get; set; }
		public List<string> Choices { get; set; } = new List<string>();
		public bool IsRequired { get; set; }
		public int FieldLength { get; set; }
	}
}

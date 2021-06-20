using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.Data
{
	public class SwMapsAttributeValue
	{
		public string FeatureID { get; set; }
		public string FieldID { get; set; }
		public string FieldName { get; set; }
		public SwMapsAttributeType DataType { get; set; }
		public string Value { get; set; }

		public override string ToString()
		{
			return Value;
		}
	}
}

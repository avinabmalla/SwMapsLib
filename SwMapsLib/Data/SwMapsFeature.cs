using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.Data
{
	public class SwMapsFeature
	{
		public int FeatureID { get; set; }
		public string UUID { get; set; } = Guid.NewGuid().ToString();
		public string Name { get; set; }
		public string Remarks { get; set; }
		public string LayerID { get; set; }
		public SwMapsGeometryType GeometryType { get; set; }

		public List<SwMapsPoint> Points { get; set; }

		public List<SwMapsAttributeValue> AttributeValues;

		public SwMapsAttributeValue GetAttributeByFieldname(string fieldName)
		{
			return AttributeValues.FirstOrDefault(iterator => iterator.FieldName == fieldName);
		}
	}
}

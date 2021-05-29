using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.Data
{
	public class SwMapsFeatureLayer
	{
		public string UUID;
		public string Name;
		public string GroupName;
		public SwMapsGeometryType GeometryType;
		public int Color;
		public int FillColor;
		public float LineWidth;
		public SwMapsPointShape PointShape;
		public bool Drawn;
		public bool Active;
		public string LabelFieldID;
		public byte[] PngSymbol;

		public List<SwMapsAttributeField> AttributeFields = new List<SwMapsAttributeField>();


		public string LabelFieldName => AttributeFields.FirstOrDefault(af => af.UUID == LabelFieldID).FieldName;
	}
}

using SwMapsLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.Data
{
	public class SwMapsFeatureLayer
	{
		public string UUID { get; set; } = Guid.NewGuid().ToString();
		public string Name { get; set; }
		public string GroupName { get; set; }
		public SwMapsGeometryType GeometryType { get; set; }
		public int Color { get; set; } = 0;
		public int FillColor { get; set; } = 0;
		public float LineWidth { get; set; } = 3;
		public SwMapsPointShape PointShape { get; set; } = SwMapsPointShape.Circle;
		public bool Drawn { get; set; } = false;
		public bool Active { get; set; } = true;
		public string LabelFieldID { get; set; }
		public byte[] PngSymbol { get; set; }

		public List<SwMapsAttributeField> AttributeFields { get; set; } = new List<SwMapsAttributeField>();
		
		public string LabelFieldName => AttributeFields.FirstOrDefault(af => af.UUID == LabelFieldID)?.FieldName ?? "";

		public SwMapsAttributeField GetAttributeField(string fieldName)
		{
			return AttributeFields.FirstOrDefault(
				a => a.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
		}

		public SwMapsFeatureLayer()
		{
			var rnd = new Random();
			var R = rnd.Next(0, 256);
			var G = rnd.Next(0, 256);
			var B = rnd.Next(0, 256);

			SetColor(System.Drawing.Color.FromArgb(R, G, B));
		}

		public void SetColor(System.Drawing.Color color)
		{
			Color = ColorUtils.GetColorInt(color);
			FillColor = ColorUtils.GetColorInt(System.Drawing.Color.FromArgb(80, color.R, color.G, color.B));
		}

		public override string ToString()
		{
			return Name;
		}

	}
}

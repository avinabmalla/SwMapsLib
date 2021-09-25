using SwMapsLib.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SwMapsLib.Conversions
{
	public  static class AttributeFieldExtensions
	{
		public static string GetExportFieldName(this SwMapsAttributeField field)
		{
			var newName = field.FieldName.Trim();
			//Replace non alphanumeric characters
			newName = Regex.Replace(newName, "[^A-Za-z0-9]", " ");
			//replace multiple spaces with single space
			newName = Regex.Replace(newName, "\\s+", " ");
			//replace spaces with underscores
			newName = Regex.Replace(newName, "\\s", "_");

			if (Char.IsDigit(newName[0]))
				newName = $"_{newName}";

			return newName;
		}
	}
}

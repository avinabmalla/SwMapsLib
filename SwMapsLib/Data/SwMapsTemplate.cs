using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwMapsLib.Data
{
	public class SwMapsTemplate
	{
		public string TemplateName { get; set; }
		public string TemplateAuthor { get; set; }
		public string DatabasePath { get; }

		public List<SwMapsFeatureLayer> Layers { get; set; } = new List<SwMapsFeatureLayer>();
		public List<SwMapsProjectAttribute> ProjectAttributes { get; set; } = new List<SwMapsProjectAttribute>();

		public SwMapsTemplate(string dbpath)
		{
			DatabasePath = dbpath;
		}


		public List<string> Groups
		{
			get
			{
				var ret = Layers.Select((l) => l.GroupName).Distinct().ToList();
				ret.RemoveAll(g => g == null || g == "");
				return ret;
			}
		}

	}
}

using SwMapsLib.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace SwMapsLib.Conversions
{
	public static class PointExtensions
	{
		public static Dictionary<string, string> AdditionalDataDictionary(this SwMapsPoint point)
		{
			var AdditionalData = point.AdditionalData;

			var ret = new Dictionary<string, string>();
			if (AdditionalData == null || AdditionalData.Trim() == "") return ret;

			JObject o1 = JObject.Parse(AdditionalData);
			List<string> keys = o1.Properties().Select(p => p.Name).ToList();

			foreach (string k in keys)
			{
				ret[k] = o1[k].ToString();
			}
			
			return ret;
		}

	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SwMapsLib.Conversions.Extensions
{
	public interface IFeatureAdditionalDataDecoder
	{
		bool CanDecode(Dictionary<string, string> additionalData);
		string GetSummaryString(Dictionary<string,string> additionalData);
	}


	public class DefaultFeatureAdditionalDataDecoder
	: IFeatureAdditionalDataDecoder
	{
		public bool CanDecode(Dictionary<string, string> additionalData)
		{
			return false;
		}

		public string GetSummaryString(Dictionary<string, string> additionalData)
		{
			return "";
		}
	}
}

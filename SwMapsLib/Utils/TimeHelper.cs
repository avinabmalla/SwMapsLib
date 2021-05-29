using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.Utils
{
	public static class TimeHelper
	{
		public static DateTime JavaTimeStampToDateTime(long javaTimeStamp)
		{
			// Java timestamp is milliseconds past epoch
			DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddMilliseconds((double)javaTimeStamp).ToLocalTime();
			return dtDateTime;
		}
	}
}

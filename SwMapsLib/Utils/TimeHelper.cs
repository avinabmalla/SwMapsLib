using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.Utils
{
	public static class TimeHelper
	{
		public static DateTime epoch { get; } = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

		public static DateTime JavaTimeStampToDateTime(long javaTimeStamp)
		{
			// Java timestamp is milliseconds past epoch
			return epoch.AddMilliseconds((double)javaTimeStamp).ToLocalTime();
		}


		public static long DateTimeToJavaTimeStamp(DateTime dateTime)
		{
			var span = dateTime - epoch;
			return (long)span.TotalMilliseconds;
		}
	}
}

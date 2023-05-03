using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace SwMapsLib.Extensions
{
	public static class SqliteExtensions
	{

		public static void CloseConnection(this SQLiteConnection conn)
		{
			try
			{
				conn.Close();
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
			catch { }
		}
	}
}

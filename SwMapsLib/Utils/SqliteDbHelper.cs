using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.Utils
{
	public static class SqliteDbHelper
	{
		public static string ReadString(this SQLiteDataReader reader, string colName)
		{
			int colIndex = reader.GetOrdinal(colName);

			if (!reader.IsDBNull(colIndex))
			{
				return reader[colName].ToString();
			}
			else
			{
				return null;
			}
		}

		public static int ReadInt32(this SQLiteDataReader reader, string colName)
		{
			int colIndex = reader.GetOrdinal(colName);

			if (!reader.IsDBNull(colIndex))
			{
				return Convert.ToInt32(reader[colName]);
			}
			else
			{
				return default(int);
			}
		}

		public static long ReadInt64(this SQLiteDataReader reader, string colName)
		{
			int colIndex = reader.GetOrdinal(colName);

			if (!reader.IsDBNull(colIndex))
			{
				return Convert.ToInt64(reader[colName]);
			}
			else
			{
				return default(long);
			}
		}

		public static float ReadSingle(this SQLiteDataReader reader, string colName)
		{
			int colIndex = reader.GetOrdinal(colName);

			if (!reader.IsDBNull(colIndex))
				return Convert.ToSingle(reader[colName]);
			else
				return default(float);
		}


		public static double ReadDouble(this SQLiteDataReader reader, string colName)
		{
			int colIndex = reader.GetOrdinal(colName);

			if (!reader.IsDBNull(colIndex))
				return Convert.ToDouble(reader[colName]);
			else
				return default(double);
		}

		public static byte[] ReadBlob(this SQLiteDataReader reader, string colName)
		{
			int colIndex = reader.GetOrdinal(colName);

			if (!reader.IsDBNull(colIndex))
				return (byte[])reader[colName];
			else
				return default(byte[]);
		}


		public static bool ExecuteSQL(this SQLiteConnection conn, string sql)
		{
			var cmd = new SQLiteCommand(sql, conn);
			try
			{
				cmd.ExecuteNonQuery();
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static long GetLastInsertedRowID(this SQLiteConnection conn)
		{
			var sql = "select last_insert_rowid()";
			using (var cmd = new SQLiteCommand(sql, conn))
			{
				return (long)cmd.ExecuteScalar();
			}
		}

		public static long Insert(this SQLiteConnection conn, string tableName, Dictionary<string, object> contentValues, SQLiteTransaction sqlTrans = null)
		{
			var fields = new List<string>();
			var values = new List<string>();
			foreach (var f in contentValues.Keys)
			{
				fields.Add(f);
				values.Add("?");
			}
			var sql = $"INSERT INTO {tableName}({String.Join(",", fields)}) VALUES({String.Join(",", values)})";
			
			SQLiteCommand cmd;
			if (sqlTrans != null)
				cmd = new SQLiteCommand(sql, conn, sqlTrans);
			else
				cmd = new SQLiteCommand(sql, conn);
			
			foreach (var f in contentValues.Keys)
			{
				cmd.Parameters.AddWithValue(f, contentValues[f]);
			}

			cmd.ExecuteNonQuery();
			return conn.GetLastInsertedRowID();

		}

	}
}

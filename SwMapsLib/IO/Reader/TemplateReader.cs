using SwMapsLib.Data;
using SwMapsLib.Extensions;
using SwMapsLib.Utils;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace SwMapsLib.IO
{
	public class TemplateReader
	{
		public readonly string TemplatePath;
		public TemplateReader(string templatePath)
		{
			TemplatePath = templatePath;
		}

		public SwMapsTemplate Read()
		{
			using (var conn = new SQLiteConnection($"Data Source={TemplatePath};Version=3;"))
			{
				try
				{

					conn.Open();
					var version = (long)conn.ExecuteScalar("pragma user_version;");
					SwMapsTemplate template = null;
					if (version < 100)
					{
						template = new TemplateV1Reader().Read(TemplatePath, conn);
					}
					else
					{
						template = new TemplateV2Reader().Read(TemplatePath, conn);
					}

					return template;
				}
				finally
				{
					conn.CloseConnection();
				}
			}

		}

	}
}

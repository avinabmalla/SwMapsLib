using SwMapsLib.Data;
using System;
using System.IO;
using System.IO.Compression;

namespace SwMapsLib.IO
{
	//Reads SW Maps V1 and V2 Project Files
	public class SwmzReader
	{
		static string SystemTempFolder = Path.Combine(Path.GetTempPath(), "SW_Maps");

		string TempFolder;

		public readonly string SwmzPath;
		public readonly string DbPath; //Extracted path
		public readonly string ProjectTempDir;
		public bool IsV1Project;

		public bool ShortenZipNames = false;
		public SwMapsProject Read(bool readMediaFiles = false)
		{
			ISwMapsDbReader reader;

			if (IsV1Project)
				reader = new SwMapsV1Reader(DbPath);
			else
				reader = new SwMapsV2Reader(DbPath);

			SwMapsProject project = reader.Read();
			if (readMediaFiles)
			{
				var dir = Path.Combine(ProjectTempDir, "Photos");

				if (Directory.Exists(dir))
				{
					var files = Directory.EnumerateFiles(dir);
					foreach (var file in files)
					{
						var name = Path.GetFileName(file);
						var data = File.ReadAllBytes(file);
						project.MediaFiles[name] = data;
					}
				}
			}

			//GNSS Raw Data
			var gnssRawDir = Path.Combine(ProjectTempDir, "RawFiles");
			if (Directory.Exists(gnssRawDir))
			{
				var files = Directory.EnumerateFiles(gnssRawDir);
				foreach (var file in files)
				{
					project.GnssRawDataFiles.Add(file);
				}
			}
			return project;
		}

		public SwmzReader(string swmzPath, bool shortenZipNames, string tempFolderPath)
		{
			SwmzPath = swmzPath;
			ShortenZipNames = shortenZipNames;

			TempFolder = tempFolderPath ?? SystemTempFolder;
			Directory.CreateDirectory(TempFolder);

			if (ShortenZipNames)
			{
				ProjectTempDir = Path.Combine(TempFolder, Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
			}
			else
			{
				ProjectTempDir = Path.Combine(TempFolder, Path.GetFileNameWithoutExtension(swmzPath).Trim());
			}

			if (Directory.Exists(ProjectTempDir))
				Directory.Delete(ProjectTempDir, true);

			Directory.CreateDirectory(ProjectTempDir);

			DbPath = "";

			using (var zip = ZipFile.Open(swmzPath, ZipArchiveMode.Read))
			{
				zip.ExtractToDirectory(ProjectTempDir);
			}
			var dirs = Directory.EnumerateDirectories(ProjectTempDir);

			foreach (var f in dirs)
			{
				if (Path.GetFileName(f) == "Projects")
				{
					var files = Directory.EnumerateFiles(f);
					foreach (var file in files)
						if (file.EndsWith(".swm2", StringComparison.OrdinalIgnoreCase))
						{
							DbPath = file;
							IsV1Project = false;
						}
				}
				else if (Path.GetFileName(f) == "MapProjects")
				{
					var files = Directory.EnumerateFiles(f);
					foreach (var file in files)
						if (file.EndsWith(".swmaps", StringComparison.OrdinalIgnoreCase))
						{
							DbPath = file;
							IsV1Project = true;
						}
				}
			}
		}

		public SwmzReader(string swmzPath, bool shortenZipNames = false) : this(swmzPath, shortenZipNames, null) { }
		public SwmzReader(string swmzPath, string tempFolderPath) : this(swmzPath, false, tempFolderPath) { }
		public SwmzReader(string swmzPath) : this(swmzPath, false, null) { }
	}
}

using SwMapsLib.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace SwMapsLib.IO
{
	public class SwmzWriter
	{
		public SwMapsProject Project { get; private set; }
		public int Version { get; private set; }
		public SwmzWriter(SwMapsProject project, int version)
		{
			Project = project;
			if (version < 1 || version > 2)
			{
				throw new Exception("Version must be 1 or 2");
			}

			Version = version;
		}

		public void Write(string path, bool includeMediaFiles = true)
		{
			if (Version == 1)
				WriteV1(path, includeMediaFiles);
			else if (Version == 2)
				WriteV2(path, includeMediaFiles);
		}


		private void WriteV1(string path, bool includeMediaFiles)
		{
			var ProjectName = Path.GetFileNameWithoutExtension(path);

			var dbPath = Path.GetTempFileName();
			new SwMapsV1Writer(Project).WriteSwmapsDb(dbPath);

			using (var archive = ZipFile.Open(path, ZipArchiveMode.Create))
			{
				ZipArchiveEntry dbEntry = archive.CreateEntry($"MapProjects/{ProjectName}.swmaps");
				using (BinaryWriter writer = new BinaryWriter(dbEntry.Open()))
				{
					writer.Write(File.ReadAllBytes(dbPath));
				}

				if (includeMediaFiles)
				{
					foreach (var ph in Project.GetAllMediaFiles())
					{
						var fileName = Path.GetFileName(ph);
						ZipArchiveEntry phEntry = archive.CreateEntry($"Photos/{fileName}");
						using (BinaryWriter writer = new BinaryWriter(phEntry.Open()))
						{
							writer.Write(File.ReadAllBytes(fileName));
						}
					}
				}
			}

		}

		private void WriteV2(string path, bool includeMediaFiles)
		{
			var ProjectName = Path.GetFileNameWithoutExtension(path);

			var dbPath = Path.GetTempFileName();
			new SwMapsV2Writer(Project).WriteSwmapsDb(dbPath);
			if (File.Exists(path)) File.Delete(path);

			using (var archive = ZipFile.Open(path, ZipArchiveMode.Create))
			{
				ZipArchiveEntry dbEntry = archive.CreateEntry($"Projects/{ProjectName}.swm2");
				using (BinaryWriter writer = new BinaryWriter(dbEntry.Open()))
				{
					writer.Write(File.ReadAllBytes(dbPath));
				}

				if (includeMediaFiles)
				{
					foreach (var ph in Project.GetAllMediaFiles())
					{
						var fileName = Path.GetFileName(ph);
						ZipArchiveEntry phEntry = archive.CreateEntry($"Photos/{fileName}");
						using (BinaryWriter writer = new BinaryWriter(phEntry.Open()))
						{
							if (File.Exists(ph))
							{
								writer.Write(File.ReadAllBytes(ph));
							}
							else if (File.Exists(Path.Combine(Project.MediaFolderPath, ph)))
							{
								writer.Write(File.ReadAllBytes(Path.Combine(Project.MediaFolderPath, ph)));
							}
						}
					}
				}
			}

		}
	}
}

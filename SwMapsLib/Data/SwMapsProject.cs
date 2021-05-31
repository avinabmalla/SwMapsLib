using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwMapsLib.Data
{
	public class SwMapsProject
	{
		public List<SwMapsFeatureLayer> FeatureLayers = new List<SwMapsFeatureLayer>();
		public List<SwMapsFeature> Features = new List<SwMapsFeature>();
		public List<SwMapsTrack> Tracks = new List<SwMapsTrack>();
		public List<SwMapsPhotoPoint> PhotoPoints = new List<SwMapsPhotoPoint>();
		public List<SwMapsProjectAttribute> ProjectAttributes = new List<SwMapsProjectAttribute>();

		public Dictionary<string, byte[]> MediaFiles = new Dictionary<string, byte[]>();

		public string DatabasePath { get; }
		public string MediaFolderPath { get; }

		public SwMapsFeature GetFeature(string id)
		{
			return Features.FirstOrDefault(iterator => iterator.UUID == id);
		}

		public SwMapsFeatureLayer GetLayer(string id)
		{
			return FeatureLayers.FirstOrDefault(iterator => iterator.UUID == id);
		}

		public SwMapsProject(string dbpath, string mediaPath)
		{
			DatabasePath = dbpath;
			MediaFolderPath = mediaPath;
		}

		/// <summary>
		/// Reassigns the sequence numbers for all the points in this project
		/// </summary>
		internal void ResequenceAll()
		{
			foreach (var f in Features)
			{
				for (int i = 0; i < f.Points.Count; i++) f.Points[i].Seq = i;
			}

			foreach (var t in Tracks)
			{
				for (int i = 0; i < t.Vertices.Count; i++) t.Vertices[i].Seq = i;
			}

			foreach (var t in PhotoPoints)
			{
				t.Location.Seq = 0;
			}
		}


		public List<string> GetAllMediaFiles()
		{
			var ret = new List<string>();

			foreach (var f in Features)
			{
				foreach (var attr in f.AttributeValues)
				{
					if (attr.DataType == SwMapsAttributeType.Audio
						|| attr.DataType == SwMapsAttributeType.Photo
						|| attr.DataType == SwMapsAttributeType.Video)
					{
						var path = GetMediaFilePath(attr.Value);
						if (path == null) continue;
						ret.Add(path);
					}
				}
			}

			foreach (var ph in PhotoPoints)
			{
				var path = ph.FileName;
				if (path == null) continue;
				ret.Add(path);
			}

			return ret;
		}



		public string GetMediaFilePath(string mediaFileName)
		{
			if (mediaFileName == null || mediaFileName == "") return null;

			try
			{
				if (File.Exists(mediaFileName))
				{
					return mediaFileName;
				}
				else if (File.Exists(Path.Combine(MediaFolderPath, mediaFileName)))
				{
					return Path.Combine(MediaFolderPath, mediaFileName);
				}
			}
			catch { return null; }

			return null;
		}
	}
}

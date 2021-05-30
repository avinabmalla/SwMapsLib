using System;
using System.Collections.Generic;
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

		/// <summary>
		/// True if the values of the media attributes contains the absolute path
		/// </summary>
		public bool IsMediaPathAbsolute { get; internal set; } = false;

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
			foreach(var f in Features)
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
	}
}

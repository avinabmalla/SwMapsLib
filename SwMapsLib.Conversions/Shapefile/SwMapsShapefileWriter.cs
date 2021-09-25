using SwMapsLib.Data;
using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.IO.ShapeFile;
using System.IO;
using NetTopologySuite.Features;
using System.Linq;
using SwMapsLib.Conversions.Extensions;
using SwMapsLib.Utils;
using System.IO.Compression;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;

namespace SwMapsLib.Conversions.Shapefile
{
	public class SwMapsShapefileWriter
	{
		string Projection = "GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137,298.257223563]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.017453292519943295]]";


		SwMapsProject Project;
		string tempDir;

		public bool ExportPhotos { get; set; } = true;
		public bool ExportTracks { get; set; } = true;
		public bool ExportAllLayers { get; set; } = true;
		public List<string> LayersToExport { get; set; } = new List<string>();
		public bool Export2D { get; set; } = false;
		public bool Export3D { get; set; } = true;

		public SwMapsShapefileWriter(SwMapsProject project)
		{
			Project = project;
			tempDir = Path.GetTempFileName() + "shp";
			Directory.CreateDirectory(tempDir);
		}

		public void Export(string path)
		{
			foreach (var l in Project.FeatureLayers)
			{
				if (LayersToExport.Contains(l.Name) || ExportAllLayers)
				{
					if (Export2D) WriteLayerShapefile(l, true);
					if (Export3D) WriteLayerShapefile(l, false);
				}
			}

			if (ExportPhotos) WritePhotosShapefile();

			if (ExportTracks)
			{
				if (Export2D) WriteTrackShapefile(true);
				if (Export3D) WriteTrackShapefile(false);
			}

			var dirFiles = Directory.GetFiles(tempDir);
			if (File.Exists(path)) File.Delete(path);

			using (var zip = ZipFile.Open(path, ZipArchiveMode.Create))
			{
				foreach (var f in dirFiles)
				{
					var entry = zip.CreateEntry(Path.GetFileName(f));
					var stream = entry.Open();
					var bytes = File.ReadAllBytes(f);
					stream.Write(bytes, 0, bytes.Length);
					stream.Close();
				}

				var photoFiles = GetPhotoFileList();
				foreach (var ph in photoFiles)
				{
					var filePath = ph;
					if (File.Exists(ph) == false)
					{
						filePath = Path.Combine(Project.MediaFolderPath, ph);
						if (File.Exists(ph) == false) continue;
					}
					var fileEntry = zip.CreateEntry("Photos/" + Path.GetFileName(ph));
					var stream = fileEntry.Open();
					var bytes = File.ReadAllBytes(filePath);
					stream.Write(bytes, 0, bytes.Length);
					stream.Close();
				}
			}
		}

		public void WritePrjFile(string name)
		{
			File.WriteAllText(name, Projection);
		}
		void WritePhotosShapefile()
		{
			if (Project.PhotoPoints.Count <= 0) return;

			var header = new NetTopologySuite.IO.DbaseFileHeader();
			header.AddColumn("PATH", 'C', 254, 0);
			header.AddColumn("REMARKS", 'C', 254, 0);
			header.AddColumn("REC_TIME", 'C', 254, 0);
			header.AddColumn("LATITUDE", 'N', 20, 10);
			header.AddColumn("LONGITUDE", 'N', 20, 10);
			header.AddColumn("ELEVATION", 'N', 20, 4);

			var features = new FeatureCollection();

			foreach (var ph in Project.PhotoPoints)
			{
				var geom = ph.Location.ToNtsGeometry3D();
				var attrTable = new AttributesTable();
				attrTable.Add("PATH", "Photos\\" + Path.GetFileName(ph.FileName));
				attrTable.Add("REMARKS", ph.Remarks);
				attrTable.Add("REC_TIME", TimeHelper.JavaTimeStampToDateTime(ph.Location.Time).ToString("R"));
				attrTable.Add("LATITUDE", ph.Location.Latitude);
				attrTable.Add("LONGITUDE", ph.Location.Longitude);
				attrTable.Add("ELEVATION", ph.Location.Elevation);

				var nf = new Feature(geom, attrTable);
				features.Add(nf);
			}

			var shpName = "photos";
			var shpPath = Path.Combine(tempDir, shpName);
			var writer = new NetTopologySuite.IO.ShapefileDataWriter(shpPath);
			writer.Header = header;
			writer.Write(features);

			WritePrjFile(shpPath + ".prj");
		}


		void WriteTrackShapefile(bool exportAs2D)
		{
			if (Project.Tracks.Count <= 0) return;

			var header = new NetTopologySuite.IO.DbaseFileHeader();
			header.AddColumn("NAME", 'C', 254, 0);
			header.AddColumn("LENGTH", 'N', 20, 6);
			header.AddColumn("REMARKS", 'C', 254, 0);
			header.AddColumn("REC_TIME", 'C', 254, 0);

			var features = new FeatureCollection();
			foreach (var track in Project.Tracks)
			{
				var geom = exportAs2D ? track.ToNtsGeometry2D() : track.ToNtsGeometry3D();
				var attrTable = new AttributesTable();
				attrTable.Add("NAME", track.Name);
				attrTable.Add("LENGTH", track.Length);
				attrTable.Add("REMARKS", track.Remarks);
				attrTable.Add("REC_TIME", TimeHelper.JavaTimeStampToDateTime(track.GetLastModifiedTime()).ToString("R"));
				var nf = new Feature(geom, attrTable);
				features.Add(nf);
			}

			var shpName = "tracks" + (exportAs2D ? "_2d" : "");
			var shpPath = Path.Combine(tempDir, shpName);
			var writer = new NetTopologySuite.IO.ShapefileDataWriter(shpPath);
			writer.Header = header;
			writer.Write(features);
			WritePrjFile(shpPath + ".prj");
		}
		void WriteLayerShapefile(SwMapsFeatureLayer layer, bool exportAs2d)
		{
			var swFeatures = Project.GetAllFeatures(layer);
			if (swFeatures.Count <= 0) return;

			var features = new FeatureCollection();
			var fieldNames = GetDbfFieldNames(layer.AttributeFields);

			var header = new NetTopologySuite.IO.DbaseFileHeader();
			header.AddColumn("_ID", 'N', 10, 0);
			header.AddColumn("_NAME", 'C', 254, 0);
			header.AddColumn("_REMARKS", 'C', 254, 0);
			header.AddColumn("_REC_TIME", 'C', 254, 0);

			if (layer.GeometryType == SwMapsGeometryType.Point)
			{
				header.AddColumn("_LATITUDE", 'N', 20, 10);
				header.AddColumn("_LONGITUDE", 'N', 20, 10);
				header.AddColumn("_ELEVATION", 'N', 20, 4);
				header.AddColumn("_ORTHO_HT", 'N', 20, 4);
			}
			else if (layer.GeometryType == SwMapsGeometryType.Line)
			{
				header.AddColumn("_LENGTH", 'N', 20, 4);
			}
			else if (layer.GeometryType == SwMapsGeometryType.Polygon)
			{
				header.AddColumn("_LENGTH", 'N', 20, 4);
				header.AddColumn("_AREA", 'N', 20, 4);
			}

			foreach (var f in layer.AttributeFields)
			{
				if (f.DataType == SwMapsAttributeType.Numeric)
					header.AddColumn(fieldNames[f.FieldName], 'N', 20, 6);
				else
					header.AddColumn(fieldNames[f.FieldName], 'C', 254, 0);

			}
			int id = 0;
			foreach (var f in swFeatures)
			{
				var geom = exportAs2d ? f.ToNtsGeometry2D() : f.ToNtsGeometry3D();
				var attrTable = new AttributesTable();
				attrTable.Add("_ID", id);
				attrTable.Add("_NAME", f.Name ?? "");
				attrTable.Add("_REMARKS", f.Remarks ?? "");
				attrTable.Add("_REC_TIME", TimeHelper.JavaTimeStampToDateTime(f.GetLastModifiedTime()).ToString("R"));
				id += 1;

				if (f.GeometryType == SwMapsGeometryType.Point)
				{
					attrTable.Add("_LATITUDE", f.Points.First().Latitude);
					attrTable.Add("_LONGITUDE", f.Points.First().Longitude);
					attrTable.Add("_ELEVATION", f.Points.First().Elevation);
					attrTable.Add("_ORTHO_HT", f.Points.First().OrthoHeight);
				}
				else if (f.GeometryType == SwMapsGeometryType.Line)
				{
					attrTable.Add("_LENGTH", f.Length);
				}
				else if (f.GeometryType == SwMapsGeometryType.Polygon)
				{
					attrTable.Add("_LENGTH", f.Length);
					attrTable.Add("_AREA", f.Area);
				}

				foreach (var field in layer.AttributeFields)
				{
					var attr = f.AttributeValues.FirstOrDefault(a => a.FieldID == field.UUID);
					if (attr == null)
					{
						if (field.DataType == SwMapsAttributeType.Numeric)
							attrTable.Add(fieldNames[field.FieldName], 0);
						else
							attrTable.Add(fieldNames[field.FieldName], "");
						continue;
					}


					if (field.DataType == SwMapsAttributeType.Numeric)
					{
						double value = 0;
						Double.TryParse(attr.Value, out value);
						attrTable.Add(fieldNames[field.FieldName], value);
					}
					else if (field.DataType == SwMapsAttributeType.Text || attr.DataType == SwMapsAttributeType.Options)
					{
						attrTable.Add(fieldNames[field.FieldName], attr.Value.Trim());
					}
					else
					{
						if (String.IsNullOrWhiteSpace(attr.Value) || File.Exists(attr.Value.Trim()) == false)
						{
							attrTable.Add(fieldNames[field.FieldName], "");
						}
						else
						{
							attrTable.Add(fieldNames[field.FieldName], "\\Photos\\" + Path.GetFileName(attr.Value.Trim()));
						}
					}
				}

				var nf = new Feature(geom, attrTable);
				features.Add(nf);
			}

			var outGeomFactory = GeometryFactory.Default;


			var shpName = layer.Name + (exportAs2d ? "_2d" : "");
			var shpPath = Path.Combine(tempDir, shpName);
			var writer = new ShapefileDataWriter(shpPath, outGeomFactory);
			var outDbaseHeader = ShapefileDataWriter.GetHeader(features[0], features.Count);
			writer.Header = outDbaseHeader;
			writer.Write(features);
			WritePrjFile(shpPath + ".prj");
		}

		private Dictionary<string, string> GetDbfFieldNames(List<SwMapsAttributeField> fields)
		{
			var ret = new Dictionary<string, string>();
			foreach (var field in fields)
			{
				var NewName = field.GetExportFieldName();
				if (NewName.Length > 10)
				{
					NewName = NewName.Substring(0, 8);
					var count = 1;
					foreach (var key in ret.Keys)
					{
						var name = ret[key];
						if (name.Length > 8 && name.Substring(0, 8) == NewName) count++;
					}
					NewName += $"_{count}";
					ret[field.FieldName] = NewName;
				}
				else
				{
					ret[field.FieldName] = NewName;
				}
			}

			return ret;
		}



		private List<string> GetPhotoFileList()
		{
			var ret = new List<string>();
			if (ExportPhotos)
			{
				foreach (var ph in Project.PhotoPoints)
				{
					ret.Add(ph.FileName);
				}
			}
			var selectedLayers = Project.FeatureLayers.Where(fl => ExportAllLayers || LayersToExport.Contains(fl.Name)).ToList();
			foreach (var lyr in selectedLayers)
			{
				var photoAttributes = lyr.AttributeFields.Where(at => at.DataType == SwMapsAttributeType.Photo).ToList();
				if (photoAttributes.Count == 0) continue;

				var features = Project.GetAllFeatures(lyr);
				foreach (var f in features)
				{
					foreach (var attr in f.AttributeValues)
					{
						if (attr.DataType != SwMapsAttributeType.Photo) continue;
						if (attr.Value != null && attr.Value != "") ret.Add(attr.Value);
					}
				}
			}

			return ret;
		}

	}
}

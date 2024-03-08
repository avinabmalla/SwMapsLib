using SwMapsLib.Conversions.Extensions;
using SwMapsLib.Data;
using SwMapsLib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SwMapsLib.Conversions.KMZ
{
	public class SwMapsKmzWriter
	{
		SwMapsProject Project;
		string ExportPath;

		public bool KmlOnly { get; set; } = false;
		public bool ExportPhotos { get; set; } = true;
		public bool ExportTracks { get; set; } = true;
		public bool ExportAllLayers { get; set; } = true;
		public bool ExportLineVertices { get; set; } = false;

		public List<string> LayersToExport { get; set; } = new List<string>();

		public IFeatureAdditionalDataDecoder FeatureAdditionalDataDecoder { get; set; } = new DefaultFeatureAdditionalDataDecoder();

		public DisplaySettings DisplaySettings { get; set; } = new DisplaySettings();

		public SwMapsKmzWriter(SwMapsProject project)
		{
			Project = project;
		}

		Dictionary<string, List<string>> folders;
		Dictionary<string, string> AdditionalAttributes;

		int ent_count = 0;

		Formatter Formatter;

		public void WriteKml(string path)
		{
			LayersToExport = new List<string>();
			folders = new Dictionary<string, List<string>>();
			AdditionalAttributes = new Dictionary<string, string>();
			Formatter = new Formatter(DisplaySettings);


			ExportPath = path;
			KmlOnly = true;
			var kml = GetKmlString();
			if (File.Exists(path)) File.Delete(path);
			File.WriteAllText(path, kml);
		}

		public void WriteKmz(string path)
		{
			LayersToExport = new List<string>();
			folders = new Dictionary<string, List<string>>();
			AdditionalAttributes = new Dictionary<string, string>();
			Formatter = new Formatter(DisplaySettings);

			ExportPath = path;
			KmlOnly = false;
			var kml = GetKmlString();
			if (File.Exists(path)) File.Delete(path);

			using (var zip = ZipFile.Open(path, ZipArchiveMode.Create))
			{
				var docEntry = zip.CreateEntry("doc.kml");
				var docStream = docEntry.Open();
				var docBytes = Encoding.UTF8.GetBytes(kml);
				docStream.Write(docBytes, 0, docBytes.Length);
				docStream.Close();

				foreach (var l in Project.FeatureLayers)
				{
					if (l.PngSymbol == null) continue;
					var fileEntry = zip.CreateEntry("files/icons/" + l.Name + ".png");
					var stream = fileEntry.Open();
					stream.Write(l.PngSymbol, 0, l.PngSymbol.Length);
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
					var fileEntry = zip.CreateEntry("files/" + Path.GetFileName(ph));
					var stream = fileEntry.Open();
					var bytes = File.ReadAllBytes(filePath);
					stream.Write(bytes, 0, bytes.Length);
					stream.Close();
				}

				var assembly = typeof(SwMapsKmzWriter).GetTypeInfo().Assembly;
				using (Stream resFilestream = assembly.GetManifestResourceStream("SwMapsLib.Conversions.Resources.map_photo.png"))
				{
					if (resFilestream != null)
					{
						byte[] ba = new byte[resFilestream.Length];
						resFilestream.Read(ba, 0, ba.Length);
						var iconEntry = zip.CreateEntry("files/image.png");
						var iconstream = iconEntry.Open();
						iconstream.Write(ba, 0, ba.Length);
						iconstream.Close();
					}
				}

			}
		}

		private string GetKmlString()
		{
			var kmlFile = new List<string>();

			if (ExportPhotos)
			{
				var photos = Project.PhotoPoints;
				if (photos.Count > 0)
				{
					folders["_PHOTOS"] = new List<string>();
					folders["_PHOTOS"].Add("<Style id=\"PhotoIcon\">");
					if (!KmlOnly)
					{
						folders["_PHOTOS"].Add("<IconStyle>");
						folders["_PHOTOS"].Add("<scale>0.5</scale>");
						folders["_PHOTOS"].Add("<Icon>");
						folders["_PHOTOS"].Add("<href>files/image.png</href>");
						folders["_PHOTOS"].Add("</Icon>");
						folders["_PHOTOS"].Add("<hotSpot x=\"0.5\" y=\"0.5\" xunits=\"fraction\" yunits=\"fraction\"/>");
						folders["_PHOTOS"].Add("</IconStyle>");
					}
					folders["_PHOTOS"].Add("<LabelStyle>");
					folders["_PHOTOS"].Add("<scale>0.6</scale>");
					folders["_PHOTOS"].Add("</LabelStyle>");
					folders["_PHOTOS"].Add("</Style>");

					photos.ForEach(ph => AddPhoto(ph));
				}
			}


			if (ExportTracks)
			{
				var tracks = Project.Tracks;
				if (tracks.Count > 0)
				{
					folders["_TRACKS"] = new List<string>();
					tracks.ForEach(tr => AddTrack(tr));
				}
			}

			var selectedLayers = Project.FeatureLayers.Where(fl => ExportAllLayers || LayersToExport.Contains(fl.Name)).ToList();

			foreach (var lyr in selectedLayers)
			{
				var features = Project.GetAllFeatures(lyr);
				if (features.Count() == 0) continue;
				folders[lyr.UUID] = new List<string>();
				AddLayerStyle(lyr);

				if (lyr.GeometryType == SwMapsGeometryType.Point)
					features.ForEach(f => AddMarker(lyr, f));
				else if (lyr.GeometryType == SwMapsGeometryType.Line)
					features.ForEach(f => AddPolyline(lyr, f));
				else if (lyr.GeometryType == SwMapsGeometryType.Polygon)
					features.ForEach(f => AddPolygon(lyr, f));
			}

			kmlFile.Add("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
			kmlFile.Add("<kml xmlns=\"http://www.opengis.net/kml/2.2\">");

			kmlFile.Add("<Document>");
			kmlFile.Add($"<name>{Path.GetFileNameWithoutExtension(ExportPath)}</name>");

			var projectAttributes = Project.ProjectAttributes;
			if (projectAttributes.Count + AdditionalAttributes.Count > 0)
			{
				kmlFile.Add("<ExtendedData>");
				foreach (var attr in projectAttributes)
				{
					kmlFile.Add($"<Data name=\"{attr.Name}\">");
					kmlFile.Add($"    <value>{attr.Value}</value>");
					kmlFile.Add("</Data>");
				}
				foreach (var attr in AdditionalAttributes.Keys)
				{
					kmlFile.Add($"<Data name=\"{attr}\">");
					kmlFile.Add($"    <value>{AdditionalAttributes[attr]}</value>");
					kmlFile.Add("</Data>");
				}
				kmlFile.Add("</ExtendedData>");
			}

			kmlFile.Add("<open>0</open>");
			kmlFile.Add("<description>Exported using SW Maps</description>");

			foreach (var folder in folders.Keys)
			{
				var layer = Project.GetLayer(folder);
				string folderName = folder;
				if (layer != null) folderName = layer.Name;

				var placemarks = folders[folder];
				kmlFile.Add("<Folder>");
				kmlFile.Add($"<name>{folderName}</name>");
				kmlFile.Add("<visibility>1</visibility>");
				kmlFile.Add("<open>0</open>");
				foreach (var line in placemarks)
				{
					kmlFile.Add(line);
				}
				kmlFile.Add("</Folder>");
			}

			kmlFile.Add("</Document>");
			kmlFile.Add("</kml>");

			return string.Join("\r\n", kmlFile);
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


		private void AddPolygon(SwMapsFeatureLayer layer, SwMapsFeature pl)
		{
			if (pl.Points.Count == 0) return;

			var styleName = $"{layer.Name}-Style";

			folders[layer.UUID].Add("<Placemark>");
			folders[layer.UUID].Add("<name>" + pl.Name + "</name>");
			folders[layer.UUID].Add("<description>" + GetAttributeTable(layer, pl) + "</description>");
			folders[layer.UUID].Add($"<styleUrl>#{styleName}</styleUrl>");

			folders[layer.UUID].Add("<Polygon>");
			folders[layer.UUID].Add("<tessellate>1</tessellate>");
			folders[layer.UUID].Add("<outerBoundaryIs>");
			folders[layer.UUID].Add("<LinearRing>");
			folders[layer.UUID].Add("<coordinates>");
			var Points = "";
			var pointArray = pl.Points;
			foreach (var v in pointArray)
			{
				Points += $"{v.Longitude},{v.Latitude},{v.Elevation} ";
			}

			if (Points.Length > 0)
			{
				Points += $"{pointArray[0].Longitude},{pointArray[0].Latitude},{pointArray[0].Elevation} ";
			}
			folders[layer.UUID].Add(Points.Trim());

			folders[layer.UUID].Add("</coordinates>");
			folders[layer.UUID].Add("</LinearRing>");
			folders[layer.UUID].Add("</outerBoundaryIs>");
			folders[layer.UUID].Add("</Polygon>");
			folders[layer.UUID].Add("</Placemark>");

			ent_count++;

			if (ExportLineVertices)
			{
				AddVertices(layer, pl);
			}
		}

		private void AddVertices(SwMapsFeatureLayer layer, SwMapsFeature f)
		{
			var points = f.Points;
			var styleName = $"{layer.Name}-PointStyle";

			foreach (var v in points)
			{
				folders[layer.UUID].Add("<Placemark>");

				var label = f.GetLabel(layer);
				if (label == "") label = f.FeatureID.ToString();

				folders[layer.UUID].Add("<name>" + layer.Name + " " + label + " vertex</name>");
				folders[layer.UUID].Add("<description>" + GetVertexAttributeTable(layer, f, v) + "</description>");
				folders[layer.UUID].Add($"<styleUrl>#{styleName}</styleUrl>");

				folders[layer.UUID].Add("<Point>");
				folders[layer.UUID].Add("<coordinates>" + v.Longitude + "," + v.Latitude + "," + v.Elevation + "</coordinates>");
				folders[layer.UUID].Add("</Point>");
				folders[layer.UUID].Add("</Placemark>");
				ent_count++;
			}
		}

		private string GetVertexAttributeTable(SwMapsFeatureLayer layer, SwMapsFeature item, SwMapsPoint pt)
		{
			var Description = " <![CDATA[";

			Description += $"<h4>Layer: {layer.Name}<br/>";
			Description += $"Feature ID: {item.FeatureID}<br/>";
			Description += $"Sequence: {pt.Seq}</h4>";

			Description += $"<h4>Time: {Formatter.GetTimeLabel(pt.Time)}<br/>";
			Description += $"Latitude: {Formatter.FormatLatLng(pt.Latitude)}<br/>";
			Description += $"Longitude: {Formatter.FormatLatLng(pt.Longitude)}<br/>";
			Description += $"Elevation: {Formatter.GetElevationLabel(pt.Elevation)}<br/>";

			if (pt.InstrumentHeight != 0.0)
			{
				Description += $"Instrument Ht: {Formatter.GetElevationLabel(pt.InstrumentHeight)}<br/>";
			}

			var fix = (NmeaFixQuality)(item.Points[0].FixID);

			if (fix != NmeaFixQuality.Invalid)
			{
				Description += $"Fix Quality: {fix}<br/>";
			}
			else
			{
				Description += "Drawn Point<br/>";
			}

			var additionalData = pt.AdditionalDataDictionary();
			if (additionalData.Count() > 0)
			{
				Description += "<br/>";
				if (FeatureAdditionalDataDecoder.CanDecode(additionalData))
				{
					Description += FeatureAdditionalDataDecoder.GetSummaryString(additionalData).Replace("\r", "").Replace("\n", "<br/>") + "<br/>";
				}
				else
				{
					foreach (var key in additionalData.Keys)
					{
						Description += key + ": " + additionalData[key] + "<br/>";
					}
				}
			}

			Description += "</h4>]]>";
			return Description;
		}

		private void AddPolyline(SwMapsFeatureLayer layer, SwMapsFeature pl)
		{
			if (pl.Points.Count == 0) return;

			var styleName = $"{layer.Name}-Style";

			folders[layer.UUID].Add("<Placemark>");
			folders[layer.UUID].Add("<name>" + pl.Name + "</name>");
			folders[layer.UUID].Add("<description>" + GetAttributeTable(layer, pl) + "</description>");
			folders[layer.UUID].Add($"<styleUrl>#{styleName}</styleUrl>");

			folders[layer.UUID].Add("<LineString>");
			folders[layer.UUID].Add("<tessellate>1</tessellate>");
			folders[layer.UUID].Add("<coordinates>");

			var Points = "";
			var pointArray = pl.Points;
			foreach (var v in pointArray)
			{
				Points += $"{v.Longitude},{v.Latitude},{v.Elevation} ";
			}
			folders[layer.UUID].Add(Points.Trim());

			folders[layer.UUID].Add("</coordinates>");
			folders[layer.UUID].Add("</LineString>");
			folders[layer.UUID].Add("</Placemark>");

			ent_count++;

			if (ExportLineVertices)
			{
				AddVertices(layer, pl);
			}
		}

		private void AddMarker(SwMapsFeatureLayer layer, SwMapsFeature mpt)
		{
			var points = mpt.Points;
			if (points.Count == 0) return;
			var styleName = $"{layer.Name}-PointStyle";

			folders[layer.UUID].Add("<Placemark>");
			var label = mpt.GetLabel(layer);
			if (label == "") label = mpt.FeatureID.ToString();

			folders[layer.UUID].Add("<name>" + layer.Name + " " + label + "</name>");
			folders[layer.UUID].Add("<description>" + GetAttributeTable(layer, mpt) + "</description>");
			folders[layer.UUID].Add($"<styleUrl>#{styleName}</styleUrl>");

			var pt = points.Last();

			folders[layer.UUID].Add("<Point>");
			folders[layer.UUID].Add("<coordinates>" + pt.Longitude + "," + pt.Latitude + "," + pt.Elevation + "</coordinates>");
			folders[layer.UUID].Add("</Point>");
			folders[layer.UUID].Add("</Placemark>");
			ent_count++;
		}

		private string GetAttributeTable(SwMapsFeatureLayer layer, SwMapsFeature item)
		{
			var Description = " <![CDATA[";
			var fields = layer.AttributeFields;
			var values = item.AttributeValues;

			Description += $"<h4>Layer: {layer.Name}<br/>";
			Description += $"Feature ID: {item.FeatureID}<br/>";
			Description += $"Remarks: {item.Remarks}</h4>";

			if (item.GeometryType == SwMapsGeometryType.Point)
			{
				var pt = item.Points[0];
				Description += $"<h4>Time:  {Formatter.GetTimeLabel(pt.Time)}<br/>";
				Description += $"Latitude: {Formatter.FormatLatLng(pt.Latitude)}<br/>";
				Description += $"Longitude: {Formatter.FormatLatLng(pt.Longitude)}<br/>";
				Description += $"Elevation:  {Formatter.GetElevationLabel(pt.Elevation)}<br/>";

				var fix = (NmeaFixQuality)(item.Points[0].FixID);

				if (fix != NmeaFixQuality.Invalid)
				{
					Description += $"Fix Quality: {fix}<br/>";
				}
				else
				{
					Description += "Drawn Point<br/>";
				}

				var additionalData = pt.AdditionalDataDictionary();
				if (additionalData.Count() > 0)
				{
					Description += "<br/>";
					if (FeatureAdditionalDataDecoder.CanDecode(additionalData))
					{
						Description += FeatureAdditionalDataDecoder.GetSummaryString(additionalData).Replace("\r", "").Replace("\n", "<br/>") + "<br/>";
					}
					else
					{
						foreach (var key in additionalData.Keys)
						{
							Description += key + ": " + additionalData[key] + "<br/>";
						}
					}
				}

				Description += "</h4>";
			}

			if (fields.Count > 0)
			{
				Description += "<table border='1'>";
				foreach (var field in fields)
				{
					var value = values.FirstOrDefault(it => it.FieldID == field.UUID);
					var valueText = "";
					if (value != null) valueText = value.Value;

					Description += " <tr><td>" + field.FieldName + "</td>";
					if (field.DataType == SwMapsAttributeType.Photo)
					{

						if (valueText == "")
						{
							Description += "<td></td></tr>";
						}
						else
						{
							var fileName = Path.GetFileName(valueText);
							Description += "<td><img src='files/" + fileName + "' width='200' /></td></tr>";
						}
					}
					else if(field.DataType == SwMapsAttributeType.Checklist)
					{
						var checkedItems = valueText.Split(new string[] { "||" },StringSplitOptions.RemoveEmptyEntries);

						Description += "<td><ul>";
						foreach(var t in checkedItems)
						{
							Description += "<li>" + t + "</li>";
						}
						Description += "</ul></td></tr>";

					}
					else if (field.DataType == SwMapsAttributeType.Text
							|| field.DataType == SwMapsAttributeType.Numeric
							|| field.DataType == SwMapsAttributeType.Options)
					{

						Description += "<td>" + valueText + "</td></tr>";
					}

				}
				Description += "</table>";
			}
			Description += "]]>";

			return Description;
		}

		private void AddLayerStyle(SwMapsFeatureLayer layer)
		{
			var iconPath = "";
			var writeColor = false;

			if (layer.PngSymbol != null && !KmlOnly)
			{
				iconPath = $"files/icons/{layer.Name}.png";
			}
			else
			{
				writeColor = true;
				if (layer.PointShape == SwMapsPointShape.Circle) iconPath = "http://maps.google.com/mapfiles/kml/shapes/donut.png";
				if (layer.PointShape == SwMapsPointShape.FilledCircle) iconPath = "http://maps.google.com/mapfiles/kml/shapes/placemark_circle.png";
				if (layer.PointShape == SwMapsPointShape.Triangle) iconPath = "http://maps.google.com/mapfiles/kml/shapes/triangle.png";
				if (layer.PointShape == SwMapsPointShape.Square) iconPath = "http://maps.google.com/mapfiles/kml/shapes/square.png";
			}

			folders[layer.UUID].Add($"<Style id=\"{layer.Name}-PointStyle\">");
			folders[layer.UUID].Add("<IconStyle>");
			if (writeColor)
			{
				folders[layer.UUID].Add("<color>#" + ColorUtils.ToAbgrHex(layer.Color) + "</color>");
				folders[layer.UUID].Add("<scale>0.5</scale>");
			}
			else
			{
				folders[layer.UUID].Add("<scale>0.5</scale>");
			}
			folders[layer.UUID].Add("<Icon>");
			folders[layer.UUID].Add($"<href>{iconPath}</href>");
			folders[layer.UUID].Add("</Icon>");
			folders[layer.UUID].Add("<hotSpot x=\"0.5\" y=\"0.5\" xunits=\"fraction\" yunits=\"fraction\"/>");
			folders[layer.UUID].Add("</IconStyle>");
			folders[layer.UUID].Add("<LabelStyle>");
			folders[layer.UUID].Add("<scale>0</scale>");
			folders[layer.UUID].Add("</LabelStyle>");
			folders[layer.UUID].Add("</Style>");


			folders[layer.UUID].Add($"<Style id=\"{layer.Name}-Style\">");
			folders[layer.UUID].Add("<LineStyle>");
			folders[layer.UUID].Add("<color>#" + ColorUtils.ToAbgrHex(layer.Color) + "</color>");
			folders[layer.UUID].Add("<width>3</width>");
			folders[layer.UUID].Add("</LineStyle>");
			folders[layer.UUID].Add("<PolyStyle>");
			folders[layer.UUID].Add("<color>#" + ColorUtils.ToAbgrHex(layer.FillColor) + "</color>");
			folders[layer.UUID].Add("</PolyStyle>");
			folders[layer.UUID].Add("</Style>");
		}

		private void AddPhoto(SwMapsPhotoPoint ph)
		{
			var fileName = Path.GetFileName(ph.FileName);
			folders["_PHOTOS"].Add("<Placemark>");
			folders["_PHOTOS"].Add($"<description><![CDATA[<img style=\"width:400px\" src=\"files/{fileName}\"/><br/><b>{ph.Remarks}</b>]]></description>");
			folders["_PHOTOS"].Add("<styleUrl>#PhotoIcon</styleUrl>");
			folders["_PHOTOS"].Add("<Point>");
			folders["_PHOTOS"].Add($"<coordinates>{ph.Location.Longitude},{ph.Location.Latitude},{ph.Location.Elevation}</coordinates>");
			folders["_PHOTOS"].Add("</Point>");
			folders["_PHOTOS"].Add("</Placemark>");
		}

		private void AddTrack(SwMapsTrack pl)
		{
			if (pl.Vertices.Count == 0) return;
			var styleName = $"P_{ent_count}";

			folders["_TRACKS"].Add($"<Style id=\"{styleName}\">");
			folders["_TRACKS"].Add("<LineStyle>");
			folders["_TRACKS"].Add("<color>#" + ColorUtils.ToAbgrHex(pl.Color) + "</color>");
			folders["_TRACKS"].Add("<width>3</width>");
			folders["_TRACKS"].Add("</LineStyle>");
			folders["_TRACKS"].Add("<PolyStyle>");
			folders["_TRACKS"].Add("<color>#" + ColorUtils.ToAbgrHex(pl.Color) + "</color>");
			folders["_TRACKS"].Add("</PolyStyle>");
			folders["_TRACKS"].Add("</Style>");


			folders["_TRACKS"].Add("<Placemark>");
			folders["_TRACKS"].Add("<name>" + pl.Name + "</name>");
			folders["_TRACKS"].Add("<description>" + pl.Remarks + "</description>");
			folders["_TRACKS"].Add($"<styleUrl>#{styleName}</styleUrl>");

			folders["_TRACKS"].Add("<LineString>");
			folders["_TRACKS"].Add("<tessellate>1</tessellate>");
			folders["_TRACKS"].Add("<coordinates>");

			var Points = "";
			foreach (var v in pl.Vertices)
			{
				Points += $"{v.Longitude},{v.Latitude},{v.Elevation} ";
			}
			folders["_TRACKS"].Add(Points.Trim());
			folders["_TRACKS"].Add("</coordinates>");
			folders["_TRACKS"].Add("</LineString>");
			folders["_TRACKS"].Add("</Placemark>");


			ent_count++;
		}
	}
}

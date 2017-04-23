using System;
using System.Globalization;
using System.Xml;

namespace Scigeobib
{
	public class KmlWriter : IDisposable
	{
		private XmlWriter xmlWriter;

		public KmlWriter(string path)
		{
			xmlWriter = XmlWriter.Create(path, new XmlWriterSettings() { Indent = true });

			xmlWriter.WriteStartDocument(true);

			xmlWriter.WriteStartElement("kml", "http://earth.google.com/kml/2.2");
			xmlWriter.WriteAttributeString("xmlns", "http://earth.google.com/kml/2.2");

			xmlWriter.WriteStartElement("Document");

			xmlWriter.WriteStartElement("Folder");
		}

		public void WriteCity(GeoCodedLocation city, double width)
		{
			xmlWriter.WriteStartElement("Placemark");

			xmlWriter.WriteStartElement("Point");

			xmlWriter.WriteStartElement("coordinates");
			xmlWriter.WriteString(city.lon + " , " + city.lat);
			xmlWriter.WriteEndElement();

			xmlWriter.WriteEndElement();

			xmlWriter.WriteStartElement("name");
			xmlWriter.WriteString(city.normalizedName);
			xmlWriter.WriteEndElement();

			xmlWriter.WriteStartElement("Style");
			xmlWriter.WriteStartElement("IconStyle");

			xmlWriter.WriteStartElement("scale");
			xmlWriter.WriteString(width.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteEndElement();

			xmlWriter.WriteStartElement("Icon");
			xmlWriter.WriteStartElement("href");
			xmlWriter.WriteString("http://maps.google.com/mapfiles/kml/pal2/icon18.png");
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();

			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();

			xmlWriter.WriteEndElement();
		}

		public void WriteLine(GeoCodedLocation from, GeoCodedLocation to, double width)
		{
			xmlWriter.WriteStartElement("Placemark");

			xmlWriter.WriteStartElement("visibility");
			xmlWriter.WriteString("1");
			xmlWriter.WriteEndElement();

			xmlWriter.WriteStartElement("open");
			xmlWriter.WriteString("0");
			xmlWriter.WriteEndElement();

			xmlWriter.WriteStartElement("description");
			xmlWriter.WriteCData(from.normalizedName + "---" + to.normalizedName);
			xmlWriter.WriteEndElement();

			xmlWriter.WriteStartElement("Style");
			xmlWriter.WriteStartElement("LineStyle");

			xmlWriter.WriteStartElement("color");
			xmlWriter.WriteString("ffff55ff");
			xmlWriter.WriteEndElement();

			xmlWriter.WriteStartElement("width");
			xmlWriter.WriteString(width.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteEndElement();

			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();

			xmlWriter.WriteStartElement("LineString");

			xmlWriter.WriteStartElement("tessellate");
			xmlWriter.WriteString("1");
			xmlWriter.WriteEndElement();

			xmlWriter.WriteStartElement("coordinates");
			xmlWriter.WriteString(from.lon + " , " + from.lat + " , 0" + "\n" + to.lon + " , " + to.lat + " , 0");
			xmlWriter.WriteEndElement();

			xmlWriter.WriteEndElement();

			xmlWriter.WriteEndElement();
		}

		public void Dispose()
		{
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();

			xmlWriter.WriteEndDocument();

			xmlWriter.Dispose();
		}
	}
}

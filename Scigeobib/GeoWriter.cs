using System.Collections.Generic;
using System.IO;
using System.Text;
using CsvHelper;

namespace Scigeobib
{
	public class GeoWriter
	{
		public static void WriteGeo(Dictionary<string, GeoCodedLocation> cities, string path)
		{
			using (TextWriter tw = new StreamWriter(path, false, Encoding.UTF8))
			{
				var csv = new CsvWriter(tw);
				foreach (var kv in cities)
				{
					if (kv.Value == null)
					{
						csv.WriteField("");
						csv.WriteField("");
					}
					else
					{
						csv.WriteField(kv.Value.lat);
						csv.WriteField(kv.Value.lon);
					}

					csv.WriteField(kv.Key);

					if (kv.Value == null)
					{
						csv.WriteField("-");
					}
					else
					{
						csv.WriteField(kv.Value.normalizedName);
					}

					csv.NextRecord();
				}
			}
		}
	}
}

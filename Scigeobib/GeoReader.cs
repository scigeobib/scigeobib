using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CsvHelper;

namespace Scigeobib
{
	public static class GeoReader
	{
		public static Dictionary<string, GeoCodedLocation> ReadGeo(string path)
		{
			Dictionary<string, GeoCodedLocation> result = new Dictionary<string, GeoCodedLocation>(StringComparer.InvariantCultureIgnoreCase);

			using (TextReader textReader = new StreamReader(path, Encoding.UTF8))
			{
				var csv = new CsvParser(textReader);
				while (true)
				{
					var row = csv.Read();
					if (row == null)
						break;

					string ourName = row[2];

					if (result.ContainsKey(ourName))
						continue;

					GeoCodedLocation city = null;

					if (row[0] != "")
					{
						city = new GeoCodedLocation(double.Parse(row[0]), double.Parse(row[1]), row[3]);
					}

					result.Add(ourName, city);
				}
			}

			return result;

		}
	}
}

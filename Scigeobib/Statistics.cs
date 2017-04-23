using System.Linq;
using System.Collections.Generic;
using CsvHelper;
using System.IO;
using System.Text;

namespace Scigeobib
{
	public class Statistics
	{
		public Dictionary<GeoCity, int> countryToPublicationsCount = new Dictionary<GeoCity, int>();
		public Dictionary<GeoCity, HashSet<string>> countryToJournals = new Dictionary<GeoCity, HashSet<string>>();

		public void AddPublicationInCountry(GeoCity country)
		{
			CollectionUtils.Increment(countryToPublicationsCount, country);
		}

		public void AddJournalInCountry(GeoCity country, string journal)
		{
			CollectionUtils.AddToSet(countryToJournals, country, journal);
		}

		public void WriteKml(IDictionary<GeoCity, int> dict, string filePath)
		{
			int maxValue = dict.Max(kv => kv.Value);
			using (KmlWriter kmlWriter = new KmlWriter(filePath))
			{
				foreach (var kv in dict)
				{
					kmlWriter.WriteCity(kv.Key, 1 + kv.Value * 2 / maxValue);
				}
			}
		}

		public void WriteCsv(IDictionary<GeoCity, int> dict, string filePath)
		{
			using (TextWriter tw = new StreamWriter(filePath, false, Encoding.UTF8))
			{
				var csv = new CsvWriter(tw);

				foreach (var kv in dict)
				{
					csv.WriteField(kv.Key.normalizedName);
					csv.WriteField(kv.Value);

					csv.NextRecord();
				}
			}
		}
	}
}

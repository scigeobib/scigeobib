using System.Linq;
using System.Collections.Generic;
using CsvHelper;
using System.IO;
using System.Text;

namespace Scigeobib
{
	public class Statistics
	{
		public Dictionary<GeoCodedLocation, int> countryToPublicationsCount = new Dictionary<GeoCodedLocation, int>();
		public Dictionary<GeoCodedLocation, HashSet<string>> countryToJournals = new Dictionary<GeoCodedLocation, HashSet<string>>();

		public void AddPublicationInCountry(GeoCodedLocation country)
		{
			CollectionUtils.Increment(countryToPublicationsCount, country);
		}

		public void AddJournalInCountry(GeoCodedLocation country, string journal)
		{
			CollectionUtils.AddToSet(countryToJournals, country, journal);
		}

		public void WriteKml(IDictionary<GeoCodedLocation, int> dict, string filePath)
		{
			int maxValue = dict.Count > 0 ? dict.Max(kv => kv.Value) : 1;
			using (KmlWriter kmlWriter = new KmlWriter(filePath))
			{
				foreach (var kv in dict)
				{
					kmlWriter.WriteCity(kv.Key, 1 + (double)kv.Value * 2 / maxValue, kv.Value);
				}
			}
		}

		public void WriteCsv(IDictionary<GeoCodedLocation, int> dict, string filePath)
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

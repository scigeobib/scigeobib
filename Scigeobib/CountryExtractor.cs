
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NLog;

namespace Scigeobib
{
	public class CountryExtractor
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static string GetCountry(Publication publication, FileType fileType)
		{
			string geoField = null;
			if (fileType == FileType.WOS) geoField = "PA";
			else if (fileType == FileType.SCOPUS) geoField = "Publisher";
			else throw new Exception("Unhandled file type: " + fileType);

			string result = null;

			if (publication.entries.ContainsKey(geoField))
			{
				PublicationEntry entry = publication.entries[geoField];

				if (entry.values.Count > 0)
				{
					string parsed = ParseCountry(entry.values[entry.values.Count - 1]);
					if (!string.IsNullOrEmpty(parsed))
						result = parsed;
				}
				else
				{
					logger.Warn("No values for country extraction");
				}
			}
			else
			{
				logger.Warn("No field for country extraction");
			}

			if (result != null)
				logger.Debug("Extracted country from publication: {0}", result);
			else
				logger.Warn("No country extracted from a publication");

			return result;
		}

		private static string ParseCountry(string address)
		{
			string[] parts = address.TrimEnd('.').Split(',');

			if (parts.Length >= 2)
			{
				string lastPart = RemoveWordsWithNumbers(parts[parts.Length - 1].Trim());



				return lastPart;
			}

			return null;
		}

		private static string RemoveWordsWithNumbers(string orig)
		{
			string[] words = orig.Split(' ');

			List<string> withoutNumbers = new List<string>();
			foreach (string word in words)
			{
				if (!Regex.IsMatch(word, "[0-9]"))
				{
					withoutNumbers.Add(word);
				}
			}

			return string.Join(" ", withoutNumbers);
		}
	}
}

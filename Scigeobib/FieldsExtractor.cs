using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using NLog;

namespace Scigeobib
{
	public class FieldsExtractor
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private FileType fileType;

		public FieldsExtractor(FileType fileType)
		{
			this.fileType = fileType;
		}

		public string GetTitle(Publication publication)
		{
			string field = null;
			if (fileType == FileType.WOS) field = "TI";
			else if (fileType == FileType.SCOPUS) field = "Title";
			else throw new Exception("Unhandled file type: " + fileType);

			string result = null;
			if (publication.entries.ContainsKey(field))
			{
				PublicationEntry entry = publication.entries[field];
				result = string.Join(" ", entry.values);
			}

			return result;
		}

		public struct ParsedLocation
		{
			public string NameForGeocoding { get; set; }
			public string AdditionalNamePrefix { get; set; }
		}

		public List<ParsedLocation> GetCities(Publication publication)
		{
			return GetLocationsFromAddresses(publication, ParseCity, x => "", "cities");
		}

		public List<ParsedLocation> GetCountries(Publication publication)
		{
			return GetLocationsFromAddresses(publication, ParseCountry, x => "", "countries");
		}

		public List<ParsedLocation> GetInstitutions(Publication publication)
		{
			return GetLocationsFromAddresses(publication, ParseCity, ParseInstitution, "institutions");
		}

		private List<ParsedLocation> GetLocationsFromAddresses(Publication publication, Func<string, string> parseNameForGeocodingFunc, Func<string, string> parseAdditionalNamePrefixFunc, string nameForLog)
		{
			string field = null;
			if (fileType == FileType.WOS) field = "C1";
			else if (fileType == FileType.SCOPUS) field = "Affiliations";
			else throw new Exception("Unhandled file type: " + fileType);

			List<ParsedLocation> result = new List<ParsedLocation>();

			if (publication.entries.ContainsKey(field))
			{
				PublicationEntry entry = publication.entries[field];
				if (entry.values.Count == 0)
				{
					logger.Debug("No values for {0} extraction", nameForLog);
				}

				foreach (string address in entry.values)
				{
					string nameForGeocoding = parseNameForGeocodingFunc(address);

					if (nameForGeocoding == null)
						logger.Warn("No {0} extracted from address: \"{1}\" (in publication \"{2}\")", nameForLog, address, GetTitle(publication));

					string additionalNamePrefix = parseAdditionalNamePrefixFunc(address);
					if (additionalNamePrefix == null)
						logger.Warn("No {0} (additional name prefix) extracted from address: \"{1}\" (in publication \"{2}\")", nameForLog, address, GetTitle(publication));

					if (nameForGeocoding != null && additionalNamePrefix != null)
					{
						if (additionalNamePrefix != "")
							additionalNamePrefix = additionalNamePrefix.ToUpperInvariant() + ", ";
						result.Add(new ParsedLocation() { NameForGeocoding = nameForGeocoding, AdditionalNamePrefix = additionalNamePrefix });
					}
				}
			}
			else
			{
				logger.Debug("No field for {0} extraction", nameForLog);
			}

			if (result.Count > 0)
				logger.Info("Extracted {0}: \"{1}\" (publication: \"{2}\")", nameForLog, string.Join(", ", result), GetTitle(publication));
			else
				logger.Warn("No {0} extracted from publication: \"{1}\"", nameForLog, GetTitle(publication));

			return result;
		}

		public string GetCountry(Publication publication)
		{
			string field = null;
			if (fileType == FileType.WOS) field = "PA";
			else if (fileType == FileType.SCOPUS) field = "Publisher";
			else throw new Exception("Unhandled file type: " + fileType);

			string result = null;

			if (publication.entries.ContainsKey(field))
			{
				PublicationEntry entry = publication.entries[field];

				if (entry.values.Count > 0)
				{
					string parsed = ParseCountry(entry.values[entry.values.Count - 1]);
					if (!string.IsNullOrEmpty(parsed))
						result = parsed;
				}
				else
				{
					logger.Debug("No values for country extraction");
				}
			}
			else
			{
				logger.Debug("No field for country extraction");
			}

			if (result != null)
				logger.Info("Extracted country: \"{0}\" (publication: \"{1}\")", result, GetTitle(publication));
			else
				logger.Warn("No country extracted from publication: \"{0}\"", GetTitle(publication));

			return result;
		}

		public string GetJournal(Publication publication)
		{
			string field = null;
			if (fileType == FileType.WOS) field = "SO";
			else if (fileType == FileType.SCOPUS) field = "Source title";
			else throw new Exception("Unhandled file type: " + fileType);

			string result = null;

			if (publication.entries.ContainsKey(field))
			{
				PublicationEntry entry = publication.entries[field];

				if (entry.values.Count > 0)
				{
					string journal = string.Join(" ", entry.values);
					if (!string.IsNullOrEmpty(journal))
						result = journal;
				}
				else
				{
					logger.Warn("No values for journal extraction");
				}
			}
			else
			{
				logger.Warn("No field for journal extraction");
			}

			if (result != null)
				logger.Info("Extracted journal: \"{0}\" (publication: \"{1}\")", result, GetTitle(publication));
			else
				logger.Warn("No journal extracted from publication: \"{0}\"", GetTitle(publication));

			return result;
		}

		private static string ParseCity(string address)
		{
			string[] parts = address.TrimEnd('.').Split(',');

			if (parts.Length >= 2)
			{
				string part1 = RemoveWordsWithNumbers(parts[parts.Length - 2].Trim());
				string part2 = RemoveWordsWithNumbers(parts[parts.Length - 1].Trim());

				string city = part1 + ", " + part2;

				if (parts.Length >= 3)
				{
					string part0 = RemoveWordsWithNumbers(parts[parts.Length - 3].Trim());
					if (part1.Length == 0 || part1.EndsWith("."))
					{
						city = part0 + ", " + part2;
					}

					if (Regex.IsMatch(part1, "^[A-Z][A-Z]$") || Regex.IsMatch(part1, "^[A-Z][A-Z][A-Z]$"))
					{
						city = part0 + ", " + part1 + ", " + part2;
					}
				}

				return city;
			}

			return null;
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

		private static string ParseInstitution(string address)
		{
			int nameEnd = address.IndexOf(']');
			if (nameEnd != -1)
			{
				address = address.Substring(nameEnd + 1).Trim();
			}
			string[] parts = address.Split(',');
			return parts[0];
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

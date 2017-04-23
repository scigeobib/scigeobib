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

		public List<string> GetCities(Publication publication)
		{
			string field = null;
			if (fileType == FileType.WOS) field = "C1";
			else if (fileType == FileType.SCOPUS) field = "Affiliations";
			else throw new Exception("Unhandled file type: " + fileType);

			List<string> result = new List<string>();

			if (publication.entries.ContainsKey(field))
			{
				PublicationEntry entry = publication.entries[field];
				if (entry.values.Count == 0)
				{
					logger.Debug("No values for cities extraction");
				}

				foreach (string address in entry.values)
				{
					string city = ParseCity(address);

					if (city != null)
						result.Add(city);
					else
						logger.Warn("No cities extracted from address: \"{0}\" (in publication \"{1}\")", address, GetTitle(publication));
				}
			}
			else
			{
				logger.Debug("No field for cities extraction");
			}

			if (result.Count > 0)
				logger.Info("Extracted cities: \"{0}\" (publication: \"{1}\")", string.Join(", ", result), GetTitle(publication));
			else
				logger.Warn("No cities extracted from publication: \"{0}\"", GetTitle(publication));

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

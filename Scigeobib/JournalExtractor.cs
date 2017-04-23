using System;
using NLog;

namespace Scigeobib
{
	public class JournalExtractor
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static string GetJournal(Publication publication, FileType fileType)
		{
			string journalField = null;
			if (fileType == FileType.WOS) journalField = "SO";
			else if (fileType == FileType.SCOPUS) journalField = "Source title";
			else throw new Exception("Unhandled file type: " + fileType);

			string result = null;

			if (publication.entries.ContainsKey(journalField))
			{
				PublicationEntry entry = publication.entries[journalField];

				if (entry.values.Count != 1)
				{
					logger.Warn("Invalid count of values for journal extraction");
				}

				if (entry.values.Count > 0)
				{
					string journal = entry.values[0];
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
				logger.Debug("Extracted journal from publication: {0}", result);
			else
				logger.Warn("No journal extracted from a publication");

			return result;
		}
	}
}

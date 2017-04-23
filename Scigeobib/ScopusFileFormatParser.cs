using System.Collections.Generic;
using System.IO;
using System.Text;
using CsvHelper;
using NLog;

namespace Scigeobib
{
	public class ScopusFileFormatParser
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static PublicationsFile Parse(string filePath)
		{
			PublicationsFile parsed = new PublicationsFile();

			parsed.Type = FileType.SCOPUS;

			using (TextReader textReader = new StreamReader(filePath, Encoding.UTF8))
			{
				var csv = new CsvParser(textReader);

				var titleRow = csv.Read();

				while (true)
				{
					var row = csv.Read();
					if (row == null)
						break;

					Publication publication = new Publication();
					for (int i = 0; i < row.Length; ++i)
					{
						PublicationEntry entry = new PublicationEntry();
						entry.field = titleRow[i];
						entry.values = new List<string>();
						string[] values = row[i].Split(';');
						foreach (string value in values)
							entry.values.Add(value.Trim());
						publication.entries.Add(entry.field, entry);
					}

					parsed.publications.Add(publication);
				}
			}

			logger.Info("Publications loaded from scopus: {0}", parsed.publications.Count);

			return parsed;
		}
	}
}

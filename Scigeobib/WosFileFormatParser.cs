using System;
using System.IO;
using System.Text;
using NLog;

namespace Scigeobib
{
	public static class WosFileFormatParser
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static PublicationsFile Parse(string filePath)
		{
			string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

			PublicationsFile parsed = new PublicationsFile();

			parsed.Type = FileType.WOS;

			Publication currentPublication = new Publication();

			PublicationEntry currentEntry = null;

			bool wasEndOfFile = false;

			foreach (string line in lines)
			{
				if (wasEndOfFile)
					throw new Exception("Data after end of file");

				if (line == "")
					continue;

				if (line == "EF")
				{
					wasEndOfFile = true;
					continue;
				}

				if (line == "ER")
				{
					parsed.publications.Add(currentPublication);
					currentPublication = new Publication();
				}
				else
				{
					string tagPart = line.Substring(0, 3).TrimEnd(' ');

					if (tagPart == "FN" || tagPart == "VR")
					{
						continue;
					}

					string valuePart = line.Substring(3);

					if (tagPart != "")
					{
						currentEntry = new PublicationEntry();
						currentEntry.field = tagPart;
						currentEntry.values.Add(valuePart);
						currentPublication.entries.Add(tagPart, currentEntry);
					}
					else
					{
						currentEntry.values.Add(valuePart);
					}
				}
			}

			logger.Info("Publications loaded from WOS: {0}", parsed.publications.Count);

			return parsed;
		}
	}
}

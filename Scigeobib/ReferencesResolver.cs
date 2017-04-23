using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using NLog;

namespace Scigeobib
{
	public class ReferencesResolver
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private List<PublicationWithErrors> publicationsWithErrors = new List<PublicationWithErrors>();

		public List<PublicationWithReferences> ResolveAll(PublicationsFile f)
		{
			if (f.Type != FileType.WOS)
				throw new Exception("Unhandled file type: " + f.Type);

			List<PublicationWithReferences> result = new List<PublicationWithReferences>();

			foreach (Publication publication in f.publications)
			{
				result.Add(Resolve(publication, f));
			}

			return result;
		}

		private PublicationWithReferences Resolve(Publication publication, PublicationsFile f)
		{
			PublicationWithReferences result = new PublicationWithReferences();
			result.publication = publication;

			PublicationWithErrors errors = new PublicationWithErrors();
			errors.publication = publication;

			List<string> values = null;
			{
				PublicationEntry found;
				if (publication.entries.TryGetValue("CR", out found))
				{
					values = found.values;
				}
				else
				{
					values = new List<string>();
				}
			}

			foreach (string value in values)
			{
				string[] parts = value.Split(new string[] { ", " }, StringSplitOptions.None);

				bool found = false;

				foreach (Publication potentialPublication in f.publications)
				{
					if (Matches(potentialPublication, parts))
					{
						if (found)
						{
							errors.more.Add(value);
						}

						if (publication == potentialPublication)
						{
							errors.self.Add(value);
						}

						found = true;
						result.resolved.Add(potentialPublication);
					}
				}

				if (!found)
				{
					errors.unresolved.Add(value);
				}
			}

			if (errors.HasErrors())
				publicationsWithErrors.Add(errors);

			return result;
		}

		public void HandleUnresolved()
		{
			File.WriteAllText("output_publications_errors.txt", JsonConvert.SerializeObject(publicationsWithErrors, Formatting.Indented), Encoding.UTF8);
			if (publicationsWithErrors.Count > 0)
			{
				logger.Warn("{0} publications with unresolved references (output_publications_errors.txt)", publicationsWithErrors.Count);
			}
		}

		private static bool Matches(Publication publication, string[] parts)
		{
			string doiBegin = "DOI ";

			foreach (string part in parts)
			{
				if (part.StartsWith(doiBegin, false, CultureInfo.InvariantCulture))
				{
					string v = part.Substring(doiBegin.Length);

					if (publication.entries.ContainsKey("DI"))
					{
						var found = publication.entries["DI"];
						return found.values.Single().Equals(v);
					}
				}
			}

			List<string> unmatchedParts = new List<string>();
			foreach (string part in parts)
			{
				if (part.StartsWith(doiBegin, false, CultureInfo.InvariantCulture))
					continue;

				if (!PartMatches(publication, part))
					unmatchedParts.Add(part);
			}

			if (unmatchedParts.Count > 0 && unmatchedParts.Count <= 1)
			{
				/*
				WosFileFormatEntry ppp = publication.entries.SingleOrDefault(x => x.tag == "BP");
				if (ppp != null)
				{
					string s = ppp.values.Single();
					logger.Warn(unmatchedParts[0] + " -> " + s);
				}
				*/
			}

			return unmatchedParts.Count == 0;
		}

		private static bool PartMatches(Publication publication, string part)
		{
			if (Regex.IsMatch(part, "^V[0-9]+$"))
			{
				string v = part.Substring(1);

				PublicationEntry found;
				if (publication.entries.TryGetValue("VL", out found))
					return found.values.Single().Equals(v);
				else
					return false;
			}
			if (Regex.IsMatch(part, "^P[0-9]+$"))
			{
				string v = part.Substring(1);

				PublicationEntry found;
				if (publication.entries.TryGetValue("BP", out found))
					return found.values.Single().Equals(v);
				else
					return false;
			}

			foreach (PublicationEntry entry in publication.entries.Values)
			{
				if (entry.field == "CR")
					continue;

				foreach (string value in entry.values)
				{
					if (NormalizeLine(value).Contains(NormalizeLine(part)))
					{
						return true;
					}
				}
			}

			return false;
		}

		private static string NormalizeLine(string line)
		{
			line = line.ToUpperInvariant();
			line = line.Normalize(NormalizationForm.FormD);

			StringBuilder sb = new StringBuilder();

			foreach (char c in line)
			{
				if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
				{
					sb.Append(c);
				}
			}


			return sb.ToString();
		}
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;

namespace Scigeobib
{
	public class CitiesResolver
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private Dictionary<string, GeoCity> knownCities = new Dictionary<string, GeoCity>(StringComparer.InvariantCultureIgnoreCase);

		private SortedSet<string> unknownCities = new SortedSet<string>(StringComparer.InvariantCultureIgnoreCase);

		private GeoCoder geoCoder;
		private bool retry;

		public void SetKnownCities(string knownCitiesFilePath)
		{
			var cities = GeoReader.ReadGeo(knownCitiesFilePath);
			knownCities.Clear();
			foreach (var kv in cities)
			{
				knownCities[kv.Key] = kv.Value;
			}
		}

		public void SetGeoCoder(GeoCoder geoCoder)
		{
			this.geoCoder = geoCoder;
		}

		public void SetRetry(bool retry)
		{
			this.retry = retry;
		}

		public GeoCity Resolve_OrNull(string city)
		{
			GeoCity found = null;
			if (knownCities.TryGetValue(city, out found))
			{
				if (found != null)
				{
					logger.Info("City {0} is alredy known and resolved to: {1}.", city, found.normalizedName);
				}
				else
				{
					if (geoCoder != null && retry)
					{
						found = geoCoder.GetGeocoded(city);
						knownCities[city] = found;
						if (found != null)
							logger.Info("City {0} is alredy known, but not successfully resolved. After retry, it was resolved to: {1}.", city, found.normalizedName);
						else
							logger.Warn("City {0} is alredy known, but not successfully resolved. After retry, it was still now resolved.", city);
					}
					else
					{
						logger.Warn("City {0} is alredy known, but not successfully resolved. Not retrying.", city);
					}
				}
			}
			else
			{
				if (geoCoder != null)
				{
					found = geoCoder.GetGeocoded(city);
					knownCities.Add(city, found);
					if (found != null)
						logger.Info("City {0} successfully resolved to: {1}.", city, found.normalizedName);
					else
						logger.Warn("City {0} not resolved.", city);
				}
				else
				{
					unknownCities.Add(city);
					logger.Warn("City {0} is not known. Not resolving.", city);
				}
			}

			return found;
		}

		public void LogTotals()
		{
			logger.Info("Known cities (included in the results): {0}.", knownCities.Count);

			var unresolved = GetUnresolved();
			if (unresolved.Length > 0)
			{
				logger.Info("Unresolved cities (not included in the results): {0}.", unresolved.Length);
			}

			if (unknownCities.Count > 0)
			{
				logger.Info("Unknown cities (not included in the results): {0}.", unknownCities.Count);
			}
		}

		public void HandleKnown(string filePath)
		{
			GeoWriter.WriteGeo(knownCities, filePath);
		}

		public void HandleUnresolved(string filePath)
		{
			File.WriteAllLines(filePath, GetUnresolved());
		}

		public void HandleUnknown(string filePath)
		{
			File.WriteAllLines(filePath, unknownCities.ToArray());
		}

		private string[] GetUnresolved()
		{
			return knownCities.Where(kv => kv.Value == null).Select(kv => kv.Key).ToArray();
		}
	}
}

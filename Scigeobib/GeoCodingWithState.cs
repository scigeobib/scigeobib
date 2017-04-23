using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;

namespace Scigeobib
{
	public class GeoCodingWithState
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private Dictionary<string, GeoCodedLocation> knownLocations = new Dictionary<string, GeoCodedLocation>(StringComparer.InvariantCultureIgnoreCase);

		private SortedSet<string> unknownLocations = new SortedSet<string>(StringComparer.InvariantCultureIgnoreCase);

		private SortedSet<string> failedNow = new SortedSet<string>(StringComparer.InvariantCultureIgnoreCase);

		private GeoCoder geoCoder;
		private bool retry;

		public void SetKnownLocations(string filePath)
		{
			var locations = GeoReader.ReadGeo(filePath);
			knownLocations.Clear();
			foreach (var kv in locations)
			{
				knownLocations[kv.Key] = kv.Value;
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

		public GeoCodedLocation GeoCode_OrNull(string location)
		{
			GeoCodedLocation found = null;
			if (knownLocations.TryGetValue(location, out found))
			{
				if (found != null)
				{
					logger.Debug("Location already known and already successfully geo coded: \"{0}\" -> \"{1}\".", location, found.normalizedName);
				}
				else
				{
					if (geoCoder != null && retry && !failedNow.Contains(location))
					{
						found = geoCoder.GetGeocoded(location);
						knownLocations[location] = found;
						if (found != null)
							logger.Info("Location alredy known, failed previously, but geo coded successfully now: \"{0}\" -> \"{1}\".", location, found.normalizedName);
						else
						{
							failedNow.Add(location);
							logger.Warn("Location already known, failed to geo code even now after retry: \"{0}\".", location);
						}
					}
					else
					{
						logger.Warn("Location already known, failed to geo code previously - not retrying: \"{0}\".", location);
					}
				}
			}
			else
			{
				if (geoCoder != null)
				{
					found = geoCoder.GetGeocoded(location);
					knownLocations.Add(location, found);
					if (found != null)
						logger.Info("Location geo coded: \"{0}\" -> \"{1}\"", location, found.normalizedName);
					else
					{
						failedNow.Add(location);
						logger.Warn("Location failed to geo code: \"{0}\".", location);
					}
				}
				else
				{
					unknownLocations.Add(location);
					logger.Warn("Location not geo coded previously, not geo coding now: \"{0}\".", location);
				}
			}

			return found;
		}

		public void LogTotals()
		{
			var success = GetSuccess();
			logger.Info("Successfully geo coded locations (included in the results): {0}.", success.Length);

			var failed = GetFailed();
			if (failed.Length > 0)
			{
				logger.Warn("Locations failed to geo code (not included in the results): {0}.", failed.Length);
			}

			if (unknownLocations.Count > 0)
			{
				logger.Warn("Not geo coded locations (not included in the results): {0}.", unknownLocations.Count);
			}
		}

		public void HandleKnown(string filePath)
		{
			GeoWriter.WriteGeo(knownLocations, filePath);
		}

		public void HandleFailed(string filePath)
		{
			File.WriteAllLines(filePath, GetFailed());
		}

		public void HandleUnknown(string filePath)
		{
			File.WriteAllLines(filePath, unknownLocations.ToArray());
		}

		private string[] GetSuccess()
		{
			return knownLocations.Where(kv => kv.Value != null).Select(kv => kv.Key).ToArray();
		}

		private string[] GetFailed()
		{
			return knownLocations.Where(kv => kv.Value == null).Select(kv => kv.Key).ToArray();
		}
	}
}

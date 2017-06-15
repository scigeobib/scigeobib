using System;
using System.Collections.Generic;

namespace Scigeobib
{
	public class CollaborationMatrix
	{
		private SortedDictionary<GeoCodedLocation, int> locationToConnectionsCount = new SortedDictionary<GeoCodedLocation, int>();

		private SortedDictionary<Tuple<GeoCodedLocation, GeoCodedLocation>, int> usedConnections = new SortedDictionary<Tuple<GeoCodedLocation, GeoCodedLocation>, int>();

		private int maxLocationValue = 1;
		private int maxConnectionValue = 1;

		public void AddConnection(GeoCodedLocation location1, GeoCodedLocation location2)
		{
			AddLocation(location1);
			AddLocation(location2);

			GeoCodedLocation locationA;
			GeoCodedLocation locationB;
			if (location1.CompareTo(location2) < 0)
			{
				locationA = location1;
				locationB = location2;
			}
			else
			{
				locationA = location2;
				locationB = location1;
			}

			var key = new Tuple<GeoCodedLocation, GeoCodedLocation>(locationA, locationB);

			int newValue = CollectionUtils.Increment(usedConnections, key);
			if (newValue > maxConnectionValue)
			{
				maxConnectionValue = newValue;
			}
		}

		private void AddLocation(GeoCodedLocation location)
		{
			int newValue = CollectionUtils.Increment(locationToConnectionsCount, location);
			if (newValue > maxLocationValue)
			{
				maxLocationValue = newValue;
			}
		}

		public void WriteKml(string filePath)
		{
			using (KmlWriter kmlWriter = new KmlWriter(filePath))
			{
				foreach (var city in locationToConnectionsCount)
				{
					kmlWriter.WriteCity(city.Key, 1 + (double)city.Value * 2 / maxLocationValue, city.Value);
				}

				foreach (var connection in usedConnections)
				{
					double width = 1 + (double)connection.Value * 4 / maxConnectionValue;
					kmlWriter.WriteLine(connection.Key.Item1, connection.Key.Item2, width, connection.Value);
				}
			}
		}
	}
}

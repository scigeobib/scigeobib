using System;
using System.Collections.Generic;

namespace Scigeobib
{
	public class CityMatrix
	{
		private SortedDictionary<GeoCodedLocation, int> cityToConnectionsCount = new SortedDictionary<GeoCodedLocation, int>();

		private SortedDictionary<Tuple<GeoCodedLocation, GeoCodedLocation>, int> usedConnections = new SortedDictionary<Tuple<GeoCodedLocation, GeoCodedLocation>, int>();

		private int maxCityValue = 1;
		private int maxConnectionValue = 1;

		public void AddConnection(GeoCodedLocation city1, GeoCodedLocation city2)
		{
			AddCity(city1);
			AddCity(city2);

			GeoCodedLocation cityA;
			GeoCodedLocation cityB;
			if (city1.CompareTo(city2) < 0)
			{
				cityA = city1;
				cityB = city2;
			}
			else
			{
				cityA = city2;
				cityB = city1;
			}

			var key = new Tuple<GeoCodedLocation, GeoCodedLocation>(cityA, cityB);

			int newValue = CollectionUtils.Increment(usedConnections, key);
			if (newValue > maxConnectionValue)
			{
				maxConnectionValue = newValue;
			}
		}

		private void AddCity(GeoCodedLocation city)
		{
			int newValue = CollectionUtils.Increment(cityToConnectionsCount, city);
			if (newValue > maxCityValue)
			{
				maxCityValue = newValue;
			}
		}

		public IDictionary<GeoCodedLocation, int> GetCities()
		{
			return cityToConnectionsCount;
		}

		public IDictionary<Tuple<GeoCodedLocation, GeoCodedLocation>, int> GetConnections()
		{
			return usedConnections;
		}

		public int GetMaxCityValue()
		{
			return maxCityValue;
		}

		public int GetMaxConnectionValue()
		{
			return maxConnectionValue;
		}
	}
}

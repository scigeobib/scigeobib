using System;
using System.Collections.Generic;

namespace Scigeobib
{
	public class CityMatrix
	{
		private SortedDictionary<GeoCity, int> cityToConnectionsCount = new SortedDictionary<GeoCity, int>();

		private SortedDictionary<Tuple<GeoCity, GeoCity>, int> usedConnections = new SortedDictionary<Tuple<GeoCity, GeoCity>, int>();

		private int maxCityValue = 1;
		private int maxConnectionValue = 1;

		public void AddConnection(GeoCity city1, GeoCity city2)
		{
			AddCity(city1);
			AddCity(city2);

			GeoCity cityA;
			GeoCity cityB;
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

			var key = new Tuple<GeoCity, GeoCity>(cityA, cityB);

			int newValue = CollectionUtils.Increment(usedConnections, key);
			if (newValue > maxConnectionValue)
			{
				maxConnectionValue = newValue;
			}
		}

		private void AddCity(GeoCity city)
		{
			int newValue = CollectionUtils.Increment(cityToConnectionsCount, city);
			if (newValue > maxCityValue)
			{
				maxCityValue = newValue;
			}
		}

		public IDictionary<GeoCity, int> GetCities()
		{
			return cityToConnectionsCount;
		}

		public IDictionary<Tuple<GeoCity, GeoCity>, int> GetConnections()
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

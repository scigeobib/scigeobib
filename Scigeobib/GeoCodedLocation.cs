using System;

namespace Scigeobib
{
	public class GeoCodedLocation : IComparable<GeoCodedLocation>, IComparable
	{
		public double lat { get; private set; }
		public double lon { get; private set; }
		public string normalizedName { get; private set; }

		public GeoCodedLocation(double lat, double lon, string normalizedName)
		{
			this.lat = lat;
			this.lon = lon;
			this.normalizedName = normalizedName;
		}

		public int CompareTo(object obj)
		{
			return normalizedName.CompareTo(((GeoCodedLocation)obj).normalizedName);
		}

		public int CompareTo(GeoCodedLocation other)
		{
			return normalizedName.CompareTo(other.normalizedName);
		}

		public override bool Equals(object obj)
		{
			return normalizedName.Equals(((GeoCodedLocation)obj).normalizedName);
		}

		public override int GetHashCode()
		{
			return normalizedName.GetHashCode();
		}
	}
}

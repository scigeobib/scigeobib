using System;

namespace Scigeobib
{
	public class GeoCity : IComparable<GeoCity>, IComparable
	{
		public double lat;
		public double lon;
		public string normalizedName;

		public int CompareTo(object obj)
		{
			return normalizedName.CompareTo(((GeoCity)obj).normalizedName);
		}

		public int CompareTo(GeoCity other)
		{
			return normalizedName.CompareTo(other.normalizedName);
		}

		public override bool Equals(object obj)
		{
			return normalizedName.Equals(((GeoCity)obj).normalizedName);
		}

		public override int GetHashCode()
		{
			return normalizedName.GetHashCode();
		}
	}
}

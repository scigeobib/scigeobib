using System;
using BingMapsRESTToolkit;

namespace Scigeobib
{
	// old: https://msdn.microsoft.com/en-us/library/jj819168.aspx
	// https://github.com/Microsoft/BingMapsRESTToolkit/wiki/Getting-Started
	public class GeoCoder
	{
		private string apiKey;

		public GeoCoder(string apiKey)
		{
			this.apiKey = apiKey;
		}

		public GeoCodedLocation GetGeocoded(string query)
		{
			var request = new GeocodeRequest()
			{
				Query = query,
				MaxResults = 1,
				BingMapsKey = apiKey
			};

			var response = ServiceManager.GetResponseAsync(request).Result;

			if (response.AuthenticationResultCode != "ValidCredentials")
				throw new Exception("Invalid GeoCoder API key");

			if (response.StatusCode != 200)
				throw new Exception("Invalid GeoCoder response status code: " + response.StatusCode);

			if (response != null &&
				response.ResourceSets != null &&
				response.ResourceSets.Length > 0 &&
				response.ResourceSets[0].Resources != null &&
				response.ResourceSets[0].Resources.Length > 0)
			{
				var result = response.ResourceSets[0].Resources[0] as BingMapsRESTToolkit.Location;

				GeoCodedLocation geoCity = new GeoCodedLocation();
				geoCity.lat = result.Point.Coordinates[0];
				geoCity.lon = result.Point.Coordinates[1];
				geoCity.normalizedName = result.Address.FormattedAddress;

				return geoCity;
			}

			return null;
		}
	}
}

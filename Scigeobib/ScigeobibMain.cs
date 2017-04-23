﻿
using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace Scigeobib
{
	public class ScigeobibMain
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private PublicationsFile publicationsFile;
		private CitiesResolver citiesResolver = new CitiesResolver();
		private CityMatrix matrix;
		private Statistics statistics;

		public void SetInput_Publications_WOS(string filePath)
		{
			try
			{
				publicationsFile = WosFileFormatParser.Parse(filePath);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while reading WOS.");
				throw;
			}
		}

		public void SetInput_Publications_ScopusCsv(string filePath)
		{
			try
			{
				publicationsFile = ScopusFileFormatParser.Parse(filePath);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while reading Scopus.");
				throw;
			}
		}

		public void SetInput_KnownCitiesCsv(string filePath)
		{
			try
			{
				citiesResolver.SetKnownCities(filePath);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while loading known cities.");
				throw;
			}
		}

		public void SetInput_GeoCoderKey(string key)
		{
			try
			{
				citiesResolver.SetGeoCoder(key != null ? new GeoCoder(key) : null);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while initializing geo coder.");
				throw;
			}
		}

		public void SetInput_RetryGeoCoding(bool retry)
		{
			try
			{
				citiesResolver.SetRetry(retry);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while setting options (retry).");
				throw;
			}
		}

		public void DoWork()
		{
			try
			{
				matrix = new CityMatrix();

				//SortedDictionary<GeoCity, int> cityToPublicationsCount = new SortedDictionary<GeoCity, int>();

				foreach (Publication publication in publicationsFile.publications)
				{
					var publicationCities = CitiesExtractor.GetCities(publication, publicationsFile.Type);

					List<GeoCity> publicationCitiesResolved = publicationCities.Select(x => citiesResolver.Resolve_OrNull(x)).Where(x => x != null).Distinct().ToList();

					for (int i = 0; i < publicationCitiesResolved.Count; ++i)
					{
						//CollectionUtils.Increment(cityToPublicationsCount, publicationCitiesResolved[i]);

						for (int j = i + 1; j < publicationCitiesResolved.Count; ++j)
						{
							matrix.AddConnection(publicationCitiesResolved[i], publicationCitiesResolved[j]);
						}
					}
				}

				/*
				{
					SortedListWriter w = new SortedListWriter();

					w.Add(cityToPublicationsCount, x => x.normalizedName);

					//w.WriteToFile("output_city_to_publications_count");
				}
				*/

				/*
				ReferencesResolver referencesResolver = new ReferencesResolver();

				List<PublicationWithReferences> withReferences = referencesResolver.ResolveAll(f);
				File.WriteAllText("output_publications_references.txt", JsonConvert.SerializeObject(withReferences, Formatting.Indented));
				referencesResolver.HandleUnresolved();

				foreach (PublicationWithReferences publicationWithReferences in withReferences)
				{
					WosFileFormatPublication publication1 = publicationWithReferences.publication;

					List<GeoCity> publication1_cities = resolvedCities.ResolveCitiesForPublication(publication1);

					foreach (WosFileFormatPublication publication2 in publicationWithReferences.resolved)
					{
						List<GeoCity> publication2_cities = resolvedCities.ResolveCitiesForPublication(publication2);

						foreach (GeoCity city1 in publication1_cities)
						{
							usedCities.Add(city1);

							foreach (GeoCity city2 in publication2_cities)
							{
								usedCities.Add(city2);

								usedConnections.Add(new Tuple<GeoCity, GeoCity>(city1, city2));
							}
						}
					}
				}
				*/

				//matrix.WriteCityToConnectionsCount();

				citiesResolver.LogTotals();

				statistics = new Statistics();
				foreach (Publication publication in publicationsFile.publications)
				{
					string country = CountryExtractor.GetCountry(publication, publicationsFile.Type);
					if (country != null)
					{
						GeoCity geoCity =  citiesResolver.Resolve_OrNull(country);
						if (geoCity != null)
						{
							statistics.AddPublicationInCountry(geoCity);

							string journal = JournalExtractor.GetJournal(publication, publicationsFile.Type);
							if (journal != null)
							{
								statistics.AddJournalInCountry(geoCity, journal);
							}
						}
					}
				}

				logger.Info("Done");
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while doing work.");
				throw;
			}
		}

		public void GetOutput_CollaborationKml(string filePath)
		{
			try
			{
				using (KmlWriter kmlWriter = new KmlWriter(filePath))
				{
					foreach (var city in matrix.GetCities())
					{
						kmlWriter.WriteCity(city.Key, 1 + city.Value * 2 / matrix.GetMaxCityValue());
					}

					foreach (var connection in matrix.GetConnections())
					{
						double width = 1 + (double)connection.Value * 2 / matrix.GetMaxConnectionValue();
						kmlWriter.WriteLine(connection.Key.Item1, connection.Key.Item2, width);
					}
				}
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while creating collaboration KML.");
				throw;
			}
		}

		public void GetOutput_PublicationsKml(string filePath)
		{
			try
			{
				statistics.WriteKml(statistics.countryToPublicationsCount, filePath);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while creating publications KML.");
				throw;
			}
		}

		public void GetOutput_PublicationsCsv(string filePath)
		{
			try
			{
				statistics.WriteCsv(statistics.countryToPublicationsCount, filePath);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while creating publications CSV.");
				throw;
			}
		}

		public void GetOutput_JournalsKml(string filePath)
		{
			try
			{
				statistics.WriteKml(CollectionUtils.SetDictToCountDict(statistics.countryToJournals), filePath);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while creating journals KML.");
				throw;
			}
		}

		public void GetOutput_JournalsCsv(string filePath)
		{
			try
			{
				statistics.WriteCsv(CollectionUtils.SetDictToCountDict(statistics.countryToJournals), filePath);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while creating journals CSV.");
				throw;
			}
		}

		public void GetOutput_KnownCitiesCsv(string filePath)
		{
			try
			{
				citiesResolver.HandleKnown(filePath);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while saving known cities.");
				throw;
			}
		}

		public void GetOutput_UnresolvedCities(string filePath)
		{
			try
			{
				citiesResolver.HandleUnresolved(filePath);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while saving unresolved cities.");
				throw;
			}
		}

		public void GetOutput_UnknownCities(string filePath)
		{
			try
			{
				citiesResolver.HandleUnknown(filePath);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while saving unknown cities.");
				throw;
			}
		}
	}
}

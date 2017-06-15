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
		private GeoCodingWithState geoCoding = new GeoCodingWithState();
		private CollaborationMatrix collaborations_byCity;
		private CollaborationMatrix collaborations_byCountry;
		private CollaborationMatrix collaborations_byInstitution;
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

		public void SetInput_KnownLocationsCsv(string filePath)
		{
			try
			{
				geoCoding.SetKnownLocations(filePath);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while loading known locations.");
				throw;
			}
		}

		public void SetInput_GeoCoderKey(string key)
		{
			try
			{
				geoCoding.SetGeoCoder(new GeoCoder(key));
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
				geoCoding.SetRetry(retry);
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
				collaborations_byCity = new CollaborationMatrix();
				collaborations_byCountry = new CollaborationMatrix();
				collaborations_byInstitution = new CollaborationMatrix();

				FieldsExtractor extractor = new FieldsExtractor(publicationsFile.Type);

				foreach (Publication publication in publicationsFile.publications)
				{
					AddCollaborations(collaborations_byCity, extractor.GetCities(publication));
					AddCollaborations(collaborations_byCountry, extractor.GetCountries(publication));
					AddCollaborations(collaborations_byInstitution, extractor.GetInstitutions(publication));
				}

				statistics = new Statistics();
				foreach (Publication publication in publicationsFile.publications)
				{
					string country = extractor.GetCountry(publication);
					string journal = extractor.GetJournal(publication);
					if (country != null)
					{
						GeoCodedLocation geoCodedCountry =  geoCoding.GeoCode_OrNull(country);

						if (geoCodedCountry != null)
						{
							statistics.AddPublicationInCountry(geoCodedCountry);

							if (journal != null)
							{
								statistics.AddJournalInCountry(geoCodedCountry, journal);
							}
						}
					}
				}

				geoCoding.LogTotals();

				logger.Info("Done");
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while doing work.");
				throw;
			}
		}

		public void GetOutput_CollaborationsByCityKml(string filePath)
		{
			try
			{
				collaborations_byCity.WriteKml(filePath);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while creating collaboration KML (by city).");
				throw;
			}
		}

		public void GetOutput_CollaborationsByCountryKml(string filePath)
		{
			try
			{
				collaborations_byCountry.WriteKml(filePath);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while creating collaboration KML (by country).");
				throw;
			}
		}
		public void GetOutput_CollaborationsByInstitutionKml(string filePath)
		{
			try
			{
				collaborations_byInstitution.WriteKml(filePath);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while creating collaboration KML (by institution).");
				throw;
			}
		}

		public void GetOutput_PublicationsKml(string filePath)
		{
			try
			{
				Statistics.WriteKml(statistics.countryToPublicationsCount, filePath);
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
				Statistics.WriteCsv(statistics.countryToPublicationsCount, filePath);
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
				Statistics.WriteKml(CollectionUtils.SetDictToCountDict(statistics.countryToJournals), filePath);
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
				Statistics.WriteCsv(CollectionUtils.SetDictToCountDict(statistics.countryToJournals), filePath);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while creating journals CSV.");
				throw;
			}
		}

		public void GetOutput_KnownLocationsCsv(string filePath)
		{
			try
			{
				geoCoding.HandleKnown(filePath);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while saving known locations.");
				throw;
			}
		}

		public void GetOutput_FailedLocations(string filePath)
		{
			try
			{
				geoCoding.HandleFailed(filePath);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while saving unresolved locations.");
				throw;
			}
		}

		public void GetOutput_UnknownLocations(string filePath)
		{
			try
			{
				geoCoding.HandleUnknown(filePath);
			}
			catch (Exception e)
			{
				logger.Error(e, "Error while saving unknown locations.");
				throw;
			}
		}

		private void AddCollaborations(CollaborationMatrix addTo, List<string> extractedLocations)
		{
			List<GeoCodedLocation> publicationCitiesResolved = extractedLocations.Select(x => geoCoding.GeoCode_OrNull(x)).Where(x => x != null).Distinct().ToList();

			for (int i = 0; i < publicationCitiesResolved.Count; ++i)
			{
				for (int j = i + 1; j < publicationCitiesResolved.Count; ++j)
				{
					addTo.AddConnection(publicationCitiesResolved[i], publicationCitiesResolved[j]);
				}
			}
		}
	}
}

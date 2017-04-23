
using NLog;
using NLog.Config;
using NLog.Targets;
using Scigeobib;

namespace ScigeobibCmd
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			var config = new LoggingConfiguration();
			Target target = new ConsoleTarget();
			config.AddTarget("console", target);
			config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, target));
			LogManager.Configuration = config;

			ScigeobibMain main = new ScigeobibMain();
			main.SetInput_Publications_ScopusCsv("input_scopus.csv");
			main.SetInput_Publications_WOS("input_wos.txt");
			main.SetInput_KnownLocationsCsv("input_known_cities.csv");
			main.SetInput_GeoCoderKey(null);
			main.SetInput_RetryGeoCoding(false);
			main.DoWork();
			main.GetOutput_CollaborationKml("output_collaborations.kml");
			main.GetOutput_PublicationsKml("output_publications.kml");
			main.GetOutput_PublicationsCsv("output_publications.csv");
			main.GetOutput_JournalsKml("output_journals.kml");
			main.GetOutput_JournalsCsv("output_journals.csv");
			main.GetOutput_KnownLocationsCsv("output_known_cities.csv");
			main.GetOutput_FailedLocations("output_unresolved_cities.txt");
			main.GetOutput_UnknownLocations("output_unknown_cities.txt");

		}
	}
}

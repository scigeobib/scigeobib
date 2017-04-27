
using System;
using System.Globalization;
using CommandLine;
using CommandLine.Text;
using NLog;
using NLog.Config;
using NLog.Targets;
using Scigeobib;

namespace ScigeobibCmd
{
	class Options
	{
		[Option("input", Required = true, HelpText = "WOS or Scopus CSV file with publications.")]
		public string Input { get; set; }

		[Option("known_locations", Required = false, HelpText = "Geo coded locations locations CSV.")]
		public string KnownLocationsCSV { get; set; }

		[Option("geo_coder_key", Required = false, HelpText = "Bing maps API key (geo code unknown locations using this).")]
		public string GeoCoderKey { get; set; }

		[Option("retry_geo_coding", Required = false, HelpText = "Retry geo coding of previously failed locations.")]
		public bool RetryGeoCoding { get; set; }

		[Option("verbose", HelpText = "Prints all messages to standard output.")]
		public bool Verbose { get; set; }

		[Option("output_collaborations_kml", Required = false, HelpText = "Collaborations KML.")]
		public string Output_Collaborations_Kml { get; set; }

		[Option("output_publications_kml", Required = false, HelpText = "Publications in countries - KML.")]
		public string Output_Publications_Kml { get; set; }

		[Option("output_publications_csv", Required = false, HelpText = "Publications in countries - CSV.")]
		public string Output_Publications_Csv { get; set; }

		[Option("output_journals_kml", Required = false, HelpText = "Journals in countries - KML.")]
		public string Output_Journals_Kml { get; set; }

		[Option("output_journals_csv", Required = false, HelpText = "Publications in countries - CSV.")]
		public string Output_Journals_Csv { get; set; }

		[Option("output_locations_known", Required = false, HelpText = "Geo coded locations CSV (known_locations + newly geo coded using geo_coder_key).")]
		public string Output_Locations_Known { get; set; }

		[Option("output_locations_failed", Required = false, HelpText = "List of geo coded locations which failed to geo code successfully.")]
		public string Output_Locations_Failed { get; set; }

		[Option("output_locations_unknown", Required = false, HelpText = "List of locations not geo coded yet.")]
		public string Output_Locations_Unknown { get; set; }

		[ParserState]
		public IParserState LastParserState { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this,
			  (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}

	class MainClass
	{
		public static void Main(string[] args)
		{
			var options = new Options();
			if (!CommandLine.Parser.Default.ParseArguments(args, options))
			{
				Environment.Exit(2);
			}

			var config = new LoggingConfiguration();
			Target target = new ConsoleTarget();
			config.AddTarget("console", target);
			config.LoggingRules.Add(new LoggingRule("*", options.Verbose ? LogLevel.Trace : LogLevel.Info, target));
			LogManager.Configuration = config;

			ScigeobibMain main = new ScigeobibMain();

			string extension = System.IO.Path.GetExtension(options.Input).ToUpper(CultureInfo.InvariantCulture);
			if (extension == ".CSV")
				main.SetInput_Publications_ScopusCsv(options.Input);
			else
				main.SetInput_Publications_WOS(options.Input);

			if (options.KnownLocationsCSV != null)
				main.SetInput_KnownLocationsCsv(options.KnownLocationsCSV);

			if (options.GeoCoderKey != null)
				main.SetInput_GeoCoderKey(options.GeoCoderKey);

			main.SetInput_RetryGeoCoding(options.RetryGeoCoding);
			main.DoWork();

			if (options.Output_Collaborations_Kml != null)
				main.GetOutput_CollaborationKml(options.Output_Collaborations_Kml);

			if (options.Output_Publications_Kml != null)
				main.GetOutput_PublicationsKml(options.Output_Publications_Kml);

			if (options.Output_Publications_Csv != null)
				main.GetOutput_PublicationsCsv(options.Output_Publications_Csv);

			if (options.Output_Journals_Kml != null)
				main.GetOutput_JournalsKml(options.Output_Journals_Kml);

			if (options.Output_Journals_Csv != null)
				main.GetOutput_JournalsCsv(options.Output_Journals_Csv);

			if (options.Output_Locations_Known != null)
				main.GetOutput_KnownLocationsCsv(options.Output_Locations_Known);

			if (options.Output_Locations_Failed != null)
				main.GetOutput_FailedLocations(options.Output_Locations_Failed);

			if (options.Output_Locations_Unknown != null)
				main.GetOutput_UnknownLocations(options.Output_Locations_Unknown);

		}
	}
}

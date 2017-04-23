using System;
using System.Linq;
using System.Globalization;
using System.Threading;
using Gtk;
using NLog;
using NLog.Config;
using NLog.Targets;
using Scigeobib;
using System.IO;

public partial class MainWindow : Gtk.Window
{
	private ScigeobibMain main;
	private bool complete;

	private Thread currentThread;

	private static TextView logTextView;

	private static TextTag tag_error;
	private static TextTag tag_warning;
	private static TextTag tag_info;

	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		Build();

		ChangeGui();

		logTextView = messages;

		var config = new LoggingConfiguration();
		MethodCallTarget target = new MethodCallTarget();
		target.ClassName = typeof(MainWindow).AssemblyQualifiedName;
		target.MethodName = "LogMethod";
		target.Parameters.Add(new MethodCallParameter("${level}"));
		target.Parameters.Add(new MethodCallParameter("${message}"));
		target.Parameters.Add(new MethodCallParameter("${exception:format=tostring}"));
		config.AddTarget("gui", target);
		config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, target));
		LogManager.Configuration = config;

		messages.SizeAllocated += new SizeAllocatedHandler(ScrollMessages);

		messages.Buffer.TagTable.Add(tag_error = new TextTag("error") { Foreground = "#ff0000" });
		messages.Buffer.TagTable.Add(tag_warning = new TextTag("warning") { Foreground = "#B26B70" });
		messages.Buffer.TagTable.Add(tag_info = new TextTag("info") { Foreground = "#0000ff" });
	}

	private void ScrollMessages(object sender, Gtk.SizeAllocatedArgs e)
	{
		messages.ScrollToIter(messages.Buffer.EndIter, 0, false, 0, 0);
	}

	public static void LogMethod(string level, string message, string exception)
	{
		string text = string.Format("{0} {1}\n", message, exception);

		Application.Invoke(delegate
		{
			var endIter = logTextView.Buffer.EndIter;

			TextTag tag = null;
			if (level == "Error") tag = tag_error;
			if (level == "Warn") tag = tag_warning;
			if (level == "Info") tag = tag_info;

			if (tag != null)
				logTextView.Buffer.InsertWithTags(ref endIter, text, new TextTag[] { tag  });
			else
				logTextView.Buffer.Insert(ref endIter, text);
		});
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();
		a.RetVal = true;
	}

	protected void OnInputPublicationsSelect(object sender, EventArgs e)
	{
		SelectInputFile(inputPublications, "*.txt;*.csv");
	}

	protected void OnInputKnownCitiesSelect(object sender, EventArgs e)
	{
		SelectInputFile(inputKnownCities, "*.txt;*.csv");
	}

	protected void OnButtonStart(object sender, EventArgs e)
	{
		messages.Buffer.Clear();
		complete = false;

		currentThread = new Thread(ThreadProc);
		currentThread.IsBackground = true;
		currentThread.Start();
		ChangeGui();
	}

	protected void OnButtonStop(object sender, EventArgs e)
	{
		currentThread.Abort();
	}

	private void ThreadProc()
	{
		try
		{
			main = new ScigeobibMain();

			string inputFilePath = inputPublications.Text;
			string extension = System.IO.Path.GetExtension(inputFilePath).ToUpper(CultureInfo.InvariantCulture);
			if (extension == ".CSV")
				main.SetInput_Publications_ScopusCsv(inputFilePath);
			else
				main.SetInput_Publications_WOS(inputFilePath);

			if (inputKnownCities.Text != "")
				main.SetInput_KnownCitiesCsv(inputKnownCities.Text);
			main.SetInput_GeoCoderKey(inputGeoCoderKey.Text != "" ? inputGeoCoderKey.Text : null);
			main.SetInput_RetryGeoCoding(inputRetryGeoCoding.State == StateType.Active);
			main.DoWork();

			complete = true;
		}
		catch
		{
		}
		finally
		{
			Application.Invoke(delegate
			{
				currentThread = null;
				ChangeGui();
			});
		}
	}

	private void ChangeGui()
	{
		bool running = (currentThread != null);

		frame_input.Sensitive = !running;
		frame_output.Sensitive = complete && !running;
		buttonStart.Sensitive = !running;
		buttonStop.Sensitive = running;
	}

	protected void OnOutputCollaborationKml(object sender, EventArgs e)
	{
		SelectOutputFile(main.GetOutput_CollaborationKml, "*.kml");
	}

	protected void OnOutputPublicationsKml(object sender, EventArgs e)
	{
		SelectOutputFile(main.GetOutput_PublicationsKml, "*.kml");
	}

	protected void OnOutputPublicationsCsv(object sender, EventArgs e)
	{
		SelectOutputFile(main.GetOutput_PublicationsCsv, "*.csv");
	}

	protected void OnOutputJournalsKml(object sender, EventArgs e)
	{
		SelectOutputFile(main.GetOutput_JournalsKml, "*.kml");
	}

	protected void OnOutputJournalsCsv(object sender, EventArgs e)
	{
		SelectOutputFile(main.GetOutput_JournalsCsv, "*.csv");
	}

	protected void OnOutputKnownCities(object sender, EventArgs e)
	{
		SelectOutputFile(main.GetOutput_KnownCitiesCsv, "*.csv");
	}

	protected void OnOutputUnresolvedCities(object sender, EventArgs e)
	{
		SelectOutputFile(main.GetOutput_UnresolvedCities, "*.txt");
	}

	protected void OnOutputUnknownCities(object sender, EventArgs e)
	{
		SelectOutputFile(main.GetOutput_UnknownCities, "*.txt");
	}

	private void SelectInputFile(Entry entry, string filter)
	{
		using (FileChooserDialog fc =
			new FileChooserDialog("Choose the file to open",
				this,
				FileChooserAction.Open,
				"Cancel", ResponseType.Cancel,
				"Open", ResponseType.Accept))
		{
			{
				FileFilter f = new FileFilter();
				f.Name = "Supported files";
				filter.Split(';').ToList().ForEach(x => f.AddPattern(x));
				fc.AddFilter(f);
			}

			{
				FileFilter f = new FileFilter();
				f.Name = "All files";
				f.AddPattern("*");
				fc.AddFilter(f);
			}

			if (fc.Run() == (int)ResponseType.Accept)
			{
				entry.Text = fc.Filename;
			}

			fc.Destroy();
		}
	}

	private void SelectOutputFile(Action<string> doWithFile, string filter)
	{
		using (FileChooserDialog fc =
			new FileChooserDialog("Choose the file to open",
				this,
				FileChooserAction.Save,
				"Cancel", ResponseType.Cancel,
				"Save", ResponseType.Accept))
		{
			FileFilter filterWithExtension = new FileFilter();
			{
				
				filterWithExtension.Name = filter;
				filterWithExtension.AddPattern(filter);
				fc.AddFilter(filterWithExtension);
			}

			{
				FileFilter f = new FileFilter();
				f.Name = "All files";
				f.AddPattern("*");
				fc.AddFilter(f);
			}

			while (true)
			{
				if (fc.Run() == (int)ResponseType.Accept)
				{
					string filename = fc.Filename;
					if (fc.Filter == filterWithExtension)
					{
						string requiredExtension = filter.TrimStart('*');
						if (System.IO.Path.GetExtension(filename).ToUpper(CultureInfo.InvariantCulture) != requiredExtension.ToUpper(CultureInfo.InvariantCulture))
						{
							filename += requiredExtension;
						}
					}
					if (File.Exists(filename))
					{
						MessageDialog md = new MessageDialog(null, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, "File already exists. Overwrite?");
						var response = md.Run();
						md.Destroy();
						if (response != (int)ResponseType.Yes)
						{
							continue;
						}
					}
					try
					{
						doWithFile(filename);

						MessageDialog md = new MessageDialog(null, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, "Open?");
						var response = md.Run();
						md.Destroy();
						if (response == (int)ResponseType.Yes)
						{
							try
							{
								System.Diagnostics.Process.Start(filename);
							}
							catch
							{
							}
						}
					}
					catch
					{
						MessageDialog md = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, "An error occurred. Check log messages for details.");
						md.Run();
						md.Destroy();
					}
				}

				break;
			}

			fc.Destroy();
		}
	}

	protected void OnOutputPJournalsKml(object sender, EventArgs e)
	{
	}
}

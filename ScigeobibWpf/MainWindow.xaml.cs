using NLog;
using NLog.Config;
using NLog.Targets;
using Scigeobib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScigeobibWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private ScigeobibMain main;
        private bool complete;

        private Thread currentThread;

        private static RichTextBox LogMessages;

        public MainWindow()
        {
            InitializeComponent();

            LogMessages = Messages;

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

            ChangeGui();
        }

        private static List<Tuple<string, string>> logTexts = new List<Tuple<string, string>>();

        private static Brush Brush_Other = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        private static Brush Brush_Error = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        private static Brush Brush_Warn = new SolidColorBrush(Color.FromRgb(0xB2, 0x6B, 0x70));
        private static Brush Brush_Info = new SolidColorBrush(Color.FromRgb(0, 0, 255));

        public static void LogMethod(string level, string message, string exception)
        {
            string formattedMessage = string.Format("{0} {1}\n", message, exception);
            lock (logTexts)
            {
                logTexts.Add(new Tuple<string, string>(formattedMessage, level));
            }

            Task.Delay(100).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(
                    new Action(
                        () => {
                            lock (logTexts)
                            {
                                if (logTexts.Count > 0)
                                {
                                    LogMessages.BeginChange();

                                    foreach (var item in logTexts)
                                    {
                                        Brush brush = Brush_Other;
                                        if (item.Item2 == "Error") brush = Brush_Error;
                                        if (item.Item2 == "Warn") brush = Brush_Warn;
                                        if (item.Item2 == "Info") brush = Brush_Info;

                                        TextRange tr = new TextRange(LogMessages.Document.ContentEnd, LogMessages.Document.ContentEnd);
                                        tr.Text = item.Item1;
                                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
                                    }

                                    LogMessages.EndChange();

                                    logTexts.Clear();

                                    LogMessages.ScrollToEnd();
                                }
                            }
                        })
                );
            });
        }

        private void ChangeGui()
        {
            bool running = (currentThread != null);

            GridInput.IsEnabled = !running;
            GridOutput.IsEnabled = complete && !running;
            ButtonStart.IsEnabled = !running;
            ButtonStop.IsEnabled = running;

            GridLogOutput.IsEnabled = !running;
        }

        private void InputPublicationsSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectInputFile(InputPublications, "All Supported Files (*.txt;*.csv)|*.txt;*.csv|TXT Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|All Files|*.*");
        }

        private void OutputCollaborationsByCityKmlSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectOutputFile(main.GetOutput_CollaborationsByCityKml, "KML Files|*.kml");
        }

        private void OutputCollaborationsByCountryKmlSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectOutputFile(main.GetOutput_CollaborationsByCountryKml, "KML Files|*.kml");
        }

        private void OutputCollaborationsByInstitutionKmlSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectOutputFile(main.GetOutput_CollaborationsByInstitutionKml, "KML Files|*.kml");
        }

        private void OutputJournalsKmlSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectOutputFile(main.GetOutput_JournalsKml, "KML Files|*.kml");
        }

        private void OutputJournalsCsvSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectOutputFile(main.GetOutput_JournalsCsv, "CSV Files|*.csv");
        }

        private void OutputPublicationsKmlSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectOutputFile(main.GetOutput_PublicationsKml, "KML Files|*.kml");
        }

        private void OutputPublicationsCsvSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectOutputFile(main.GetOutput_PublicationsCsv, "CSV Files|*.csv");
        }

        private void OutputLogTxtSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectOutputFile(SaveLog, "TXT Files|*.txt");
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            Messages.Document.Blocks.Clear();
            TextRange tr = new TextRange(LogMessages.Document.ContentEnd, LogMessages.Document.ContentEnd);
            tr.Text = " ";

            complete = false;

            Config config = new Config(InputPublications.Text, InputGeoCoderKey.Text);

            currentThread = new Thread(() => ThreadProc(config));
            currentThread.IsBackground = true;
            currentThread.Start();
            ChangeGui();
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            currentThread.Abort();
        }

        private class Config
        {
            public string FilePath { get; private set; }
            public string GeoCoderKey { get; private set; }

            internal Config(string filePath, string geoCoderKey)
            {
                FilePath = filePath;
                GeoCoderKey = geoCoderKey;
            }
        }

        private void ThreadProc(Config config)
        {
            try
            {
                if (config.FilePath == "")
                {
                    logger.Error("Missing Input File");
                    throw new Exception("Missing Input File");
                }

                if (config.GeoCoderKey == "")
                {
                    logger.Error("Missing Geocoder Key");
                    throw new Exception("Missing Geocoder Key");
                }

                main = new ScigeobibMain();

                string extension = System.IO.Path.GetExtension(config.FilePath).ToUpper(CultureInfo.InvariantCulture);
                if (extension == ".CSV")
                    main.SetInput_Publications_ScopusCsv(config.FilePath);
                else
                    main.SetInput_Publications_WOS(config.FilePath);

                main.SetInput_GeoCoderKey(config.GeoCoderKey);

                main.DoWork();

                complete = true;
            }
            catch
            {
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(
                    new Action(
                        () => {
                            currentThread = null;
                            ChangeGui();
                        })
                );
            }
        }

        private void SelectInputFile(TextBox textBox, string filter)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Filter = filter;

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                textBox.Text = dlg.FileName;
            }
        }

        private void SelectOutputFile(Action<string> SaveFileDialog, string filter)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.Filter = filter;

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                SaveFileDialog(dlg.FileName);
            }
        }

        private void SaveLog(string filePath)
        {
            File.WriteAllText(filePath, GetLogText(), Encoding.UTF8);
        }

        private string GetLogText()
        {
            return new TextRange(LogMessages.Document.ContentStart, LogMessages.Document.ContentEnd).Text;
        }

        private void InputPublications_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}

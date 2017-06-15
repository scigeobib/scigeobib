using System;
using Gtk;

namespace ScigeobibGtk
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Application.Init();
			MainWindow win = new MainWindow();
			if (!(args.Length >= 1) || args[0] != "advanced")
			{
				win.HideAdvanced();
			}
			win.Show();
			Application.Run();
		}
	}
}

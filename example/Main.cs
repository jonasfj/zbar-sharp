using System;
using Gtk;

/// <summary>
/// Example shows how to use the GtkZBar.Scanner widget
/// </summary>
namespace example
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();
			Application.Run ();
		}
	}
}

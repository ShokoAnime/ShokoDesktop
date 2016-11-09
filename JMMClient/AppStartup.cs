using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.IO;

namespace JMMClient
{
	public class AppStartup
	{
		[STAThread]
		static public void Main(string[] args)
		{
			try
			{
                App app = new App();
				app.InitializeComponent();
				app.Run();
			}
			catch (Exception ex)
			{
				File.WriteAllText(@"shoko_error.txt", ex.ToString());
				MessageBox.Show(ex.Message + "\r\r" + ex.StackTrace, "Application Exception", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Resources;
using System.Globalization;
using System.Threading;
using System.Security;

namespace JMMClient
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	/// 
	
	public partial class App : Application
	{
		public static ResourceManager ResGlobal = null;

		public App()
		{
			/*ResGlobal = new ResourceManager("JMMClient.Properties.Resources", typeof(App).Assembly);

			// Set application startup culture based on config settings
			string culture = AppSettings.Culture;

			CultureInfo ci = new CultureInfo(culture);
			Thread.CurrentThread.CurrentCulture = ci;
			Thread.CurrentThread.CurrentUICulture = ci;

			//string hello = ResGlobal.GetString("Favorite");*/
		}

	}
}

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
using NLog;

namespace JMMClient
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	/// 
	
	public partial class App : Application
	{
		public static ResourceManager ResGlobal = null;
        private static Logger logger = LogManager.GetCurrentClassLogger();

		public App()
		{
			/*ResGlobal = new ResourceManager("JMMClient.Properties.Resources", typeof(App).Assembly);

			// Set application startup culture based on config settings
			string culture = AppSettings.Culture;

			CultureInfo ci = new CultureInfo(culture);
			Thread.CurrentThread.CurrentCulture = ci;
			Thread.CurrentThread.CurrentUICulture = ci;

			//string hello = ResGlobal.GetString("Favorite");*/

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
		}

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            logger.ErrorException(e.Exception.ToString(), e.Exception);
            e.Handled = true;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
                logger.ErrorException(ex.ToString(), ex);
        }

	}
}

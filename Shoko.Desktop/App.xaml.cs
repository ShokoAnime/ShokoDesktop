using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using Shoko.Commons;
using Shoko.Commons.Downloads;
using Shoko.Desktop.ViewModel;

namespace Shoko.Desktop
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
            // Try to load culture info first, without it could fail UI startup
            try
            {
                ResGlobal = new ResourceManager("Shoko.Commons.Properties.Resources", typeof(App).Assembly);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured during App() culture info load: {ex}");
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            logger.Error(e.Exception, e.Exception.ToString());
            e.Handled = true;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                logger.Error(ex, ex.ToString());
        }

    }
}

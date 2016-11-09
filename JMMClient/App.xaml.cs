using NLog;
using System;
using System.IO;
using System.Resources;
using System.Windows;

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
            // Migrate programdata folder from JMMServer to ShokoServer
            // this needs to run before UnhandledExceptionManager.AddHandler(), because that will probably lock the log file
            if (!MigrateProgramDataLocation())
            {
                MessageBox.Show(JMMClient.Properties.Resources.Migration_ProgramDataError,
                    JMMClient.Properties.Resources.ShokoDesktop, MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }            /*ResGlobal = new ResourceManager("JMMClient.Properties.Resources", typeof(App).Assembly);

			// Set application startup culture based on config settings
			string culture = AppSettings.Culture;

			CultureInfo ci = new CultureInfo(culture);
			Thread.CurrentThread.CurrentCulture = ci;
			Thread.CurrentThread.CurrentUICulture = ci;

			//string hello = ResGlobal.GetString("Favorite");*/
            AppSettings.LoadSettings();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }
        public bool MigrateProgramDataLocation()
        {
            string oldApplicationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "JMMDesktop");
            string newApplicationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
            if (Directory.Exists(oldApplicationPath) && !Directory.Exists(newApplicationPath))
            {
                try
                {

                    Directory.Move(oldApplicationPath, newApplicationPath);
                    logger.Log(LogLevel.Info, "Successfully migrated programdata folder.");
                    return true;
                }
                catch (Exception e)
                {
                    logger.Log(LogLevel.Error, "Error occured during MigrateProgramDataLocation()");
                    logger.Error(e);
                    return false;
                }
            }

            return true;
        }

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            logger.Error(e.Exception, e.Exception.ToString());
            e.Handled = true;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
                logger.Error(ex, ex.ToString());
        }

    }
}

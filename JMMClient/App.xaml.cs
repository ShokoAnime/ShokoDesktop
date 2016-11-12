using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Resources;
using System.Windows;
using Microsoft.Win32;

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

            // First check if we have a settings.json in case migration had issues as otherwise might clear out existing old configurations
            string path = Path.Combine(AppSettings.ApplicationPath, "settings.json");
            if (File.Exists(path))
            {
                UninstallJMMDesktop();
            }

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
                    List<MigrationDirectory> migrationdirs = new List<MigrationDirectory>()
                    {
                        new MigrationDirectory
                        {
                            From = oldApplicationPath,
                            To = newApplicationPath
                        }
                    };

                    foreach (MigrationDirectory md in migrationdirs)
                    {
                        if (!md.SafeMigrate())
                        {
                            break;
                        }
                    }

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

        void UninstallJMMDesktop()
        {
            // Check in registry if installed
            string jmmDesktopUninstallPath =
                (string)
                    Registry.GetValue(
                        @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{AD24689F-020C-4C53-B649-99BB49ED6238}_is1",
                        "UninstallString", null);

            if (!string.IsNullOrEmpty(jmmDesktopUninstallPath))
            {
                // Ask if user wants to uninstall first
                MessageBoxResult dr =
                    MessageBox.Show(JMMClient.Properties.Resources.DuplicateInstallDetectedQuestion,
                        JMMClient.Properties.Resources.DuplicateInstallDetected, MessageBoxButton.YesNo);
                if (dr == MessageBoxResult.Yes)
                {
                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = jmmDesktopUninstallPath;
                        startInfo.Arguments = " /SILENT";
                        startInfo.CreateNoWindow = true;

                        Process p = Process.Start(startInfo);
                        p?.Start();

                        logger.Log(LogLevel.Info, "JMM Desktop successfully uninstalled");
                    }
                    catch
                        (Exception e)
                    {
                        logger.Log(LogLevel.Error, "Error occured during uninstall of JMM Desktop");
                    }
                }
                else
                {
                    logger.Log(LogLevel.Info, "User cancelled JMM Desktop uninstall");
                }
            }
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

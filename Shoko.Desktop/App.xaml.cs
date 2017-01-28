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
            // Migrate programdata folder from JMMServer to ShokoServer
            // this needs to run before UnhandledExceptionManager.AddHandler(), because that will probably lock the log file
            if (!MigrateProgramDataLocation())
            {
                MessageBox.Show(Shoko.Commons.Properties.Resources.Migration_ProgramDataError,
                    Shoko.Commons.Properties.Resources.ShokoDesktop, MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }

            logger.Info("App startup - Setting up culture");

            // Try to load culture info first, without it could fail UI startup
            try
            {
                ResGlobal = new ResourceManager("Shoko.Commons.Properties.Resources", typeof(App).Assembly);

                // Set application startup culture based on config settings
                string culture = AppSettings.Culture;

                CultureInfo ci = new CultureInfo(culture);
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured during App() culture info load: {ex}");
            }

            logger.Info("App startup - Culture set up");
            logger.Info("App startup - Loading settings...");
            FolderMappings.Instance.SetLoadAndSaveCallback(AppSettings.GetMappings,AppSettings.SetMappings);
            AppSettings.LoadSettings();
            logger.Info("App startup - Loaded settings");

            logger.Info("App startup - Checking for uninstall requirements");
            // First check if we have a settings.json in case migration had issues as otherwise might clear out existing old configurations
            if (!string.IsNullOrEmpty(AppSettings.ApplicationPath))
            {
              string path = Path.Combine(AppSettings.ApplicationPath, "settings.json");
              if (File.Exists(path))
              {
                Thread t = new Thread(UninstallJMMDesktop);
                t.IsBackground = true;
                t.Start();
              }
            }
            logger.Info("App startup - Checked uninstall requirements");
            logger.Info("App startup - Loading UI");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }
        public bool MigrateProgramDataLocation()
        {
            try
            {
                logger.Log(LogLevel.Info, "Checking to see if we have an old programdata folder for JMMDesktop");

                string oldApplicationPath =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "JMMDesktop");
                string newApplicationPath =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
                if (Directory.Exists(oldApplicationPath) && !Directory.Exists(newApplicationPath))
                {
                    logger.Log(LogLevel.Info, "Found old programdata folder for JMMDesktop and migrating");

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

                        logger.Log(LogLevel.Info, "Successfully migrated old programdata folder.");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, "Error occured during MigrateProgramDataLocation()");
                        logger.Error(ex);
                        return false;
                    }
                }
                else
                {
                    logger.Log(LogLevel.Info, "Found no old programdata folder for JMMDesktop");
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "Error occured during MigrateProgramDataLocation()");
                logger.Error(ex);
            }
            return true;
        }

        void UninstallJMMDesktop()
        {
          try
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
                MessageBox.Show(Shoko.Commons.Properties.Resources.DuplicateInstallDetectedQuestion,
                  Shoko.Commons.Properties.Resources.DuplicateInstallDetected, MessageBoxButton.YesNo);
              if (dr == MessageBoxResult.Yes)
              {
                try
                {
                  ProcessStartInfo startInfo = new ProcessStartInfo();
                  startInfo.FileName = jmmDesktopUninstallPath;
                  startInfo.Arguments = " /SILENT";
                  Process.Start(startInfo);

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
          catch (Exception ex)
          {
            logger.Log(LogLevel.Error, "Error occured during UninstallJMMDesktop()");
            logger.Error(ex);
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

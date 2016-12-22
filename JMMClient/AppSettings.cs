using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using JMMClient.UserControls.Settings;
using Microsoft.Win32;
using Newtonsoft.Json;
using NLog.Targets;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace JMMClient
{
    public class AppSettings
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static Dictionary<string, string> appSettings = new Dictionary<string, string>();

        private static string Get(string key)
        {
            if (appSettings.ContainsKey(key))
                return appSettings[key];
            return null;
        }

        private static void Set(string key, string value)
        {
            string orig = Get(key);
            if (value != orig)
            {
                appSettings[key] = value;
                SaveSettings();
            }
        }


        public static string DefaultInstance { get; set; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

        public static string ApplicationPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), DefaultInstance);

        public static string JMMServerPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), JMMServerInstance);

        public static string DefaultImagePath => Path.Combine(ApplicationPath, "images");

        public static string JMMServerImagePath
        {
            get
            {
                if (Directory.Exists(JMMServerPath) && File.Exists(Path.Combine(JMMServerPath, "settings.json")))
                {
                    Dictionary<string, string> serverSettings =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(JMMServerPath, "settings.json")));
                    if (serverSettings.ContainsKey("ImagesPath"))
                        return serverSettings["ImagesPath"];
                }
                return null;

            }
        }

        private static bool disabledSave = false;
        public static void SaveSettings()
        {
            if (disabledSave)
                return;
            lock (appSettings)
            {
                if (appSettings.Count <= 1)
                    return;//Somehow debugging may fuck up the settings so this shit will eject

                string path = Path.Combine(ApplicationPath, "settings.json");
                File.WriteAllText(path, JsonConvert.SerializeObject(appSettings));
            }
        }
        /*
                string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string logName = System.IO.Path.Combine(appPath, "AnimeEpisodes.txt");
                */
        public static void LoadSettings()
        {
            try
            {
                //Reconfigure log file to applicationpath
                var target = (FileTarget)LogManager.Configuration.FindTargetByName("file");
                target.FileName = ApplicationPath + "/logs/${shortdate}.txt";
                LogManager.ReconfigExistingLoggers();


                disabledSave = true;
                string programlocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                List<MigrationDirectory> migrationdirs = new List<MigrationDirectory>()
                {
                    new MigrationDirectory
                    {
                        From = Path.Combine(programlocation, "logs"),
                        To = Path.Combine(ApplicationPath, "logs")
                    }
                };

                string path = Path.Combine(ApplicationPath, "settings.json");
                if (File.Exists(path))
                {
                    appSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
                }
                else
                {
                    LoadLegacySettingsFromFile(true);

                }

                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);
                if (BaseImagesPathIsDefault || !Directory.Exists(BaseImagesPath))
                {
                    migrationdirs.Add(new MigrationDirectory
                    {
                        From = Path.Combine(programlocation, "images"),
                        To = DefaultImagePath
                    });
                }
                else if (Directory.Exists(BaseImagesPath))
                {
                    ImagesPath = BaseImagesPath;
                }
                bool migrate = !Directory.Exists(ApplicationPath) || File.Exists(Path.Combine(programlocation, "AnimeEpisodes.txt"));
                foreach (MigrationDirectory m in migrationdirs)
                {
                    if (m.ShouldMigrate)
                    {
                        migrate = true;
                        break;
                    }
                }
                if (migrate)
                {
                    if (!Utils.IsAdministrator())
                    {
                        MessageBox.Show(Properties.Resources.Migration_AdminFail, Properties.Resources.Migration_Header,
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        Application.Current.Shutdown();
                        return;
                    }
                    Migration m = null;
                    try
                    {
                        m = new Migration($"{Properties.Resources.Migration_AdminPass1} {ApplicationPath}, {Properties.Resources.Migration_AdminPass2}");
	                    m.Show();
	                    if (!Directory.Exists(ApplicationPath))
                        {
                            Directory.CreateDirectory(ApplicationPath);
                        }
                        Utils.GrantAccess(ApplicationPath);
                        disabledSave = false;
                        SaveSettings();

	                    foreach (MigrationDirectory md in migrationdirs)
                        {
                            if (!md.SafeMigrate())
                            {
                                break;
                            }
                        }
                        if (File.Exists(Path.Combine(programlocation, "AnimeEpisodes.txt")))
                        {
                            File.Move(Path.Combine(programlocation, "AnimeEpisodes.txt"), AnimeEpisodesText);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(Properties.Resources.Migration_Error + " ", e.ToString());
	                    logger.Error(e, "Error occured during LoadSettings: {0}", e.ToString());
                    }

                    m?.Close();
                    Thread.Sleep(5000);
	                Application.Current.Shutdown();
	                return;
                }
                disabledSave = false;

                if (Directory.Exists(BaseImagesPath) && string.IsNullOrEmpty(ImagesPath))
                {
                    ImagesPath = BaseImagesPath;
                }
                if (string.IsNullOrEmpty(ImagesPath))
                {
                    if (string.IsNullOrEmpty(JMMServerImagePath))
                        ImagesPath = DefaultImagePath;
                    else
                        ImagesPath = JMMServerImagePath;
                }
                SaveSettings();


            }
            catch (Exception e)
            {
	            logger.Error(e, "Error occured during LoadSettings: {0}", e.ToString());
	            MessageBox.Show(Properties.Resources.Migration_LoadError + " ", e.ToString());
                Application.Current.Shutdown();
                return;
            }
        }

        public static void LoadLegacySettingsFromFile(bool locateAutomatically)
        {
            try
            {
                string configFile = "";
                if (locateAutomatically)
                {
                    // First try to locate it from old JMM Desktop installer entry
                    string jmmDesktopInstallLocation = (string) Registry.GetValue(
                        @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{AD24689F-020C-4C53-B649-99BB49ED6238}_is1",
                        "InstallLocation", null);

                    if (!string.IsNullOrEmpty(jmmDesktopInstallLocation))
                    {
                        configFile = Path.Combine(jmmDesktopInstallLocation, "JMMDesktop.exe.config");
                    }

                    // Try to locate old config if we don't have new format one (JSON) in several locations
                    if (!File.Exists(configFile))
                        configFile = @"C:\Program Files (x86)\JMM\JMM Desktop\JMMDesktop.exe.config";

                    if (!File.Exists(configFile))
                        configFile = @"C:\Program Files (x86)\JMM Desktop\JMMDesktop.exe.config";
                    if (!File.Exists(configFile))
                        configFile = "JMMDesktop.exe.config";
                    if (!File.Exists(configFile))
                        configFile = "old.config";
                }

                // Ask user if they want to find config manually
                if (!File.Exists(configFile))
                    configFile = LocateLegacyConfigFile();

	            if (!File.Exists(configFile))
	            {
					// first run or cancelled file selection
		            // Load default settings as otherwise will fail to start entirely
		            var col = ConfigurationManager.AppSettings;
		            appSettings = col.AllKeys.ToDictionary(a => a, a => col[a]);
		            logger.Log(LogLevel.Error, string.Format("Settings file was not selected, using default."));
		            return;
	            }

	            if (configFile.ToLower().Contains("settings.json"))
                {
                    appSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(configFile));
                }
                else
                {
                    // Store default settings for later use
                    var colDefault = ConfigurationManager.AppSettings;
                    var appSettingDefault = colDefault.AllKeys.ToDictionary(a => a, a => colDefault[a]);

                    var col = GetNameValueCollectionSection("appSettings", configFile);

                    // if old settings found store and replace with new ShokoServer naming if needed
                    // else fallback on current one we have
                    if (col.Count > 0)
                    {
                        appSettings.Clear();
                        Dictionary<string, string> appSettingsBeforeRename = col.AllKeys.ToDictionary(a => a,
                            a => col[a]);
                        foreach (var setting in appSettingsBeforeRename)
                        {
                            if (!string.IsNullOrEmpty(setting.Value))
                            {
                                string newKey = setting.Key.Replace("JMMServer", "ShokoServer");
                                appSettings.Add(newKey, setting.Value);
                            }
                        }

                        // Check if we missed any setting keys and re-add from stock one
                        foreach (var setting in appSettingDefault)
                        {
                            if (!string.IsNullOrEmpty(setting.Value))
                            {
                                if (!appSettings.ContainsKey(setting.Key))
                                {
                                    string newKey = setting.Key.Replace("JMMServer", "ShokoServer");
                                    appSettings.Add(newKey, setting.Value);
                                }
                            }
                        }
                    }
                    else
                    {
                        col = ConfigurationManager.AppSettings;
                        appSettings = col.AllKeys.ToDictionary(a => a, a => col[a]);
                    }
                }
            }
            catch (Exception e)
            {
                // Load default settings as otherwise will fail to start entirely
                var col = ConfigurationManager.AppSettings;
                appSettings = col.AllKeys.ToDictionary(a => a, a => col[a]);
                logger.Log(LogLevel.Error, string.Format("Error occured during LoadSettingsManuallyFromFile: {0}", e.Message));
            }
        }

        public static string LocateLegacyConfigFile()
        {
            string configPath = "";
            MessageBoxResult dr = MessageBox.Show(Properties.Resources.LocateSettingsFileQuestion, Properties.Resources.LocateSettingsFile, MessageBoxButton.YesNo);
            switch (dr)
            {
                case MessageBoxResult.Yes:
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "JMM config|JMMDesktop.exe.config;settings.json";

                    DialogResult browseFile = openFileDialog.ShowDialog();
                    if (browseFile == DialogResult.OK && !string.IsNullOrEmpty(openFileDialog.FileName.Trim()))
                    {
                        configPath = openFileDialog.FileName;
                    }

                    break;
            }

            return configPath;
        }

        public static string AnimeEpisodesText
        {
            get
            {
                string dir = Get("AnimeEpisodesText");
                if (string.IsNullOrEmpty(dir))
                {
                    dir = Path.Combine(ApplicationPath, "AnimeEpisodes.txt");
                    Set("AnimeEpisodesText", dir);
                }
                return dir;
            }
            set
            {
                Set("AnimeEpisodesText", value);
            }
        }

        public static string Culture
        {
            get
            {
                
                string cult = Get("Culture");
                if (!string.IsNullOrEmpty(cult))
                    return cult;
                else
                {
                    // default value
                    cult = "en";
                    Set("Culture", cult);
                    return cult;
                }
            }
            set
            {
                Set("Culture", value);
            }
        }

        public static string JMMServerInstance
        {
            get
            {
                string instance = Get("JMMServerInstance");
                if (string.IsNullOrEmpty(instance))
                {
                    instance = "ShokoServer";
                    JMMServerInstance = instance;
                }
                return instance;
            }
            set
            {
                Set("JMMServerInstance",value);
            }
        }

        public static AvailableEpisodeType Episodes_Availability
        {
            get
            {
                
                int val = 1;
                if (int.TryParse(Get("Episodes_Availability"), out val))
                    return (AvailableEpisodeType)val;
                else
                    return AvailableEpisodeType.All; // default value
            }
            set
            {
                Set("Episodes_Availability", ((int)value).ToString());
            }
        }

        public static WatchedStatus Episodes_WatchedStatus
        {
            get
            {
                
                int val = 1;
                if (int.TryParse(Get("Episodes_WatchedStatus"), out val))
                    return (WatchedStatus)val;
                else
                    return WatchedStatus.All; // default value
            }
            set
            {
                Set("Episodes_WatchedStatus", ((int)value).ToString());
            }
        }

        public static string ImagesPath
        {
            get
            {
                
                return Get("ImagesPath");
            }
            set
            {
                Set("ImagesPath", value);
                JMMServerVM.Instance.BaseImagePath = Utils.GetBaseImagesPath();
            }
        }

        private static string BaseImagesPath
        {
            get
            {
                
                return Get("BaseImagesPath");
            }
            set
            {
                Set("BaseImagesPath", value);
                JMMServerVM.Instance.BaseImagePath = Utils.GetBaseImagesPath();
            }
        }
        private static bool BaseImagesPathIsDefault
        {
            get
            {
                string basePath = Get("BaseImagesPathIsDefault");
                if (!string.IsNullOrEmpty(basePath))
                {
                    bool val = true;
                    bool.TryParse(basePath, out val);
                    return val;
                }
                else return true;

            }

        }

        public static string JMMServer_Address
        {
            get
            {
                

                string val = Get("ShokoServer_Address");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "127.0.0.1";
                    Set("ShokoServer_Address", val);
                }
                return val;
            }
            set
            {
                Set("ShokoServer_Address", value);
            }
        }

        public static string JMMServer_Port
        {
            get
            {
                

                string val = Get("ShokoServer_Port");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "8111";
                    Set("ShokoServer_Port", val);
                }
                return val;
            }
            set
            {
                Set("ShokoServer_Port", value);
            }
        }

        public static string JMMServer_FilePort
        {
            get
            {
                

                string val = Get("ShokoServer_FilePort");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "8112";
                    Set("ShokoServer_FilePort", val);
                }
                return val;
            }
            set
            {
                Set("ShokoServer_FilePort", value);
            }
        }

        public static int DisplayHeight_GroupList
        {
            get
            {
                
                string val = Get("DisplayHeight_GroupList");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival > 30 && ival < 300)
                        return ival;
                    else
                        return 80;
                }
                else
                    return 80; // default value
            }
            set
            {
                Set("DisplayHeight_GroupList", value.ToString());
            }
        }

        public static int PlaylistHeader_Image_Height
        {
            get
            {
                
                string val = Get("PlaylistHeader_Image_Height");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival > 30 && ival < 400)
                        return ival;
                    else
                        return 200;
                }
                else
                    return 200; // default value
            }
            set
            {
                Set("PlaylistHeader_Image_Height", value.ToString());
            }
        }

        public static int PlaylistItems_Image_Height
        {
            get
            {
                
                string val = Get("PlaylistItems_Image_Height");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival > 30 && ival < 400)
                        return ival;
                    else
                        return 130;
                }
                else
                    return 130; // default value
            }
            set
            {
                Set("PlaylistItems_Image_Height", value.ToString());
            }
        }

        public static int PlaylistEpisode_Image_Width
        {
            get
            {
                
                string val = Get("PlaylistEpisode_Image_Width");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival > 10 && ival < 400)
                        return ival;
                    else
                        return 120;
                }
                else
                    return 120; // default value
            }
            set
            {
                Set("PlaylistEpisode_Image_Width", value.ToString());
            }
        }

        public static bool PlaylistItems_ShowDetails
        {
            get
            {
                
                string val = Get("PlaylistItems_ShowDetails");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("PlaylistItems_ShowDetails", value.ToString());
            }
        }

        public static int DisplayHeight_SeriesInfo
        {
            get
            {
                
                string val = Get("DisplayHeight_SeriesInfo");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival > 30 && ival < 300)
                        return ival;
                    else
                        return 200;
                }
                else
                    return 200; // default value
            }
            set
            {
                Set("DisplayHeight_SeriesInfo", value.ToString());
            }
        }

        public static int DisplayWidth_EpisodeImage
        {
            get
            {
                
                string val = Get("DisplayWidth_EpisodeImage");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival > 10 && ival < 400)
                        return ival;
                    else
                        return 120;
                }
                else
                    return 120; // default value
            }
            set
            {
                Set("DisplayWidth_EpisodeImage", value.ToString());
            }
        }

        public static int DisplayStyle_GroupList
        {
            get
            {
                
                string val = Get("DisplayStyle_GroupList");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival >= 1 && ival <= 2)
                        return ival;
                    else
                        return 1;
                }
                else
                    return 1; // default value
            }
            set
            {
                Set("DisplayStyle_GroupList", value.ToString());
            }
        }
        public static int DefaultPlayer_GroupList
        {
            get
            {
                
                string val = Get("DefaultPlayer_GroupList");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    return ival;
                }
                else
                    return (int)VideoPlayer.WindowsDefault; // default value
            }
            set
            {
                Set("DefaultPlayer_GroupList", value.ToString());
            }
        }

        public static int DisplayHeight_DashImage
        {
            get
            {
                
                string val = Get("DisplayHeight_DashImage");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival < 30)
                        return 30;

                    if (ival > 300)
                        return 300;

                    return ival;
                }
                else
                {
                    return 150; // default value
                }
            }
            set
            {
                Set("DisplayHeight_DashImage", value.ToString());
            }
        }

        public static int EpisodeImageOverviewStyle
        {
            get
            {
                
                string val = Get("EpisodeImageOverviewStyle");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival >= 1 && ival <= 3)
                        return ival;
                    else
                        return 1;
                }
                else
                    return 1; // default value
            }
            set
            {
                Set("EpisodeImageOverviewStyle", value.ToString());
            }
        }

        public static bool HideEpisodeImageWhenUnwatched
        {
            get
            {
                
                string val = Get("HideEpisodeImageWhenUnwatched");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("HideEpisodeImageWhenUnwatched", value.ToString());
            }
        }

        public static bool HideEpisodeOverviewWhenUnwatched
        {
            get
            {
                
                string val = Get("HideEpisodeOverviewWhenUnwatched");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("HideEpisodeOverviewWhenUnwatched", value.ToString());
            }
        }

        public static bool HideDownloadButtonWhenFilesExist
        {
            get
            {
                
                string val = Get("HideDownloadButtonWhenFilesExist");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("HideDownloadButtonWhenFilesExist", value.ToString());
            }
        }

        public static bool DisplayRatingDialogOnCompletion
        {
            get
            {
                
                string val = Get("DisplayRatingDialogOnCompletion");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return true; // default value
            }
            set
            {
                Set("DisplayRatingDialogOnCompletion", value.ToString());
            }
        }

        public static bool DisplaySeriesSimple
        {
            get
            {
                
                string val = Get("DisplaySeriesSimple");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return true; // default value
            }
            set
            {
                Set("DisplaySeriesSimple", value.ToString());
            }
        }

        public static bool UseFanartOnSeries
        {
            get
            {
                
                string val = Get("UseFanartOnSeries");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return true; // default value
            }
            set
            {
                Set("UseFanartOnSeries", value.ToString());
            }
        }

        public static bool AlwaysUseAniDBPoster
        {
            get
            {
                
                string val = Get("AlwaysUseAniDBPoster");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return true; // default value
            }
            set
            {
                Set("AlwaysUseAniDBPoster", value.ToString());
            }
        }

        public static bool UseFanartOnPlaylistHeader
        {
            get
            {
                
                string val = Get("UseFanartOnPlaylistHeader");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return true; // default value
            }
            set
            {
                Set("UseFanartOnPlaylistHeader", value.ToString());
            }
        }

        public static bool UseFanartOnPlaylistItems
        {
            get
            {
                
                string val = Get("UseFanartOnPlaylistItems");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("UseFanartOnPlaylistItems", value.ToString());
            }
        }

        public static string SeriesWidgetsOrder
        {
            get
            {
                
                string val = Get("SeriesWidgetsOrder");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "5;2;1;6;4;3;7";
                    Set("SeriesWidgetsOrder", val);
                }

                // make sure the setting contains all the widgets
                // just in case the user has manually edited the config, or is using an old config
                string[] widgets = val.Split(';');
                int maxEnum = 7;
                for (int i = 1; i <= maxEnum; i++)
                {

                    if (!widgets.Contains(i.ToString()))
                    {
                        if (val.Length > 0) val += ";";
                        val += i.ToString();
                    }
                }

                return val;
            }
            set
            {
                Set("SeriesWidgetsOrder", value);
            }
        }

        public static string DashboardWidgetsOrder
        {
            get
            {
                
                string val = Get("DashboardWidgetsOrder");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "1;2;3;4;5;7;8";
                    Set("DashboardWidgetsOrder", val);
                }

                // make sure the setting contains all the widgets
                // just in case the user has manually edited the config, or is using an old config
                string[] widgets = val.Split(';');
                int maxEnum = 8;
                for (int i = 1; i <= maxEnum; i++)
                {
                    // skip Trakt as this has been deprecated
                    DashboardWidgets sectionType = (DashboardWidgets)i;
                    if (sectionType == DashboardWidgets.TraktFriends) continue;

                    if (!widgets.Contains(i.ToString()))
                    {
                        if (val.Length > 0) val += ";";
                        val += i.ToString();
                    }
                }

                return val;
            }
            set
            {
                Set("DashboardWidgetsOrder", value);
            }
        }

        public static string MissingEpsExportColumns
        {
            get
            {
                
                string val = Get("MissingEpsExportColumns");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "1;1;1;1;1;1;1;1";
                    Set("MissingEpsExportColumns", val);
                }

                // make sure the setting contains all the columns
                // just in case the user has manually edited the config, or is using an old config
                string[] columns = val.Split(';');
                if (columns.Length != 8)
                {
                    val = "1;1;1;1;1;1;1;1";
                    Set("MissingEpsExportColumns", val);
                }

                return val;
            }
            set
            {
                Set("MissingEpsExportColumns", value);
            }
        }

        public static bool SeriesTvDBLinksExpanded
        {
            get
            {
                
                string val = Get("SeriesTvDBLinksExpanded");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("SeriesTvDBLinksExpanded", value.ToString());
            }
        }


        public static bool TitlesExpanded
        {
            get
            {
                
                string val = Get("TitlesExpanded");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("TitlesExpanded", value.ToString());
            }
        }

        public static bool TagsExpanded
        {
            get
            {
                
                string val = Get("TagsExpanded");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("TagsExpanded", value.ToString());
            }
        }

        public static bool CustomTagsExpanded
        {
            get
            {
                
                string val = Get("CustomTagsExpanded");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("CustomTagsExpanded", value.ToString());
            }
        }

        public static bool WindowFullScreen
        {
            get
            {
                
                string val = Get("WindowFullScreen");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("WindowFullScreen", value.ToString());
            }
        }

        public static bool WindowNormal
        {
            get
            {
                
                string val = Get("WindowNormal");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return true; // default value
            }
            set
            {
                Set("WindowNormal", value.ToString());
            }
        }

        public static bool SeriesNextEpisodeExpanded
        {
            get
            {
                
                string val = Get("SeriesNextEpisodeExpanded");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("SeriesNextEpisodeExpanded", value.ToString());
            }
        }

        public static bool SeriesGroupExpanded
        {
            get
            {
                
                string val = Get("SeriesGroupExpanded");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("SeriesGroupExpanded", value.ToString());
            }
        }

        public static bool DashWatchNextEpExpanded
        {
            get
            {
                
                string val = Get("DashWatchNextEpExpanded");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return true; // default value
            }
            set
            {
                Set("DashWatchNextEpExpanded", value.ToString());
            }
        }

        public static bool DashRecentlyWatchEpsExpanded
        {
            get
            {
                
                string val = Get("DashRecentlyWatchEpsExpanded");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("DashRecentlyWatchEpsExpanded", value.ToString());
            }
        }

        public static bool DashSeriesMissingEpisodesExpanded
        {
            get
            {
                
                string val = Get("DashSeriesMissingEpisodesExpanded");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("DashSeriesMissingEpisodesExpanded", value.ToString());
            }
        }

        public static bool DashMiniCalendarExpanded
        {
            get
            {
                
                string val = Get("DashMiniCalendarExpanded");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("DashMiniCalendarExpanded", value.ToString());
            }
        }

        public static bool DashRecommendationsWatchExpanded
        {
            get
            {
                
                string val = Get("DashRecommendationsWatchExpanded");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("DashRecommendationsWatchExpanded", value.ToString());
            }
        }

        public static bool DashRecommendationsDownloadExpanded
        {
            get
            {
                
                string val = Get("DashRecommendationsDownloadExpanded");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("DashRecommendationsDownloadExpanded", value.ToString());
            }
        }

        public static bool DashRecentAdditionsExpanded
        {
            get
            {
                
                string val = Get("DashRecentAdditionsExpanded");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("DashRecentAdditionsExpanded", value.ToString());
            }
        }

        public static int DashRecentAdditionsType
        {
            get
            {
                
                string val = Get("DashRecentAdditionsType");
                int bval = 0;
                if (int.TryParse(val, out bval))
                    return bval;
                else
                    return 0; // default value
            }
            set
            {
                Set("DashRecentAdditionsType", value.ToString());
            }
        }

        public static bool DashTraktFriendsExpanded
        {
            get
            {
                
                string val = Get("DashTraktFriendsExpanded");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return true; // default value
            }
            set
            {
                Set("DashTraktFriendsExpanded", value.ToString());
            }
        }

        public static int Dash_WatchNext_Items
        {
            get
            {
                
                string val = Get("Dash_WatchNext_Items");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival >= 0 && ival <= 100)
                        return ival;
                    else
                        return 10;
                }
                else
                    return 10; // default value
            }
            set
            {
                Set("Dash_WatchNext_Items", value.ToString());
            }
        }

        public static int Dash_RecentAdditions_Items
        {
            get
            {
                
                string val = Get("Dash_RecentAdditions_Items");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival >= 0 && ival <= 100)
                        return ival;
                    else
                        return 10;
                }
                else
                    return 10; // default value
            }
            set
            {
                Set("Dash_RecentAdditions_Items", value.ToString());
            }
        }

        public static int Dash_WatchNext_Height
        {
            get
            {
                
                string val = Get("Dash_WatchNext_Height");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival < 30)
                        return 30;

                    if (ival > 300)
                        return 300;

                    return ival;
                }
                else
                {
                    return 150; // default value
                }
            }
            set
            {
                Set("Dash_WatchNext_Height", value.ToString());
            }
        }

        public static int Dash_RecentAdditions_Height
        {
            get
            {
                
                string val = Get("Dash_RecentAdditions_Height");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival < 30)
                        return 30;

                    if (ival > 300)
                        return 300;

                    return ival;
                }
                else
                {
                    return 150; // default value
                }
            }
            set
            {
                Set("Dash_RecentAdditions_Height", value.ToString());
            }
        }

        public static DashWatchNextStyle Dash_WatchNext_Style
        {
            get
            {
                
                int val = 1;
                if (int.TryParse(Get("Dash_WatchNext_Style"), out val))
                    return (DashWatchNextStyle)val;
                else
                    return DashWatchNextStyle.Detailed; // default value
            }
            set
            {
                Set("Dash_WatchNext_Style", ((int)value).ToString());
            }
        }







        public static int Dash_RecentlyWatchedEp_Items
        {
            get
            {
                
                string val = Get("Dash_RecentlyWatchedEp_Items");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival >= 0 && ival <= 100)
                        return ival;
                    else
                        return 10;
                }
                else
                    return 10; // default value
            }
            set
            {
                Set("Dash_RecentlyWatchedEp_Items", value.ToString());
            }
        }

        public static int Dash_RecentlyWatchedEp_Height
        {
            get
            {
                
                string val = Get("Dash_RecentlyWatchedEp_Height");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival < 30)
                        return 30;

                    if (ival > 300)
                        return 300;

                    return ival;
                }
                else
                {
                    return 150; // default value
                }
            }
            set
            {
                Set("Dash_RecentlyWatchedEp_Height", value.ToString());
            }
        }


        public static int Dash_MissingEps_Items
        {
            get
            {
                
                string val = Get("Dash_MissingEps_Items");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival >= 0 && ival <= 100)
                        return ival;
                    else
                        return 10;
                }
                else
                    return 10; // default value
            }
            set
            {
                Set("Dash_MissingEps_Items", value.ToString());
            }
        }

        public static int Dash_MissingEps_Height
        {
            get
            {
                
                string val = Get("Dash_MissingEps_Height");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival < 30)
                        return 30;

                    if (ival > 300)
                        return 300;

                    return ival;
                }
                else
                {
                    return 150; // default value
                }
            }
            set
            {
                Set("Dash_MissingEps_Height", value.ToString());
            }
        }

        public static int Dash_MiniCalendarDays
        {
            get
            {
                
                string val = Get("Dash_MiniCalendarDays");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival >= 0 && ival <= 100)
                        return ival;
                    else
                        return 10;
                }
                else
                    return 10; // default value
            }
            set
            {
                Set("Dash_MiniCalendarDays", value.ToString());
            }
        }

        public static bool Dash_MiniCalendarUpcomingOnly
        {
            get
            {
                
                string val = Get("Dash_MiniCalendarUpcomingOnly");
                bool bval = false;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return true; // default value
            }
            set
            {
                Set("Dash_MiniCalendarUpcomingOnly", value.ToString());
            }
        }

        public static int Dash_MiniCalendar_Height
        {
            get
            {
                
                string val = Get("Dash_MiniCalendar_Height");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival < 30)
                        return 30;

                    if (ival > 300)
                        return 300;

                    return ival;
                }
                else
                {
                    return 150; // default value
                }
            }
            set
            {
                Set("Dash_MiniCalendar_Height", value.ToString());
            }
        }

        public static int Dash_RecWatch_Height
        {
            get
            {
                
                string val = Get("Dash_RecWatch_Height");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival < 30)
                        return 30;

                    if (ival > 300)
                        return 300;

                    return ival;
                }
                else
                {
                    return 150; // default value
                }
            }
            set
            {
                Set("Dash_RecWatch_Height", value.ToString());
            }
        }

        public static int Dash_RecWatch_Items
        {
            get
            {
                
                string val = Get("Dash_RecWatch_Items");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival >= 0 && ival <= 100)
                        return ival;
                    else
                        return 10;
                }
                else
                    return 10; // default value
            }
            set
            {
                Set("Dash_RecWatch_Items", value.ToString());
            }
        }

        public static int Dash_RecDownload_Height
        {
            get
            {
                
                string val = Get("Dash_RecDownload_Height");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival < 30)
                        return 30;

                    if (ival > 300)
                        return 300;

                    return ival;
                }
                else
                {
                    return 150; // default value
                }
            }
            set
            {
                Set("Dash_RecDownload_Height", value.ToString());
            }
        }

        public static int Dash_RecDownload_Items
        {
            get
            {
                
                string val = Get("Dash_RecDownload_Items");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival >= 0 && ival <= 100)
                        return ival;
                    else
                        return 10;
                }
                else
                    return 10; // default value
            }
            set
            {
                Set("Dash_RecDownload_Items", value.ToString());
            }
        }

        public static int Dash_TraktFriends_Height
        {
            get
            {
                
                string val = Get("Dash_TraktFriends_Height");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival < 30)
                        return 30;

                    if (ival > 300)
                        return 300;

                    return ival;
                }
                else
                {
                    return 150; // default value
                }
            }
            set
            {
                Set("Dash_TraktFriends_Height", value.ToString());
            }
        }

        public static int Dash_TraktFriends_Items
        {
            get
            {
                
                string val = Get("Dash_TraktFriends_Items");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival >= 0 && ival <= 100)
                        return ival;
                    else
                        return 10;
                }
                else
                    return 10; // default value
            }
            set
            {
                Set("Dash_TraktFriends_Items", value.ToString());
            }
        }

        public static bool Dash_TraktFriends_AnimeOnly
        {
            get
            {
                
                string val = Get("Dash_TraktFriends_AnimeOnly");
                bool bval = false;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("Dash_TraktFriends_AnimeOnly", value.ToString());
            }
        }

        public static bool AutoStartLocalJMMServer
        {
            get
            {
                
                string val = Get("AutoStartLocalJMMServer");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; //default value
            }
            set
            {
                Set("AutoStartLocalJMMServer", value.ToString());
            }
        }

        public static int SeriesGroup_Image_Height
        {
            get
            {
                
                string val = Get("SeriesGroup_Image_Height");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival < 80)
                        return 80;

                    if (ival > 400)
                        return 400;

                    return ival;
                }
                else
                {
                    return 150; // default value
                }
            }
            set
            {
                Set("SeriesGroup_Image_Height", value.ToString());
            }
        }

        public static System.Windows.WindowState DefaultWindowState
        {
            get
            {
                
                string val = Get("DefaultWindowState");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival < 0)
                        return System.Windows.WindowState.Normal;

                    if (ival > 2)
                        return System.Windows.WindowState.Normal;

                    return (System.Windows.WindowState)ival;
                }
                else
                {
                    return System.Windows.WindowState.Normal; // default value
                }
            }
            set
            {
                Set("DefaultWindowState", ((int)value).ToString());
            }
        }

        public static Dictionary<int, string> ImportFolderMappings
        {
            get
            {
                Dictionary<int, string> mappings = new Dictionary<int, string>();
                
                string mpgs = Get("ImportFolderMappings");

                if (string.IsNullOrEmpty(mpgs)) return mappings;

                string[] arrmpgs = mpgs.Split(';');
                foreach (string arrval in arrmpgs)
                {
                    if (string.IsNullOrEmpty(arrval)) continue;

                    string[] vals = arrval.Split('|');
                    mappings[int.Parse(vals[0])] = vals[1];
                }

                return mappings;
            }
        }

        public static void SetImportFolderMapping(int folderID, string localPath)
        {
            string mpgs = Get("ImportFolderMappings");

            string output = "";

            // check if we already have this in the existing settings
            bool exists = ImportFolderMappings.ContainsKey(folderID);

            if (!string.IsNullOrEmpty(mpgs))
            {
                string[] arrmpgs = mpgs.Split(';');
                if (arrmpgs.Any())
                {
                    foreach (string arrval in arrmpgs)
                    {
                        if (string.IsNullOrEmpty(arrval)) continue;

                        if (output.Length > 0) output += ";";

                        string[] vals = arrval.Split('|');

                        int thisFolderID = 0;
                        bool isFolderID = int.TryParse(vals[0], out thisFolderID);
                        if (isFolderID)
                        {
                            if (thisFolderID == folderID)
                                output += string.Format("{0}|{1}", thisFolderID, localPath);
                            else
                                output += string.Format("{0}|{1}", thisFolderID, vals[1]);
                        }
                        else
                        {
                            output += string.Format("{0}|{1}", thisFolderID, vals[1]);
                        }
                    }
                }
            }

            // new entry
            if (!exists)
            {
                if (output.Length > 0) output += ";";
                output += string.Format("{0}|{1}", folderID, localPath);
            }

            Set("ImportFolderMappings", output);
        }

        public static void RemoveImportFolderMapping(int folderID)
        {
            if (ImportFolderMappings.ContainsKey(folderID))
            {
                
                string mpgs = Get("ImportFolderMappings");

                string output = "";

                string[] arrmpgs = mpgs.Split(';');
                foreach (string arrval in arrmpgs)
                {
                    if (string.IsNullOrEmpty(arrval)) continue;

                    string[] vals = arrval.Split('|');

                    int thisFolderID = int.Parse(vals[0]);
                    if (thisFolderID != folderID)
                    {
                        if (output.Length > 0) output += ";";
                        output += string.Format("{0}|{1}", thisFolderID, vals[1]);
                    }
                }

                Set("ImportFolderMappings", output);
            }
        }

        private static void SaveImportFolderMappings()
        {
            string output = "";
            foreach (KeyValuePair<int, string> kvp in ImportFolderMappings)
            {
                if (output.Length > 0) output += ";";
                output += string.Format("{0}|{1}", kvp.Key, kvp.Value);
            }
            Set("ImportFolderMappings", output);
        }



        public static bool TorrentBlackhole
        {
            get
            {
                

                string val = Get("TorrentBlackhole");
                bool bval = false;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("TorrentBlackhole", value.ToString());
            }
        }

        public static string TorrentBlackholeFolder
        {
            get
            {
                

                string val = Get("TorrentBlackholeFolder");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "";
                    Set("TorrentBlackholeFolder", val);
                }
                return val;
            }
            set
            {
                Set("TorrentBlackholeFolder", value);
            }
        }

        public static string UTorrentAddress
        {
            get
            {
                

                string val = Get("UTorrentAddress");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "";
                    Set("UTorrentAddress", val);
                }
                return val;
            }
            set
            {
                Set("UTorrentAddress", value);
            }
        }

        public static string UTorrentPort
        {
            get
            {
                

                string val = Get("UTorrentPort");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "";
                    Set("UTorrentPort", val);
                }
                return val;
            }
            set
            {
                Set("UTorrentPort", value);
            }
        }

        public static string UTorrentUsername
        {
            get
            {
                

                string val = Get("UTorrentUsername");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "";
                    Set("UTorrentUsername", val);
                }
                return val;
            }
            set
            {
                Set("UTorrentUsername", value);
            }
        }

        public static string UTorrentPassword
        {
            get
            {
                

                string val = Get("UTorrentPassword");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "";
                    Set("UTorrentPassword", val);
                }
                return val;
            }
            set
            {
                Set("UTorrentPassword", value);
            }
        }

        public static int UTorrentRefreshInterval
        {
            get
            {
                

                string val = Get("UTorrentRefreshInterval");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival < 1)
                        return 5;

                    if (ival > 999)
                        return 5;

                    return ival;
                }
                else
                {
                    return 5; // default value
                }
            }
            set
            {
                Set("UTorrentRefreshInterval", value.ToString());
            }
        }

        public static bool UTorrentAutoRefresh
        {
            get
            {
                
                string val = Get("UTorrentAutoRefresh");
                bool bval = false;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return true; // default value
            }
            set
            {
                Set("UTorrentAutoRefresh", value.ToString());
            }
        }

        public static bool TorrentSearchPreferOwnGroups
        {
            get
            {
                
                string val = Get("TorrentSearchPreferOwnGroups");
                bool bval = false;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return true; // default value
            }
            set
            {
                Set("TorrentSearchPreferOwnGroups", value.ToString());
            }
        }

        public static string TorrentSources
        {
            get
            {
                
                string val = Get("TorrentSources");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "1;4;5";
                    Set("TorrentSources", val);
                }

                // make sure the selected sources are valid
                // just in case the user has manually edited the config, or is using an old config
                string[] sources = val.Split(';');
                bool invalidSource = false;
                int maxEnum = 7;
                foreach (string src in sources)
                {
                    int iSrc = 0;
                    if (!int.TryParse(src, out iSrc))
                    {
                        invalidSource = true;
                        break;
                    }

                    if (iSrc <= 0 || iSrc > maxEnum)
                    {
                        invalidSource = true;
                        break;
                    }
                }

                if (invalidSource)
                {
                    // default value
                    val = "1;4;5";
                    Set("TorrentSources", val);
                }


                return val;
            }
            set
            {
                Set("TorrentSources", value);
            }
        }

        public static string BakaBTUsername
        {
            get
            {
                

                string val = Get("BakaBTUsername");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "";
                    Set("BakaBTUsername", val);
                }
                return val;
            }
            set
            {
                Set("BakaBTUsername", value);
            }
        }

        public static string BakaBTPassword
        {
            get
            {
                

                string val = Get("BakaBTPassword");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "";
                    Set("BakaBTPassword", val);
                }
                return val;
            }
            set
            {
                Set("BakaBTPassword", value);
            }
        }

        public static bool BakaBTOnlyUseForSeriesSearches
        {
            get
            {
                
                string val = Get("BakaBTOnlyUseForSeriesSearches");
                bool bval = false;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return true; // default value
            }
            set
            {
                Set("BakaBTOnlyUseForSeriesSearches", value.ToString());
            }
        }

        public static string AnimeBytesUsername
        {
            get
            {
                

                string val = Get("AnimeBytesUsername");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "";
                    Set("AnimeBytesUsername", val);
                }
                return val;
            }
            set
            {
                Set("AnimeBytesUsername", value);
            }
        }

        public static string AnimeBytesPassword
        {
            get
            {
                

                string val = Get("AnimeBytesPassword");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "";
                    Set("AnimeBytesPassword", val);
                }
                return val;
            }
            set
            {
                Set("AnimeBytesPassword", value);
            }
        }

        public static bool AnimeBytesOnlyUseForSeriesSearches
        {
            get
            {
                
                string val = Get("AnimeBytesOnlyUseForSeriesSearches");
                bool bval = false;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return true; // default value
            }
            set
            {
                Set("AnimeBytesOnlyUseForSeriesSearches", value.ToString());
            }
        }

        public static bool UseStreaming
        {
            get
            {
                
                string val = Get("UseStreaming");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                return true; // default value
            }
            set
            {
                Set("UseStreaming", value.ToString());
            }
        }


        public static string MPCFolder
        {
            get
            {
                

                string val = Get("MPCFolder");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "";
                    Set("MPCFolder", val);
                }
                return val;
            }
            set
            {
                Set("MPCFolder", value);
            }
        }

        public static string MPCWebUIUrl
        {
            get
            {
                

                string value = Get("MPCWebUIUrl");
                if (string.IsNullOrEmpty(value))
                {
                    // default value
                    value = "localhost";
                    Set("MPCWebUIUrl", value);
                }
                return value;
            }
            set
            {
                Set("MPCWebUIUrl", value);
            }
        }

        public static string MPCWebUIPort
        {
            get
            {
                

                string value = Get("MPCWebUIPort");
                if(string.IsNullOrEmpty(value))
                {
                    // default value
                    value = "13579";
                    Set("MPCWebUIPort", value);
                }
                return value;
            }
            set
            {
                Set("MPCWebUIPort", value);
            }
        }

        public static string PotPlayerFolder
		{
			get
			{
				

                string val = Get("PotPlayerFolder");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "";
                    Set("PotPlayerFolder", val);
                }
                return val;
            }
            set
            {
                Set("PotPlayerFolder", value);
            }
        }

        public static int VideoWatchedPct
        {
            get
            {
                

                string val = Get("VideoWatchedPct");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival < 1)
                        return 85;

                    if (ival > 100)
                        return 85;

                    return ival;
                }
                else
                {
                    return 85; // default value
                }
            }
            set
            {
                Set("VideoWatchedPct", value.ToString());
            }
        }

        public static bool VideoAutoSetWatched
        {
            get
            {
                
                string val = Get("VideoAutoSetWatched");
                bool bval = false;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("VideoAutoSetWatched", value.ToString());
            }
        }

        public static bool MPCIniIntegration
        {
            get
            {
                
                string stringValue = Get("MPCIniIntegration");
                bool booleanValue = false;
                bool.TryParse(stringValue, out booleanValue);
                    
                return booleanValue;
            }
            set
            {
                Set("MPCIniIntegration", value.ToString());
            }
        }

        public static bool MPCWebUiIntegration
        {
            get
            {
                
                string stringValue = Get("MPCWebUiIntegration");
                bool booleanValue = false;
                bool.TryParse(stringValue, out booleanValue);

                return booleanValue;
            }
            set
            {
                Set("MPCWebUiIntegration", value.ToString());
            }
        }
   
        public static bool MultipleFilesOnlyFinished
		{
			get
			{
				
				string val = Get("MultipleFilesOnlyFinished");
				bool bval = true;
				if (bool.TryParse(val, out bval))
					return bval;
				else
					return false; // default value
			}
			set
			{
				Set("MultipleFilesOnlyFinished", value.ToString());
			}
		}

        public static int FileSummaryTypeDefault
        {
            get
            {
                

                string val = Get("FileSummaryTypeDefault");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival < 0)
                        return 0;

                    if (ival > 1)
                        return 0;

                    return ival;
                }
                else
                {
                    return 0; // default value
                }
            }
            set
            {
                Set("FileSummaryTypeDefault", value.ToString());
            }
        }

        public static int FileSummaryQualSortDefault
        {
            get
            {
                

                string val = Get("FileSummaryQualSortDefault");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival < 0)
                        return 0;

                    if (ival > 1)
                        return 0;

                    return ival;
                }
                else
                {
                    return 0; // default value
                }
            }
            set
            {
                Set("FileSummaryQualSortDefault", value.ToString());
            }
        }

        public static int AutoFileFirst
        {
            get
            {
                

                string val = Get("AutoFileFirst");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival < 0)
                        return 0;

                    if (ival > 1)
                        return 0;

                    return ival;
                }
                else
                {
                    return 0; // default value
                }
            }
            set
            {
                Set("AutoFileFirst", value.ToString());
            }
        }

        public static int AutoFileSubsequent
        {
            get
            {
                

                string val = Get("AutoFileSubsequent");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival < 0)
                        return 0;

                    if (ival > 1)
                        return 0;

                    return ival;
                }
                else
                {
                    return 0; // default value
                }
            }
            set
            {
                Set("AutoFileSubsequent", value.ToString());
            }
        }

        public static bool AutoFileSingleEpisode
        {
            get
            {
                
                string val = Get("AutoFileSingleEpisode");
                bool bval = true;
                if (bool.TryParse(val, out bval))
                    return bval;
                else
                    return false; // default value
            }
            set
            {
                Set("AutoFileSingleEpisode", value.ToString());
            }
        }

        public static int DownloadsRecItems
        {
            get
            {
                
                string val = Get("DownloadsRecItems");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival >= 0 && ival <= 100)
                        return ival;
                    else
                        return 10;
                }
                else
                    return 10; // default value
            }
            set
            {
                Set("DownloadsRecItems", value.ToString());
            }
        }

        public static string LastLoginUsername
        {
            get
            {
                

                string val = Get("LastLoginUsername");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "";
                    Set("LastLoginUsername", val);
                }
                return val;
            }
            set
            {
                Set("LastLoginUsername", value);
            }
        }

        public static DashboardType DashboardType
        {
            get
            {
                
                int val = 1;
                if (int.TryParse(Get("DashboardType"), out val))
                    return (DashboardType)val;
                else
                    return DashboardType.Normal; // default value
            }
            set
            {
                Set("DashboardType", ((int)value).ToString());
            }
        }

        public static int DashMetro_WatchNext_Items
        {
            get
            {
                
                string val = Get("DashMetro_WatchNext_Items");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival >= 0 && ival <= 100)
                        return ival;
                    else
                        return 5;
                }
                else
                    return 5; // default value
            }
            set
            {
                Set("DashMetro_WatchNext_Items", value.ToString());
            }
        }

        public static int DashMetro_RandomSeries_Items
        {
            get
            {
                
                string val = Get("DashMetro_RandomSeries_Items");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival >= 0 && ival <= 100)
                        return ival;
                    else
                        return 5;
                }
                else
                    return 5; // default value
            }
            set
            {
                Set("DashMetro_RandomSeries_Items", value.ToString());
            }
        }

        public static int DashMetro_TraktActivity_Items
        {
            get
            {
                
                string val = Get("DashMetro_TraktActivity_Items");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival >= 0 && ival <= 100)
                        return ival;
                    else
                        return 5;
                }
                else
                    return 5; // default value
            }
            set
            {
                Set("DashMetro_TraktActivity_Items", value.ToString());
            }
        }

        public static int DashMetro_NewEpisodes_Items
        {
            get
            {
                
                string val = Get("DashMetro_NewEpisodes_Items");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival >= 0 && ival <= 100)
                        return ival;
                    else
                        return 5;
                }
                else
                    return 5; // default value
            }
            set
            {
                Set("DashMetro_NewEpisodes_Items", value.ToString());
            }
        }

        public static int DashMetro_Image_Height
        {
            get
            {
                
                string val = Get("DashMetro_Image_Height");
                int ival = 0;
                if (int.TryParse(val, out ival))
                {
                    if (ival < 30)
                        return 30;

                    if (ival > 300)
                        return 300;

                    return ival;
                }
                else
                {
                    return 136; // default value
                }
            }
            set
            {
                Set("DashMetro_Image_Height", value.ToString());
            }
        }

        public static DashboardMetroImageType DashMetroImageType
        {
            get
            {
                
                int val = 1;
                if (int.TryParse(Get("DashMetroImageType"), out val))
                    return (DashboardMetroImageType)val;
                else
                    return DashboardMetroImageType.Fanart; // default value
            }
            set
            {
                Set("DashMetroImageType", ((int)value).ToString());
            }
        }

        public static string DashboardMetroSectionOrder
        {
            get
            {
                
                string val = Get("DashboardMetroSectionOrder");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "2:true;3:true;4:true";
                    Set("DashboardMetroSectionOrder", val);
                }

                // make sure the setting contains all the widgets
                // just in case the user has manually edited the config, or is using an old config
                string[] widgets = val.Split(';');
                List<string> tempWidgets = new List<string>();
                foreach (string w in widgets)
                {
                    string[] vals = w.Split(':');
                    tempWidgets.Add(vals[0]);
                }

                int maxEnum = 4;
                for (int i = 1; i <= maxEnum; i++)
                {
                    // skip Trakt as this has been deprecated
                    DashboardMetroProcessType sectionType = (DashboardMetroProcessType)i;
                    if (sectionType == DashboardMetroProcessType.TraktActivity) continue;

                    if (!tempWidgets.Contains(i.ToString()))
                    {
                        if (val.Length > 0) val += ";";
                        val += i.ToString() + ":true";
                    }
                }

                return val;
            }
            set
            {
                Set("DashboardMetroSectionOrder", value);
            }
        }

        public static string DashboardMetroSectionVisibility
        {
            get
            {
                
                string val = Get("DashboardMetroSectionVisibility");
                if (string.IsNullOrEmpty(val))
                {
                    // default value
                    val = "1;1;1";
                    Set("DashboardMetroSectionVisibility", val);
                }

                // make sure the setting contains all the widgets
                // just in case the user has manually edited the config, or is using an old config
                string[] widgets = val.Split(';');
                int maxEnum = 4;
                for (int i = 1; i <= maxEnum; i++)
                {
                    // skip Trakt as this has been deprecated
                    DashboardMetroProcessType sectionType = (DashboardMetroProcessType)i;
                    if (sectionType == DashboardMetroProcessType.TraktActivity) continue;

                    if (!widgets.Contains(i.ToString()))
                    {
                        if (val.Length > 0) val += ";";
                        val += i.ToString();
                    }
                }

                return val;
            }
            set
            {
                Set("DashboardMetroSectionVisibility", value);
            }
        }

        public static void DebugSettingsToLog()
        {
            #region System Info
            logger.Info("-------------------- SYSTEM INFO -----------------------");

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            try
            {
                if (a != null)
                {
                    logger.Info(string.Format("JMM Desktop Version: v{0}", Utils.GetApplicationVersion(a)));
                }
            }
            catch (Exception ex)
            {
                // oopps, can't create file
                logger.Warn("Error in log: {0}", ex.ToString());
            }

            logger.Info(string.Format("Operating System: {0}", Utils.GetOSInfo()));

            string screenSize = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width.ToString() + "x" +
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height.ToString();
            logger.Info(string.Format("Screen Size: {0}", screenSize));


            logger.Info("-------------------------------------------------------");
            #endregion

            logger.Info("----------------- DESKTOP SETTINGS ----------------------");

            logger.Info("Culture: {0}", Culture);
            logger.Info("Episodes_Availability: {0}", Episodes_Availability);
            logger.Info("Episodes_WatchedStatus: {0}", Episodes_WatchedStatus);
            logger.Info("BaseImagesPath: {0}", BaseImagesPath);
            logger.Info("BaseImagesPathIsDefault: {0}", BaseImagesPathIsDefault);
            logger.Info("ShokoServer_Address: {0}", JMMServer_Address);
            logger.Info("ShokoServer_Port: {0}", JMMServer_Port);
            logger.Info("ShokoServer_FilePort: {0}", JMMServer_FilePort);
            logger.Info("EpisodeImageOverviewStyle: {0}", EpisodeImageOverviewStyle);
            logger.Info("HideEpisodeImageWhenUnwatched: {0}", HideEpisodeImageWhenUnwatched);
            logger.Info("HideEpisodeOverviewWhenUnwatched: {0}", HideEpisodeOverviewWhenUnwatched);
            logger.Info("Dash_WatchNext_Style: {0}", Dash_WatchNext_Style);

            logger.Info("-------------------------------------------------------");
        }
        private static NameValueCollection GetNameValueCollectionSection(string section, string filePath)
        {
            string file = filePath;
            System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
            NameValueCollection nameValueColl = new NameValueCollection();

            System.Configuration.ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = file;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            string xml = config.GetSection(section).SectionInformation.GetRawXml();
            xDoc.LoadXml(xml);

            System.Xml.XmlNode xList = xDoc.ChildNodes[0];
            foreach (System.Xml.XmlNode xNodo in xList)
            {
                nameValueColl.Add(xNodo.Attributes[0].Value, xNodo.Attributes[1].Value);

            }

            return nameValueColl;
        }
    }
}

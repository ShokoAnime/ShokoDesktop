using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Xml;
using Newtonsoft.Json;
using NLog;
using NLog.Targets;
using NutzCode.MPVPlayer.WPF.Wrapper.Models;
using Shoko.Desktop.Enums;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Models.Enums;
using Application = System.Windows.Application;
using Formatting = Newtonsoft.Json.Formatting;
using MessageBox = System.Windows.MessageBox;

namespace Shoko.Desktop
{
    public static class AppSettings
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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
            if (value == orig) return;
            appSettings[key] = value;
            SaveSettings();
        }


        public static string DefaultInstance { get; } =
            Assembly.GetExecutingAssembly().GetName().Name;

        public static string ApplicationPath
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                DefaultInstance);

        public static string JMMServerPath
            =>
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    JMMServerInstance);

        public static string DefaultImagePath => Path.Combine(ApplicationPath, "images");

        public static string JMMServerImagePath
        {
            get
            {
                if (!Directory.Exists(JMMServerPath) || !File.Exists(Path.Combine(JMMServerPath, "settings.json")))
                    return null;
                Dictionary<string, string> serverSettings =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(
                        File.ReadAllText(Path.Combine(JMMServerPath, "settings.json")));
                if (serverSettings.ContainsKey("ImagesPath"))
                    return serverSettings["ImagesPath"];
                return null;

            }
        }

        private static bool disabledSave;

        public static void SaveSettings()
        {
            if (disabledSave)
                return;
            lock (appSettings)
            {
                if (appSettings.Count <= 1)
                    return; //Somehow debugging may fuck up the settings so this shit will eject

                string path = Path.Combine(ApplicationPath, "settings.json");
                File.WriteAllText(path, JsonConvert.SerializeObject(appSettings, Formatting.Indented));
            }
        }

        public static void LoadSettings()
        {
            try
            {
                try
                {
                    //Reconfigure log file to applicationpath
                    var target = LogManager.Configuration?.FindTargetByName("file") as FileTarget;
                    if (target == null) throw new NullReferenceException("LogManager Configuration was null");
                    target.FileName = ApplicationPath + "/logs/${shortdate}.txt";
                }
                catch
                {
                    // ignore
                }
                try
                {
                    LogManager.ReconfigExistingLoggers();
                }
                catch
                {
                    // ignore
                }

                bool startedWithFreshConfig = false;

                disabledSave = true;

                if (!string.IsNullOrEmpty(ApplicationPath))
                {
                    // Check if programdata is write-able
                    if (Directory.Exists(ApplicationPath))
                        if (!Utils.IsDirectoryWritable(ApplicationPath))
                            try
                            {
                                Utils.GrantAccess(ApplicationPath);
                            }
                            catch (Exception)
                            {
                                logger.Error("Unable to grant permissions for program data");
                            }
                }

                string path = Path.Combine(ApplicationPath, "settings.json");
                if (File.Exists(path))
                    appSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
                else
                    startedWithFreshConfig = true;

                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(Culture);
                if (!BaseImagesPathIsDefault && Directory.Exists(BaseImagesPath) && Directory.Exists(BaseImagesPath))
                    ImagesPath = BaseImagesPath;

                disabledSave = false;

                if (string.IsNullOrEmpty(BaseImagesPath)) BaseImagesPath = Utils.GetBaseImagesPath();

                if (Directory.Exists(BaseImagesPath) && string.IsNullOrEmpty(ImagesPath))
                    ImagesPath = BaseImagesPath;

                if (string.IsNullOrEmpty(ImagesPath))
                    ImagesPath = string.IsNullOrEmpty(JMMServerImagePath) ? DefaultImagePath : JMMServerImagePath;

                SaveSettings();

                // Just in case start once for new configurations as admin to set permissions if needed
                if (startedWithFreshConfig && !Utils.IsAdministrator())
                {
                    logger.Info("User has fresh config, restarting once as admin.");
                    Utils.RestartAsAdmin();
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Error(ex, $"Error occured during LoadSettings (UnauthorizedAccessException): {ex}");
                var message = "Failed to set folder permissions, do you want to automatically retry as admin?";

                if (!Utils.IsAdministrator())
                    message = "Failed to set folder permissions, do you want to try and reset folder permissions?";

                DialogResult dr =
                    FlexibleMessageBox.Show(message, "Failed to set folder permissions",
                        MessageBoxButtons.YesNo);

                switch (dr)
                {
                    case DialogResult.Yes:
                        // gonna try grant access again in advance
                        try
                        {
                            Utils.GrantAccess(ApplicationPath);
                        }
                        catch (Exception)
                        {
                            logger.Error("Unable to set permissions for program data");
                        }
                        Utils.RestartAsAdmin();
                        break;
                    case DialogResult.No:
                        Application.Current.Shutdown();
                        Environment.Exit(0);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured during LoadSettings: {ex}");
                MessageBox.Show("Settings Load Error:" + " " + ex,
                    "Settings Load Error!");
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
        }

        public static string AnimeEpisodesText
        {
            get
            {
                string dir = Get("AnimeEpisodesText");
                if (!string.IsNullOrEmpty(dir)) return dir;
                dir = Path.Combine(ApplicationPath, "AnimeEpisodes.txt");
                Set("AnimeEpisodesText", dir);
                return dir;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("AnimeEpisodesText", value);
        }

        public static string Culture
        {
            get
            {
                string cult = Get("Culture");
                if (!string.IsNullOrEmpty(cult))
                    return cult;
                // default value
                cult = "en";
                Set("Culture", cult);
                return cult;
            }
            set => Set("Culture", value);
        }

        public static string JMMServerInstance
        {
            get
            {
                string instance = Get("JMMServerInstance");
                if (!string.IsNullOrEmpty(instance)) return instance;
                instance = "ShokoServer";
                JMMServerInstance = instance;
                return instance;
            }
            set => Set("JMMServerInstance",value);
        }

        public static AvailableEpisodeType Episodes_Availability
        {
            get
            {
                if (int.TryParse(Get("Episodes_Availability"), out var val))
                    return (AvailableEpisodeType)val;
                return AvailableEpisodeType.All; // default value
            }
            set => Set("Episodes_Availability", ((int)value).ToString());
        }

        public static WatchedStatus Episodes_WatchedStatus
        {
            get
            {
                if (int.TryParse(Get("Episodes_WatchedStatus"), out var val))
                    return (WatchedStatus)val;
                return WatchedStatus.All; // default value
            }
            set => Set("Episodes_WatchedStatus", ((int)value).ToString());
        }

        public static PlayerSettings MpvPlayerSettings
        {
            get
            {
                PlayerSettings result;
                string data = Get("MpvPlayerSettings");
                if (!string.IsNullOrEmpty(data))
                {
                    result = JsonConvert.DeserializeObject<PlayerSettings>(data);
                }
                else
                {
                    result=new PlayerSettings();
                    MpvPlayerSettings = result;
                }
                return result;
            }
            set
            {
                if (value == null) return;
                string settings = JsonConvert.SerializeObject(value);
                Set("MpvPlayerSettings",settings);
            }
        }
        public static string ImagesPath
        {
            get => Get("ImagesPath");
            set
            {
                Set("ImagesPath", value);
                VM_ShokoServer.Instance.BaseImagePath = Utils.GetBaseImagesPath();
            }
        }

        private static string BaseImagesPath
        {
            get => Get("BaseImagesPath") ?? Utils.GetBaseImagesPath();
            // ReSharper disable once UnusedMember.Local
            set
            {
                Set("BaseImagesPath", value);
                VM_ShokoServer.Instance.BaseImagePath = Utils.GetBaseImagesPath();
            }
        }
        private static bool BaseImagesPathIsDefault
        {
            get
            {
                string basePath = Get("BaseImagesPathIsDefault");
                if (string.IsNullOrEmpty(basePath)) return true;
                bool.TryParse(basePath, out var val);
                return val;
            }
        }

        public static string JMMServer_Address
        {
            get
            {
                string val = Get("ShokoServer_Address");
                if (!string.IsNullOrEmpty(val)) return val;
                // default value
                val = "127.0.0.1";
                Set("ShokoServer_Address", val);
                return val;
            }
            set => Set("ShokoServer_Address", value);
        }

        public static string JMMServer_Port
        {
            get
            {
                string val = Get("ShokoServer_Port");
                if (!string.IsNullOrEmpty(val)) return val;
                // default value
                val = "8111";
                Set("ShokoServer_Port", val);
                return val;
            }
            set => Set("ShokoServer_Port", value);
        }

        public static string JMMServer_FilePort
        {
            get
            {
                string val = Get("ShokoServer_FilePort");
                if (!string.IsNullOrEmpty(val)) return val;
                // default value
                val = "8111";
                Set("ShokoServer_FilePort", val);
                return val;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("ShokoServer_FilePort", value);
        }

        public static string ProxyAddress
        {
            get
            {
                string val = Get("ProxyAddress");
                if (!string.IsNullOrEmpty(val)) return val;
                // default value
                val = "";
                Set("ProxyAddress", val);
                return val;
            }
            set => Set("ProxyAddress", value);
        }

        public static int DisplayHeight_GroupList
        {
            get
            {
                string val = Get("DisplayHeight_GroupList");
                if (!int.TryParse(val, out var ival)) return 80; // default value
                if (ival > 30 && ival < 300)
                    return ival;
                return 80;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DisplayHeight_GroupList", value.ToString());
        }

        public static int PlaylistHeader_Image_Height
        {
            get
            {
                string val = Get("PlaylistHeader_Image_Height");
                if (!int.TryParse(val, out var ival)) return 200; // default value
                if (ival > 30 && ival < 400)
                    return ival;
                return 200;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("PlaylistHeader_Image_Height", value.ToString());
        }

        public static int PlaylistItems_Image_Height
        {
            get
            {
                string val = Get("PlaylistItems_Image_Height");
                if (!int.TryParse(val, out var ival)) return 130; // default value
                if (ival > 30 && ival < 400)
                    return ival;
                return 130;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("PlaylistItems_Image_Height", value.ToString());
        }

        public static int PlaylistEpisode_Image_Width
        {
            get
            {
                string val = Get("PlaylistEpisode_Image_Width");
                if (!int.TryParse(val, out var ival)) return 120; // default value
                if (ival > 10 && ival < 400)
                    return ival;
                return 120;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("PlaylistEpisode_Image_Width", value.ToString());
        }

        public static bool PlaylistItems_ShowDetails
        {
            get
            {
                string val = Get("PlaylistItems_ShowDetails");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("PlaylistItems_ShowDetails", value.ToString());
        }

        public static int DisplayHeight_SeriesInfo
        {
            get
            {
                string val = Get("DisplayHeight_SeriesInfo");
                if (!int.TryParse(val, out var ival)) return 200; // default value
                if (ival > 30 && ival < 300)
                    return ival;
                return 200;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DisplayHeight_SeriesInfo", value.ToString());
        }

        public static int DisplayWidth_EpisodeImage
        {
            get
            {
                string val = Get("DisplayWidth_EpisodeImage");
                if (!int.TryParse(val, out var ival)) return 120; // default value
                if (ival > 10 && ival < 400)
                    return ival;
                return 120;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DisplayWidth_EpisodeImage", value.ToString());
        }

        public static int DisplayStyle_GroupList
        {
            get
            {
                string val = Get("DisplayStyle_GroupList");
                if (!int.TryParse(val, out var ival)) return 1; // default value
                if (ival >= 1 && ival <= 2)
                    return ival;
                return 1;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DisplayStyle_GroupList", value.ToString());
        }
        public static int DefaultPlayer_GroupList
        {
            get
            {
                string val = Get("DefaultPlayer_GroupList");
                if (int.TryParse(val, out var ival))
                    return ival;
                return (int) VideoPlayer.MPV;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DefaultPlayer_GroupList", value.ToString());
        }

        public static int DisplayHeight_DashImage
        {
            get
            {
                string val = Get("DisplayHeight_DashImage");
                if (!int.TryParse(val, out var ival)) return 200; // default value
                if (ival < 30)
                    return 30;

                if (ival > 300)
                    return 300;

                return ival;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DisplayHeight_DashImage", value.ToString());
        }

        public static int EpisodeImageOverviewStyle
        {
            get
            {
                string val = Get("EpisodeImageOverviewStyle");
                if (!int.TryParse(val, out var ival)) return 1; // default value
                if (ival >= 1 && ival <= 3)
                    return ival;
                return 1;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("EpisodeImageOverviewStyle", value.ToString());
        }

        public static bool HideEpisodeImageWhenUnwatched
        {
            get
            {
                string val = Get("HideEpisodeImageWhenUnwatched");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("HideEpisodeImageWhenUnwatched", value.ToString());
        }

        public static bool HideEpisodeOverviewWhenUnwatched
        {
            get
            {

                string val = Get("HideEpisodeOverviewWhenUnwatched");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("HideEpisodeOverviewWhenUnwatched", value.ToString());
        }

        public static bool DisplayRatingDialogOnCompletion
        {
            get
            {
                string val = Get("DisplayRatingDialogOnCompletion");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return true; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DisplayRatingDialogOnCompletion", value.ToString());
        }

        public static bool DisplaySeriesSimple
        {
            get
            {
                string val = Get("DisplaySeriesSimple");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            set => Set("DisplaySeriesSimple", value.ToString());
        }

        public static bool UseFanartOnSeries
        {
            get
            {
                string val = Get("UseFanartOnSeries");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return true; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("UseFanartOnSeries", value.ToString());
        }

        public static bool AlwaysUseAniDBPoster
        {
            get
            {
                string val = Get("AlwaysUseAniDBPoster");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return true; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("AlwaysUseAniDBPoster", value.ToString());
        }

        public static bool UseFanartOnPlaylistHeader
        {
            get
            {
                string val = Get("UseFanartOnPlaylistHeader");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return true; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("UseFanartOnPlaylistHeader", value.ToString());
        }

        public static bool UseFanartOnPlaylistItems
        {
            get
            {
                string val = Get("UseFanartOnPlaylistItems");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("UseFanartOnPlaylistItems", value.ToString());
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
                    if (!widgets.Contains(i.ToString()))
                    {
                        if (val.Length > 0) val += ";";
                        val += i.ToString();
                    }

                return val;
            }
            set => Set("SeriesWidgetsOrder", value);
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

                    if (widgets.Contains(i.ToString())) continue;
                    if (val.Length > 0) val += ";";
                    val += i.ToString();
                }

                return val;
            }
            set => Set("DashboardWidgetsOrder", value);
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
                if (columns.Length == 8) return val;
                val = "1;1;1;1;1;1;1;1";
                Set("MissingEpsExportColumns", val);

                return val;
            }
            set => Set("MissingEpsExportColumns", value);
        }

        public static bool SeriesTvDBLinksExpanded
        {
            get
            {
                string val = Get("SeriesTvDBLinksExpanded");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("SeriesTvDBLinksExpanded", value.ToString());
        }


        public static bool TitlesExpanded
        {
            get
            {
                string val = Get("TitlesExpanded");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("TitlesExpanded", value.ToString());
        }

        public static bool TagsExpanded
        {
            get
            {
                string val = Get("TagsExpanded");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("TagsExpanded", value.ToString());
        }

        public static bool CustomTagsExpanded
        {
            get
            {
                string val = Get("CustomTagsExpanded");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("CustomTagsExpanded", value.ToString());
        }

        public static bool WindowFullScreen
        {
            get
            {
                string val = Get("WindowFullScreen");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("WindowFullScreen", value.ToString());
        }

        public static bool WindowNormal
        {
            get
            {
                string val = Get("WindowNormal");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return true; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("WindowNormal", value.ToString());
        }

        public static bool SeriesNextEpisodeExpanded
        {
            get
            {
                string val = Get("SeriesNextEpisodeExpanded");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("SeriesNextEpisodeExpanded", value.ToString());
        }

        public static bool SeriesGroupExpanded
        {
            get
            {
                string val = Get("SeriesGroupExpanded");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("SeriesGroupExpanded", value.ToString());
        }

        public static bool DashWatchNextEpExpanded
        {
            get
            {
                string val = Get("DashWatchNextEpExpanded");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return true; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DashWatchNextEpExpanded", value.ToString());
        }

        public static bool DashRecentlyWatchEpsExpanded
        {
            get
            {
                string val = Get("DashRecentlyWatchEpsExpanded");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DashRecentlyWatchEpsExpanded", value.ToString());
        }

        public static bool DashSeriesMissingEpisodesExpanded
        {
            get
            {
                string val = Get("DashSeriesMissingEpisodesExpanded");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DashSeriesMissingEpisodesExpanded", value.ToString());
        }

        public static bool DashMiniCalendarExpanded
        {
            get
            {
                string val = Get("DashMiniCalendarExpanded");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DashMiniCalendarExpanded", value.ToString());
        }

        public static bool DashRecommendationsWatchExpanded
        {
            get
            {
                string val = Get("DashRecommendationsWatchExpanded");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DashRecommendationsWatchExpanded", value.ToString());
        }

        public static bool DashRecommendationsDownloadExpanded
        {
            get
            {
                string val = Get("DashRecommendationsDownloadExpanded");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DashRecommendationsDownloadExpanded", value.ToString());
        }

        public static bool DashRecentAdditionsExpanded
        {
            get
            {
                string val = Get("DashRecentAdditionsExpanded");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DashRecentAdditionsExpanded", value.ToString());
        }

        public static int DashRecentAdditionsType
        {
            get
            {
                string val = Get("DashRecentAdditionsType");
                if (int.TryParse(val, out var bval))
                    return bval;
                return 0; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DashRecentAdditionsType", value.ToString());
        }

        public static bool DashTraktFriendsExpanded
        {
            get
            {
                string val = Get("DashTraktFriendsExpanded");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return true; // default value
            }
            set => Set("DashTraktFriendsExpanded", value.ToString());
        }

        public static int Dash_WatchNext_Items
        {
            get
            {
                string val = Get("Dash_WatchNext_Items");
                if (!int.TryParse(val, out var ival)) return 10; // default value
                if (ival >= 0 && ival <= 100)
                    return ival;
                return 10;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("Dash_WatchNext_Items", value.ToString());
        }

        public static int Dash_RecentAdditions_Items
        {
            get
            {
                string val = Get("Dash_RecentAdditions_Items");
                if (!int.TryParse(val, out var ival)) return 10; // default value
                if (ival >= 0 && ival <= 100)
                    return ival;
                return 10;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("Dash_RecentAdditions_Items", value.ToString());
        }

        public static int Dash_WatchNext_Height
        {
            get
            {
                string val = Get("Dash_WatchNext_Height");
                if (!int.TryParse(val, out var ival)) return 200; // default value
                if (ival < 30)
                    return 30;

                if (ival > 300)
                    return 300;

                return ival;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("Dash_WatchNext_Height", value.ToString());
        }

        public static int Dash_RecentAdditions_Height
        {
            get
            {
                string val = Get("Dash_RecentAdditions_Height");
                if (!int.TryParse(val, out var ival)) return 200; // default value
                if (ival < 30)
                    return 30;

                if (ival > 300)
                    return 300;

                return ival;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("Dash_RecentAdditions_Height", value.ToString());
        }

        public static DashWatchNextStyle Dash_WatchNext_Style
        {
            get
            {
                if (int.TryParse(Get("Dash_WatchNext_Style"), out var val))
                    return (DashWatchNextStyle)val;
                return DashWatchNextStyle.Detailed; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("Dash_WatchNext_Style", ((int)value).ToString());
        }

        public static int Dash_RecentlyWatchedEp_Items
        {
            get
            {
                string val = Get("Dash_RecentlyWatchedEp_Items");
                if (!int.TryParse(val, out var ival)) return 10; // default value
                if (ival >= 0 && ival <= 100)
                    return ival;
                return 10;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("Dash_RecentlyWatchedEp_Items", value.ToString());
        }

        public static int Dash_RecentlyWatchedEp_Height
        {
            get
            {
                string val = Get("Dash_RecentlyWatchedEp_Height");
                if (!int.TryParse(val, out var ival)) return 200; // default value
                if (ival < 30)
                    return 30;

                if (ival > 300)
                    return 300;

                return ival;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("Dash_RecentlyWatchedEp_Height", value.ToString());
        }


        public static int Dash_MissingEps_Items
        {
            get
            {
                string val = Get("Dash_MissingEps_Items");
                if (!int.TryParse(val, out var ival)) return 10; // default value
                if (ival >= 0 && ival <= 100)
                    return ival;
                return 10;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("Dash_MissingEps_Items", value.ToString());
        }

        public static int Dash_MissingEps_Height
        {
            get
            {
                string val = Get("Dash_MissingEps_Height");
                if (!int.TryParse(val, out var ival)) return 200; // default value
                if (ival < 30)
                    return 30;

                if (ival > 300)
                    return 300;

                return ival;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("Dash_MissingEps_Height", value.ToString());
        }

        public static int Dash_MiniCalendarDays
        {
            get
            {
                string val = Get("Dash_MiniCalendarDays");
                if (!int.TryParse(val, out var ival)) return 10; // default value
                if (ival >= 0 && ival <= 100)
                    return ival;
                return 10;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("Dash_MiniCalendarDays", value.ToString());
        }

        public static bool Dash_MiniCalendarUpcomingOnly
        {
            get
            {
                string val = Get("Dash_MiniCalendarUpcomingOnly");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return true; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("Dash_MiniCalendarUpcomingOnly", value.ToString());
        }

        public static int Dash_MiniCalendar_Height
        {
            get
            {
                string val = Get("Dash_MiniCalendar_Height");
                if (!int.TryParse(val, out var ival)) return 200; // default value
                if (ival < 30)
                    return 30;

                if (ival > 300)
                    return 300;

                return ival;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("Dash_MiniCalendar_Height", value.ToString());
        }

        public static int Dash_RecWatch_Height
        {
            get
            {
                string val = Get("Dash_RecWatch_Height");
                if (!int.TryParse(val, out var ival)) return 200; // default value
                if (ival < 30)
                    return 30;

                if (ival > 300)
                    return 300;

                return ival;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("Dash_RecWatch_Height", value.ToString());
        }

        public static int Dash_RecWatch_Items
        {
            get
            {
                string val = Get("Dash_RecWatch_Items");
                if (!int.TryParse(val, out var ival)) return 10; // default value
                if (ival >= 0 && ival <= 100)
                    return ival;
                return 10;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("Dash_RecWatch_Items", value.ToString());
        }

        public static int Dash_RecDownload_Height
        {
            get
            {
                string val = Get("Dash_RecDownload_Height");
                if (!int.TryParse(val, out var ival)) return 200; // default value
                if (ival < 30)
                    return 30;

                if (ival > 300)
                    return 300;

                return ival;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("Dash_RecDownload_Height", value.ToString());
        }

        public static int Dash_RecDownload_Items
        {
            get
            {
                string val = Get("Dash_RecDownload_Items");
                if (!int.TryParse(val, out var ival)) return 10; // default value
                if (ival >= 0 && ival <= 100)
                    return ival;
                return 10;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("Dash_RecDownload_Items", value.ToString());
        }

        public static int Dash_TraktFriends_Height
        {
            get
            {
                string val = Get("Dash_TraktFriends_Height");
                if (!int.TryParse(val, out var ival)) return 200; // default value
                if (ival < 30)
                    return 30;

                if (ival > 300)
                    return 300;

                return ival;
            }
            set => Set("Dash_TraktFriends_Height", value.ToString());
        }

        public static int Dash_TraktFriends_Items
        {
            get
            {
                string val = Get("Dash_TraktFriends_Items");
                if (!int.TryParse(val, out var ival)) return 10; // default value
                if (ival >= 0 && ival <= 100)
                    return ival;
                return 10;
            }
            set => Set("Dash_TraktFriends_Items", value.ToString());
        }

        public static bool Dash_TraktFriends_AnimeOnly
        {
            get
            {
                string val = Get("Dash_TraktFriends_AnimeOnly");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            set => Set("Dash_TraktFriends_AnimeOnly", value.ToString());
        }

        public static bool AutoStartLocalJMMServer
        {
            get
            {
                string val = Get("AutoStartLocalJMMServer");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; //default value
            }
            set => Set("AutoStartLocalJMMServer", value.ToString());
        }

        public static int SeriesGroup_Image_Height
        {
            get
            {
                string val = Get("SeriesGroup_Image_Height");
                if (!int.TryParse(val, out var ival)) return 200; // default value
                if (ival < 80)
                    return 80;

                if (ival > 400)
                    return 400;

                return ival;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("SeriesGroup_Image_Height", value.ToString());
        }

        public static WindowState DefaultWindowState
        {
            get
            {
                string val = Get("DefaultWindowState");
                if (!int.TryParse(val, out var ival)) return WindowState.Normal; // default value
                if (ival < 0)
                    return WindowState.Normal;

                if (ival > 2)
                    return WindowState.Normal;

                return (WindowState)ival;
            }
            set => Set("DefaultWindowState", ((int)value).ToString());
        }


        public static Dictionary<int, string> GetMappings()
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

        public static void SetMappings(Dictionary<int, string> mappings)
        {
            string[] maps = mappings.Select(a => a.Key.ToString(CultureInfo.InvariantCulture) + "|" + a.Value).ToArray();
            Set("ImportFolderMappings", string.Join(";",maps));
        }

        public static bool UseStreaming
        {
            get
            {
                string val = Get("UseStreaming");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return true; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("UseStreaming", value.ToString());
        }


        public static string MPCFolder
        {
            get
            {
                string val = Get("MPCFolder");
                if (!string.IsNullOrEmpty(val)) return val;
                // default value
                val = "";
                Set("MPCFolder", val);
                return val;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("MPCFolder", value);
        }

        public static string MPCWebUIUrl
        {
            get
            {
                string value = Get("MPCWebUIUrl");
                if (!string.IsNullOrEmpty(value)) return value;
                // default value
                value = "localhost";
                Set("MPCWebUIUrl", value);
                return value;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("MPCWebUIUrl", value);
        }

        public static string MPCWebUIPort
        {
            get
            {
                string value = Get("MPCWebUIPort");
                if (!string.IsNullOrEmpty(value)) return value;
                // default value
                value = "13579";
                Set("MPCWebUIPort", value);
                return value;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("MPCWebUIPort", value);
        }

        public static string PotPlayerFolder
        {
            get
            {
                string val = Get("PotPlayerFolder");
                if (!string.IsNullOrEmpty(val)) return val;
                // default value
                val = "";
                Set("PotPlayerFolder", val);
                return val;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("PotPlayerFolder", value);
        }

        public static int VideoWatchedPct
        {
            get
            {
                string val = Get("VideoWatchedPct");
                if (!int.TryParse(val, out var ival)) return 85; // default value
                if (ival < 1)
                    return 85;

                if (ival > 100)
                    return 85;

                return ival;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("VideoWatchedPct", value.ToString());
        }

        public static bool VideoAutoSetWatched
        {
            get
            {
                string val = Get("VideoAutoSetWatched");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("VideoAutoSetWatched", value.ToString());
        }

        public static bool MPCIniIntegration
        {
            get
            {
                string stringValue = Get("MPCIniIntegration");
                bool.TryParse(stringValue, out var booleanValue);
                return booleanValue;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("MPCIniIntegration", value.ToString());
        }

        public static bool MPCWebUiIntegration
        {
            get
            {
                string stringValue = Get("MPCWebUiIntegration");
                bool.TryParse(stringValue, out var booleanValue);
                return booleanValue;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("MPCWebUiIntegration", value.ToString());
        }

        public static bool MultipleFilesOnlyFinished
        {
            get
            {
                string val = Get("MultipleFilesOnlyFinished");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            set => Set("MultipleFilesOnlyFinished", value.ToString());
        }

        public static int FileSummaryTypeDefault
        {
            get
            {
                string val = Get("FileSummaryTypeDefault");
                if (!int.TryParse(val, out var ival)) return 0; // default value
                if (ival < 0)
                    return 0;

                if (ival > 1)
                    return 0;

                return ival;
            }
            set => Set("FileSummaryTypeDefault", value.ToString());
        }

        public static int FileSummaryQualSortDefault
        {
            get
            {
                string val = Get("FileSummaryQualSortDefault");
                if (!int.TryParse(val, out var ival)) return 0; // default value
                if (ival < 0)
                    return 0;

                if (ival > 1)
                    return 0;

                return ival;
            }
            set => Set("FileSummaryQualSortDefault", value.ToString());
        }

        public static int AutoFileFirst
        {
            get
            {
                string val = Get("AutoFileFirst");
                if (!int.TryParse(val, out var ival)) return 0; // default value
                if (ival < 0)
                    return 0;

                if (ival > 1)
                    return 0;

                return ival;
            }
            set => Set("AutoFileFirst", value.ToString());
        }

        public static int AutoFileSubsequent
        {
            get
            {
                string val = Get("AutoFileSubsequent");
                if (!int.TryParse(val, out var ival)) return 0; // default value
                if (ival < 0)
                    return 0;

                if (ival > 1)
                    return 0;

                return ival;
            }
            set => Set("AutoFileSubsequent", value.ToString());
        }

        public static bool AutoFileSingleEpisode
        {
            get
            {
                string val = Get("AutoFileSingleEpisode");
                if (bool.TryParse(val, out var bval))
                    return bval;
                return false; // default value
            }
            set => Set("AutoFileSingleEpisode", value.ToString());
        }

        public static int DownloadsRecItems
        {
            get
            {
                string val = Get("DownloadsRecItems");
                if (!int.TryParse(val, out var ival)) return 10; // default value
                if (ival >= 0 && ival <= 100)
                    return ival;
                return 10;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DownloadsRecItems", value.ToString());
        }

        public static string LastLoginUsername
        {
            get
            {
                string val = Get("LastLoginUsername");
                if (!string.IsNullOrEmpty(val)) return val;
                // default value
                val = "";
                Set("LastLoginUsername", val);
                return val;
            }
            set => Set("LastLoginUsername", value);
        }

        public static DashboardType DashboardType
        {
            get
            {
                if (int.TryParse(Get("DashboardType"), out var val))
                    return (DashboardType)val;
                return DashboardType.Normal; // default value
            }
            set => Set("DashboardType", ((int)value).ToString());
        }

        public static int DashMetro_WatchNext_Items
        {
            get
            {
                string val = Get("DashMetro_WatchNext_Items");
                if (!int.TryParse(val, out var ival)) return 5; // default value
                if (ival >= 0 && ival <= 100)
                    return ival;
                return 5;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DashMetro_WatchNext_Items", value.ToString());
        }

        public static int DashMetro_RandomSeries_Items
        {
            get
            {
                string val = Get("DashMetro_RandomSeries_Items");
                if (!int.TryParse(val, out var ival)) return 5; // default value
                if (ival >= 0 && ival <= 100)
                    return ival;
                return 5;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DashMetro_RandomSeries_Items", value.ToString());
        }

        public static int DashMetro_TraktActivity_Items
        {
            get
            {
                string val = Get("DashMetro_TraktActivity_Items");
                if (!int.TryParse(val, out var ival)) return 5; // default value
                if (ival >= 0 && ival <= 100)
                    return ival;
                return 5;
            }
            set => Set("DashMetro_TraktActivity_Items", value.ToString());
        }

        public static int DashMetro_NewEpisodes_Items
        {
            get
            {
                string val = Get("DashMetro_NewEpisodes_Items");
                if (!int.TryParse(val, out var ival)) return 5; // default value
                if (ival >= 0 && ival <= 100)
                    return ival;
                return 5;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DashMetro_NewEpisodes_Items", value.ToString());
        }

        public static int DashMetro_Image_Height
        {
            get
            {
                string val = Get("DashMetro_Image_Height");
                if (!int.TryParse(val, out var ival)) return 136; // default value
                if (ival < 30)
                    return 30;

                if (ival > 300)
                    return 300;

                return ival;
            }
            // ReSharper disable once UnusedMember.Global
            set => Set("DashMetro_Image_Height", value.ToString());
        }

        public static DashboardMetroImageType DashMetroImageType
        {
            get
            {
                if (int.TryParse(Get("DashMetroImageType"), out var val))
                    return (DashboardMetroImageType)val;
                return DashboardMetroImageType.Fanart; // default value
            }
            set => Set("DashMetroImageType", ((int)value).ToString());
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
                List<string> tempWidgets = widgets.Select(w => w.Split(':')).Select(vals => vals[0]).ToList();

                const int maxEnum = 4;
                for (int i = 1; i <= maxEnum; i++)
                {
                    // skip Trakt as this has been deprecated
                    DashboardMetroProcessType sectionType = (DashboardMetroProcessType)i;
                    if (sectionType == DashboardMetroProcessType.TraktActivity) continue;

                    if (tempWidgets.Contains(i.ToString())) continue;
                    if (val.Length > 0) val += ";";
                    val += i + ":true";
                }

                return val;
            }
            set => Set("DashboardMetroSectionOrder", value);
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
                const int maxEnum = 4;
                for (int i = 1; i <= maxEnum; i++)
                {
                    // skip Trakt as this has been deprecated
                    DashboardMetroProcessType sectionType = (DashboardMetroProcessType)i;
                    if (sectionType == DashboardMetroProcessType.TraktActivity) continue;

                    if (widgets.Contains(i.ToString())) continue;
                    if (val.Length > 0) val += ";";
                    val += i.ToString();
                }

                return val;
            }
            set => Set("DashboardMetroSectionVisibility", value);
        }

        public static string UpdateChannel
        {
            get
            {
                string val = Get("UpdateChannel");
                if (!string.IsNullOrEmpty(val)) return val;
                // default value
                val = "Stable";
                Set("UpdateChannel", val);
                return val;
            }
            set => Set("UpdateChannel", value);
        }

        public static void DebugSettingsToLog()
        {
            #region System Info
            logger.Info("-------------------- SYSTEM INFO -----------------------");

            Assembly a = Assembly.GetExecutingAssembly();
            try
            {
                logger.Info($"Shoko Desktop Version: v{Utils.GetApplicationVersion(a)}");
            }
            catch (Exception ex)
            {
                logger.Warn($"Error getting Desktop Version: {ex}");
            }

            logger.Info($"Operating System: {Utils.GetOSInfo()}");

            string screenSize = Screen.PrimaryScreen.Bounds.Width + "x" +
                Screen.PrimaryScreen.Bounds.Height;
            logger.Info($"Screen Size: {screenSize}");


            logger.Info("-------------------------------------------------------");
            #endregion

            logger.Info("----------------- DESKTOP SETTINGS ----------------------");

            logger.Info($"Culture: {Culture}");
            logger.Info($"Episodes_Availability: {Episodes_Availability}");
            logger.Info($"Episodes_WatchedStatus: {Episodes_WatchedStatus}");
            logger.Info($"BaseImagesPath: {BaseImagesPath}");
            logger.Info($"BaseImagesPathIsDefault: {BaseImagesPathIsDefault}");
            logger.Info($"ShokoServer_Address: {JMMServer_Address}");
            logger.Info($"ShokoServer_Port: {JMMServer_Port}");
            logger.Info($"ShokoServer_FilePort: {JMMServer_FilePort}");
            logger.Info($"EpisodeImageOverviewStyle: {EpisodeImageOverviewStyle}");
            logger.Info($"HideEpisodeImageWhenUnwatched: {HideEpisodeImageWhenUnwatched}");
            logger.Info($"HideEpisodeOverviewWhenUnwatched: {HideEpisodeOverviewWhenUnwatched}");
            logger.Info($"Dash_WatchNext_Style: {Dash_WatchNext_Style}");

            logger.Info("-------------------------------------------------------");
        }

        private static NameValueCollection GetNameValueCollectionSection(string section, string filePath)
        {
            string file = filePath;
            XmlDocument xDoc = new XmlDocument();
            NameValueCollection nameValueColl = new NameValueCollection();

            ExeConfigurationFileMap map = new ExeConfigurationFileMap {ExeConfigFilename = file};
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            string xml = config.GetSection(section).SectionInformation.GetRawXml();
            xDoc.LoadXml(xml);

            XmlNode xList = xDoc.ChildNodes[0];
            foreach (XmlNode xNodo in xList)
            {
                if (xNodo?.Attributes?[0]?.Value == null) continue;
                nameValueColl.Add(xNodo.Attributes[0].Value, xNodo.Attributes[1].Value);
            }

            return nameValueColl;
        }
    }
}

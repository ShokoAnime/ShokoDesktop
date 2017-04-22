﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Nancy.Rest.Client;
using NLog;
using Shoko.Commons.Extensions;
using Shoko.Commons.Languages;
using Shoko.Commons.Notification;
using Shoko.Commons.Properties;
using Shoko.Commons.Queue;
using Shoko.Models.Enums;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Properties;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Azure;
using Shoko.Models.Client;
using Shoko.Models.Interfaces;
using Shoko.Models.Queue;
using Shoko.Models.Server;
using Shoko.Models.TvDB;
using ImportFolder = Shoko.Models.Server.ImportFolder;
using Timer = System.Timers.Timer;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel
{
    public class VM_ShokoServer : INotifyPropertyChangedExt
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static VM_ShokoServer _instance;
        private Timer serverStatusTimer;
        private Timer saveTimer;
        private static DateTime lastVersionCheck = DateTime.Now.AddDays(-5);

        public object userLock = new object();
        public bool UserAuthenticated { get; set; }
        public VM_JMMUser CurrentUser { get; set; }

        private IShokoServer _shokoservices;
        public IShokoServer ShokoServices
        {
            get
            {
                if (_shokoservices == null)
                {
                    try
                    {
                        SetupClient();
                    }
                    catch
                    {
                        // ignored
                    }
                }
                return _shokoservices;
            }
        }
        public ObservableCollection<VM_CloudAccount> FolderProviders { get; set; }=new ObservableCollection<VM_CloudAccount>();
        public void RefreshCloudAccounts()
        {

            FolderProviders = new ObservableCollection<VM_CloudAccount>(Instance.ShokoServices.GetCloudProviders().Cast<VM_CloudAccount>());
        }
        private IShokoServerImage _imageClient;
        public IShokoServerImage ShokoImages
        {
            get
            {
                if (_imageClient == null)
                {
                    try
                    {
                        SetupImageClient();
                    }
                    catch
                    {
                        // ignored
                    }
                }
                return _imageClient;
            }
        }



        public static bool SettingsAreValid()
        {
            if (string.IsNullOrEmpty(AppSettings.JMMServer_Address) || string.IsNullOrEmpty(AppSettings.JMMServer_Port))
                return false;


            return true;
        }

        public void SetupImageClient()
        {
            //ServerOnline = false;
            _imageClient = null;

            if (!SettingsAreValid()) return;

            try
            {
                _imageClient = ClientFactory.Create<IShokoServerImage>($"http://{AppSettings.JMMServer_Address}:{AppSettings.JMMServer_Port}/");
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public bool SetupClient()
        {
            ServerOnline = false;
            _shokoservices = null;

            if (!SettingsAreValid()) return false;

            try
            {
                Dictionary<Type, Type> mappings = new Dictionary<Type, Type>();

                //Mappings area.
                mappings.Add(typeof(CL_AniDB_Anime), typeof(VM_AniDB_Anime));
                mappings.Add(typeof(CL_AniDB_Anime_DefaultImage), typeof(VM_AniDB_Anime_DefaultImage));
                mappings.Add(typeof(CL_AniDB_Anime_Relation), typeof(VM_AniDB_Anime_Relation));
                mappings.Add(typeof(CL_AniDB_Anime_Similar), typeof(VM_AniDB_Anime_Similar));
                mappings.Add(typeof(CL_AniDB_AnimeCrossRefs), typeof(VM_AniDB_AnimeCrossRefs));
                mappings.Add(typeof(CL_AniDB_AnimeDetailed), typeof(VM_AniDB_AnimeDetailed));
                mappings.Add(typeof(CL_AniDB_Character), typeof(VM_AniDB_Character));
                mappings.Add(typeof(AniDB_Episode), typeof(VM_AniDB_Episode));
                mappings.Add(typeof(AniDB_Recommendation), typeof(VM_AniDB_Recommendation));
                mappings.Add(typeof(AniDB_Seiyuu), typeof(VM_AniDB_Seiyuu));
                mappings.Add(typeof(CL_AnimeEpisode_User), typeof(VM_AnimeEpisode_User));
                mappings.Add(typeof(CL_AnimeGroup_User), typeof(VM_AnimeGroup_User));
                mappings.Add(typeof(CL_AnimeRating), typeof(VM_AnimeRating));
                mappings.Add(typeof(CL_AnimeSearch), typeof(VM_AnimeSearch));
                mappings.Add(typeof(CL_AnimeSeries_User), typeof(VM_AnimeSeries_User));
                mappings.Add(typeof(CL_AnimeTitle), typeof(VM_AnimeTitle));
                mappings.Add(typeof(CL_BookmarkedAnime), typeof(VM_BookmarkedAnime));
                mappings.Add(typeof(CL_CloudAccount), typeof(VM_CloudAccount));
                mappings.Add(typeof(CrossRef_AniDB_MAL), typeof(VM_CrossRef_AniDB_MAL));
                mappings.Add(typeof(CL_CrossRef_AniDB_MAL_Response), typeof(VM_CrossRef_AniDB_MAL_Response));
                mappings.Add(typeof(Azure_CrossRef_AniDB_Trakt), typeof(VM_CrossRef_AniDB_TraktV2));
                mappings.Add(typeof(Azure_CrossRef_AniDB_TvDB), typeof(VM_CrossRef_AniDB_TvDBV2));
                mappings.Add(typeof(CL_DuplicateFile), typeof(VM_DuplicateFile));
                mappings.Add(typeof(CL_GroupFileSummary), typeof(VM_GroupFileSummary));
                mappings.Add(typeof(CL_GroupFilter), typeof(VM_GroupFilter));
                mappings.Add(typeof(GroupFilterCondition), typeof(VM_GroupFilterCondition));
                mappings.Add(typeof(CL_IgnoreAnime), typeof(VM_IgnoreAnime));
                mappings.Add(typeof(ImportFolder), typeof(VM_ImportFolder));
                mappings.Add(typeof(JMMUser), typeof(VM_JMMUser));
                mappings.Add(typeof(CL_MALAnime_Response), typeof(VM_MALAnime_Response));
                mappings.Add(typeof(CL_MissingEpisode), typeof(VM_MissingEpisode));
                mappings.Add(typeof(CL_MissingFile), typeof(VM_MissingFile));
                mappings.Add(typeof(MovieDB_Fanart), typeof(VM_MovieDB_Fanart));
                mappings.Add(typeof(MovieDB_Movie), typeof(VM_MovieDB_Movie));
                mappings.Add(typeof(MovieDB_Poster), typeof(VM_MovieDB_Poster));
                mappings.Add(typeof(CL_MovieDBMovieSearch_Response), typeof(VM_MovieDBMovieSearch_Response));
                mappings.Add(typeof(Playlist), typeof(VM_Playlist));
                mappings.Add(typeof(CL_Recommendation), typeof(VM_Recommendation));
                mappings.Add(typeof(RenameScript), typeof(VM_RenameScript));
                mappings.Add(typeof(CL_Trakt_Comment), typeof(VM_Trakt_Comment));
                mappings.Add(typeof(CL_Trakt_CommentUser), typeof(VM_Trakt_CommentUser));
                mappings.Add(typeof(Trakt_Episode), typeof(VM_Trakt_Episode));
                mappings.Add(typeof(Trakt_ImageFanart), typeof(VM_Trakt_ImageFanart));
                mappings.Add(typeof(Trakt_ImagePoster), typeof(VM_Trakt_ImagePoster));
                mappings.Add(typeof(CL_Trakt_Show), typeof(VM_Trakt_Show));
                mappings.Add(typeof(TvDB_Episode), typeof(VM_TvDB_Episode));
                mappings.Add(typeof(TvDB_ImageFanart), typeof(VM_TvDB_ImageFanart));
                mappings.Add(typeof(TvDB_ImagePoster), typeof(VM_TvDB_ImagePoster));
                mappings.Add(typeof(TvDB_ImageWideBanner), typeof(VM_TvDB_ImageWideBanner));
                mappings.Add(typeof(TvDB_Language), typeof(VM_TvDB_Language));
                mappings.Add(typeof(TvDB_Series), typeof(VM_TvDB_Series));
                mappings.Add(typeof(TVDB_Series_Search_Response), typeof(VM_TVDB_Series_Search_Response));
                mappings.Add(typeof(CL_VideoDetailed), typeof(VM_VideoDetailed));
                mappings.Add(typeof(CL_VideoLocal_Renamed), typeof(VM_VideoLocal_Renamed));
                mappings.Add(typeof(CL_VideoLocal), typeof(VM_VideoLocal));
                mappings.Add(typeof(CrossRef_AniDB_TraktV2), typeof(VM_CrossRef_AniDB_TraktV2));
                mappings.Add(typeof(CrossRef_AniDB_TvDBV2), typeof(VM_CrossRef_AniDB_TvDBV2));
                mappings.Add(typeof(CL_GroupVideoQuality), typeof(VM_GroupVideoQuality));
                _shokoservices = ClientFactory.Create<IShokoServer>($"http://{AppSettings.JMMServer_Address}:{AppSettings.JMMServer_Port}/",mappings);
                // try connecting to see if the server is responding
                Instance.ShokoServices.GetServerStatus();
                ServerOnline = true;
                GetServerSettings();
                return true;
            }
            catch (Exception ex)
            {
                logger.Trace("Unable to connect to JMM Server. Internal exception given: " + ex.Message);
                Utils.ShowErrorMessage(ex);
                return false;
            }

        }

        private void SetShowServerSettings()
        {
            if (CurrentUser == null)
            {
                ShowServerSettings = false;
                return;
            }

            ShowServerSettings = ServerOnline && CurrentUser.CanEditSettings;
        }

        public bool LoginAsLastUser()
        {
            if (string.IsNullOrEmpty(AppSettings.LastLoginUsername)) return false;

            VM_JMMUser retUser = (VM_JMMUser)Instance.ShokoServices.AuthenticateUser(AppSettings.LastLoginUsername, "");
            if (retUser != null)
            {
                CurrentUser = retUser;
                Username = CurrentUser.Username;
                IsAdminUser = CurrentUser.IsAdmin == 1;
                UserAuthenticated = true;
                SetShowServerSettings();

                return true;
            }

            return false;
        }

        public bool AuthenticateUser()
        {
            //CurrentUser = null;
            //Username = "";

            LoginForm frm = new LoginForm();

            bool? result = frm.ShowDialog();
            if (result.HasValue)
            {
                UserAuthenticated = result.Value;
                if (UserAuthenticated)
                {
                    CurrentUser = frm.ThisUser;
                    Username = CurrentUser.Username;
                    IsAdminUser = CurrentUser.IsAdmin == 1;
                    AppSettings.LastLoginUsername = CurrentUser.Username;
                }
            }
            else
                UserAuthenticated = false;

            SetShowServerSettings();

            return UserAuthenticated;
        }


        public void GetServerSettings()
        {
            CL_ServerSettings contract = ShokoServices.GetServerSettings();

            AniDB_Username = contract.AniDB_Username;
            AniDB_Password = contract.AniDB_Password;
            AniDB_ServerAddress = contract.AniDB_ServerAddress;
            AniDB_ServerPort = contract.AniDB_ServerPort;
            AniDB_ClientPort = contract.AniDB_ClientPort;
            AniDB_AVDumpClientPort = contract.AniDB_AVDumpClientPort;
            AniDB_AVDumpKey = contract.AniDB_AVDumpKey;

            AniDB_DownloadRelatedAnime = contract.AniDB_DownloadRelatedAnime;
            AniDB_DownloadSimilarAnime = contract.AniDB_DownloadSimilarAnime;
            AniDB_DownloadReviews = contract.AniDB_DownloadReviews;
            AniDB_DownloadReleaseGroups = contract.AniDB_DownloadReleaseGroups;

            AniDB_MyList_AddFiles = contract.AniDB_MyList_AddFiles;
            AniDB_MyList_StorageState = contract.AniDB_MyList_StorageState;
            AniDB_MyList_DeleteType = (AniDBFileDeleteType)contract.AniDB_MyList_DeleteType;
            AniDB_MyList_ReadWatched = contract.AniDB_MyList_ReadWatched;
            AniDB_MyList_ReadUnwatched = contract.AniDB_MyList_ReadUnwatched;
            AniDB_MyList_SetWatched = contract.AniDB_MyList_SetWatched;
            AniDB_MyList_SetUnwatched = contract.AniDB_MyList_SetUnwatched;

            AniDB_Anime_UpdateFrequency = (ScheduledUpdateFrequency)contract.AniDB_Anime_UpdateFrequency;
            AniDB_Calendar_UpdateFrequency = (ScheduledUpdateFrequency)contract.AniDB_Calendar_UpdateFrequency;
            AniDB_MyList_UpdateFrequency = (ScheduledUpdateFrequency)contract.AniDB_MyList_UpdateFrequency;
            AniDB_MyListStats_UpdateFrequency = (ScheduledUpdateFrequency)contract.AniDB_MyListStats_UpdateFrequency;
            AniDB_File_UpdateFrequency = (ScheduledUpdateFrequency)contract.AniDB_File_UpdateFrequency;

            AniDB_DownloadCharacters = contract.AniDB_DownloadCharacters;
            AniDB_DownloadCreators = contract.AniDB_DownloadCreators;

            // Web Cache
            WebCache_Address = contract.WebCache_Address;
            WebCache_Anonymous = contract.WebCache_Anonymous;
            WebCache_TvDB_Get = contract.WebCache_TvDB_Get;
            WebCache_TvDB_Send = contract.WebCache_TvDB_Send;
            WebCache_Trakt_Get = contract.WebCache_Trakt_Get;
            WebCache_Trakt_Send = contract.WebCache_Trakt_Send;
            WebCache_MAL_Get = contract.WebCache_MAL_Get;
            WebCache_MAL_Send = contract.WebCache_MAL_Send;
            WebCache_XRefFileEpisode_Get = contract.WebCache_XRefFileEpisode_Get;
            WebCache_XRefFileEpisode_Send = contract.WebCache_XRefFileEpisode_Send;
            WebCache_UserInfo = contract.WebCache_UserInfo;

            // TvDB
            TvDB_AutoFanart = contract.TvDB_AutoFanart;
            TvDB_AutoFanartAmount = contract.TvDB_AutoFanartAmount;
            TvDB_AutoWideBanners = contract.TvDB_AutoWideBanners;
            TvDB_AutoWideBannersAmount = contract.TvDB_AutoWideBannersAmount;
            TvDB_AutoPosters = contract.TvDB_AutoPosters;
            TvDB_AutoPostersAmount = contract.TvDB_AutoPostersAmount;
            TvDB_UpdateFrequency = (ScheduledUpdateFrequency)contract.TvDB_UpdateFrequency;
            TvDB_Language = contract.TvDB_Language;

            // MovieDB
            MovieDB_AutoFanart = contract.MovieDB_AutoFanart;
            MovieDB_AutoFanartAmount = contract.MovieDB_AutoFanartAmount;
            MovieDB_AutoPosters = contract.MovieDB_AutoPosters;
            MovieDB_AutoPostersAmount = contract.MovieDB_AutoPostersAmount;

            // Import settings
            VideoExtensions = contract.VideoExtensions;
            AutoGroupSeries = contract.AutoGroupSeries;
            AutoGroupSeriesUseScoreAlgorithm = contract.AutoGroupSeriesUseScoreAlgorithm;
            AutoGroupSeriesRelationExclusions = contract.AutoGroupSeriesRelationExclusions;
            FileQualityFilterEnabled = contract.FileQualityFilterEnabled;
            FileQualityPreferences = contract.FileQualityFilterPreferences;
            UseEpisodeStatus = contract.Import_UseExistingFileWatchedStatus;
            RunImportOnStart = contract.RunImportOnStart;
            ScanDropFoldersOnStart = contract.ScanDropFoldersOnStart;
            Hash_CRC32 = contract.Hash_CRC32;
            Hash_MD5 = contract.Hash_MD5;
            Hash_SHA1 = contract.Hash_SHA1;

            // Language
            LanguagePreference = contract.LanguagePreference;
            LanguageUseSynonyms = contract.LanguageUseSynonyms;
            EpisodeTitleSource = (DataSourceType)contract.EpisodeTitleSource;
            SeriesDescriptionSource = (DataSourceType)contract.SeriesDescriptionSource;
            SeriesNameSource = (DataSourceType)contract.SeriesNameSource;

            // trakt
            Trakt_IsEnabled = contract.Trakt_IsEnabled;
            Trakt_AuthToken = contract.Trakt_AuthToken;
            Trakt_RefreshToken = contract.Trakt_RefreshToken;
            Trakt_TokenExpirationDate = contract.Trakt_TokenExpirationDate;
            Trakt_UpdateFrequency = (ScheduledUpdateFrequency)contract.Trakt_UpdateFrequency;
            Trakt_SyncFrequency = (ScheduledUpdateFrequency)contract.Trakt_SyncFrequency;
            Trakt_DownloadFanart = contract.Trakt_DownloadFanart;
            Trakt_DownloadPosters = contract.Trakt_DownloadPosters;
            Trakt_DownloadEpisodes = contract.Trakt_DownloadEpisodes;

            // MAL
            MAL_Username = contract.MAL_Username;
            MAL_Password = contract.MAL_Password;
            MAL_UpdateFrequency = (ScheduledUpdateFrequency)contract.MAL_UpdateFrequency;
            MAL_NeverDecreaseWatchedNums = contract.MAL_NeverDecreaseWatchedNums;

            Plex_ServerHost = contract.Plex_ServerHost ?? "";
            Plex_Sections = string.IsNullOrEmpty(contract.Plex_Sections) ? new ObservableCollection<int>() : new ObservableCollection<int>(contract.Plex_Sections.Split(',').Select(int.Parse).ToList());
        }



        public void SaveServerSettingsAsync()
        {
            saveTimer?.Stop();

            saveTimer = new Timer();
            saveTimer.AutoReset = false;
            saveTimer.Interval = 1 * 1000; // 1 second
            saveTimer.Elapsed += saveTimer_Elapsed;
            saveTimer.Enabled = true;
        }

        void saveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SaveServerSettings();
        }

        private void SaveServerSettings()
        {
            try
            {
                CL_ServerSettings contract = new CL_ServerSettings();

                contract.AniDB_Username = AniDB_Username;
                contract.AniDB_Password = AniDB_Password;
                contract.AniDB_ServerAddress = AniDB_ServerAddress;
                contract.AniDB_ServerPort = AniDB_ServerPort;
                contract.AniDB_ClientPort = AniDB_ClientPort;
                contract.AniDB_AVDumpClientPort = AniDB_AVDumpClientPort;
                contract.AniDB_AVDumpKey = AniDB_AVDumpKey;

                contract.AniDB_DownloadRelatedAnime = AniDB_DownloadRelatedAnime;
                contract.AniDB_DownloadSimilarAnime = AniDB_DownloadSimilarAnime;
                contract.AniDB_DownloadReviews = AniDB_DownloadReviews;
                contract.AniDB_DownloadReleaseGroups = AniDB_DownloadReleaseGroups;

                contract.AniDB_MyList_AddFiles = AniDB_MyList_AddFiles;
                contract.AniDB_MyList_StorageState = AniDB_MyList_StorageState;
                contract.AniDB_MyList_DeleteType = (int)AniDB_MyList_DeleteType;
                contract.AniDB_MyList_ReadWatched = AniDB_MyList_ReadWatched;
                contract.AniDB_MyList_ReadUnwatched = AniDB_MyList_ReadUnwatched;
                contract.AniDB_MyList_SetWatched = AniDB_MyList_SetWatched;
                contract.AniDB_MyList_SetUnwatched = AniDB_MyList_SetUnwatched;

                contract.AniDB_Anime_UpdateFrequency = (int)AniDB_Anime_UpdateFrequency;
                contract.AniDB_Calendar_UpdateFrequency = (int)AniDB_Calendar_UpdateFrequency;
                contract.AniDB_MyList_UpdateFrequency = (int)AniDB_MyList_UpdateFrequency;
                contract.AniDB_MyListStats_UpdateFrequency = (int)AniDB_MyListStats_UpdateFrequency;
                contract.AniDB_File_UpdateFrequency = (int)AniDB_File_UpdateFrequency;

                contract.AniDB_DownloadCharacters = AniDB_DownloadCharacters;
                contract.AniDB_DownloadCreators = AniDB_DownloadCreators;

                // Web Cache
                contract.WebCache_Address = WebCache_Address;
                contract.WebCache_Anonymous = WebCache_Anonymous;
                contract.WebCache_TvDB_Get = WebCache_TvDB_Get;
                contract.WebCache_TvDB_Send = WebCache_TvDB_Send;
                contract.WebCache_Trakt_Get = WebCache_Trakt_Get;
                contract.WebCache_Trakt_Send = WebCache_Trakt_Send;
                contract.WebCache_MAL_Get = WebCache_MAL_Get;
                contract.WebCache_MAL_Send = WebCache_MAL_Send;
                contract.WebCache_XRefFileEpisode_Get = WebCache_XRefFileEpisode_Get;
                contract.WebCache_XRefFileEpisode_Send = WebCache_XRefFileEpisode_Send;
                contract.WebCache_UserInfo = WebCache_UserInfo;

                // TvDB
                contract.TvDB_AutoFanart = TvDB_AutoFanart;
                contract.TvDB_AutoFanartAmount = TvDB_AutoFanartAmount;
                contract.TvDB_AutoWideBanners = TvDB_AutoWideBanners;
                contract.TvDB_AutoWideBannersAmount = TvDB_AutoWideBannersAmount;
                contract.TvDB_AutoPosters = TvDB_AutoPosters;
                contract.TvDB_AutoPostersAmount = TvDB_AutoPostersAmount;
                contract.TvDB_UpdateFrequency = (int)TvDB_UpdateFrequency;
                contract.TvDB_Language = TvDB_Language;

                // MovieDB
                contract.MovieDB_AutoFanart = MovieDB_AutoFanart;
                contract.MovieDB_AutoFanartAmount = MovieDB_AutoFanartAmount;
                contract.MovieDB_AutoPosters = MovieDB_AutoPosters;
                contract.MovieDB_AutoPostersAmount = MovieDB_AutoPostersAmount;

                // Import settings
                contract.VideoExtensions = VideoExtensions;
                contract.Import_UseExistingFileWatchedStatus = UseEpisodeStatus;
                contract.RunImportOnStart = RunImportOnStart;
                contract.AutoGroupSeries = AutoGroupSeries;
                contract.AutoGroupSeriesUseScoreAlgorithm = AutoGroupSeriesUseScoreAlgorithm;
                contract.AutoGroupSeriesRelationExclusions = AutoGroupSeriesRelationExclusions;
                contract.FileQualityFilterEnabled = FileQualityFilterEnabled;
                contract.FileQualityFilterPreferences = FileQualityPreferences;
                contract.ScanDropFoldersOnStart = ScanDropFoldersOnStart;
                contract.Hash_CRC32 = Hash_CRC32;
                contract.Hash_MD5 = Hash_MD5;
                contract.Hash_SHA1 = Hash_SHA1;

                // Language
                contract.LanguagePreference = LanguagePreference;
                contract.LanguageUseSynonyms = LanguageUseSynonyms;
                contract.EpisodeTitleSource = (int)EpisodeTitleSource;
                contract.SeriesDescriptionSource = (int)SeriesDescriptionSource;
                contract.SeriesNameSource = (int)SeriesNameSource;

                // trakt
                contract.Trakt_IsEnabled = Trakt_IsEnabled;
                contract.Trakt_AuthToken = Trakt_AuthToken;
                contract.Trakt_RefreshToken = Trakt_RefreshToken;
                contract.Trakt_TokenExpirationDate = Trakt_TokenExpirationDate;
                contract.Trakt_UpdateFrequency = (int)Trakt_UpdateFrequency;
                contract.Trakt_SyncFrequency = (int)Trakt_SyncFrequency;
                contract.Trakt_DownloadFanart = Trakt_DownloadFanart;
                contract.Trakt_DownloadPosters = Trakt_DownloadPosters;
                contract.Trakt_DownloadEpisodes = Trakt_DownloadEpisodes;

                // MAL
                contract.MAL_Username = MAL_Username;
                contract.MAL_Password = MAL_Password;
                contract.MAL_UpdateFrequency = (int)MAL_UpdateFrequency;
                contract.MAL_NeverDecreaseWatchedNums = MAL_NeverDecreaseWatchedNums;
                
                //plex
                contract.Plex_ServerHost = Plex_ServerHost;
                contract.Plex_Sections = string.Join(",", Plex_Sections);

                CL_Response response = Instance.ShokoServices.SaveServerSettings(contract);
                if (response.ErrorMessage.Length > 0)
                    Utils.ShowErrorMessage(response.ErrorMessage);
            }
            catch (Exception ex)
            {
                logger.Trace("Error saving server JMM Server Settings. Internal exception given: " + ex.Message);
                //Utils.ShowErrorMessage(ex);
            }
        }

        public void TestAniDBLogin()
        {
            try
            {
                SaveServerSettings();

                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

                string response = ShokoServices.TestAniDBConnection();
                MessageBox.Show(response, Resources.AniDBLogin, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }


        }

        public void AuthorizeTraktPIN(string pin)
        {
            try
            {
                SaveServerSettings();

                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

                string response = ShokoServices.EnterTraktPIN(pin);
                MessageBox.Show(response, Shoko.Commons.Properties.Resources.ShokoServer_TraktAuth, MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void TestMALLogin()
        {
            try
            {
                SaveServerSettings();

                string response = ShokoServices.TestMALLogin();
                if (string.IsNullOrEmpty(response))
                    MessageBox.Show(Resources.MAL_LoginCorrect, Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show(response, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        #region Observable Properties

        public ObservableCollection<VM_ImportFolder> ImportFolders { get; set; }
        public ObservableCollection<VM_JMMUser> AllUsers { get; set; }
        public ObservableCollection<string> AllTags { get; set; }
        public ObservableCollection<CustomTag> AllCustomTags { get; set; }
        public ICollectionView ViewCustomTagsAll { get; set; }

        public ObservableCollection<NamingLanguage> UnselectedLanguages { get; set; }
        public ObservableCollection<NamingLanguage> SelectedLanguages { get; set; }

        private bool isBanned;
        public bool IsBanned
        {
            get { return isBanned; }
            set
            {
                this.SetField(()=>isBanned,value);
            }
        }

        private bool adminMessagesAvailable;
        public bool AdminMessagesAvailable
        {
            get { return adminMessagesAvailable; }
            set
            {
                this.SetField(()=>adminMessagesAvailable,value);
            }
        }

        private bool isAdminUser;
        public bool IsAdminUser
        {
            get { return isAdminUser; }
            set
            {
                this.SetField(()=>isAdminUser,value);
            }
        }

        private string banReason = "";
        public string BanReason
        {
            get { return banReason; }
            set
            {
                this.SetField(()=>banReason,value);
            }
        }

        private string banOrigin = "";
        public string BanOrigin
        {
            get { return banOrigin; }
            set
            {
                this.SetField(()=>banOrigin,value);
            }
        }

        private string baseImagePath = "";
        public string BaseImagePath
        {
            get { return baseImagePath; }
            set
            {
                this.SetField(()=>baseImagePath,value);
            }
        }

        private bool baseImagesPathIsDefault = true;
        public bool BaseImagesPathIsDefault
        {
            get { return baseImagesPathIsDefault; }
            set
            {
                this.SetField(()=>baseImagesPathIsDefault,value);
            }
        }

        private string username = "";
        public string Username
        {
            get { return username; }
            set
            {
                this.SetField(()=>username,value);
            }
        }

        private int hasherQueueCount;
        public int HasherQueueCount
        {
            get { return hasherQueueCount; }
            set
            {
                this.SetField(()=>hasherQueueCount,value);
            }
        }

        private string hasherQueueState = "";
        public string HasherQueueState
        {
            get { return hasherQueueState; }
            set
            {
                this.SetField(()=>hasherQueueState,value);
            }
        }

        private int serverImageQueueCount;
        public int ServerImageQueueCount
        {
            get { return serverImageQueueCount; }
            set
            {
                this.SetField(()=>serverImageQueueCount,value);
            }
        }

        private string serverImageQueueState = "";
        public string ServerImageQueueState
        {
            get { return serverImageQueueState; }
            set
            {
                this.SetField(()=>serverImageQueueState,value);
            }
        }

        private int generalQueueCount;
        public int GeneralQueueCount
        {
            get { return generalQueueCount; }
            set
            {
                this.SetField(()=>generalQueueCount,value);
            }
        }

        private string generalQueueState = "";
        public string GeneralQueueState
        {
            get { return generalQueueState; }
            set
            {
                this.SetField(()=>generalQueueState,value);
            }
        }

        private bool hasherQueuePaused;
        public bool HasherQueuePaused
        {
            get { return hasherQueuePaused; }
            set
            {
                this.SetField(()=>hasherQueuePaused,value);
            }
        }

        private bool hasherQueueRunning = true;
        public bool HasherQueueRunning
        {
            get { return hasherQueueRunning; }
            set
            {
                this.SetField(()=>hasherQueueRunning,value);
            }
        }

        private bool serverImageQueuePaused;
        public bool ServerImageQueuePaused
        {
            get { return serverImageQueuePaused; }
            set
            {
                this.SetField(()=>serverImageQueuePaused,value);
            }
        }

        private bool serverImageQueueRunning = true;
        public bool ServerImageQueueRunning
        {
            get { return serverImageQueueRunning; }
            set
            {
                this.SetField(()=>serverImageQueueRunning,value);
            }
        }

        private bool generalQueuePaused;
        public bool GeneralQueuePaused
        {
            get { return generalQueuePaused; }
            set
            {
                this.SetField(()=>generalQueuePaused,value);
            }
        }

        private bool generalQueueRunning;
        public bool GeneralQueueRunning
        {
            get { return generalQueueRunning; }
            set
            {
                this.SetField(()=>generalQueueRunning,value);
            }
        }

        private bool serverOnline = true;
        public bool ServerOnline
        {
            get
            {
                return serverOnline;
            }
            set
            {
                this.SetField(()=>serverOnline,value);
                SetShowServerSettings();
            }
        }

        private bool showCommunity;
        public bool ShowCommunity
        {
            get { return showCommunity; }
            set
            {
                this.SetField(()=>showCommunity,value);
            }
        }

        private bool showServerSettings;
        public bool ShowServerSettings
        {
            get { return showServerSettings; }
            set
            {
                this.SetField(()=>showServerSettings,value);
            }
        }

        private bool newVersionAvailable;
        public bool NewVersionAvailable
        {
            get { return newVersionAvailable; }
            set
            {
                this.SetField(()=>newVersionAvailable,value);
            }
        }

        private string newVersionNumber = "";
        public string NewVersionNumber
        {
            get { return newVersionNumber; }
            set
            {
                this.SetField(()=>newVersionNumber,value);
            }
        }

        private string newVersionDownloadLink = "";
        public string NewVersionDownloadLink
        {
            get { return newVersionDownloadLink; }
            set
            {
                this.SetField(()=>newVersionDownloadLink,value);
            }
        }

        private string applicationVersion = "";
        public string ApplicationVersion
        {
            get { return applicationVersion; }
            set
            {
                this.SetField(()=>applicationVersion,value);
            }
        }

        private string applicationVersionLatest = "";
        public string ApplicationVersionLatest
        {
            get { return applicationVersionLatest; }
            set
            {
                this.SetField(()=>applicationVersionLatest,value);
            }
        }


        #endregion

        #region Server Settings

        private string aniDB_Username = "";
        public string AniDB_Username
        {
            get { return aniDB_Username; }
            set
            {
                this.SetField(()=>aniDB_Username,value);
            }
        }

        private string aniDB_Password = "";
        public string AniDB_Password
        {
            get { return aniDB_Password; }
            set
            {
                this.SetField(()=>aniDB_Password,value);
            }
        }

        private string aniDB_ServerAddress = "";
        public string AniDB_ServerAddress
        {
            get { return aniDB_ServerAddress; }
            set
            {
                this.SetField(()=>aniDB_ServerAddress,value);
            }
        }

        private string aniDB_ServerPort = "";
        public string AniDB_ServerPort
        {
            get { return aniDB_ServerPort; }
            set
            {
                this.SetField(()=>aniDB_ServerPort,value);
            }
        }

        private string aniDB_ClientPort = "";
        public string AniDB_ClientPort
        {
            get { return aniDB_ClientPort; }
            set
            {
                this.SetField(()=>aniDB_ClientPort,value);
            }
        }

        private string aniDB_AVDumpClientPort = "";
        public string AniDB_AVDumpClientPort
        {
            get { return aniDB_AVDumpClientPort; }
            set
            {
                this.SetField(()=>aniDB_AVDumpClientPort,value);
            }
        }

        private string aniDB_AVDumpKey = "";
        public string AniDB_AVDumpKey
        {
            get { return aniDB_AVDumpKey; }
            set
            {
                this.SetField(()=>aniDB_AVDumpKey,value);
            }
        }

        private bool aniDB_DownloadRelatedAnime;
        public bool AniDB_DownloadRelatedAnime
        {
            get { return aniDB_DownloadRelatedAnime; }
            set
            {
                this.SetField(()=>aniDB_DownloadRelatedAnime,value);
            }
        }

        private bool aniDB_DownloadSimilarAnime;
        public bool AniDB_DownloadSimilarAnime
        {
            get { return aniDB_DownloadSimilarAnime; }
            set
            {
                this.SetField(()=>aniDB_DownloadSimilarAnime,value);
            }
        }

        private bool aniDB_DownloadReviews;
        public bool AniDB_DownloadReviews
        {
            get { return aniDB_DownloadReviews; }
            set
            {
                this.SetField(()=>aniDB_DownloadReviews,value);
            }
        }

        private bool aniDB_DownloadReleaseGroups;
        public bool AniDB_DownloadReleaseGroups
        {
            get { return aniDB_DownloadReleaseGroups; }
            set
            {
                this.SetField(()=>aniDB_DownloadReleaseGroups,value);
            }
        }

        private bool aniDB_MyList_AddFiles;
        public bool AniDB_MyList_AddFiles
        {
            get { return aniDB_MyList_AddFiles; }
            set
            {
                this.SetField(()=>aniDB_MyList_AddFiles,value);
            }
        }

        private int aniDB_MyList_StorageState = 1;
        public int AniDB_MyList_StorageState
        {
            get { return aniDB_MyList_StorageState; }
            set
            {
                this.SetField(()=>aniDB_MyList_StorageState,value);
            }
        }

        private AniDBFileDeleteType aniDB_MyList_DeleteType = AniDBFileDeleteType.Delete;
        public AniDBFileDeleteType AniDB_MyList_DeleteType
        {
            get { return aniDB_MyList_DeleteType; }
            set
            {
                this.SetField(()=>aniDB_MyList_DeleteType,value);
            }
        }

        private bool aniDB_MyList_ReadWatched;
        public bool AniDB_MyList_ReadWatched
        {
            get { return aniDB_MyList_ReadWatched; }
            set
            {
                this.SetField(()=>aniDB_MyList_ReadWatched,value);
            }
        }

        private bool aniDB_MyList_ReadUnwatched;
        public bool AniDB_MyList_ReadUnwatched
        {
            get { return aniDB_MyList_ReadUnwatched; }
            set
            {
                this.SetField(()=>aniDB_MyList_ReadUnwatched,value);
            }
        }

        private bool aniDB_MyList_SetWatched;
        public bool AniDB_MyList_SetWatched
        {
            get { return aniDB_MyList_SetWatched; }
            set
            {
                this.SetField(()=>aniDB_MyList_SetWatched,value);
            }
        }

        private bool aniDB_MyList_SetUnwatched;
        public bool AniDB_MyList_SetUnwatched
        {
            get { return aniDB_MyList_SetUnwatched; }
            set
            {
                this.SetField(()=>aniDB_MyList_SetUnwatched,value);
            }
        }

        private ScheduledUpdateFrequency aniDB_MyList_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve;
        public ScheduledUpdateFrequency AniDB_MyList_UpdateFrequency
        {
            get { return aniDB_MyList_UpdateFrequency; }
            set
            {
                this.SetField(()=>aniDB_MyList_UpdateFrequency,value);
            }
        }

        private ScheduledUpdateFrequency aniDB_MyListStats_UpdateFrequency = ScheduledUpdateFrequency.Never;
        public ScheduledUpdateFrequency AniDB_MyListStats_UpdateFrequency
        {
            get { return aniDB_MyListStats_UpdateFrequency; }
            set
            {
                this.SetField(()=>aniDB_MyListStats_UpdateFrequency,value);
            }
        }

        private ScheduledUpdateFrequency aniDB_Calendar_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve;
        public ScheduledUpdateFrequency AniDB_Calendar_UpdateFrequency
        {
            get { return aniDB_Calendar_UpdateFrequency; }
            set
            {
                this.SetField(()=>aniDB_Calendar_UpdateFrequency,value);
            }
        }

        private ScheduledUpdateFrequency aniDB_Anime_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve;
        public ScheduledUpdateFrequency AniDB_Anime_UpdateFrequency
        {
            get { return aniDB_Anime_UpdateFrequency; }
            set
            {
                this.SetField(()=>aniDB_Anime_UpdateFrequency,value);
            }
        }

        private ScheduledUpdateFrequency aniDB_File_UpdateFrequency = ScheduledUpdateFrequency.Daily;
        public ScheduledUpdateFrequency AniDB_File_UpdateFrequency
        {
            get { return aniDB_File_UpdateFrequency; }
            set
            {
                this.SetField(()=>aniDB_File_UpdateFrequency,value);
            }
        }

        private bool aniDB_DownloadCharacters;
        public bool AniDB_DownloadCharacters
        {
            get { return aniDB_DownloadCharacters; }
            set
            {
                this.SetField(()=>aniDB_DownloadCharacters,value);
            }
        }

        private bool aniDB_DownloadCreators;
        public bool AniDB_DownloadCreators
        {
            get { return aniDB_DownloadCreators; }
            set
            {
                this.SetField(()=>aniDB_DownloadCreators,value);
            }
        }

        private string webCache_Address = "";
        public string WebCache_Address
        {
            get { return webCache_Address; }
            set
            {
                this.SetField(()=>webCache_Address,value);
            }
        }

        private bool webCache_Anonymous;
        public bool WebCache_Anonymous
        {
            get { return webCache_Anonymous; }
            set
            {
                this.SetField(()=>webCache_Anonymous,value);
            }
        }

        private bool webCache_XRefFileEpisode_Get;
        public bool WebCache_XRefFileEpisode_Get
        {
            get { return webCache_XRefFileEpisode_Get; }
            set
            {
                this.SetField(()=>webCache_XRefFileEpisode_Get,value);
            }
        }

        private bool webCache_XRefFileEpisode_Send;
        public bool WebCache_XRefFileEpisode_Send
        {
            get { return webCache_XRefFileEpisode_Send; }
            set
            {
                this.SetField(()=>webCache_XRefFileEpisode_Send,value);
            }
        }

        private bool webCache_TvDB_Get;
        public bool WebCache_TvDB_Get
        {
            get { return webCache_TvDB_Get; }
            set
            {
                this.SetField(()=>webCache_TvDB_Get,value);
            }
        }

        private bool webCache_TvDB_Send;
        public bool WebCache_TvDB_Send
        {
            get { return webCache_TvDB_Send; }
            set
            {
                this.SetField(()=>webCache_TvDB_Send,value);
            }
        }

        private bool webCache_Trakt_Get;
        public bool WebCache_Trakt_Get
        {
            get { return webCache_Trakt_Get; }
            set
            {
                this.SetField(()=>webCache_Trakt_Get,value);
            }
        }

        private bool webCache_Trakt_Send;
        public bool WebCache_Trakt_Send
        {
            get { return webCache_Trakt_Send; }
            set
            {
                this.SetField(()=>webCache_Trakt_Send,value);
            }
        }


        private bool webCache_MAL_Get;
        public bool WebCache_MAL_Get
        {
            get { return webCache_MAL_Get; }
            set
            {
                this.SetField(()=>webCache_MAL_Get,value);
            }
        }

        private bool webCache_MAL_Send;
        public bool WebCache_MAL_Send
        {
            get { return webCache_MAL_Send; }
            set
            {
                this.SetField(()=>webCache_MAL_Send,value);
            }
        }

        private bool webCache_UserInfo;
        public bool WebCache_UserInfo
        {
            get { return webCache_UserInfo; }
            set
            {
                this.SetField(()=>webCache_UserInfo,value);
            }
        }


        private bool tvDB_AutoFanart;
        public bool TvDB_AutoFanart
        {
            get { return tvDB_AutoFanart; }
            set
            {
                this.SetField(()=>tvDB_AutoFanart,value);
            }
        }

        private int tvDB_AutoFanartAmount = 10;
        public int TvDB_AutoFanartAmount
        {
            get { return tvDB_AutoFanartAmount; }
            set
            {
                this.SetField(()=>tvDB_AutoFanartAmount,value);
            }
        }

        private bool tvDB_AutoWideBanners;
        public bool TvDB_AutoWideBanners
        {
            get { return tvDB_AutoWideBanners; }
            set
            {
                this.SetField(()=>tvDB_AutoWideBanners,value);
            }
        }

        private int tvDB_AutoWideBannersAmount = 10;
        public int TvDB_AutoWideBannersAmount
        {
            get { return tvDB_AutoWideBannersAmount; }
            set
            {
                this.SetField(()=>tvDB_AutoWideBannersAmount,value);
            }
        }

        private bool tvDB_AutoPosters;
        public bool TvDB_AutoPosters
        {
            get { return tvDB_AutoPosters; }
            set
            {
                this.SetField(()=>tvDB_AutoPosters,value);
            }
        }

        private int tvDB_AutoPostersAmount = 10;
        public int TvDB_AutoPostersAmount
        {
            get { return tvDB_AutoPostersAmount; }
            set
            {
                this.SetField(()=>tvDB_AutoPostersAmount,value);
            }
        }

        private string tvDB_Language = "";
        public string TvDB_Language
        {
            get { return tvDB_Language; }
            set
            {
                this.SetField(()=>tvDB_Language,value);
            }
        }

        private ScheduledUpdateFrequency tvDB_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve;
        public ScheduledUpdateFrequency TvDB_UpdateFrequency
        {
            get { return tvDB_UpdateFrequency; }
            set
            {
                this.SetField(()=>tvDB_UpdateFrequency,value);
            }
        }


        private bool movieDB_AutoFanart;
        public bool MovieDB_AutoFanart
        {
            get { return movieDB_AutoFanart; }
            set
            {
                this.SetField(()=>movieDB_AutoFanart,value);
            }
        }

        private int movieDB_AutoFanartAmount = 10;
        public int MovieDB_AutoFanartAmount
        {
            get { return movieDB_AutoFanartAmount; }
            set
            {
                this.SetField(()=>movieDB_AutoFanartAmount,value);
            }
        }

        private bool movieDB_AutoPosters;
        public bool MovieDB_AutoPosters
        {
            get { return movieDB_AutoPosters; }
            set
            {
                this.SetField(()=>movieDB_AutoPosters,value);
            }
        }

        private int movieDB_AutoPostersAmount = 10;
        public int MovieDB_AutoPostersAmount
        {
            get { return movieDB_AutoPostersAmount; }
            set
            {
                this.SetField(()=>movieDB_AutoPostersAmount,value);
            }
        }

        private string videoExtensions = "";
        public string VideoExtensions
        {
            get { return videoExtensions; }
            set
            {
                this.SetField(()=>videoExtensions,value);
            }
        }

        private bool autoGroupSeries;
        public bool AutoGroupSeries
        {
            get { return autoGroupSeries; }
            set
            {
                this.SetField(()=>autoGroupSeries,value);
            }
        }

	    private bool autoGroupSeriesUseScoreAlgorithm;
	    public bool AutoGroupSeriesUseScoreAlgorithm
	    {
		    get { return autoGroupSeriesUseScoreAlgorithm; }
		    set
		    {
			    this.SetField(()=>autoGroupSeriesUseScoreAlgorithm,value);
		    }
	    }

	    // The actual server setting
        private string autoGroupSeriesRelationExclusions = "";
        private string AutoGroupSeriesRelationExclusions
        {
            get
            {
                return autoGroupSeriesRelationExclusions;
            }
            set
            {
                this.SetField(()=>autoGroupSeriesRelationExclusions,value);
            }
        }

        private bool isRelationInExclusion(string relation)
        {
            if (AutoGroupSeriesRelationExclusions == null)
                return false;

            foreach (string a in AutoGroupSeriesRelationExclusions.Split('|'))
            {
                // relation will always be lowercase, but a may not be yet
                if (a.ToLowerInvariant().Equals(relation.ToLowerInvariant())) return true;
            }
            return false;
        }

        private void setRelationinExclusion(string setting, bool value)
        {
            string final = AutoGroupSeriesRelationExclusions;
            if (value)
            {
                if (!isRelationInExclusion(setting))
                {
                    // remove all trailing bars that may have been added
                    do
                    {
                        if (final.EndsWith("|"))
                            final = final.Substring(0, final.Length - 1);
                        else
                            break;
                    } while (true);
                    // if not empty, add a single bar to separate
                    if (final.Length > 0) final = final + "|";
                    final = final + setting;
                    AutoGroupSeriesRelationExclusions = final;
                }
            }
            else
            {
                if (isRelationInExclusion(setting))
                {
                    final = "";
                    foreach (string a in AutoGroupSeriesRelationExclusions.Split('|'))
                    {
                        if (a.Length == 0) continue;
                        // add all except value and fix any uppercase
                        if (!a.ToLowerInvariant().Equals(setting.ToLowerInvariant())) final += a.ToLowerInvariant() + "|";
                    }
                    // this will be "" if all are unchecked
                    // remove last '|' added in previous loop
                    if (final.EndsWith("|"))
                        final = final.Substring(0, final.Length - 1);
                    AutoGroupSeriesRelationExclusions = final;
                }
            }
        }

	    public bool RelationAllowDissimilarTitleExclusion
	    {
		    get
		    {
			    return isRelationInExclusion("AllowDissimilarTitleExclusion");
		    }
		    set
		    {
			    setRelationinExclusion("AllowDissimilarTitleExclusion", value);
		    }
	    }

	    public bool RelationExcludeOVA
        {
            get
            {
                return isRelationInExclusion("ova");
            }
            set
            {
                setRelationinExclusion("ova", value);
            }
        }

        public bool RelationExcludeMovie
        {
            get
            {
                return isRelationInExclusion("movie");
            }
            set
            {
                setRelationinExclusion("movie", value);
            }
        }

        public bool RelationExcludeSameSetting
        {
            get
            {
                return isRelationInExclusion("same setting");
            }
            set
            {
                setRelationinExclusion("same setting", value);
            }
        }

        public bool RelationExcludeAltSetting
        {
            get
            {
                return isRelationInExclusion("alternate setting");
            }
            set
            {
                setRelationinExclusion("alternate setting", value);
            }
        }

        public bool RelationExcludeAltVersion
        {
            get
            {
                return isRelationInExclusion("alternate version");
            }
            set
            {
                setRelationinExclusion("alternate version", value);
            }
        }

        public bool RelationExcludeCharacter
        {
            get
            {
                return isRelationInExclusion("character");
            }
            set
            {
                setRelationinExclusion("character", value);
            }
        }

        public bool RelationExcludeSideStory
        {
            get
            {
                return isRelationInExclusion("side story");
            }
            set
            {
                setRelationinExclusion("side story", value);
            }
        }

        public bool RelationExcludeParentStory
        {
            get
            {
                return isRelationInExclusion("parent story");
            }
            set
            {
                setRelationinExclusion("parent story", value);
            }
        }

        public bool RelationExcludeSummary
        {
            get
            {
                return isRelationInExclusion("summary");
            }
            set
            {
                setRelationinExclusion("summary", value);
            }
        }

        public bool RelationExcludeFullStory
        {
            get
            {
                return isRelationInExclusion("full story");
            }
            set
            {
                setRelationinExclusion("full story", value);
            }
        }

        public bool RelationExcludeOther
        {
            get
            {
                return isRelationInExclusion("other");
            }
            set
            {
                setRelationinExclusion("other", value);
            }
        }

        private bool _fileQualityFilterEnabled;
        public bool FileQualityFilterEnabled
        {
            get { return _fileQualityFilterEnabled; }
            set
            {
                this.SetField(()=>_fileQualityFilterEnabled,value);
            }
        }

        // The actual server setting
        private string _fileQualityPreferences = "";
        public string FileQualityPreferences
        {
            get
            {
                return _fileQualityPreferences;
            }
            set
            {
                this.SetField(()=>_fileQualityPreferences,value);
            }
        }

        private bool useEpisodeStatus;
        public bool UseEpisodeStatus
        {
            get { return useEpisodeStatus; }
            set
            {
                this.SetField(()=>useEpisodeStatus,value);
            }
        }

        private bool runImportOnStart;
        public bool RunImportOnStart
        {
            get { return runImportOnStart; }
            set
            {
                this.SetField(()=>runImportOnStart,value);
            }
        }

        private bool scanDropFoldersOnStart;
        public bool ScanDropFoldersOnStart
        {
            get { return scanDropFoldersOnStart; }
            set
            {
                this.SetField(()=>scanDropFoldersOnStart,value);
            }
        }


        private bool hash_CRC32;
        public bool Hash_CRC32
        {
            get { return hash_CRC32; }
            set
            {
                this.SetField(()=>hash_CRC32,value);
            }
        }

        private bool hash_MD5;
        public bool Hash_MD5
        {
            get { return hash_MD5; }
            set
            {
                this.SetField(()=>hash_MD5,value);
            }
        }

        private bool hash_SHA1;
        public bool Hash_SHA1
        {
            get { return hash_SHA1; }
            set
            {
                this.SetField(()=>hash_SHA1,value);
            }
        }

        private string languagePreference = "en,x-jat";
        public string LanguagePreference
        {
            get { return languagePreference; }
            set
            {
                this.SetField(()=>languagePreference,value);
            }
        }

        private bool languageUseSynonyms;
        public bool LanguageUseSynonyms
        {
            get { return languageUseSynonyms; }
            set
            {
                this.SetField(()=>languageUseSynonyms,value);
            }
        }

        private DataSourceType episodeTitleSource = DataSourceType.AniDB;
        public DataSourceType EpisodeTitleSource
        {
            get { return episodeTitleSource; }
            set
            {
                this.SetField(()=>episodeTitleSource,value);
            }
        }

        private DataSourceType seriesDescriptionSource = DataSourceType.AniDB;
        public DataSourceType SeriesDescriptionSource
        {
            get { return seriesDescriptionSource; }
            set
            {
                this.SetField(()=>seriesDescriptionSource,value);
            }
        }

        private DataSourceType seriesNameSource = DataSourceType.AniDB;
        public DataSourceType SeriesNameSource
        {
            get { return seriesNameSource; }
            set
            {
                this.SetField(()=>seriesNameSource,value);
            }
        }

        private bool trakt_IsEnabled = true;
        public bool Trakt_IsEnabled
        {
            get { return trakt_IsEnabled; }
            set
            {
                this.SetField(()=>trakt_IsEnabled,value);
            }
        }

        private string trakt_AuthToken = "";
        public string Trakt_AuthToken
        {
            get { return trakt_AuthToken; }
            set
            {
                this.SetField(()=>trakt_AuthToken,value);
            }
        }

        private string trakt_RefreshToken = "";
        public string Trakt_RefreshToken
        {
            get { return trakt_RefreshToken; }
            set
            {
                this.SetField(()=>trakt_RefreshToken,value);
            }
        }

        private string trakt_TokenExpirationDate = "";
        public string Trakt_TokenExpirationDate
        {
            get { return trakt_TokenExpirationDate; }
            set
            {
                this.SetField(()=>trakt_TokenExpirationDate,value);
            }
        }

        private bool trakt_DownloadFanart = true;
        public bool Trakt_DownloadFanart
        {
            get { return trakt_DownloadFanart; }
            set
            {
                this.SetField(()=>trakt_DownloadFanart,value);
            }
        }

        private bool trakt_DownloadPosters = true;
        public bool Trakt_DownloadPosters
        {
            get { return trakt_DownloadPosters; }
            set
            {
                this.SetField(()=>trakt_DownloadPosters,value);
            }
        }

        private bool trakt_DownloadEpisodes = true;
        public bool Trakt_DownloadEpisodes
        {
            get { return trakt_DownloadEpisodes; }
            set
            {
                this.SetField(()=>trakt_DownloadEpisodes,value);
            }
        }

        private string mAL_Username = "";
        public string MAL_Username
        {
            get { return mAL_Username; }
            set
            {
                this.SetField(()=>mAL_Username,value);
            }
        }

        private string mAL_Password = "";
        public string MAL_Password
        {
            get { return mAL_Password; }
            set
            {
                this.SetField(()=>mAL_Password,value);
            }
        }

        private ScheduledUpdateFrequency mAL_UpdateFrequency = ScheduledUpdateFrequency.Daily;
        public ScheduledUpdateFrequency MAL_UpdateFrequency
        {
            get { return mAL_UpdateFrequency; }
            set
            {
                this.SetField(()=>mAL_UpdateFrequency,value);
            }
        }

        private bool mAL_NeverDecreaseWatchedNums;
        public bool MAL_NeverDecreaseWatchedNums
        {
            get { return mAL_NeverDecreaseWatchedNums; }
            set
            {
                this.SetField(()=>mAL_NeverDecreaseWatchedNums,value);
            }
        }

        private ScheduledUpdateFrequency trakt_UpdateFrequency = ScheduledUpdateFrequency.Daily;
        public ScheduledUpdateFrequency Trakt_UpdateFrequency
        {
            get { return trakt_UpdateFrequency; }
            set
            {
                this.SetField(()=>trakt_UpdateFrequency,value);
            }
        }

        private ScheduledUpdateFrequency trakt_SyncFrequency = ScheduledUpdateFrequency.Daily;
        public ScheduledUpdateFrequency Trakt_SyncFrequency
        {
            get { return trakt_SyncFrequency; }
            set
            {
                this.SetField(()=>trakt_SyncFrequency,value);
            }
        }

        private string plex_ServerHost = "";
        public string Plex_ServerHost
        {
            get { return plex_ServerHost; }
            set { this.SetField(() => plex_ServerHost, value); }
        }

        private ObservableCollection<int> plex_Sections = new ObservableCollection<int>();
        public ObservableCollection<int> Plex_Sections
        {
            get { return plex_Sections; }
            set { this.SetField(() => plex_Sections, value); }
        }

        public ObservableCollection<Azure_AdminMessage> AdminMessages { get; set; }
        #endregion

        public static VM_ShokoServer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new VM_ShokoServer();
                    _instance.Init();
                }
                return _instance;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }


        private VM_ShokoServer()
        {

        }

        public void Test()
        {
        }

        private void Init()
        {
            UserAuthenticated = false;
            ImportFolders = new ObservableCollection<VM_ImportFolder>();
            UnselectedLanguages = new ObservableCollection<NamingLanguage>();
            SelectedLanguages = new ObservableCollection<NamingLanguage>();
            AllUsers = new ObservableCollection<VM_JMMUser>();
            AllTags = new ObservableCollection<string>();
            AllCustomTags = new ObservableCollection<CustomTag>();
            ViewCustomTagsAll = CollectionViewSource.GetDefaultView(Instance.AllCustomTags);
            ViewCustomTagsAll.SortDescriptions.Add(new SortDescription("TagName", ListSortDirection.Ascending));

            AdminMessages = new ObservableCollection<Azure_AdminMessage>();

            try
            {
                //SetupClient();
                //SetupTCPClient();
                SetupClient();
            }
            catch
            {
                // ignored
            }

            // timer for server status
            serverStatusTimer = new Timer();
            serverStatusTimer.AutoReset = false;
            serverStatusTimer.Interval = 4 * 1000; // 4 seconds
            serverStatusTimer.Elapsed += serverStatusTimer_Elapsed;
            serverStatusTimer.Enabled = true;
        }

        void serverStatusTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!ServerOnline)
                {
                    serverStatusTimer.Start();
                    return;
                }

                updateServerStatus();
            }

            catch
            {
                // ignored
            }

            serverStatusTimer.Start();
        }

        void updateServerStatus()
        {
            try
            {
                TimeSpan ts = DateTime.Now - lastVersionCheck;

                CL_ServerStatus status = Instance.ShokoServices.GetServerStatus();
                CL_AppVersions appv = null;
                if (ts.TotalMinutes > 180)
                {
                    //appv = VM_ShokoServer.Instance.clientBinaryHTTP.GetAppVersions();

                    lastVersionCheck = DateTime.Now;
                    // check for admin messages
                    AdminMessages.Clear();
                    List<Azure_AdminMessage> msgs = Instance.ShokoServices.GetAdminMessages();
                    if (msgs != null)
                    {
                        foreach (Azure_AdminMessage msg in msgs)
                        {
                            AdminMessages.Add(msg);
                        }
                    }

                    AdminMessagesAvailable = AdminMessages.Count > 0;

                    // check if this user is allowed to admin the web cache
                    ShowCommunity = Instance.ShokoServices.IsWebCacheAdmin();
                }

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                {
                    HasherQueueCount = status.HashQueueCount;
                    GeneralQueueCount = status.GeneralQueueCount;
                    ServerImageQueueCount = status.ImagesQueueCount;

                    QueueStateStruct queueState = new QueueStateStruct { queueState = (QueueStateEnum)status.HashQueueStateId, extraParams = status.HashQueueStateParams.ToArray() };
                    HasherQueueState = queueState.formatMessage();
                    HasherQueuePaused = queueState.queueState == QueueStateEnum.Paused;
                    HasherQueueRunning = queueState.queueState != QueueStateEnum.Paused;

                    
                    queueState.queueState = (QueueStateEnum)status.GeneralQueueStateId;
                    queueState.extraParams = status.GeneralQueueStateParams.ToArray();
                    GeneralQueueState = queueState.formatMessage();
                    GeneralQueuePaused = queueState.queueState == QueueStateEnum.Paused;
                    GeneralQueueRunning = queueState.queueState != QueueStateEnum.Paused;

                    queueState.queueState = (QueueStateEnum)status.ImagesQueueStateId;
                    queueState.extraParams = status.ImagesQueueStateParams.ToArray();
                    ServerImageQueueState = queueState.formatMessage();
                    ServerImageQueuePaused = queueState.queueState == QueueStateEnum.Paused;
                    ServerImageQueueRunning = queueState.queueState != QueueStateEnum.Paused;

                    IsBanned = status.IsBanned;
                    BanReason = status.BanReason;
                    BanOrigin = status.BanOrigin;

                    if (appv != null)
                    {
                        string curVersion = Utils.GetApplicationVersion(Assembly.GetExecutingAssembly());

                        string[] latestNumbers = appv.JMMDesktopVersion.Split('.');
                        string[] curNumbers = curVersion.Split('.');

                        string latestMajor = $"{latestNumbers[0]}.{latestNumbers[1]}";
                        string curMajor = $"{curNumbers[0]}.{curNumbers[1]}";

                        decimal lmajor = decimal.Parse(latestMajor);
                        decimal cmajor = decimal.Parse(curMajor);

                        NewVersionAvailable = false;

                        if (lmajor > cmajor)
                        {
                            NewVersionAvailable = true;
                            NewVersionDownloadLink = appv.JMMDesktopDownload;
                            NewVersionNumber = appv.JMMDesktopVersion;
                        }
                        else if (lmajor == cmajor)
                        {
                            if (int.Parse(latestNumbers[2]) > int.Parse(curNumbers[2]))
                            {
                                NewVersionAvailable = true;
                                NewVersionDownloadLink = appv.JMMDesktopDownload;
                                NewVersionNumber = appv.JMMDesktopVersion;
                            }
                        }


                    }
                });
            }

            catch
            {
                // ignored
            }
        }

        public void UpdateServerStatus()
        {
            updateServerStatus();
        }

        public void RemoveNamingLanguage(string oldLan)
        {
            string[] lans = LanguagePreference.Split(',');
            LanguagePreference = string.Empty;

            foreach (string lan in lans)
            {
                if (string.IsNullOrEmpty(lan)) continue;
                if (lan.Trim().Length < 2) continue;
                if (lan.Trim().ToUpper() == oldLan.Trim().ToUpper()) continue;

                if (!string.IsNullOrEmpty(LanguagePreference))
                    LanguagePreference += ",";

                LanguagePreference += lan;
            }

            RefreshNamingLanguages();
            SaveServerSettings();
        }

        public int MoveUpNamingLanguage(string moveLan)
        {
            string[] lans = LanguagePreference.Split(',');

            List<string> languages = new List<string>();

            // get a list of valid languages
            foreach (string lan in lans)
            {
                if (string.IsNullOrEmpty(lan)) continue;
                if (lan.Trim().Length < 2) continue;
                languages.Add(lan);
            }

            // find the position of the language to be moved
            int pos = -1;
            for (int i = 0; i < languages.Count; i++)
            {
                if (languages[i].Trim().ToUpper() == moveLan.Trim().ToUpper()) pos = i;
            }

            if (pos == -1) return -1; // not found
            if (pos == 0) return -1; // already at top

            string lan1 = languages[pos - 1];
            languages[pos - 1] = moveLan;
            languages[pos] = lan1;

            LanguagePreference = string.Empty;
            foreach (string lan in languages)
            {
                if (!string.IsNullOrEmpty(LanguagePreference))
                    LanguagePreference += ",";

                LanguagePreference += lan;
            }


            RefreshNamingLanguages();
            SaveServerSettings();

            return pos - 1;
        }

        public int MoveDownNamingLanguage(string moveLan)
        {
            string[] lans = LanguagePreference.Split(',');

            List<string> languages = new List<string>();

            // get a list of valid languages
            foreach (string lan in lans)
            {
                if (string.IsNullOrEmpty(lan)) continue;
                if (lan.Trim().Length < 2) continue;
                languages.Add(lan);
            }

            // find the position of the language to be moved
            int pos = -1;
            for (int i = 0; i < languages.Count; i++)
            {
                if (languages[i].Trim().ToUpper() == moveLan.Trim().ToUpper()) pos = i;
            }

            if (pos == -1) return -1; // not found
            if (pos == languages.Count - 1) return -1; // already at bottom

            string lan1 = languages[pos + 1];
            languages[pos + 1] = moveLan;
            languages[pos] = lan1;

            LanguagePreference = string.Empty;
            foreach (string lan in languages)
            {
                if (!string.IsNullOrEmpty(LanguagePreference))
                    LanguagePreference += ",";

                LanguagePreference += lan;
            }


            RefreshNamingLanguages();
            SaveServerSettings();

            return pos + 1;
        }

        public void AddNamingLanguage(string newLan)
        {
            if (!string.IsNullOrEmpty(LanguagePreference))
                LanguagePreference += ",";

            LanguagePreference += newLan;

            RefreshNamingLanguages();
            SaveServerSettings();
        }

        public void RefreshNamingLanguages()
        {
            UnselectedLanguages.Clear();
            SelectedLanguages.Clear();

            if (!ServerOnline) return;
            try
            {
                string[] lans = LanguagePreference.Split(',');

                foreach (string lan in lans)
                {
                    if (string.IsNullOrEmpty(lan)) continue;
                    if (lan.Trim().Length < 2) continue;

                    NamingLanguage selLan = new NamingLanguage(lan);
                    SelectedLanguages.Add(selLan);
                }

                foreach (NamingLanguage nlan in Languages.AllNamingLanguages)
                {
                    bool inSelected = false;
                    foreach (NamingLanguage selLan in SelectedLanguages)
                    {
                        if (nlan.Language.Trim().ToUpper() == selLan.Language.Trim().ToUpper())
                        {
                            inSelected = true;
                            break;
                        }
                    }
                    if (!inSelected)
                        UnselectedLanguages.Add(nlan);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void RenameAllGroups()
        {
            if (!ServerOnline) return;
            try
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

                string msg = Instance.ShokoServices.RenameAllGroups();
                if (string.IsNullOrEmpty(msg))
                    MessageBox.Show(Resources.Language_RenameComplete, Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    Utils.ShowErrorMessage(msg);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        public void RefreshImportFolders()
        {
            ImportFolders.Clear();

            if (!ServerOnline) return;
            try
            {
                List<VM_ImportFolder> importFolders = Instance.ShokoServices.GetImportFolders().CastList<VM_ImportFolder>();

                foreach (VM_ImportFolder ifolder in importFolders)
                {
                    ImportFolders.Add(ifolder);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        public void RefreshAllUsers()
        {
            lock (userLock)
            {
                AllUsers.Clear();

                if (!ServerOnline) return;
                try
                {
                    List<VM_JMMUser> users = Instance.ShokoServices.GetAllUsers().CastList<VM_JMMUser>();
                    foreach (VM_JMMUser jmmUser in users)
                    {
                        if (CurrentUser != null && CurrentUser.JMMUserID == jmmUser.JMMUserID)
                        {
                            CurrentUser = jmmUser;
                            SetShowServerSettings();
                        }
                        AllUsers.Add(jmmUser);
                    }
                }
                catch (Exception ex)
                {
                    Utils.ShowErrorMessage(ex);
                }
            }

        }

        public void RefreshAllTags()
        {

            AllTags.Clear();

            if (!ServerOnline) return;
            try
            {
                List<string> tagsRaw = Instance.ShokoServices.GetAllTagNames();

                foreach (string tag in tagsRaw)
                    AllTags.Add(tag);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        public void RefreshAllCustomTags()
        {

            AllCustomTags.Clear();

            if (!ServerOnline) return;
            try
            {
                List<CustomTag> tagsRaw = Instance.ShokoServices.GetAllCustomTags().CastList<CustomTag>();

                foreach (CustomTag tag in tagsRaw)
                    AllCustomTags.Add(tag);

                ViewCustomTagsAll.Refresh();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        public void RunImport()
        {
            if (!ServerOnline) return;
            Instance.ShokoServices.RunImport();
        }

        public void RemoveMissingFiles()
        {
            if (!ServerOnline) return;
            Instance.ShokoServices.RemoveMissingFiles();
        }

        public void SyncMyList()
        {
            if (!ServerOnline) return;
            Instance.ShokoServices.SyncMyList();
        }

        public void SyncVotes()
        {
            if (!ServerOnline) return;
            Instance.ShokoServices.SyncVotes();
        }

        public void RevokeVote(int animeID)
        {
            if (!ServerOnline) return;
            Instance.ShokoServices.VoteAnimeRevoke(animeID);
        }

        public void VoteAnime(int animeID, decimal voteValue, int voteType)
        {
            if (!ServerOnline) return;
            Instance.ShokoServices.VoteAnime(animeID, voteValue, voteType);
        }
    }
}

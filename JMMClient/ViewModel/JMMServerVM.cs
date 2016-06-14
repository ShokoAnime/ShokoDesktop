using JMMClient.Forms;
using JMMClient.ViewModel;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using System.Windows;

namespace JMMClient
{
    public class JMMServerVM : INotifyPropertyChanged
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static JMMServerVM _instance;
        private System.Timers.Timer serverStatusTimer = null;
        private System.Timers.Timer saveTimer = null;
        private static DateTime lastVersionCheck = DateTime.Now.AddDays(-5);

        public object userLock = new object();
        public bool UserAuthenticated { get; set; }
        public JMMUserVM CurrentUser { get; set; }

        private JMMServerBinary.IJMMServer _clientBinaryHTTP = null;
        public JMMServerBinary.IJMMServer clientBinaryHTTP
        {
            get
            {
                if (_clientBinaryHTTP == null)
                {
                    try
                    {
                        SetupBinaryClient();
                    }
                    catch { }
                }
                return _clientBinaryHTTP;
            }
        }

        private JMMImageServer.JMMServerImageClient _imageClient = null;
        public JMMImageServer.JMMServerImageClient imageClient
        {
            get
            {
                if (_imageClient == null)
                {
                    try
                    {
                        SetupImageClient();
                    }
                    catch { }
                }
                return _imageClient;
            }
        }

        private JMMServerStreaming.IJMMServerStreaming _streamingClient = null;
        public JMMServerStreaming.IJMMServerStreaming streamingClient
        {
            get
            {
                if (_streamingClient == null)
                {
                    try
                    {
                        SetupStreamingClient();
                    }
                    catch { }
                }
                return _streamingClient;
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
                string url = string.Format(@"http://{0}:{1}/JMMServerImage", AppSettings.JMMServer_Address, AppSettings.JMMServer_Port);
                BasicHttpBinding binding = new BasicHttpBinding();
                binding.MessageEncoding = WSMessageEncoding.Mtom;
                binding.MaxReceivedMessageSize = 2147483647;
                binding.ReaderQuotas.MaxArrayLength = 2147483647;
                EndpointAddress endpoint = new EndpointAddress(new Uri(url));
                _imageClient = new JMMImageServer.JMMServerImageClient(binding, endpoint);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void SetupStreamingClient()
        {
            //ServerOnline = false;
            _streamingClient = null;

            if (!SettingsAreValid()) return;

            try
            {
                string url = string.Format(@"net.tcp://{0}:{1}/JMMServerStreaming", AppSettings.JMMServer_Address, AppSettings.JMMServer_FilePort);


                NetTcpBinding netTCPbinding = new NetTcpBinding();
                netTCPbinding.TransferMode = TransferMode.Streamed;
                netTCPbinding.SendTimeout = TimeSpan.MaxValue;
                netTCPbinding.MaxReceivedMessageSize = int.MaxValue;

                EndpointAddress endpoint = new EndpointAddress(new Uri(url));

                var factory = new ChannelFactory<JMMServerStreaming.IJMMServerStreamingChannel>(netTCPbinding, endpoint);
                foreach (OperationDescription op in factory.Endpoint.Contract.Operations)
                {
                    var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
                    if (dataContractBehavior != null)
                    {
                        dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
                    }
                }

                _streamingClient = factory.CreateChannel();


            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public bool SetupBinaryClient()
        {
            ServerOnline = false;
            _clientBinaryHTTP = null;

            if (!SettingsAreValid()) return false;

            try
            {

                string url = string.Format(@"http://{0}:{1}/JMMServerBinary", AppSettings.JMMServer_Address, AppSettings.JMMServer_Port);

                BinaryMessageEncodingBindingElement encoding = new BinaryMessageEncodingBindingElement();
                encoding.ReaderQuotas.MaxArrayLength = int.MaxValue;
                encoding.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
                encoding.ReaderQuotas.MaxDepth = int.MaxValue;
                encoding.ReaderQuotas.MaxNameTableCharCount = int.MaxValue;
                encoding.ReaderQuotas.MaxStringContentLength = int.MaxValue;

                HttpTransportBindingElement transport = new HttpTransportBindingElement();
                transport.MaxReceivedMessageSize = int.MaxValue;
                transport.MaxBufferPoolSize = int.MaxValue;
                transport.MaxBufferSize = int.MaxValue;
                transport.MaxReceivedMessageSize = int.MaxValue;


                Binding binding = new CustomBinding(encoding, transport);

                binding.SendTimeout = new TimeSpan(30, 0, 30);
                binding.ReceiveTimeout = new TimeSpan(30, 0, 30);
                binding.OpenTimeout = new TimeSpan(30, 0, 30);
                binding.CloseTimeout = new TimeSpan(30, 0, 30);

                EndpointAddress endpoint = new EndpointAddress(new Uri(url));

                var factory = new ChannelFactory<JMMServerBinary.IJMMServerChannel>(binding, endpoint);
                foreach (OperationDescription op in factory.Endpoint.Contract.Operations)
                {
                    var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
                    if (dataContractBehavior != null)
                    {
                        dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
                    }
                }

                _clientBinaryHTTP = factory.CreateChannel();

                // try connecting to see if the server is responding
                JMMServerBinary.Contract_ServerStatus status = JMMServerVM.Instance.clientBinaryHTTP.GetServerStatus();
                ServerOnline = true;

                GetServerSettings();

                return true;
            }
            catch (Exception ex)
            {
                logger.Trace("Unable to connect to JMM Server. Internal exception given: " + ex.Message);
                //Utils.ShowErrorMessage(ex);
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

            JMMServerBinary.Contract_JMMUser retUser = JMMServerVM.Instance.clientBinaryHTTP.AuthenticateUser(AppSettings.LastLoginUsername, "");
            if (retUser != null)
            {
                CurrentUser = new JMMUserVM(retUser);
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
            if (result.Value)
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
            JMMServerBinary.Contract_ServerSettings contract = _clientBinaryHTTP.GetServerSettings();

            this.AniDB_Username = contract.AniDB_Username;
            this.AniDB_Password = contract.AniDB_Password;
            this.AniDB_ServerAddress = contract.AniDB_ServerAddress;
            this.AniDB_ServerPort = contract.AniDB_ServerPort;
            this.AniDB_ClientPort = contract.AniDB_ClientPort;
            this.AniDB_AVDumpClientPort = contract.AniDB_AVDumpClientPort;
            this.AniDB_AVDumpKey = contract.AniDB_AVDumpKey;

            this.AniDB_DownloadRelatedAnime = contract.AniDB_DownloadRelatedAnime;
            this.AniDB_DownloadSimilarAnime = contract.AniDB_DownloadSimilarAnime;
            this.AniDB_DownloadReviews = contract.AniDB_DownloadReviews;
            this.AniDB_DownloadReleaseGroups = contract.AniDB_DownloadReleaseGroups;

            this.AniDB_MyList_AddFiles = contract.AniDB_MyList_AddFiles;
            this.AniDB_MyList_StorageState = contract.AniDB_MyList_StorageState;
            this.AniDB_MyList_DeleteType = (AniDBFileDeleteType)contract.AniDB_MyList_DeleteType;
            this.AniDB_MyList_ReadWatched = contract.AniDB_MyList_ReadWatched;
            this.AniDB_MyList_ReadUnwatched = contract.AniDB_MyList_ReadUnwatched;
            this.AniDB_MyList_SetWatched = contract.AniDB_MyList_SetWatched;
            this.AniDB_MyList_SetUnwatched = contract.AniDB_MyList_SetUnwatched;

            this.AniDB_Anime_UpdateFrequency = (ScheduledUpdateFrequency)contract.AniDB_Anime_UpdateFrequency;
            this.AniDB_Calendar_UpdateFrequency = (ScheduledUpdateFrequency)contract.AniDB_Calendar_UpdateFrequency;
            this.AniDB_MyList_UpdateFrequency = (ScheduledUpdateFrequency)contract.AniDB_MyList_UpdateFrequency;
            this.AniDB_MyListStats_UpdateFrequency = (ScheduledUpdateFrequency)contract.AniDB_MyListStats_UpdateFrequency;
            this.AniDB_File_UpdateFrequency = (ScheduledUpdateFrequency)contract.AniDB_File_UpdateFrequency;

            this.AniDB_DownloadCharacters = contract.AniDB_DownloadCharacters;
            this.AniDB_DownloadCreators = contract.AniDB_DownloadCreators;

            // Web Cache
            this.WebCache_Address = contract.WebCache_Address;
            this.WebCache_Anonymous = contract.WebCache_Anonymous;
            this.WebCache_TvDB_Get = contract.WebCache_TvDB_Get;
            this.WebCache_TvDB_Send = contract.WebCache_TvDB_Send;
            this.WebCache_Trakt_Get = contract.WebCache_Trakt_Get;
            this.WebCache_Trakt_Send = contract.WebCache_Trakt_Send;
            this.WebCache_MAL_Get = contract.WebCache_MAL_Get;
            this.WebCache_MAL_Send = contract.WebCache_MAL_Send;
            this.WebCache_XRefFileEpisode_Get = contract.WebCache_XRefFileEpisode_Get;
            this.WebCache_XRefFileEpisode_Send = contract.WebCache_XRefFileEpisode_Send;
            this.WebCache_UserInfo = contract.WebCache_UserInfo;

            // TvDB
            this.TvDB_AutoFanart = contract.TvDB_AutoFanart;
            this.TvDB_AutoFanartAmount = contract.TvDB_AutoFanartAmount;
            this.TvDB_AutoWideBanners = contract.TvDB_AutoWideBanners;
            this.TvDB_AutoWideBannersAmount = contract.TvDB_AutoWideBannersAmount;
            this.TvDB_AutoPosters = contract.TvDB_AutoPosters;
            this.TvDB_AutoPostersAmount = contract.TvDB_AutoPostersAmount;
            this.TvDB_UpdateFrequency = (ScheduledUpdateFrequency)contract.TvDB_UpdateFrequency;
            this.TvDB_Language = contract.TvDB_Language;

            // MovieDB
            this.MovieDB_AutoFanart = contract.MovieDB_AutoFanart;
            this.MovieDB_AutoFanartAmount = contract.MovieDB_AutoFanartAmount;
            this.MovieDB_AutoPosters = contract.MovieDB_AutoPosters;
            this.MovieDB_AutoPostersAmount = contract.MovieDB_AutoPostersAmount;

            // Import settings
            this.VideoExtensions = contract.VideoExtensions;
            this.AutoGroupSeries = contract.AutoGroupSeries;
            this.AutoGroupSeriesRelationExclusions = contract.AutoGroupSeriesRelationExclusions;
            this.UseEpisodeStatus = contract.Import_UseExistingFileWatchedStatus;
            this.RunImportOnStart = contract.RunImportOnStart;
            this.ScanDropFoldersOnStart = contract.ScanDropFoldersOnStart;
            this.Hash_CRC32 = contract.Hash_CRC32;
            this.Hash_MD5 = contract.Hash_MD5;
            this.Hash_SHA1 = contract.Hash_SHA1;

            // Language
            this.LanguagePreference = contract.LanguagePreference;
            this.LanguageUseSynonyms = contract.LanguageUseSynonyms;
            this.EpisodeTitleSource = (DataSourceType)contract.EpisodeTitleSource;
            this.SeriesDescriptionSource = (DataSourceType)contract.SeriesDescriptionSource;
            this.SeriesNameSource = (DataSourceType)contract.SeriesNameSource;

            // trakt
            this.Trakt_IsEnabled = contract.Trakt_IsEnabled;
            this.Trakt_AuthToken = contract.Trakt_AuthToken;
            this.Trakt_RefreshToken = contract.Trakt_RefreshToken;
            this.Trakt_TokenExpirationDate = contract.Trakt_TokenExpirationDate;
            this.Trakt_UpdateFrequency = (ScheduledUpdateFrequency)contract.Trakt_UpdateFrequency;
            this.Trakt_SyncFrequency = (ScheduledUpdateFrequency)contract.Trakt_SyncFrequency;
            this.Trakt_DownloadFanart = contract.Trakt_DownloadFanart;
            this.Trakt_DownloadPosters = contract.Trakt_DownloadPosters;
            this.Trakt_DownloadEpisodes = contract.Trakt_DownloadEpisodes;

            // MAL
            this.MAL_Username = contract.MAL_Username;
            this.MAL_Password = contract.MAL_Password;
            this.MAL_UpdateFrequency = (ScheduledUpdateFrequency)contract.MAL_UpdateFrequency;
            this.MAL_NeverDecreaseWatchedNums = contract.MAL_NeverDecreaseWatchedNums;
        }



        public void SaveServerSettingsAsync()
        {
            if (saveTimer != null)
                saveTimer.Stop();

            saveTimer = new System.Timers.Timer();
            saveTimer.AutoReset = false;
            saveTimer.Interval = 1 * 1000; // 1 second
            saveTimer.Elapsed += new System.Timers.ElapsedEventHandler(saveTimer_Elapsed);
            saveTimer.Enabled = true;
        }

        void saveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SaveServerSettings();
        }

        private bool SaveServerSettings()
        {
            try
            {
                JMMServerBinary.Contract_ServerSettings contract = new JMMServerBinary.Contract_ServerSettings();

                contract.AniDB_Username = this.AniDB_Username;
                contract.AniDB_Password = this.AniDB_Password;
                contract.AniDB_ServerAddress = this.AniDB_ServerAddress;
                contract.AniDB_ServerPort = this.AniDB_ServerPort;
                contract.AniDB_ClientPort = this.AniDB_ClientPort;
                contract.AniDB_AVDumpClientPort = this.AniDB_AVDumpClientPort;
                contract.AniDB_AVDumpKey = this.AniDB_AVDumpKey;

                contract.AniDB_DownloadRelatedAnime = this.AniDB_DownloadRelatedAnime;
                contract.AniDB_DownloadSimilarAnime = this.AniDB_DownloadSimilarAnime;
                contract.AniDB_DownloadReviews = this.AniDB_DownloadReviews;
                contract.AniDB_DownloadReleaseGroups = this.AniDB_DownloadReleaseGroups;

                contract.AniDB_MyList_AddFiles = this.AniDB_MyList_AddFiles;
                contract.AniDB_MyList_StorageState = this.AniDB_MyList_StorageState;
                contract.AniDB_MyList_DeleteType = (int)this.AniDB_MyList_DeleteType;
                contract.AniDB_MyList_ReadWatched = this.AniDB_MyList_ReadWatched;
                contract.AniDB_MyList_ReadUnwatched = this.AniDB_MyList_ReadUnwatched;
                contract.AniDB_MyList_SetWatched = this.AniDB_MyList_SetWatched;
                contract.AniDB_MyList_SetUnwatched = this.AniDB_MyList_SetUnwatched;

                contract.AniDB_Anime_UpdateFrequency = (int)this.AniDB_Anime_UpdateFrequency;
                contract.AniDB_Calendar_UpdateFrequency = (int)this.AniDB_Calendar_UpdateFrequency;
                contract.AniDB_MyList_UpdateFrequency = (int)this.AniDB_MyList_UpdateFrequency;
                contract.AniDB_MyListStats_UpdateFrequency = (int)this.AniDB_MyListStats_UpdateFrequency;
                contract.AniDB_File_UpdateFrequency = (int)this.AniDB_File_UpdateFrequency;

                contract.AniDB_DownloadCharacters = this.AniDB_DownloadCharacters;
                contract.AniDB_DownloadCreators = this.AniDB_DownloadCreators;

                // Web Cache
                contract.WebCache_Address = this.WebCache_Address;
                contract.WebCache_Anonymous = this.WebCache_Anonymous;
                contract.WebCache_TvDB_Get = this.WebCache_TvDB_Get;
                contract.WebCache_TvDB_Send = this.WebCache_TvDB_Send;
                contract.WebCache_Trakt_Get = this.WebCache_Trakt_Get;
                contract.WebCache_Trakt_Send = this.WebCache_Trakt_Send;
                contract.WebCache_MAL_Get = this.WebCache_MAL_Get;
                contract.WebCache_MAL_Send = this.WebCache_MAL_Send;
                contract.WebCache_XRefFileEpisode_Get = this.WebCache_XRefFileEpisode_Get;
                contract.WebCache_XRefFileEpisode_Send = this.WebCache_XRefFileEpisode_Send;
                contract.WebCache_UserInfo = this.WebCache_UserInfo;

                // TvDB
                contract.TvDB_AutoFanart = this.TvDB_AutoFanart;
                contract.TvDB_AutoFanartAmount = this.TvDB_AutoFanartAmount;
                contract.TvDB_AutoWideBanners = this.TvDB_AutoWideBanners;
                contract.TvDB_AutoWideBannersAmount = this.TvDB_AutoWideBannersAmount;
                contract.TvDB_AutoPosters = this.TvDB_AutoPosters;
                contract.TvDB_AutoPostersAmount = this.TvDB_AutoPostersAmount;
                contract.TvDB_UpdateFrequency = (int)this.TvDB_UpdateFrequency;
                contract.TvDB_Language = this.TvDB_Language;

                // MovieDB
                contract.MovieDB_AutoFanart = this.MovieDB_AutoFanart;
                contract.MovieDB_AutoFanartAmount = this.MovieDB_AutoFanartAmount;
                contract.MovieDB_AutoPosters = this.MovieDB_AutoPosters;
                contract.MovieDB_AutoPostersAmount = this.MovieDB_AutoPostersAmount;

                // Import settings
                contract.VideoExtensions = this.VideoExtensions;
                contract.Import_UseExistingFileWatchedStatus = this.UseEpisodeStatus;
                contract.AutoGroupSeries = this.AutoGroupSeries;
                contract.RunImportOnStart = this.RunImportOnStart;
                contract.AutoGroupSeriesRelationExclusions = this.AutoGroupSeriesRelationExclusions;
                contract.ScanDropFoldersOnStart = this.ScanDropFoldersOnStart;
                contract.Hash_CRC32 = this.Hash_CRC32;
                contract.Hash_MD5 = this.Hash_MD5;
                contract.Hash_SHA1 = this.Hash_SHA1;

                // Language
                contract.LanguagePreference = this.LanguagePreference;
                contract.LanguageUseSynonyms = this.LanguageUseSynonyms;
                contract.EpisodeTitleSource = (int)this.EpisodeTitleSource;
                contract.SeriesDescriptionSource = (int)this.SeriesDescriptionSource;
                contract.SeriesNameSource = (int)this.SeriesNameSource;

                // trakt
                contract.Trakt_IsEnabled = this.Trakt_IsEnabled;
                contract.Trakt_AuthToken = this.Trakt_AuthToken;
                contract.Trakt_RefreshToken = this.Trakt_RefreshToken;
                contract.Trakt_TokenExpirationDate = this.Trakt_TokenExpirationDate;
                contract.Trakt_UpdateFrequency = (int)this.Trakt_UpdateFrequency;
                contract.Trakt_SyncFrequency = (int)this.Trakt_SyncFrequency;
                contract.Trakt_DownloadFanart = this.Trakt_DownloadFanart;
                contract.Trakt_DownloadPosters = this.Trakt_DownloadPosters;
                contract.Trakt_DownloadEpisodes = this.Trakt_DownloadEpisodes;

                // MAL
                contract.MAL_Username = this.MAL_Username;
                contract.MAL_Password = this.MAL_Password;
                contract.MAL_UpdateFrequency = (int)this.MAL_UpdateFrequency;
                contract.MAL_NeverDecreaseWatchedNums = this.MAL_NeverDecreaseWatchedNums;

                JMMServerBinary.Contract_ServerSettings_SaveResponse response = _clientBinaryHTTP.SaveServerSettings(contract);
                if (response.ErrorMessage.Length > 0)
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                logger.Trace("Error saving server JMM Server Settings. Internal exception given: " + ex.Message);
                //Utils.ShowErrorMessage(ex);
                return false;
            }
        }

        public void TestAniDBLogin()
        {
            try
            {
                SaveServerSettings();

                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

                string response = _clientBinaryHTTP.TestAniDBConnection();
                MessageBox.Show(response, Properties.Resources.AniDBLogin, MessageBoxButton.OK, MessageBoxImage.Information);
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

                string response = _clientBinaryHTTP.EnterTraktPIN(pin);
                MessageBox.Show(response, Properties.Resources.JMMServer_TraktAuth, MessageBoxButton.OK, MessageBoxImage.Information);

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

                string response = _clientBinaryHTTP.TestMALLogin();
                if (string.IsNullOrEmpty(response))
                    MessageBox.Show(Properties.Resources.MAL_LoginCorrect, Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show(response, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        #region Observable Properties

        public ObservableCollection<ImportFolderVM> ImportFolders { get; set; }
        public ObservableCollection<JMMUserVM> AllUsers { get; set; }
        public ObservableCollection<string> AllTags { get; set; }
        public ObservableCollection<CustomTagVM> AllCustomTags { get; set; }
        public ICollectionView ViewCustomTagsAll { get; set; }

        public ObservableCollection<NamingLanguage> UnselectedLanguages { get; set; }
        public ObservableCollection<NamingLanguage> SelectedLanguages { get; set; }

        private bool isBanned = false;
        public bool IsBanned
        {
            get { return isBanned; }
            set
            {
                isBanned = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsBanned"));
            }
        }

        private bool adminMessagesAvailable = false;
        public bool AdminMessagesAvailable
        {
            get { return adminMessagesAvailable; }
            set
            {
                adminMessagesAvailable = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AdminMessagesAvailable"));
            }
        }

        private bool isAdminUser = false;
        public bool IsAdminUser
        {
            get { return isAdminUser; }
            set
            {
                isAdminUser = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsAdminUser"));
            }
        }

        private string banReason = "";
        public string BanReason
        {
            get { return banReason; }
            set
            {
                banReason = value;
                OnPropertyChanged(new PropertyChangedEventArgs("BanReason"));
            }
        }

        private string banOrigin = "";
        public string BanOrigin
        {
            get { return banOrigin; }
            set
            {
                banOrigin = value;
                OnPropertyChanged(new PropertyChangedEventArgs("BanOrigin"));
            }
        }

        private string baseImagePath = "";
        public string BaseImagePath
        {
            get { return baseImagePath; }
            set
            {
                baseImagePath = value;
                OnPropertyChanged(new PropertyChangedEventArgs("BaseImagePath"));
            }
        }

        private bool baseImagesPathIsDefault = true;
        public bool BaseImagesPathIsDefault
        {
            get { return baseImagesPathIsDefault; }
            set
            {
                baseImagesPathIsDefault = value;
                OnPropertyChanged(new PropertyChangedEventArgs("BaseImagesPathIsDefault"));
            }
        }

        private string username = "";
        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Username"));
            }
        }

        private int hasherQueueCount = 0;
        public int HasherQueueCount
        {
            get { return hasherQueueCount; }
            set
            {
                hasherQueueCount = value;
                OnPropertyChanged(new PropertyChangedEventArgs("HasherQueueCount"));
            }
        }

        private string hasherQueueState = "";
        public string HasherQueueState
        {
            get { return hasherQueueState; }
            set
            {
                hasherQueueState = value;
                OnPropertyChanged(new PropertyChangedEventArgs("HasherQueueState"));
            }
        }

        private int serverImageQueueCount = 0;
        public int ServerImageQueueCount
        {
            get { return serverImageQueueCount; }
            set
            {
                serverImageQueueCount = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ServerImageQueueCount"));
            }
        }

        private string serverImageQueueState = "";
        public string ServerImageQueueState
        {
            get { return serverImageQueueState; }
            set
            {
                serverImageQueueState = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ServerImageQueueState"));
            }
        }

        private int generalQueueCount = 0;
        public int GeneralQueueCount
        {
            get { return generalQueueCount; }
            set
            {
                generalQueueCount = value;
                OnPropertyChanged(new PropertyChangedEventArgs("GeneralQueueCount"));
            }
        }

        private string generalQueueState = "";
        public string GeneralQueueState
        {
            get { return generalQueueState; }
            set
            {
                generalQueueState = value;
                OnPropertyChanged(new PropertyChangedEventArgs("GeneralQueueState"));
            }
        }

        private bool hasherQueuePaused = false;
        public bool HasherQueuePaused
        {
            get { return hasherQueuePaused; }
            set
            {
                hasherQueuePaused = value;
                OnPropertyChanged(new PropertyChangedEventArgs("HasherQueuePaused"));
            }
        }

        private bool hasherQueueRunning = true;
        public bool HasherQueueRunning
        {
            get { return hasherQueueRunning; }
            set
            {
                hasherQueueRunning = value;
                OnPropertyChanged(new PropertyChangedEventArgs("HasherQueueRunning"));
            }
        }

        private bool serverImageQueuePaused = false;
        public bool ServerImageQueuePaused
        {
            get { return serverImageQueuePaused; }
            set
            {
                serverImageQueuePaused = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ServerImageQueuePaused"));
            }
        }

        private bool serverImageQueueRunning = true;
        public bool ServerImageQueueRunning
        {
            get { return serverImageQueueRunning; }
            set
            {
                serverImageQueueRunning = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ServerImageQueueRunning"));
            }
        }

        private bool generalQueuePaused = false;
        public bool GeneralQueuePaused
        {
            get { return generalQueuePaused; }
            set
            {
                generalQueuePaused = value;
                OnPropertyChanged(new PropertyChangedEventArgs("GeneralQueuePaused"));
            }
        }

        private bool generalQueueRunning = false;
        public bool GeneralQueueRunning
        {
            get { return generalQueueRunning; }
            set
            {
                generalQueueRunning = value;
                OnPropertyChanged(new PropertyChangedEventArgs("GeneralQueueRunning"));
            }
        }

        private bool serverOnline = false;
        public bool ServerOnline
        {
            get { return serverOnline; }
            set
            {
                serverOnline = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ServerOnline"));
                SetShowServerSettings();
            }
        }

        private bool showCommunity = false;
        public bool ShowCommunity
        {
            get { return showCommunity; }
            set
            {
                showCommunity = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ShowCommunity"));
            }
        }

        private bool showServerSettings = false;
        public bool ShowServerSettings
        {
            get { return showServerSettings; }
            set
            {
                showServerSettings = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ShowServerSettings"));
            }
        }

        private bool newVersionAvailable = false;
        public bool NewVersionAvailable
        {
            get { return newVersionAvailable; }
            set
            {
                newVersionAvailable = value;
                OnPropertyChanged(new PropertyChangedEventArgs("NewVersionAvailable"));
            }
        }

        private string newVersionNumber = "";
        public string NewVersionNumber
        {
            get { return newVersionNumber; }
            set
            {
                newVersionNumber = value;
                OnPropertyChanged(new PropertyChangedEventArgs("NewVersionNumber"));
            }
        }

        private string newVersionDownloadLink = "";
        public string NewVersionDownloadLink
        {
            get { return newVersionDownloadLink; }
            set
            {
                newVersionDownloadLink = value;
                OnPropertyChanged(new PropertyChangedEventArgs("NewVersionDownloadLink"));
            }
        }

        private string applicationVersion = "";
        public string ApplicationVersion
        {
            get { return applicationVersion; }
            set
            {
                applicationVersion = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ApplicationVersion"));
            }
        }

        private string applicationVersionLatest = "";
        public string ApplicationVersionLatest
        {
            get { return applicationVersionLatest; }
            set
            {
                applicationVersionLatest = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ApplicationVersionLatest"));
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
                aniDB_Username = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_Username"));
            }
        }

        private string aniDB_Password = "";
        public string AniDB_Password
        {
            get { return aniDB_Password; }
            set
            {
                aniDB_Password = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_Password"));
            }
        }

        private string aniDB_ServerAddress = "";
        public string AniDB_ServerAddress
        {
            get { return aniDB_ServerAddress; }
            set
            {
                aniDB_ServerAddress = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_ServerAddress"));
            }
        }

        private string aniDB_ServerPort = "";
        public string AniDB_ServerPort
        {
            get { return aniDB_ServerPort; }
            set
            {
                aniDB_ServerPort = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_ServerPort"));
            }
        }

        private string aniDB_ClientPort = "";
        public string AniDB_ClientPort
        {
            get { return aniDB_ClientPort; }
            set
            {
                aniDB_ClientPort = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_ClientPort"));
            }
        }

        private string aniDB_AVDumpClientPort = "";
        public string AniDB_AVDumpClientPort
        {
            get { return aniDB_AVDumpClientPort; }
            set
            {
                aniDB_AVDumpClientPort = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_AVDumpClientPort"));
            }
        }

        private string aniDB_AVDumpKey = "";
        public string AniDB_AVDumpKey
        {
            get { return aniDB_AVDumpKey; }
            set
            {
                aniDB_AVDumpKey = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_AVDumpKey"));
            }
        }

        private bool aniDB_DownloadRelatedAnime = false;
        public bool AniDB_DownloadRelatedAnime
        {
            get { return aniDB_DownloadRelatedAnime; }
            set
            {
                aniDB_DownloadRelatedAnime = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_DownloadRelatedAnime"));
            }
        }

        private bool aniDB_DownloadSimilarAnime = false;
        public bool AniDB_DownloadSimilarAnime
        {
            get { return aniDB_DownloadSimilarAnime; }
            set
            {
                aniDB_DownloadSimilarAnime = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_DownloadSimilarAnime"));
            }
        }

        private bool aniDB_DownloadReviews = false;
        public bool AniDB_DownloadReviews
        {
            get { return aniDB_DownloadReviews; }
            set
            {
                aniDB_DownloadReviews = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_DownloadReviews"));
            }
        }

        private bool aniDB_DownloadReleaseGroups = false;
        public bool AniDB_DownloadReleaseGroups
        {
            get { return aniDB_DownloadReleaseGroups; }
            set
            {
                aniDB_DownloadReleaseGroups = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_DownloadReleaseGroups"));
            }
        }

        private bool aniDB_MyList_AddFiles = false;
        public bool AniDB_MyList_AddFiles
        {
            get { return aniDB_MyList_AddFiles; }
            set
            {
                aniDB_MyList_AddFiles = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_MyList_AddFiles"));
            }
        }

        private int aniDB_MyList_StorageState = 1;
        public int AniDB_MyList_StorageState
        {
            get { return aniDB_MyList_StorageState; }
            set
            {
                aniDB_MyList_StorageState = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_MyList_WatchedState"));
            }
        }

        private AniDBFileDeleteType aniDB_MyList_DeleteType = AniDBFileDeleteType.Delete;
        public AniDBFileDeleteType AniDB_MyList_DeleteType
        {
            get { return aniDB_MyList_DeleteType; }
            set
            {
                aniDB_MyList_DeleteType = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_MyList_DeleteType"));
            }
        }

        private bool aniDB_MyList_ReadWatched = false;
        public bool AniDB_MyList_ReadWatched
        {
            get { return aniDB_MyList_ReadWatched; }
            set
            {
                aniDB_MyList_ReadWatched = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_MyList_ReadWatched"));
            }
        }

        private bool aniDB_MyList_ReadUnwatched = false;
        public bool AniDB_MyList_ReadUnwatched
        {
            get { return aniDB_MyList_ReadUnwatched; }
            set
            {
                aniDB_MyList_ReadUnwatched = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_MyList_ReadUnwatched"));
            }
        }

        private bool aniDB_MyList_SetWatched = false;
        public bool AniDB_MyList_SetWatched
        {
            get { return aniDB_MyList_SetWatched; }
            set
            {
                aniDB_MyList_SetWatched = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_MyList_SetWatched"));
            }
        }

        private bool aniDB_MyList_SetUnwatched = false;
        public bool AniDB_MyList_SetUnwatched
        {
            get { return aniDB_MyList_SetUnwatched; }
            set
            {
                aniDB_MyList_SetUnwatched = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_MyList_SetUnwatched"));
            }
        }

        private ScheduledUpdateFrequency aniDB_MyList_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve;
        public ScheduledUpdateFrequency AniDB_MyList_UpdateFrequency
        {
            get { return aniDB_MyList_UpdateFrequency; }
            set
            {
                aniDB_MyList_UpdateFrequency = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_MyList_UpdateFrequency"));
            }
        }

        private ScheduledUpdateFrequency aniDB_MyListStats_UpdateFrequency = ScheduledUpdateFrequency.Never;
        public ScheduledUpdateFrequency AniDB_MyListStats_UpdateFrequency
        {
            get { return aniDB_MyListStats_UpdateFrequency; }
            set
            {
                aniDB_MyListStats_UpdateFrequency = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_MyListStats_UpdateFrequency"));
            }
        }

        private ScheduledUpdateFrequency aniDB_Calendar_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve;
        public ScheduledUpdateFrequency AniDB_Calendar_UpdateFrequency
        {
            get { return aniDB_Calendar_UpdateFrequency; }
            set
            {
                aniDB_Calendar_UpdateFrequency = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_Calendar_UpdateFrequency"));
            }
        }

        private ScheduledUpdateFrequency aniDB_Anime_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve;
        public ScheduledUpdateFrequency AniDB_Anime_UpdateFrequency
        {
            get { return aniDB_Anime_UpdateFrequency; }
            set
            {
                aniDB_Anime_UpdateFrequency = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_Anime_UpdateFrequency"));
            }
        }

        private ScheduledUpdateFrequency aniDB_File_UpdateFrequency = ScheduledUpdateFrequency.Daily;
        public ScheduledUpdateFrequency AniDB_File_UpdateFrequency
        {
            get { return aniDB_File_UpdateFrequency; }
            set
            {
                aniDB_File_UpdateFrequency = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_File_UpdateFrequency"));
            }
        }

        private bool aniDB_DownloadCharacters = false;
        public bool AniDB_DownloadCharacters
        {
            get { return aniDB_DownloadCharacters; }
            set
            {
                aniDB_DownloadCharacters = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_DownloadCharacters"));
            }
        }

        private bool aniDB_DownloadCreators = false;
        public bool AniDB_DownloadCreators
        {
            get { return aniDB_DownloadCreators; }
            set
            {
                aniDB_DownloadCreators = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AniDB_DownloadCreators"));
            }
        }

        private string webCache_Address = "";
        public string WebCache_Address
        {
            get { return webCache_Address; }
            set
            {
                webCache_Address = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WebCache_Address"));
            }
        }

        private bool webCache_Anonymous = false;
        public bool WebCache_Anonymous
        {
            get { return webCache_Anonymous; }
            set
            {
                webCache_Anonymous = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WebCache_Anonymous"));
            }
        }

        private bool webCache_XRefFileEpisode_Get = false;
        public bool WebCache_XRefFileEpisode_Get
        {
            get { return webCache_XRefFileEpisode_Get; }
            set
            {
                webCache_XRefFileEpisode_Get = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WebCache_XRefFileEpisode_Get"));
            }
        }

        private bool webCache_XRefFileEpisode_Send = false;
        public bool WebCache_XRefFileEpisode_Send
        {
            get { return webCache_XRefFileEpisode_Send; }
            set
            {
                webCache_XRefFileEpisode_Send = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WebCache_XRefFileEpisode_Send"));
            }
        }

        private bool webCache_TvDB_Get = false;
        public bool WebCache_TvDB_Get
        {
            get { return webCache_TvDB_Get; }
            set
            {
                webCache_TvDB_Get = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WebCache_TvDB_Get"));
            }
        }

        private bool webCache_TvDB_Send = false;
        public bool WebCache_TvDB_Send
        {
            get { return webCache_TvDB_Send; }
            set
            {
                webCache_TvDB_Send = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WebCache_TvDB_Send"));
            }
        }

        private bool webCache_Trakt_Get = false;
        public bool WebCache_Trakt_Get
        {
            get { return webCache_Trakt_Get; }
            set
            {
                webCache_Trakt_Get = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WebCache_Trakt_Get"));
            }
        }

        private bool webCache_Trakt_Send = false;
        public bool WebCache_Trakt_Send
        {
            get { return webCache_Trakt_Send; }
            set
            {
                webCache_Trakt_Send = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WebCache_Trakt_Send"));
            }
        }


        private bool webCache_MAL_Get = false;
        public bool WebCache_MAL_Get
        {
            get { return webCache_MAL_Get; }
            set
            {
                webCache_MAL_Get = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WebCache_MAL_Get"));
            }
        }

        private bool webCache_MAL_Send = false;
        public bool WebCache_MAL_Send
        {
            get { return webCache_MAL_Send; }
            set
            {
                webCache_MAL_Send = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WebCache_MAL_Send"));
            }
        }

        private bool webCache_UserInfo = false;
        public bool WebCache_UserInfo
        {
            get { return webCache_UserInfo; }
            set
            {
                webCache_UserInfo = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WebCache_UserInfo"));
            }
        }


        private bool tvDB_AutoFanart = false;
        public bool TvDB_AutoFanart
        {
            get { return tvDB_AutoFanart; }
            set
            {
                tvDB_AutoFanart = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TvDB_AutoFanart"));
            }
        }

        private int tvDB_AutoFanartAmount = 10;
        public int TvDB_AutoFanartAmount
        {
            get { return tvDB_AutoFanartAmount; }
            set
            {
                tvDB_AutoFanartAmount = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TvDB_AutoFanartAmount"));
            }
        }

        private bool tvDB_AutoWideBanners = false;
        public bool TvDB_AutoWideBanners
        {
            get { return tvDB_AutoWideBanners; }
            set
            {
                tvDB_AutoWideBanners = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TvDB_AutoWideBanners"));
            }
        }

        private int tvDB_AutoWideBannersAmount = 10;
        public int TvDB_AutoWideBannersAmount
        {
            get { return tvDB_AutoWideBannersAmount; }
            set
            {
                tvDB_AutoWideBannersAmount = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TvDB_AutoWideBannersAmount"));
            }
        }

        private bool tvDB_AutoPosters = false;
        public bool TvDB_AutoPosters
        {
            get { return tvDB_AutoPosters; }
            set
            {
                tvDB_AutoPosters = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TvDB_AutoPosters"));
            }
        }

        private int tvDB_AutoPostersAmount = 10;
        public int TvDB_AutoPostersAmount
        {
            get { return tvDB_AutoPostersAmount; }
            set
            {
                tvDB_AutoPostersAmount = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TvDB_AutoPostersAmount"));
            }
        }

        private string tvDB_Language = "";
        public string TvDB_Language
        {
            get { return tvDB_Language; }
            set
            {
                tvDB_Language = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TvDB_Language"));
            }
        }

        private ScheduledUpdateFrequency tvDB_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve;
        public ScheduledUpdateFrequency TvDB_UpdateFrequency
        {
            get { return tvDB_UpdateFrequency; }
            set
            {
                tvDB_UpdateFrequency = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TvDB_UpdateFrequency"));
            }
        }


        private bool movieDB_AutoFanart = false;
        public bool MovieDB_AutoFanart
        {
            get { return movieDB_AutoFanart; }
            set
            {
                movieDB_AutoFanart = value;
                OnPropertyChanged(new PropertyChangedEventArgs("MovieDB_AutoFanart"));
            }
        }

        private int movieDB_AutoFanartAmount = 10;
        public int MovieDB_AutoFanartAmount
        {
            get { return movieDB_AutoFanartAmount; }
            set
            {
                movieDB_AutoFanartAmount = value;
                OnPropertyChanged(new PropertyChangedEventArgs("MovieDB_AutoFanartAmount"));
            }
        }

        private bool movieDB_AutoPosters = false;
        public bool MovieDB_AutoPosters
        {
            get { return movieDB_AutoPosters; }
            set
            {
                movieDB_AutoPosters = value;
                OnPropertyChanged(new PropertyChangedEventArgs("MovieDB_AutoPosters"));
            }
        }

        private int movieDB_AutoPostersAmount = 10;
        public int MovieDB_AutoPostersAmount
        {
            get { return movieDB_AutoPostersAmount; }
            set
            {
                movieDB_AutoPostersAmount = value;
                OnPropertyChanged(new PropertyChangedEventArgs("MovieDB_AutoPostersAmount"));
            }
        }

        private string videoExtensions = "";
        public string VideoExtensions
        {
            get { return videoExtensions; }
            set
            {
                videoExtensions = value;
                OnPropertyChanged(new PropertyChangedEventArgs("VideoExtensions"));
            }
        }

        private bool autoGroupSeries = false;
        public bool AutoGroupSeries
        {
            get { return autoGroupSeries; }
            set
            {
                autoGroupSeries = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AutoGroupSeries"));
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
                autoGroupSeriesRelationExclusions = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AutoGroupSeriesRelationExclusions"));
            }
        }

        private bool isRelationInExclusion(string relation)
        {
            if (AutoGroupSeriesRelationExclusions == null)
                return false;

            foreach (string a in AutoGroupSeriesRelationExclusions.Split('|'))
            {
                // relation will always be lowercase, but a may not be yet
                if (a.ToLowerInvariant().Equals(relation)) return true;
            }
            return false;
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
                autoGroupSeriesRelationExclusions = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AutoGroupSeriesRelationExclusions"));
            }
        }

        private bool isRelationInExclusion(string relation)
        {
            foreach (string a in AutoGroupSeriesRelationExclusions.Split('|'))
            {
                // relation will always be lowercase, but a may not be yet
                if (a.ToLowerInvariant().Equals(relation)) return true;
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
                        if (!a.ToLowerInvariant().Equals(setting)) final += a.ToLowerInvariant() + "|";
                    }
                    // this will be "" if all are unchecked
                    // remove last '|' added in previous loop
                    if (final.EndsWith("|"))
                        final = final.Substring(0, final.Length - 1);
                    AutoGroupSeriesRelationExclusions = final;
                }
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

        private bool useEpisodeStatus = false;
        public bool UseEpisodeStatus
        {
            get { return useEpisodeStatus; }
            set
            {
                useEpisodeStatus = value;
                OnPropertyChanged(new PropertyChangedEventArgs("UseEpisodeStatus"));
            }
        }

        private bool runImportOnStart = false;
        public bool RunImportOnStart
        {
            get { return runImportOnStart; }
            set
            {
                runImportOnStart = value;
                OnPropertyChanged(new PropertyChangedEventArgs("RunImportOnStart"));
            }
        }

        private bool scanDropFoldersOnStart = false;
        public bool ScanDropFoldersOnStart
        {
            get { return scanDropFoldersOnStart; }
            set
            {
                scanDropFoldersOnStart = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ScanDropFoldersOnStart"));
            }
        }


        private bool hash_CRC32 = false;
        public bool Hash_CRC32
        {
            get { return hash_CRC32; }
            set
            {
                hash_CRC32 = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Hash_CRC32"));
            }
        }

        private bool hash_MD5 = false;
        public bool Hash_MD5
        {
            get { return hash_MD5; }
            set
            {
                hash_MD5 = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Hash_MD5"));
            }
        }

        private bool hash_SHA1 = false;
        public bool Hash_SHA1
        {
            get { return hash_SHA1; }
            set
            {
                hash_SHA1 = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Hash_SHA1"));
            }
        }

        private string languagePreference = "en,x-jat";
        public string LanguagePreference
        {
            get { return languagePreference; }
            set
            {
                languagePreference = value;
                OnPropertyChanged(new PropertyChangedEventArgs("LanguagePreference"));
            }
        }

        private bool languageUseSynonyms = false;
        public bool LanguageUseSynonyms
        {
            get { return languageUseSynonyms; }
            set
            {
                languageUseSynonyms = value;
                OnPropertyChanged(new PropertyChangedEventArgs("LanguageUseSynonyms"));
            }
        }

        private DataSourceType episodeTitleSource = DataSourceType.AniDB;
        public DataSourceType EpisodeTitleSource
        {
            get { return episodeTitleSource; }
            set
            {
                episodeTitleSource = value;
                OnPropertyChanged(new PropertyChangedEventArgs("EpisodeTitleSource"));
            }
        }

        private DataSourceType seriesDescriptionSource = DataSourceType.AniDB;
        public DataSourceType SeriesDescriptionSource
        {
            get { return seriesDescriptionSource; }
            set
            {
                seriesDescriptionSource = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SeriesDescriptionSource"));
            }
        }

        private DataSourceType seriesNameSource = DataSourceType.AniDB;
        public DataSourceType SeriesNameSource
        {
            get { return seriesNameSource; }
            set
            {
                seriesNameSource = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SeriesNameSource"));
            }
        }

        private bool trakt_IsEnabled = true;
        public bool Trakt_IsEnabled
        {
            get { return trakt_IsEnabled; }
            set
            {
                trakt_IsEnabled = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Trakt_IsEnabled"));
            }
        }

        private string trakt_AuthToken = "";
        public string Trakt_AuthToken
        {
            get { return trakt_AuthToken; }
            set
            {
                trakt_AuthToken = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Trakt_AuthToken"));
            }
        }

        private string trakt_RefreshToken = "";
        public string Trakt_RefreshToken
        {
            get { return trakt_RefreshToken; }
            set
            {
                trakt_RefreshToken = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Trakt_RefreshToken"));
            }
        }

        private string trakt_TokenExpirationDate = "";
        public string Trakt_TokenExpirationDate
        {
            get { return trakt_TokenExpirationDate; }
            set
            {
                trakt_TokenExpirationDate = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Trakt_TokenExpirationDate"));
            }
        }

        private bool trakt_DownloadFanart = true;
        public bool Trakt_DownloadFanart
        {
            get { return trakt_DownloadFanart; }
            set
            {
                trakt_DownloadFanart = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Trakt_DownloadFanart"));
            }
        }

        private bool trakt_DownloadPosters = true;
        public bool Trakt_DownloadPosters
        {
            get { return trakt_DownloadPosters; }
            set
            {
                trakt_DownloadPosters = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Trakt_DownloadPosters"));
            }
        }

        private bool trakt_DownloadEpisodes = true;
        public bool Trakt_DownloadEpisodes
        {
            get { return trakt_DownloadEpisodes; }
            set
            {
                trakt_DownloadEpisodes = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Trakt_DownloadEpisodes"));
            }
        }

        private string mAL_Username = "";
        public string MAL_Username
        {
            get { return mAL_Username; }
            set
            {
                mAL_Username = value;
                OnPropertyChanged(new PropertyChangedEventArgs("MAL_Username"));
            }
        }

        private string mAL_Password = "";
        public string MAL_Password
        {
            get { return mAL_Password; }
            set
            {
                mAL_Password = value;
                OnPropertyChanged(new PropertyChangedEventArgs("MAL_Password"));
            }
        }

        private ScheduledUpdateFrequency mAL_UpdateFrequency = ScheduledUpdateFrequency.Daily;
        public ScheduledUpdateFrequency MAL_UpdateFrequency
        {
            get { return mAL_UpdateFrequency; }
            set
            {
                mAL_UpdateFrequency = value;
                OnPropertyChanged(new PropertyChangedEventArgs("MAL_UpdateFrequency"));
            }
        }

        private bool mAL_NeverDecreaseWatchedNums = false;
        public bool MAL_NeverDecreaseWatchedNums
        {
            get { return mAL_NeverDecreaseWatchedNums; }
            set
            {
                mAL_NeverDecreaseWatchedNums = value;
                OnPropertyChanged(new PropertyChangedEventArgs("MAL_NeverDecreaseWatchedNums"));
            }
        }

        private ScheduledUpdateFrequency trakt_UpdateFrequency = ScheduledUpdateFrequency.Daily;
        public ScheduledUpdateFrequency Trakt_UpdateFrequency
        {
            get { return trakt_UpdateFrequency; }
            set
            {
                trakt_UpdateFrequency = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Trakt_UpdateFrequency"));
            }
        }

        private ScheduledUpdateFrequency trakt_SyncFrequency = ScheduledUpdateFrequency.Daily;
        public ScheduledUpdateFrequency Trakt_SyncFrequency
        {
            get { return trakt_SyncFrequency; }
            set
            {
                trakt_SyncFrequency = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Trakt_SyncFrequency"));
            }
        }

        public ObservableCollection<AdminMessage> AdminMessages { get; set; }


        #endregion


        public static JMMServerVM Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new JMMServerVM();
                    _instance.Init();
                }
                return _instance;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private JMMServerVM()
        {

        }

        public void Test()
        {
        }

        private void Init()
        {
            UserAuthenticated = false;
            ImportFolders = new ObservableCollection<ImportFolderVM>();
            UnselectedLanguages = new ObservableCollection<NamingLanguage>();
            SelectedLanguages = new ObservableCollection<NamingLanguage>();
            AllUsers = new ObservableCollection<JMMUserVM>();
            AllTags = new ObservableCollection<string>();
            AllCustomTags = new ObservableCollection<CustomTagVM>();
            ViewCustomTagsAll = System.Windows.Data.CollectionViewSource.GetDefaultView(JMMServerVM.Instance.AllCustomTags);
            ViewCustomTagsAll.SortDescriptions.Add(new SortDescription("TagName", ListSortDirection.Ascending));

            AdminMessages = new ObservableCollection<AdminMessage>();

            try
            {
                //SetupClient();
                //SetupTCPClient();
                SetupBinaryClient();
            }
            catch { }

            // timer for server status
            serverStatusTimer = new System.Timers.Timer();
            serverStatusTimer.AutoReset = false;
            serverStatusTimer.Interval = 4 * 1000; // 4 seconds
            serverStatusTimer.Elapsed += new System.Timers.ElapsedEventHandler(serverStatusTimer_Elapsed);
            serverStatusTimer.Enabled = true;
        }

        void serverStatusTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (!ServerOnline)
                {
                    serverStatusTimer.Start();
                    return;
                }

                TimeSpan ts = DateTime.Now - lastVersionCheck;

                JMMServerBinary.Contract_ServerStatus status = JMMServerVM.Instance.clientBinaryHTTP.GetServerStatus();
                JMMServerBinary.Contract_AppVersions appv = null;
                if (ts.TotalMinutes > 180)
                {
                    //appv = JMMServerVM.Instance.clientBinaryHTTP.GetAppVersions();

                    lastVersionCheck = DateTime.Now;
                    // check for admin messages
                    AdminMessages.Clear();
                    List<JMMServerBinary.Contract_AdminMessage> msgs = JMMServerVM.Instance.clientBinaryHTTP.GetAdminMessages();
                    if (msgs != null)
                    {
                        foreach (JMMServerBinary.Contract_AdminMessage msg in msgs)
                        {
                            AdminMessage newMsg = new AdminMessage(msg);
                            AdminMessages.Add(newMsg);
                        }
                    }

                    AdminMessagesAvailable = AdminMessages.Count > 0;

                    // check if this user is allowed to admin the web cache
                    ShowCommunity = JMMServerVM.Instance.clientBinaryHTTP.IsWebCacheAdmin();
                }




                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
                {
                    HasherQueueCount = status.HashQueueCount;
                    GeneralQueueCount = status.GeneralQueueCount;
                    ServerImageQueueCount = status.ImagesQueueCount;

                    HasherQueueState = status.HashQueueState;
                    GeneralQueueState = status.GeneralQueueState;
                    ServerImageQueueState = status.ImagesQueueState;

                    IsBanned = status.IsBanned;
                    BanReason = status.BanReason;
                    BanOrigin = status.BanOrigin;

                    HasherQueuePaused = HasherQueueState.ToLower().Contains("pause");
                    HasherQueueRunning = !HasherQueueState.ToLower().Contains("pause");

                    GeneralQueuePaused = GeneralQueueState.ToLower().Contains("pause");
                    GeneralQueueRunning = !GeneralQueueState.ToLower().Contains("pause");

                    ServerImageQueuePaused = ServerImageQueueState.ToLower().Contains("pause");
                    ServerImageQueueRunning = !ServerImageQueueState.ToLower().Contains("pause");

                    if (appv != null)
                    {
                        string curVersion = Utils.GetApplicationVersion(System.Reflection.Assembly.GetExecutingAssembly());

                        string[] latestNumbers = appv.JMMDesktopVersion.Split('.');
                        string[] curNumbers = curVersion.Split('.');

                        string latestMajor = string.Format("{0}.{1}", latestNumbers[0], latestNumbers[1]);
                        string curMajor = string.Format("{0}.{1}", curNumbers[0], curNumbers[1]);

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

            catch { }

            serverStatusTimer.Start();
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

                string msg = Instance.clientBinaryHTTP.RenameAllGroups();
                if (string.IsNullOrEmpty(msg))
                    MessageBox.Show(Properties.Resources.JMMServer_Complete);
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
                List<JMMServerBinary.Contract_ImportFolder> importFolders = Instance.clientBinaryHTTP.GetImportFolders();

                foreach (JMMServerBinary.Contract_ImportFolder ifolder in importFolders)
                {
                    ImportFolderVM grpNew = new ImportFolderVM(ifolder);
                    ImportFolders.Add(grpNew);
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
                    List<JMMServerBinary.Contract_JMMUser> users = JMMServerVM.Instance.clientBinaryHTTP.GetAllUsers();
                    foreach (JMMServerBinary.Contract_JMMUser user in users)
                    {
                        JMMUserVM jmmUser = new JMMUserVM(user);
                        if (CurrentUser != null && CurrentUser.JMMUserID.Value == jmmUser.JMMUserID.Value)
                        {
                            CurrentUser = jmmUser;
                            SetShowServerSettings();
                        }
                        AllUsers.Add(new JMMUserVM(user));
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
                List<string> tagsRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllTagNames();

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
                List<JMMServerBinary.Contract_CustomTag> tagsRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllCustomTags();

                foreach (JMMServerBinary.Contract_CustomTag tag in tagsRaw)
                    AllCustomTags.Add(new CustomTagVM(tag));

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
            Instance.clientBinaryHTTP.RunImport();
        }

        public void RemoveMissingFiles()
        {
            if (!ServerOnline) return;
            Instance.clientBinaryHTTP.RemoveMissingFiles();
        }

        public void SyncMyList()
        {
            if (!ServerOnline) return;
            Instance.clientBinaryHTTP.SyncMyList();
        }

        public void SyncVotes()
        {
            if (!ServerOnline) return;
            Instance.clientBinaryHTTP.SyncVotes();
        }

        public void RevokeVote(int animeID)
        {
            if (!ServerOnline) return;
            Instance.clientBinaryHTTP.VoteAnimeRevoke(animeID);
        }

        public void VoteAnime(int animeID, decimal voteValue, int voteType)
        {
            if (!ServerOnline) return;
            Instance.clientBinaryHTTP.VoteAnime(animeID, voteValue, voteType);
        }
    }
}

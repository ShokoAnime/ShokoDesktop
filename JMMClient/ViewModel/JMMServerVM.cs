using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using JMMClient.ViewModel;
using System.Windows;
using System.ServiceModel;
using System.ComponentModel;
using System.ServiceModel.Channels;
using System.Xml;
using System.ServiceModel.Description;
using JMMClient.Forms;

namespace JMMClient
{
	public class JMMServerVM : INotifyPropertyChanged
	{
		private static JMMServerVM _instance;
		private System.Timers.Timer serverStatusTimer = null;
		private System.Timers.Timer saveTimer = null;

		public object userLock = new object();
		public bool UserAuthenticated { get; set; }
		public JMMUserVM CurrentUser { get; set; }


		/*private JMMServer.IJMMServer _client = null;
		public JMMServer.IJMMServer client
		{
			get
			{
				if (_client == null)
				{
					try
					{
						SetupClient();
					}
					catch { }
				}
				return _client;
			}
		}*/

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

		/*private JMMServerTCP.IJMMServer _clientTCP = null;
		public JMMServerTCP.IJMMServer clientTCP
		{
			get
			{
				if (_clientTCP == null)
				{
					try
					{
						SetupTCPClient();
					}
					catch { }
				}
				return _clientTCP;
			}
		}*/

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

		/*
		public void SetupClient()
		{
			ServerOnline = false;
			_client = null;

			if (!SettingsAreValid()) return;

			try
			{

				string url = string.Format(@"http://{0}:{1}/JMMServer", AppSettings.JMMServer_Address, AppSettings.JMMServer_Port);
				BasicHttpBinding binding = new BasicHttpBinding();
				//binding.MaxReceivedMessageSize = 2097152; // 2 megabytes
				binding.MaxReceivedMessageSize = 20971520; // 20 megabytes
				binding.MaxBufferPoolSize = 20971520; // 20 megabytes
				binding.MaxBufferSize = 20971520; // 20 megabytes
				binding.ReceiveTimeout = new TimeSpan(0, 0, 60);

				binding.ReaderQuotas.MaxArrayLength = int.MaxValue;
				binding.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
				binding.ReaderQuotas.MaxDepth = int.MaxValue;
				binding.ReaderQuotas.MaxNameTableCharCount = int.MaxValue;
				binding.ReaderQuotas.MaxStringContentLength = int.MaxValue;


				EndpointAddress endpoint = new EndpointAddress(new Uri(url));


				var factory = new ChannelFactory<JMMServer.IJMMServer>(binding, endpoint);
				foreach (OperationDescription op in factory.Endpoint.Contract.Operations)
				{
					var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
					if (dataContractBehavior != null)
					{
						dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
					}
				}

				_client = factory.CreateChannel();

				// try connecting to see if the server is responding
				JMMServerBinary.Contract_ServerStatus status = JMMServerVM.Instance.clientBinaryHTTP.GetServerStatus();
				ServerOnline = true;

				GetServerSettings();
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			
		}

		public void SetupTCPClient()
		{
			_clientTCP = null;

			if (!SettingsAreValid()) return;

			try
			{

				string url = string.Format(@"net.tcp://{0}:{1}/JMMServerTCP", AppSettings.JMMServer_Address, 8112);
				NetTcpBinding binding = new NetTcpBinding(); 
				//transport.MaxReceivedMessageSize = 2097152; // 2 megabytes

				binding.SendTimeout = new TimeSpan(30, 0, 30);
				binding.ReceiveTimeout = new TimeSpan(30, 0, 30);
				binding.OpenTimeout = new TimeSpan(30, 0, 30);
				binding.CloseTimeout = new TimeSpan(30, 0, 30);
				binding.MaxBufferPoolSize = int.MaxValue;
				binding.MaxBufferSize = int.MaxValue;
				binding.MaxReceivedMessageSize = int.MaxValue;

				XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
				quotas.MaxArrayLength = int.MaxValue;
				quotas.MaxBytesPerRead = int.MaxValue;
				quotas.MaxDepth = int.MaxValue;

				quotas.MaxNameTableCharCount = int.MaxValue;
				quotas.MaxStringContentLength = int.MaxValue;

				binding.ReaderQuotas = quotas;

				
				
				EndpointAddress endpoint = new EndpointAddress(new Uri(url));
				

				var factory = new ChannelFactory<JMMServerTCP.IJMMServerChannel>(binding, endpoint);
				foreach (OperationDescription op in factory.Endpoint.Contract.Operations)
				{
					var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
					if (dataContractBehavior != null)
					{
						dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
					}
				}

				//_clientTCP = new JMMServerTCP.JMMServerClient(binding, endpoint);
				_clientTCP = factory.CreateChannel();

				// try connecting to see if the server is responding
				JMMServerTCP.Contract_ServerStatus status = JMMServerVM.Instance.clientTCP.GetServerStatus();
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

		}*/

		public bool SetupBinaryClient()
		{
			ServerOnline = false;
			_clientBinaryHTTP = null;

			if (!SettingsAreValid()) return false;

			try
			{

				string url = string.Format(@"http://{0}:{1}/JMMServerBinary", AppSettings.JMMServer_Address, AppSettings.JMMServer_Port);

				BinaryMessageEncodingBindingElement encoding = new BinaryMessageEncodingBindingElement();
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
				Utils.ShowErrorMessage(ex);
				return false;
			}

		}

		public bool AuthenticateUser()
		{
			CurrentUser = null;
			Username = "";

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
				}
			}
			else
				UserAuthenticated = false;

			return UserAuthenticated;
		}

		private void GetServerSettings()
		{
			JMMServerBinary.Contract_ServerSettings contract = _clientBinaryHTTP.GetServerSettings();

			this.AniDB_Username = contract.AniDB_Username;
			this.AniDB_Password = contract.AniDB_Password;
			this.AniDB_ServerAddress = contract.AniDB_ServerAddress;
			this.AniDB_ServerPort = contract.AniDB_ServerPort;
			this.AniDB_ClientPort = contract.AniDB_ClientPort;

			this.AniDB_DownloadRelatedAnime = contract.AniDB_DownloadRelatedAnime;
			this.AniDB_DownloadSimilarAnime = contract.AniDB_DownloadSimilarAnime;
			this.AniDB_DownloadCharactersCreators = contract.AniDB_DownloadCharactersCreators;
			this.AniDB_DownloadReviews = contract.AniDB_DownloadReviews;
			this.AniDB_DownloadReleaseGroups = contract.AniDB_DownloadReleaseGroups;

			this.AniDB_MyList_AddFiles = contract.AniDB_MyList_AddFiles;
			this.AniDB_MyList_StorageState = contract.AniDB_MyList_StorageState;
			this.AniDB_MyList_ReadWatched = contract.AniDB_MyList_ReadWatched;
			this.AniDB_MyList_ReadUnwatched = contract.AniDB_MyList_ReadUnwatched;
			this.AniDB_MyList_SetWatched = contract.AniDB_MyList_SetWatched;
			this.AniDB_MyList_SetUnwatched = contract.AniDB_MyList_SetUnwatched;

			this.AniDB_Anime_UpdateFrequency = (ScheduledUpdateFrequency)contract.AniDB_Anime_UpdateFrequency;
			this.AniDB_Calendar_UpdateFrequency = (ScheduledUpdateFrequency)contract.AniDB_Calendar_UpdateFrequency;
			this.AniDB_MyList_UpdateFrequency = (ScheduledUpdateFrequency)contract.AniDB_MyList_UpdateFrequency;

			// Web Cache
			this.WebCache_Address = contract.WebCache_Address;
			this.WebCache_Anonymous = contract.WebCache_Anonymous;
			this.WebCache_FileHashes_Get = contract.WebCache_FileHashes_Get;
			this.WebCache_FileHashes_Send = contract.WebCache_FileHashes_Send;
			this.WebCache_TvDB_Get = contract.WebCache_TvDB_Get;
			this.WebCache_TvDB_Send = contract.WebCache_TvDB_Send;
			this.WebCache_XRefFileEpisode_Get = contract.WebCache_XRefFileEpisode_Get;
			this.WebCache_XRefFileEpisode_Send = contract.WebCache_XRefFileEpisode_Send;

			// TvDB
			this.TvDB_AutoFanart = contract.TvDB_AutoFanart;
			this.TvDB_AutoFanartAmount = contract.TvDB_AutoFanartAmount;
			this.TvDB_AutoWideBanners = contract.TvDB_AutoWideBanners;
			this.TvDB_AutoPosters = contract.TvDB_AutoPosters;
			this.TvDB_UpdateFrequency = (ScheduledUpdateFrequency)contract.TvDB_UpdateFrequency;

			// MovieDB
			this.MovieDB_AutoFanart = contract.MovieDB_AutoFanart;
			this.MovieDB_AutoFanartAmount = contract.MovieDB_AutoFanartAmount;
			this.MovieDB_AutoPosters = contract.MovieDB_AutoPosters;

			// Import settings
			this.VideoExtensions = contract.VideoExtensions;
			this.WatchForNewFiles = contract.WatchForNewFiles;
			this.AutoGroupSeries = contract.AutoGroupSeries;
			this.UseEpisodeStatus = contract.Import_UseExistingFileWatchedStatus;
			this.RunImportOnStart = contract.RunImportOnStart;
			this.Hash_CRC32 = contract.Hash_CRC32;
			this.Hash_MD5 = contract.Hash_MD5;
			this.Hash_SHA1 = contract.Hash_SHA1;

			// Language
			this.LanguagePreference = contract.LanguagePreference;
			this.LanguageUseSynonyms = contract.LanguageUseSynonyms;

			// trakt
			this.Trakt_Username = contract.Trakt_Username;
			this.Trakt_Password = contract.Trakt_Password;
			this.Trakt_UpdateFrequency = (ScheduledUpdateFrequency)contract.Trakt_UpdateFrequency;
			this.Trakt_SyncFrequency = (ScheduledUpdateFrequency)contract.Trakt_SyncFrequency;
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

				contract.AniDB_DownloadRelatedAnime = this.AniDB_DownloadRelatedAnime;
				contract.AniDB_DownloadSimilarAnime = this.AniDB_DownloadSimilarAnime;
				contract.AniDB_DownloadCharactersCreators = this.AniDB_DownloadCharactersCreators;
				contract.AniDB_DownloadReviews = this.AniDB_DownloadReviews;
				contract.AniDB_DownloadReleaseGroups = this.AniDB_DownloadReleaseGroups;

				contract.AniDB_MyList_AddFiles = this.AniDB_MyList_AddFiles;
				contract.AniDB_MyList_StorageState = this.AniDB_MyList_StorageState;
				contract.AniDB_MyList_ReadWatched = this.AniDB_MyList_ReadWatched;
				contract.AniDB_MyList_ReadUnwatched = this.AniDB_MyList_ReadUnwatched;
				contract.AniDB_MyList_SetWatched = this.AniDB_MyList_SetWatched;
				contract.AniDB_MyList_SetUnwatched = this.AniDB_MyList_SetUnwatched;

				contract.AniDB_Anime_UpdateFrequency = (int)this.AniDB_Anime_UpdateFrequency;
				contract.AniDB_Calendar_UpdateFrequency = (int)this.AniDB_Calendar_UpdateFrequency;
				contract.AniDB_MyList_UpdateFrequency = (int)this.AniDB_MyList_UpdateFrequency;

				// Web Cache
				contract.WebCache_Address = this.WebCache_Address;
				contract.WebCache_Anonymous = this.WebCache_Anonymous;
				contract.WebCache_FileHashes_Get = this.WebCache_FileHashes_Get;
				contract.WebCache_FileHashes_Send = this.WebCache_FileHashes_Send;
				contract.WebCache_TvDB_Get = this.WebCache_TvDB_Get;
				contract.WebCache_TvDB_Send = this.WebCache_TvDB_Send;
				contract.WebCache_XRefFileEpisode_Get = this.WebCache_XRefFileEpisode_Get;
				contract.WebCache_XRefFileEpisode_Send = this.WebCache_XRefFileEpisode_Send;

				// TvDB
				contract.TvDB_AutoFanart = this.TvDB_AutoFanart;
				contract.TvDB_AutoFanartAmount = this.TvDB_AutoFanartAmount;
				contract.TvDB_AutoWideBanners = this.TvDB_AutoWideBanners;
				contract.TvDB_AutoPosters = this.TvDB_AutoPosters;
				contract.TvDB_UpdateFrequency = (int)this.TvDB_UpdateFrequency;

				// MovieDB
				contract.MovieDB_AutoFanart = this.MovieDB_AutoFanart;
				contract.MovieDB_AutoFanartAmount = this.MovieDB_AutoFanartAmount;
				contract.MovieDB_AutoPosters = this.MovieDB_AutoPosters;

				// Import settings
				contract.VideoExtensions = this.VideoExtensions;
				contract.Import_UseExistingFileWatchedStatus = this.UseEpisodeStatus;
				contract.WatchForNewFiles = this.WatchForNewFiles;
				contract.AutoGroupSeries = this.AutoGroupSeries;
				contract.RunImportOnStart = this.RunImportOnStart;
				contract.Hash_CRC32 = this.Hash_CRC32;
				contract.Hash_MD5 = this.Hash_MD5;
				contract.Hash_SHA1 = this.Hash_SHA1;

				// Language
				contract.LanguagePreference = this.LanguagePreference;
				contract.LanguageUseSynonyms = this.LanguageUseSynonyms;

				// trakt
				contract.Trakt_Username = this.Trakt_Username;
				contract.Trakt_Password = this.Trakt_Password;
				contract.Trakt_UpdateFrequency = (int)this.Trakt_UpdateFrequency;
				contract.Trakt_SyncFrequency = (int)this.Trakt_SyncFrequency;

				JMMServerBinary.Contract_ServerSettings_SaveResponse response = _clientBinaryHTTP.SaveServerSettings(contract);
				if (response.ErrorMessage.Length > 0)
					return false;
				else
					return true;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
				return false;
			}
		}

		public void TestAniDBLogin()
		{
			try
			{
				SaveServerSettings();

				string response = _clientBinaryHTTP.TestAniDBConnection();
				MessageBox.Show(response);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

			
		}

		public void TestTraktLogin()
		{
			try
			{
				SaveServerSettings();

				string response = _clientBinaryHTTP.TestTraktLogin();
				if (string.IsNullOrEmpty(response))
					MessageBox.Show("Success", "", MessageBoxButton.OK, MessageBoxImage.Information);
				else
					MessageBox.Show(response, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		#region Observable Properties

		public ObservableCollection<ImportFolderVM> ImportFolders { get; set; }
		public ObservableCollection<JMMUserVM> AllUsers { get; set; }
		public ObservableCollection<string> AllCategories { get; set; }

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

		private bool aniDB_DownloadCharactersCreators = false;
		public bool AniDB_DownloadCharactersCreators
		{
			get { return aniDB_DownloadCharactersCreators; }
			set
			{
				aniDB_DownloadCharactersCreators = value;
				OnPropertyChanged(new PropertyChangedEventArgs("AniDB_DownloadCharactersCreators"));
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

		private bool webCache_FileHashes_Get = false;
		public bool WebCache_FileHashes_Get
		{
			get { return webCache_FileHashes_Get; }
			set
			{
				webCache_FileHashes_Get = value;
				OnPropertyChanged(new PropertyChangedEventArgs("WebCache_FileHashes_Get"));
			}
		}

		private bool webCache_FileHashes_Send = false;
		public bool WebCache_FileHashes_Send
		{
			get { return webCache_FileHashes_Send; }
			set
			{
				webCache_FileHashes_Send = value;
				OnPropertyChanged(new PropertyChangedEventArgs("WebCache_FileHashes_Send"));
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

		private bool watchForNewFiles = false;
		public bool WatchForNewFiles
		{
			get { return watchForNewFiles; }
			set
			{
				watchForNewFiles = value;
				OnPropertyChanged(new PropertyChangedEventArgs("WatchForNewFiles"));
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

		private string trakt_Username = "";
		public string Trakt_Username
		{
			get { return trakt_Username; }
			set
			{
				trakt_Username = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Trakt_Username"));
			}
		}

		private string trakt_Password = "";
		public string Trakt_Password
		{
			get { return trakt_Password; }
			set
			{
				trakt_Password = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Trakt_Password"));
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
			AllCategories = new ObservableCollection<string>();

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

				JMMServerBinary.Contract_ServerStatus status = JMMServerVM.Instance.clientBinaryHTTP.GetServerStatus();

				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					HasherQueueCount = status.HashQueueCount;
					GeneralQueueCount = status.GeneralQueueCount;

					HasherQueueState = status.HashQueueState;
					GeneralQueueState = status.GeneralQueueState;

					IsBanned = status.IsBanned;
					BanReason = status.BanReason;

					HasherQueuePaused = HasherQueueState.ToLower().Contains("pause");
					HasherQueueRunning = !HasherQueueState.ToLower().Contains("pause");

					GeneralQueuePaused = GeneralQueueState.ToLower().Contains("pause");
					GeneralQueueRunning = !GeneralQueueState.ToLower().Contains("pause");
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
			for (int i=0;i<languages.Count;i++)
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
				string msg = Instance.clientBinaryHTTP.RenameAllGroups();
				if (string.IsNullOrEmpty(msg))
					MessageBox.Show("Complete!");
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
						AllUsers.Add(new JMMUserVM(user));
				}
				catch (Exception ex)
				{
					Utils.ShowErrorMessage(ex);
				}
			}

		}

		public void RefreshAllCategories()
		{

			AllCategories.Clear();

			if (!ServerOnline) return;
			try
			{
				List<string> catsRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllCategoryNames();

				foreach (string cat in catsRaw)
					AllCategories.Add(cat);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using NLog;
using System.Collections.ObjectModel;
using System.Windows.Data;
using DevExpress.Xpf.LayoutControl;
using JMMClient.ViewModel;
using JMMClient.ImageDownload;
using System.IO;
using System.Net;
using JMMClient.Metro;

namespace JMMClient
{
	public class DashboardMetroVM : INotifyPropertyChanged
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private BlockingList<object> imagesToDownload = new BlockingList<object>();
		private BackgroundWorker workerImages = new BackgroundWorker();
		private static System.Timers.Timer rotateShoutsTimer = null;

		private MainWindow mainWdw;
		private List<NavContainer> NavigationHistory = new List<NavContainer>();
		private int CurrentNavIndex = 0;


		public ObservableCollection<ContinueWatchingTile> ContinueWatching { get; set; }
		public ICollectionView ViewContinueWatching { get; set; }

		public ObservableCollection<RandomSeriesTile> RandomSeries { get; set; }
		public ICollectionView ViewRandomSeries { get; set; }

		public ObservableCollection<object> TraktActivity { get; set; }
		private List<TraktShoutTile> TraktShouts = new List<TraktShoutTile>();
		private int CurrentShoutIndex = 0;

		private static DashboardMetroVM _instance;
		
		public static DashboardMetroVM Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new DashboardMetroVM();
				}
				return _instance;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

		public delegate void FinishedProcessHandler(FinishedProcessEventArgs ev);
		public event FinishedProcessHandler OnFinishedProcessEvent;
		protected void OnFinishedProcess(FinishedProcessEventArgs ev)
		{
			if (OnFinishedProcessEvent != null)
			{
				OnFinishedProcessEvent(ev);
			}
		}

		private Boolean isReadOnly = true;
		public Boolean IsReadOnly
		{
			get { return isReadOnly; }
			set
			{
				isReadOnly = value;
				NotifyPropertyChanged("IsReadOnly");
			}
		}

		private Boolean isBeingEdited = false;
		public Boolean IsBeingEdited
		{
			get { return isBeingEdited; }
			set
			{
				isBeingEdited = value;
				NotifyPropertyChanged("IsBeingEdited");
			}
		}

		private Boolean isLoadingData = true;
		public Boolean IsLoadingData
		{
			get { return isLoadingData; }
			set
			{
				isLoadingData = value;
				NotifyPropertyChanged("IsLoadingData");
			}
		}

		private DashboardMetroVM()
		{
			IsLoadingData = false;

			ContinueWatching = new ObservableCollection<ContinueWatchingTile>();
			ViewContinueWatching = CollectionViewSource.GetDefaultView(ContinueWatching);

			RandomSeries = new ObservableCollection<RandomSeriesTile>();
			ViewRandomSeries = CollectionViewSource.GetDefaultView(RandomSeries);

			TraktActivity = new ObservableCollection<object>();

			rotateShoutsTimer = new System.Timers.Timer();
			rotateShoutsTimer.AutoReset = false;
			rotateShoutsTimer.Interval = 15 * 1000; // 15 seconds
			rotateShoutsTimer.Elapsed += new System.Timers.ElapsedEventHandler(rotateShoutsTimer_Elapsed);

			workerImages.DoWork += new DoWorkEventHandler(workerImages_DoWork);

			workerImages.RunWorkerAsync();

			rotateShoutsTimer.Start();
		}

		public void InitNavigator(MainWindow wdw)
		{
			mainWdw = wdw;
			NavigationHistory.Clear();
			CurrentNavIndex = 0;
		}

		public void NavigateForward(MetroViews viewType, object content)
		{
			NavigationHistory.Add(new NavContainer() { NavView = viewType, NavContent = content });
			CurrentNavIndex = NavigationHistory.Count;
			mainWdw.ShowDashMetroView(MetroViews.ContinueWatching, content);
		}

		public void NavigateBack()
		{
			NavigationHistory.RemoveAt(NavigationHistory.Count - 1);
			CurrentNavIndex = CurrentNavIndex - 1;

			if (NavigationHistory.Count > 0)
				mainWdw.ShowDashMetroView(NavigationHistory[NavigationHistory.Count - 1].NavView, NavigationHistory[NavigationHistory.Count - 1].NavContent);
			else
				mainWdw.ShowDashMetroView(MetroViews.MainMetro);
		}

		void rotateShoutsTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			try
			{
				rotateShoutsTimer.Stop();

				if (TraktShouts.Count > 1)
				{
					CurrentShoutIndex = CurrentShoutIndex + 1;
					if ((CurrentShoutIndex + 1) > TraktShouts.Count) CurrentShoutIndex = 0;

					System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
					{
						TraktActivity[0] = TraktShouts[CurrentShoutIndex];
					});
				}
			}
			catch (Exception ex)
			{
				logger.ErrorException(ex.ToString(), ex);
			}
			finally
			{
				rotateShoutsTimer.Start();
			}
			
		}

		void workerImages_DoWork(object sender, DoWorkEventArgs e)
		{
			ProcessImages();
		}

		private void ProcessImages()
		{
			foreach (object req in imagesToDownload)
			{
				try
				{
					if (req.GetType() == typeof(TraktActivityTile))
					{
						TraktActivityTile tile = req as TraktActivityTile;

						using (WebClient client = new WebClient())
						{
							client.Headers.Add("user-agent", "JMM");

							// show
							if (!string.IsNullOrEmpty(tile.Scrobble.Episode.OnlineImagePath))
							{
								Uri uriShow = new Uri(tile.Scrobble.Episode.OnlineImagePath);
								string filenameShow = Path.GetFileName(uriShow.LocalPath);
								string tempNameShow = Path.Combine(Path.GetTempPath(), filenameShow);

								if (!File.Exists(tempNameShow))
								{
									if (tile.Scrobble.Episode.OnlineImagePath.Length > 0)
										client.DownloadFile(tile.Scrobble.Episode.OnlineImagePath, tempNameShow);
								}
								if (File.Exists(tempNameShow)) tile.ShowPicture = tempNameShow;
							}

							//user
							if (!string.IsNullOrEmpty(tile.Scrobble.UserOnlineImagePath))
							{
								Uri uriUser = new Uri(tile.Scrobble.UserOnlineImagePath);
								string filenameUser = Path.GetFileName(uriUser.LocalPath);
								string tempNameUser = Path.Combine(Path.GetTempPath(), filenameUser);

								if (!File.Exists(tempNameUser))
								{
									if (tile.Scrobble.UserOnlineImagePath.Length > 0)
										client.DownloadFile(tile.Scrobble.UserOnlineImagePath, tempNameUser);
								}

								if (File.Exists(tempNameUser)) tile.FriendPicture = tempNameUser;
							}
							
						}
						
					}

					if (req.GetType() == typeof(TraktShoutTile))
					{
						TraktShoutTile tile = req as TraktShoutTile;

						using (WebClient client = new WebClient())
						{
							client.Headers.Add("user-agent", "JMM");

							// show
							if (!string.IsNullOrEmpty(tile.OnlineShowPicture))
							{
								Uri uriShow = new Uri(tile.OnlineShowPicture);
								string filenameShow = Path.GetFileName(uriShow.LocalPath);
								string tempNameShow = Path.Combine(Path.GetTempPath(), filenameShow);
								if (!File.Exists(tempNameShow))
								{
									if (tile.OnlineShowPicture.Length > 0)
										client.DownloadFile(tile.OnlineShowPicture, tempNameShow);
								}
								if (File.Exists(tempNameShow)) tile.ShowPicture = tempNameShow;
							}

							//user
							if (!string.IsNullOrEmpty(tile.OnlineFriendPicture))
							{
								Uri uriUser = new Uri(tile.OnlineFriendPicture);
								string filenameUser = Path.GetFileName(uriUser.LocalPath);
								string tempNameUser = Path.Combine(Path.GetTempPath(), filenameUser);
								if (!File.Exists(tempNameUser))
								{
									if (tile.OnlineFriendPicture.Length > 0)
										client.DownloadFile(tile.OnlineFriendPicture, tempNameUser);
								}


								if (File.Exists(tempNameUser)) tile.FriendPicture = tempNameUser;
							}

						}

					}

					
					imagesToDownload.Remove(req);
				}
				catch (Exception ex)
				{
					imagesToDownload.Remove(req);
					logger.ErrorException(ex.ToString(), ex);
				}
			}

		}

		public void RefreshAllData()
		{
			try
			{
				RefreshBaseData();
				RefreshTraktActivity();
				RefreshContinueWatching();
				RefreshRandomSeries();

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
			}
		}

		public void RefreshBaseData()
		{
			try
			{
				IsLoadingData = true;

				DateTime start = DateTime.Now;
				MainListHelperVM.Instance.RefreshGroupsSeriesData();
				TimeSpan ts = DateTime.Now - start;

				logger.Trace("Dashboard Time: RefreshGroupsSeriesData: {0}", ts.TotalMilliseconds);


				IsLoadingData = false;

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
			}
		}

		public void RefreshContinueWatching()
		{
			try
			{
				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					ContinueWatching.Clear();
				});

				DateTime start = DateTime.Now;

				List<JMMServerBinary.Contract_AnimeEpisode> epContracts =
					JMMServerVM.Instance.clientBinaryHTTP.GetEpisodesToWatch_RecentlyWatched(UserSettingsVM.Instance.DashMetro_WatchNext_Items, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

				TimeSpan ts = DateTime.Now - start;
				logger.Trace("Dashboard Time: RefreshEpsWatchNext_Recent: contracts: {0}", ts.TotalMilliseconds);

				start = DateTime.Now;
				List<AnimeEpisodeVM> epList = new List<AnimeEpisodeVM>();
				foreach (JMMServerBinary.Contract_AnimeEpisode contract in epContracts)
				{
					AnimeEpisodeVM ep = new AnimeEpisodeVM(contract);
					string animename = ep.AnimeName; // just do this to force anidb anime detail record to be loaded
					ep.RefreshAnime();
					//ep.SetTvDBInfo();
					epList.Add(ep);
				}
				ts = DateTime.Now - start;
				logger.Trace("Dashboard Time: RefreshEpsWatchNext_Recent: episode details: {0}", ts.TotalMilliseconds);

				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					foreach (AnimeEpisodeVM ep in epList)
					{
						string imageName = "";
						if (AppSettings.DashMetroImageType == DashboardMetroImageType.Fanart)
							imageName = ep.AniDB_Anime.FanartPathPreferThumb;
						else
							imageName = ep.AniDB_Anime.DefaultPosterPath;

						ContinueWatching.Add(new ContinueWatchingTile() { EpisodeDetails = ep.EpisodeNumberAndName, AnimeName = ep.AnimeSeries.SeriesName,
																		  Picture = imageName,
																		  AnimeSeries = ep.AnimeSeries,
																		  TileSize = "Large",
																		  Height = 100
						});
					}

					ViewContinueWatching.Refresh();
				});

				OnFinishedProcess(new FinishedProcessEventArgs(DashboardMetroProcessType.ContinueWatching));
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
			}
		}

		public void RefreshRandomSeries()
		{
			try
			{
				logger.Trace("XXX1 RefreshRandomSeries");
				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					RandomSeries.Clear();
				});

				logger.Trace("XXX2 RefreshRandomSeries");
				List<AnimeSeriesVM> serList = new List<AnimeSeriesVM>();
				List<AnimeGroupVM> grps = new List<AnimeGroupVM>(MainListHelperVM.Instance.AllGroups);
				logger.Trace("XXX3 RefreshRandomSeries");

				foreach (AnimeGroupVM grp in grps)
				{
					// ignore sub groups
					if (grp.AnimeGroupParentID.HasValue) continue;

					foreach (AnimeSeriesVM ser in grp.AllAnimeSeries)
					{
						if (!ser.IsComplete) continue;
						if (ser.AllFilesWatched) continue;

						serList.Add(ser);
					}
				}

				DateTime start = DateTime.Now;
				logger.Trace("XXX4 RefreshRandomSeries");

				var serShuffledList = serList.OrderBy(a => Guid.NewGuid());

				//serList.Shuffle();

				TimeSpan ts = DateTime.Now - start;
				logger.Trace(string.Format("XXX5 Shuffled {0} series list in {1} ms", serList.Count, ts.TotalMilliseconds));

				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					foreach (AnimeSeriesVM ser in serShuffledList.Take(AppSettings.DashMetro_RandomSeries_Items))
					{
						string imageName = "";
						if (AppSettings.DashMetroImageType == DashboardMetroImageType.Fanart)
							imageName = ser.AniDB_Anime.FanartPath;
						else
							imageName = ser.AniDB_Anime.DefaultPosterPath;

						RandomSeries.Add(new RandomSeriesTile()
						{
							Details = "",
							AnimeName = ser.SeriesName,
							Picture = imageName,
							AnimeSeries = ser,
							TileSize = "Large",
							Height = 100
						});
					}

					ViewRandomSeries.Refresh();
				});

				OnFinishedProcess(new FinishedProcessEventArgs(DashboardMetroProcessType.RandomSeries));
			}
			catch (Exception ex)
			{
				logger.ErrorException(ex.ToString(), ex);
			}
			finally
			{
			}
		}

		public void RefreshTraktActivity()
		{
			try
			{
				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					TraktActivity.Clear();
					TraktShouts.Clear();
				});

				JMMServerBinary.Contract_Trakt_Activity traktActivity = JMMServerVM.Instance.clientBinaryHTTP.GetTraktFriendInfo(20,
					AppSettings.Dash_TraktFriends_AnimeOnly, true, false);

				if (traktActivity.HasTraktAccount)
				{

					string blankImageName = @"/Images/blankposter.png";
					if (AppSettings.DashMetroImageType == DashboardMetroImageType.Fanart)
						blankImageName = @"/Images/blankfanart.png";

					int numItems = 0;

					// first get all the shouts
					foreach (JMMServerBinary.Contract_Trakt_FriendActivity contractAct in traktActivity.TraktFriendActivity)
					{
						if (contractAct.ActivityAction == (int)TraktActivityAction.Shout)
						{
							if (contractAct.ActivityType == (int)TraktActivityType.Episode)
							{
								Trakt_ActivityShoutEpisodeVM shoutEp = new Trakt_ActivityShoutEpisodeVM(contractAct);

								TraktShoutTile tile = new TraktShoutTile()
								{
									ShowName = shoutEp.Shout.ShowTitle,
									ShowPicture = blankImageName,
									Details = shoutEp.Shout.EpisodeDescription + Environment.NewLine + shoutEp.Shout.Text,
									ShoutDateString = shoutEp.ActivityDateString,
									FriendName = shoutEp.User.Username,
									FriendPicture = blankImageName,
									OnlineShowPicture = shoutEp.Shout.OnlineImagePath,
									OnlineFriendPicture = shoutEp.User.Avatar,
									URL = shoutEp.Shout.Episode_Url,
									TileSize = "Large",
									Height = 100
								};

								TraktShouts.Add(tile);
								imagesToDownload.Add(tile);
								numItems = 1;
							}
							else
							{
								Trakt_ActivityShoutShowVM shoutShow = new Trakt_ActivityShoutShowVM(contractAct);

								TraktShoutTile tile = new TraktShoutTile()
								{
									ShowName = shoutShow.Shout.ShowTitle,
									ShowPicture = blankImageName,
									Details = shoutShow.Shout.Text,
									ShoutDateString = shoutShow.ActivityDateString,
									FriendName = shoutShow.User.Username,
									FriendPicture = blankImageName,
									URL = shoutShow.Shout.TraktShow.url,
									OnlineShowPicture = shoutShow.Shout.OnlineImagePath,
									OnlineFriendPicture = shoutShow.User.Avatar,
									TileSize = "Large",
									Height = 100
								};

								TraktShouts.Add(tile);
								imagesToDownload.Add(tile);
								numItems = 1;
							}
						}


					}

					if (TraktShouts.Count > 0)
					{
						System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
						{
							TraktActivity.Add(TraktShouts[0]);
						});
					}

					traktActivity = JMMServerVM.Instance.clientBinaryHTTP.GetTraktFriendInfo(AppSettings.DashMetro_TraktActivity_Items + 1,
						AppSettings.Dash_TraktFriends_AnimeOnly, false, true);

					foreach (JMMServerBinary.Contract_Trakt_FriendActivity contractAct in traktActivity.TraktFriendActivity)
					{
						if (numItems == AppSettings.DashMetro_TraktActivity_Items) break;

						if (contractAct.ActivityAction == (int)TraktActivityAction.Scrobble)
						{
							Trakt_ActivityScrobbleVM scrobble = new Trakt_ActivityScrobbleVM(contractAct);

							TraktActivityTile tile = new TraktActivityTile()
							{
								Scrobble = scrobble,
								ShowName = scrobble.Episode.ShowTitle,
								ShowPicture = blankImageName,
								EpisodeDetails = scrobble.Episode.EpisodeDescription,
								URL = scrobble.Episode.Episode_Url,
								FriendName = scrobble.User.Username,
								FriendPicture = blankImageName,
								TileSize = "Large",
								Height = 100
							};

							numItems++;

							System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
							{
								TraktActivity.Add(tile);
							});

							imagesToDownload.Add(tile);
						}
					}
				}
				else
				{
					Trakt_SignupVM signup = new Trakt_SignupVM();
					System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
					{
						TraktActivity.Add(signup);
					});
				}

				OnFinishedProcess(new FinishedProcessEventArgs(DashboardMetroProcessType.TraktActivity));
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
			}
		}
	}

	public class FinishedProcessEventArgs : EventArgs
	{
		public readonly DashboardMetroProcessType ProcessType;

		public FinishedProcessEventArgs(DashboardMetroProcessType processType)
		{
			ProcessType = processType;
		}
	}
}

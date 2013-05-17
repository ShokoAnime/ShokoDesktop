using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using JMMClient.ViewModel;
using System.Windows;
using JMMClient.UserControls;
using System.Threading;
using JMMClient.ImageDownload;
using NLog;

namespace JMMClient
{
	public class DashboardVM : INotifyPropertyChanged
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private static DashboardVM _instance;
		//public ICollectionView ViewGroups { get; set; }
		public ObservableCollection<AnimeEpisodeVM> EpsWatchNext_Recent { get; set; }
		public ICollectionView ViewEpsWatchNext_Recent { get; set; }

		public ObservableCollection<AnimeEpisodeVM> EpsWatchedRecently { get; set; }
		public ICollectionView ViewEpsWatchedRecently { get; set; }

		public ObservableCollection<AnimeSeriesVM> SeriesMissingEps { get; set; }
		public ICollectionView ViewSeriesMissingEps { get; set; }

		public ObservableCollection<AniDB_AnimeVM> MiniCalendar { get; set; }
		public ICollectionView ViewMiniCalendar { get; set; }

		public ObservableCollection<object> RecommendationsWatch { get; set; }
		public ICollectionView ViewRecommendationsWatch { get; set; }

		public ObservableCollection<object> RecommendationsDownload { get; set; }
		public ICollectionView ViewRecommendationsDownload { get; set; }

		public ObservableCollection<object> TraktActivity { get; set; }
		public ICollectionView ViewTraktActivity { get; set; }

		public ObservableCollection<object> RecentAdditions { get; set; }
		public ICollectionView ViewRecentAdditions { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

		public static DashboardVM Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new DashboardVM();
				}
				return _instance;
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

		private DashboardVM()
		{
			IsLoadingData = false;

			EpsWatchNext_Recent = new ObservableCollection<AnimeEpisodeVM>();
			ViewEpsWatchNext_Recent = CollectionViewSource.GetDefaultView(EpsWatchNext_Recent);

			EpsWatchedRecently = new ObservableCollection<AnimeEpisodeVM>();
			ViewEpsWatchedRecently = CollectionViewSource.GetDefaultView(EpsWatchedRecently);

			SeriesMissingEps = new ObservableCollection<AnimeSeriesVM>();
			ViewSeriesMissingEps = CollectionViewSource.GetDefaultView(SeriesMissingEps);

			MiniCalendar = new ObservableCollection<AniDB_AnimeVM>();
			ViewMiniCalendar = CollectionViewSource.GetDefaultView(MiniCalendar);

			RecommendationsWatch = new ObservableCollection<object>();
			ViewRecommendationsWatch = CollectionViewSource.GetDefaultView(RecommendationsWatch);

			RecommendationsDownload = new ObservableCollection<object>();
			ViewRecommendationsDownload = CollectionViewSource.GetDefaultView(RecommendationsDownload);

			TraktActivity = new ObservableCollection<object>();
			ViewTraktActivity = CollectionViewSource.GetDefaultView(TraktActivity);

			RecentAdditions = new ObservableCollection<object>();
			ViewRecentAdditions = CollectionViewSource.GetDefaultView(RecentAdditions);
			
		}



		public void RefreshData(bool traktScrobbles, bool traktShouts, bool refreshContinueWatching, bool refreshRecentAdditions, bool refreshOtherWidgets, RecentAdditionsType addType)
		{
			try
			{
				IsLoadingData = true;

				// clear all displayed data
				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					if (refreshContinueWatching) EpsWatchNext_Recent.Clear();
					if (refreshRecentAdditions) RecentAdditions.Clear();

					if (refreshOtherWidgets)
					{
						SeriesMissingEps.Clear();
						EpsWatchedRecently.Clear();
						MiniCalendar.Clear();
						RecommendationsWatch.Clear();
						RecommendationsDownload.Clear();
						TraktActivity.Clear();
					}

					if (refreshOtherWidgets)
					{
						ViewEpsWatchedRecently.Refresh();
						ViewSeriesMissingEps.Refresh();
						ViewMiniCalendar.Refresh();
						ViewRecommendationsWatch.Refresh();
						ViewRecommendationsDownload.Refresh();
						ViewTraktActivity.Refresh();
						ViewRecentAdditions.Refresh();
					}

					if (refreshContinueWatching) ViewEpsWatchNext_Recent.Refresh();
					if (refreshRecentAdditions) ViewRecentAdditions.Refresh();
				});

				DateTime start = DateTime.Now;
				MainListHelperVM.Instance.RefreshGroupsSeriesData();
				TimeSpan ts = DateTime.Now - start;

				logger.Trace("Dashboard Time: RefreshGroupsSeriesData: {0}", ts.TotalMilliseconds);

				if (refreshContinueWatching && UserSettingsVM.Instance.DashWatchNextEpExpanded)
					RefreshEpsWatchNext_Recent();

				if (refreshRecentAdditions && UserSettingsVM.Instance.DashRecentAdditionsExpanded)
					RefreshRecentAdditions(addType);

				if (refreshOtherWidgets)
				{
					if (UserSettingsVM.Instance.DashRecentlyWatchEpsExpanded)
						RefreshRecentlyWatchedEps();

					if (UserSettingsVM.Instance.DashSeriesMissingEpisodesExpanded)
						RefreshSeriesMissingEps();

					if (UserSettingsVM.Instance.DashMiniCalendarExpanded)
						RefreshMiniCalendar();

					if (UserSettingsVM.Instance.DashRecommendationsWatchExpanded)
						RefreshRecommendationsWatch();

					if (UserSettingsVM.Instance.DashRecommendationsDownloadExpanded)
						RefreshRecommendationsDownload();

					if (UserSettingsVM.Instance.DashTraktFriendsExpanded)
						RefreshTraktFriends(traktScrobbles, traktShouts);
				}

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

		public void RefreshRecentAdditions(RecentAdditionsType addType)
		{
			try
			{
				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					RecentAdditions.Clear();
				});

				if (addType == RecentAdditionsType.Episode)
				{
					List<JMMServerBinary.Contract_AnimeEpisode> epContracts =
						JMMServerVM.Instance.clientBinaryHTTP.GetEpisodesRecentlyAdded(UserSettingsVM.Instance.Dash_RecentAdditions_Items, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

					System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
					{
						foreach (JMMServerBinary.Contract_AnimeEpisode contract in epContracts)
						{
							AnimeEpisodeVM ep = new AnimeEpisodeVM(contract);
							ep.RefreshAnime();
							if (ep.AniDB_Anime != null)
							{
								ep.SetTvDBInfo();
								RecentAdditions.Add(ep);
							}
						}
						ViewRecentAdditions.Refresh();
					});
				}
				else
				{
					List<JMMServerBinary.Contract_AnimeSeries> serContracts =
						JMMServerVM.Instance.clientBinaryHTTP.GetSeriesRecentlyAdded(UserSettingsVM.Instance.Dash_RecentAdditions_Items, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

					System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
					{
						foreach (JMMServerBinary.Contract_AnimeSeries contract in serContracts)
						{
							AnimeSeriesVM ser = new AnimeSeriesVM(contract);
							RecentAdditions.Add(ser);
						}
						ViewRecentAdditions.Refresh();
					});
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
			}
		}
		

		public void RefreshTraktFriends(bool traktScrobbles, bool traktShouts)
		{
			try
			{
				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					TraktActivity.Clear();
				});

				JMMServerBinary.Contract_Trakt_Activity traktActivity = JMMServerVM.Instance.clientBinaryHTTP.GetTraktFriendInfo(AppSettings.Dash_TraktFriends_Items,
					AppSettings.Dash_TraktFriends_AnimeOnly, traktShouts, traktScrobbles);

				List<object> activity = new List<object>();

				if (traktActivity.HasTraktAccount)
				{
					foreach (JMMServerBinary.Contract_Trakt_FriendFrequest contractFriend in traktActivity.TraktFriendRequests)
					{
						Trakt_FriendRequestVM req = new Trakt_FriendRequestVM(contractFriend);
						activity.Add(req);
					}

					foreach (JMMServerBinary.Contract_Trakt_FriendActivity contractAct in traktActivity.TraktFriendActivity)
					{
						if (contractAct.ActivityAction == (int)TraktActivityAction.Scrobble)
						{
							Trakt_ActivityScrobbleVM scrobble = new Trakt_ActivityScrobbleVM(contractAct);

							if (!string.IsNullOrEmpty(scrobble.UserFullImagePath) && !File.Exists(scrobble.UserFullImagePath))
							{
								// re-download the friends avatar image
								try
								{
									System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
									{
										ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.Trakt_ActivityScrobble, scrobble, true);
										MainWindow.imageHelper.DownloadImage(req);
									});
									
								}
								catch (Exception ex)
								{
									logger.ErrorException(ex.ToString(), ex);
								}
							}

							activity.Add(scrobble);
						}
						else if (contractAct.ActivityAction == (int)TraktActivityAction.Shout)
						{
							if (contractAct.ActivityType == (int)TraktActivityType.Episode)
							{
								Trakt_ActivityShoutEpisodeVM shoutEp = new Trakt_ActivityShoutEpisodeVM(contractAct);
								activity.Add(shoutEp);
							}
							else
							{
								Trakt_ActivityShoutShowVM shoutShow = new Trakt_ActivityShoutShowVM(contractAct);
								activity.Add(shoutShow);
							}
						}
					}

					foreach (JMMServerBinary.Contract_Trakt_Friend contract in traktActivity.TraktFriends)
					{
						if (contract.WatchedEpisodes != null && contract.WatchedEpisodes.Count > 0)
						{
							Trakt_FriendVM friend = new Trakt_FriendVM(contract);
							activity.Add(friend);
						}
					}
				}
				else
				{
					Trakt_SignupVM signup = new Trakt_SignupVM();
					activity.Add(signup);
				}

				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					foreach (object act in activity)
						TraktActivity.Add(act);
					ViewTraktActivity.Refresh();
				});
				
			}
			catch (Exception ex)
			{
				logger.ErrorException(ex.ToString(), ex);
			}
			finally
			{
			}
		}

		public void RefreshRecommendationsWatch()
		{
			try
			{
				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					RecommendationsWatch.Clear();
				});

				List<JMMServerBinary.Contract_Recommendation> contracts =
					JMMServerVM.Instance.clientBinaryHTTP.GetRecommendations(UserSettingsVM.Instance.Dash_RecWatch_Items, JMMServerVM.Instance.CurrentUser.JMMUserID.Value,
					(int)RecommendationType.Watch);

				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					foreach (JMMServerBinary.Contract_Recommendation contract in contracts)
					{
						RecommendationVM rec = new RecommendationVM();
						rec.Populate(contract);
						RecommendationsWatch.Add(rec);
					}

					// add a dummy object so that we can display a prompt
					// for the user to sync thier votes
					if (RecommendationsWatch.Count == 0)
						RecommendationsWatch.Add(new SyncVotesDummy());

					ViewRecommendationsWatch.Refresh();
				});
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
			}
		}

		public void RefreshRecommendationsDownload()
		{
			try
			{
				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					RecommendationsDownload.Clear();
				});

				List<JMMServerBinary.Contract_Recommendation> contracts =
					JMMServerVM.Instance.clientBinaryHTTP.GetRecommendations(UserSettingsVM.Instance.Dash_RecDownload_Items, JMMServerVM.Instance.CurrentUser.JMMUserID.Value,
					(int)RecommendationType.Download);

				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					foreach (JMMServerBinary.Contract_Recommendation contract in contracts)
					{
						RecommendationVM rec = new RecommendationVM();
						rec.Populate(contract);
						RecommendationsDownload.Add(rec);
					}

					// add a dummy object so that we can display a prompt
					// for the user to sync thier votes
					if (RecommendationsDownload.Count == 0)
						RecommendationsDownload.Add(new SyncVotesDummy());

					ViewRecommendationsDownload.Refresh();
				});
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
			}
		}

		public void GetMissingRecommendationsDownload()
		{
			try
			{
				IsLoadingData = true;

				foreach (RecommendationVM rec in RecommendationsDownload)
				{
					if (rec.Recommended_AnimeInfoNotExists)
					{
						string result = JMMServerVM.Instance.clientBinaryHTTP.UpdateAnimeData(rec.RecommendedAnimeID);
						if (string.IsNullOrEmpty(result))
						{
							JMMServerBinary.Contract_AniDBAnime animeContract = JMMServerVM.Instance.clientBinaryHTTP.GetAnime(rec.RecommendedAnimeID);
							System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, (Action)delegate()
							{
								rec.PopulateRecommendedAnime(animeContract);
								ViewRecommendationsDownload.Refresh();
							});
						}
					}
				}

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

		public void RefreshSeriesMissingEps()
		{
			try
			{
				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					SeriesMissingEps.Clear();
				});

				List<JMMServerBinary.Contract_AnimeSeries> epSeries =
					JMMServerVM.Instance.clientBinaryHTTP.GetSeriesWithMissingEpisodes(UserSettingsVM.Instance.Dash_MissingEps_Items, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					
					foreach (JMMServerBinary.Contract_AnimeSeries contract in epSeries)
					{
						AnimeSeriesVM ser = new AnimeSeriesVM(contract);
						if (JMMServerVM.Instance.CurrentUser.EvaluateSeries(ser))
							SeriesMissingEps.Add(ser);
					}
					ViewSeriesMissingEps.Refresh();
				});
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
			}
		}

		public void RefreshEpsWatchNext_Recent()
		{
			try
			{
				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					EpsWatchNext_Recent.Clear();
				});

				DateTime start = DateTime.Now;

				List<JMMServerBinary.Contract_AnimeEpisode> epContracts =
					JMMServerVM.Instance.clientBinaryHTTP.GetContinueWatchingFilter(JMMServerVM.Instance.CurrentUser.JMMUserID.Value, UserSettingsVM.Instance.Dash_WatchNext_Items);

				TimeSpan ts = DateTime.Now - start;
				logger.Trace("Dashboard Time: RefreshEpsWatchNext_Recent: contracts: {0}", ts.TotalMilliseconds);

				start = DateTime.Now;
				List<AnimeEpisodeVM> epList = new List<AnimeEpisodeVM>();
				foreach (JMMServerBinary.Contract_AnimeEpisode contract in epContracts)
				{
					AnimeEpisodeVM ep = new AnimeEpisodeVM(contract);
					string animename = ep.AnimeName; // just do this to force anidb anime detail record to be loaded
					ep.RefreshAnime();
					ep.SetTvDBInfo();
					epList.Add(ep);
				}
				ts = DateTime.Now - start;
				logger.Trace("Dashboard Time: RefreshEpsWatchNext_Recent: episode details: {0}", ts.TotalMilliseconds);

				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					foreach (AnimeEpisodeVM ep in epList)
					{
						EpsWatchNext_Recent.Add(ep);
					}
					
					ViewEpsWatchNext_Recent.Refresh();
				});
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
			}
		}

		public void RefreshEpsWatchNext_Recent_Old()
		{
			try
			{
				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					EpsWatchNext_Recent.Clear();
				});

				List<JMMServerBinary.Contract_AnimeEpisode> epContracts =
					JMMServerVM.Instance.clientBinaryHTTP.GetEpisodesToWatch_RecentlyWatched(UserSettingsVM.Instance.Dash_WatchNext_Items, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					foreach (JMMServerBinary.Contract_AnimeEpisode contract in epContracts)
					{
						AnimeEpisodeVM ep = new AnimeEpisodeVM(contract);
						ep.RefreshAnime();

						if (ep.AniDB_Anime != null && JMMServerVM.Instance.CurrentUser.EvaluateAnime(ep.AniDB_Anime))
						{
							ep.SetTvDBInfo();
							EpsWatchNext_Recent.Add(ep);
						}
					}
					ViewEpsWatchNext_Recent.Refresh();
				});
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
			}
		}

		public void RefreshRecentlyWatchedEps()
		{
			try
			{
				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					EpsWatchedRecently.Clear();
				});

				List<JMMServerBinary.Contract_AnimeEpisode> epContracts =
					JMMServerVM.Instance.clientBinaryHTTP.GetEpisodesRecentlyWatched(UserSettingsVM.Instance.Dash_RecentlyWatchedEp_Items, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					foreach (JMMServerBinary.Contract_AnimeEpisode contract in epContracts)
					{
						AnimeEpisodeVM ep = new AnimeEpisodeVM(contract);
						ep.RefreshAnime();
						if (ep.AniDB_Anime != null && JMMServerVM.Instance.CurrentUser.EvaluateAnime(ep.AniDB_Anime))
						{
							ep.SetTvDBInfo();
							EpsWatchedRecently.Add(ep);
						}
					}
					ViewEpsWatchedRecently.Refresh();
				});
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
			}
		}

		public void RefreshMiniCalendar()
		{
			try
			{
				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					MiniCalendar.Clear();
				});

				List<JMMServerBinary.Contract_AniDBAnime> contracts = 
					JMMServerVM.Instance.clientBinaryHTTP.GetMiniCalendar(JMMServerVM.Instance.CurrentUser.JMMUserID.Value, UserSettingsVM.Instance.Dash_MiniCalendarDays);

				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					foreach (JMMServerBinary.Contract_AniDBAnime contract in contracts)
					{
						AniDB_AnimeVM anime = new AniDB_AnimeVM(contract);
						if (JMMServerVM.Instance.CurrentUser.EvaluateAnime(anime))
							MiniCalendar.Add(anime);
					}
					ViewEpsWatchNext_Recent.Refresh();
				});
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


}

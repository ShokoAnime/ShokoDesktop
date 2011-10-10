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

namespace JMMClient
{
	public class DashboardVM : INotifyPropertyChanged
	{
		private static DashboardVM _instance;
		//public ICollectionView ViewGroups { get; set; }
		public ObservableCollection<AnimeEpisodeVM> EpsWatchNext_Recent { get; set; }
		public ICollectionView ViewEpsWatchNext_Recent { get; set; }

		public ObservableCollection<AnimeSeriesVM> SeriesMissingEps { get; set; }
		public ICollectionView ViewSeriesMissingEps { get; set; }

		public ObservableCollection<AniDB_AnimeVM> MiniCalendar { get; set; }
		public ICollectionView ViewMiniCalendar { get; set; }

		

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

			SeriesMissingEps = new ObservableCollection<AnimeSeriesVM>();
			ViewSeriesMissingEps = CollectionViewSource.GetDefaultView(SeriesMissingEps);

			MiniCalendar = new ObservableCollection<AniDB_AnimeVM>();
			ViewMiniCalendar = CollectionViewSource.GetDefaultView(MiniCalendar);
		}

		

		public void RefreshData()
		{
			try
			{
				IsLoadingData = true;

				// clear all displayed data
				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					SeriesMissingEps.Clear();
					EpsWatchNext_Recent.Clear();
					MiniCalendar.Clear();

					ViewEpsWatchNext_Recent.Refresh();
					ViewSeriesMissingEps.Refresh();
					ViewMiniCalendar.Refresh();
				});

				MainListHelperVM.Instance.RefreshGroupsSeriesData();

				
				if (UserSettingsVM.Instance.DashWatchNextEpExpanded)
					RefreshEpsWatchNext_Recent();

				if (UserSettingsVM.Instance.DashSeriesMissingEpisodesExpanded)
					RefreshSeriesMissingEps();

				if (UserSettingsVM.Instance.DashMiniCalendarExpanded)
					RefreshMiniCalendar();

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
							ep.SetTvDBImageAndOverview();
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

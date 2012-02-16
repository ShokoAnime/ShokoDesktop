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

namespace JMMClient
{
	public class MainListHelperVM : INotifyPropertyChanged
	{
		private static MainListHelperVM _instance;
		public ICollectionView ViewGroups { get; set; }
		public ICollectionView ViewGroupsForms { get; set; }
		public ICollectionView ViewAVDumpFiles { get; set; }
		public ICollectionView ViewBookmarkedAnime { get; set; }

		// contains a value for each AnimeSeries and the last highlighted episode for that series
		public Dictionary<int, int> LastEpisodeForSeries { get; set; }

		public ObservableCollection<MainListWrapper> BreadCrumbs { get; set; }
		public ObservableCollection<MainListWrapper> CurrentWrapperList { get; set; }
		public ObservableCollection<GroupFilterVM> AllGroupFilters { get; set; }
		public ObservableCollection<AnimeGroupVM> AllGroups { get; set; }
		public ObservableCollection<AnimeSeriesVM> AllSeries { get; set; }
		public ObservableCollection<AVDumpVM> AVDumpFiles { get; set; }
		public ObservableCollection<BookmarkedAnimeVM> BookmarkedAnime { get; set; }
	

		public Dictionary<int, AnimeGroupVM> AllGroupsDictionary { get; set; }
		public Dictionary<int, AnimeSeriesVM> AllSeriesDictionary { get; set; }
		public Dictionary<int, AniDB_AnimeVM> AllAnimeDictionary { get; set; }
		

		public ObservableCollection<AnimeEpisodeVM> EpisodesForSeries { get; set; }
		//public ObservableCollection<FileDetailedVM> FilesForSeries { get; set; }

		//public MainListWrapper CurrentWrapper { get; set; }

		private GroupFilterVM currentGroupFilter = null;
		public GroupFilterVM CurrentGroupFilter 
		{
			get { return currentGroupFilter; }
			set
			{
				currentGroupFilter = value;
				//new FilterMainListBox(ViewGroups, SearchTextBox, CurrentGroupFilter);
			}
		}

		private TextBox searchTextBox = null;
		public TextBox SearchTextBox
		{
			get { return searchTextBox; }
			set
			{
				searchTextBox = value;
				//new FilterMainListBox(ViewGroups, SearchTextBox, CurrentGroupFilter);
			}
		}

		public ICollectionView ViewSeriesSearch { get; set; }
		private System.Timers.Timer searchTimer = null;

		private int searchResultCount = 0;

		public bool BookmarkFilter_Downloading = true;
		public bool BookmarkFilter_NotDownloading = true;

		private TextBox seriesSearchTextBox = null;
		public TextBox SeriesSearchTextBox
		{
			get { return seriesSearchTextBox; }
			set
			{
				seriesSearchTextBox = value;
				seriesSearchTextBox.TextChanged += new TextChangedEventHandler(SearchTextBox_TextChanged);
			}
		}

		public int LastAnimeGroupID { get; set; }
		public int LastAnimeSeriesID { get; set; }
		public int LastGroupFilterID { get; set; }

		public Dictionary<int, int> LastGroupForGF { get; set; } // GroupFilterID, position in list of last selected group

		public GroupFilterVM AllGroupFilter
		{
			get
			{
				return GroupFilterHelper.AllGroupsFilter;
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

		private MainListWrapper currentWrapper = null;
		public MainListWrapper CurrentWrapper
		{
			get { return currentWrapper; }
			set
			{
				currentWrapper = value;
				OnPropertyChanged(new PropertyChangedEventArgs("CurrentWrapper"));
			}
		}

		private double fullScrollerWidth = 10;
		public double FullScrollerWidth
		{
			get { return fullScrollerWidth; }
			set
			{
				fullScrollerWidth = value;
				OnPropertyChanged(new PropertyChangedEventArgs("FullScrollerWidth"));
			}
		}

		private double mainScrollerWidth = 10;
		public double MainScrollerWidth
		{
			get { return mainScrollerWidth; }
			set
			{
				mainScrollerWidth = value;
				OnPropertyChanged(new PropertyChangedEventArgs("MainScrollerWidth"));

				double temp = mainScrollerWidth - 10;
				if (temp < 10) temp = 10;
				MainScrollerChildrenWidth = temp;
				

				temp = mainScrollerWidth - 80;
				if (temp < 10) temp = 10;
				MainScrollerSeriesChildrenWidth = temp;
			}
		}

		private double playlistScrollerWidth = 10;
		public double PlaylistScrollerWidth
		{
			get { return playlistScrollerWidth; }
			set
			{
				playlistScrollerWidth = value;
				OnPropertyChanged(new PropertyChangedEventArgs("PlaylistScrollerWidth"));

			}
		}

		private double mainScrollerChildrenWidth = 10;
		public double MainScrollerChildrenWidth
		{
			get { return mainScrollerChildrenWidth; }
			set
			{
				mainScrollerChildrenWidth = value;
				OnPropertyChanged(new PropertyChangedEventArgs("MainScrollerChildrenWidth"));
			}
		}

		private double mainScrollerSeriesChildrenWidth = 10;
		public double MainScrollerSeriesChildrenWidth
		{
			get { return mainScrollerSeriesChildrenWidth; }
			set
			{
				mainScrollerSeriesChildrenWidth = value;
				OnPropertyChanged(new PropertyChangedEventArgs("MainScrollerSeriesChildrenWidth"));
			}
		}

		private bool currentWrapperIsGroup = false;
		public bool CurrentWrapperIsGroup
		{
			get { return currentWrapperIsGroup; }
			set
			{
				currentWrapperIsGroup = value;
				OnPropertyChanged(new PropertyChangedEventArgs("CurrentWrapperIsGroup"));
			}
		}

		private bool currentListWrapperIsGroup = false;
		public bool CurrentListWrapperIsGroup
		{
			get { return currentListWrapperIsGroup; }
			set
			{
				currentListWrapperIsGroup = value;
				OnPropertyChanged(new PropertyChangedEventArgs("CurrentListWrapperIsGroup"));
			}
		}

		private bool showEpisodes = false;
		public bool ShowEpisodes
		{
			get { return showEpisodes; }
			set
			{
				showEpisodes = value;
				OnPropertyChanged(new PropertyChangedEventArgs("ShowEpisodes"));
			}
		}

		private AnimeSeriesVM currentSeries = null;
		public AnimeSeriesVM CurrentSeries
		{
			get { return currentSeries; }
			set
			{
				currentSeries = value;
				OnPropertyChanged(new PropertyChangedEventArgs("TestSeries"));
			}
		}

		private Dictionary<int, AniDB_AnimeDetailedVM> allAnimeDetailedDictionary = null;
		public Dictionary<int, AniDB_AnimeDetailedVM> AllAnimeDetailedDictionary
		{
			get
			{
				if (allAnimeDetailedDictionary == null)
				{
					List<JMMServerBinary.Contract_AniDB_AnimeDetailed> allAnimeDetailed = JMMServerVM.Instance.clientBinaryHTTP.GetAllAnimeDetailed();
					allAnimeDetailedDictionary = new Dictionary<int, AniDB_AnimeDetailedVM>();
					foreach (JMMServerBinary.Contract_AniDB_AnimeDetailed aniDetail in allAnimeDetailed)
					{
						AniDB_AnimeDetailedVM anid = new AniDB_AnimeDetailedVM();
						anid.Populate(aniDetail, aniDetail.AniDBAnime.AnimeID);
						allAnimeDetailedDictionary[anid.AniDB_Anime.AnimeID] = anid;
					}
				}
				return allAnimeDetailedDictionary;
			}
			set
			{
				allAnimeDetailedDictionary = value;
			}
		}

		public static MainListHelperVM Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new MainListHelperVM();
				}
				return _instance;
			}
		}

		private MainListHelperVM()
		{
			CurrentWrapperList = new ObservableCollection<MainListWrapper>();
			AllGroupFilters = new ObservableCollection<GroupFilterVM>();
			AllGroups = new ObservableCollection<AnimeGroupVM>();
			AllSeries = new ObservableCollection<AnimeSeriesVM>();
			EpisodesForSeries = new ObservableCollection<AnimeEpisodeVM>();
			AVDumpFiles = new ObservableCollection<AVDumpVM>();
			BookmarkedAnime = new ObservableCollection<BookmarkedAnimeVM>();
			

			AllGroupsDictionary = new Dictionary<int, AnimeGroupVM>();
			AllSeriesDictionary = new Dictionary<int, AnimeSeriesVM>();
			AllAnimeDictionary = new Dictionary<int, AniDB_AnimeVM>();

			ViewGroups = CollectionViewSource.GetDefaultView(CurrentWrapperList);
			ViewGroupsForms = CollectionViewSource.GetDefaultView(AllGroups);
			ViewAVDumpFiles = CollectionViewSource.GetDefaultView(AVDumpFiles);
			ViewBookmarkedAnime = CollectionViewSource.GetDefaultView(BookmarkedAnime);

			ViewSeriesSearch = CollectionViewSource.GetDefaultView(AllSeries);
			ViewSeriesSearch.SortDescriptions.Add(new SortDescription("SeriesName", ListSortDirection.Ascending));
			ViewSeriesSearch.Filter = SeriesSearchFilter;

			BreadCrumbs = new ObservableCollection<MainListWrapper>();

			LastAnimeGroupID = 0;
			LastAnimeSeriesID = 0;
			LastGroupFilterID = 0;
			LastGroupForGF = new Dictionary<int, int>();
			LastEpisodeForSeries = new Dictionary<int, int>();
		}

		void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (searchTimer != null)
				searchTimer.Stop();

			searchTimer = new System.Timers.Timer();
			searchTimer.AutoReset = false;
			searchTimer.Interval = 500; // 500ms
			searchTimer.Elapsed += new System.Timers.ElapsedEventHandler(searchTimer_Elapsed);
			searchTimer.Enabled = true;
		}

		void searchTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
			{
				searchResultCount = 0;
				ViewSeriesSearch.Refresh();
			});
		}

		private bool SeriesSearchFilter(object obj)
		{
			AnimeSeriesVM ser = obj as AnimeSeriesVM;
			if (ser == null) return false;

			if (searchResultCount > 40) return false;

			bool passed = false;
			System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
			{
				passed = GroupSearchFilterHelper.EvaluateAnimeTextSearch(ser.AniDB_Anime, SeriesSearchTextBox.Text);
			});

			if (passed)
				searchResultCount++;

			return passed;
		}

		public void UpdateAnime(int animeID)
		{
			try
			{
				JMMServerBinary.Contract_AniDBAnime animeRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAnime(animeID);
				if (animeRaw != null)
				{
					AniDB_AnimeVM anime = new AniDB_AnimeVM(animeRaw);
					AllAnimeDictionary[anime.AnimeID] = anime;

					// update the series
					foreach (AnimeSeriesVM ser in AllSeries)
					{
						if (ser.AniDB_ID == anime.AnimeID)
						{
							ser.RefreshBase();
							ser.AniDB_Anime.Detail.RefreshBase();
							AllSeriesDictionary[ser.AnimeSeriesID.Value] = ser;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public void MoveBackUpHeirarchy()
		{
			if (BreadCrumbs.Count <= 1) return;

			try
			{

				// get the second last child wrapper
				MainListWrapper wrapper = BreadCrumbs[BreadCrumbs.Count - 2];
				ShowChildWrappers(wrapper);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public void ShowAllGroups()
		{
			try
			{
				CurrentWrapper = null;
				CurrentWrapperIsGroup = true;
				BreadCrumbs.Clear();
				BreadCrumbs.Add(null);
				CurrentWrapperList.Clear();


				AllGroupFilters.Clear();
				foreach (GroupFilterVM grpFilter in GroupFilterHelper.AllGroupFilters)
				{
					//if (grpFilter.FilterConditions.Count == 0)
					//	AllGroupFilter = grpFilter;
					AllGroupFilters.Add(grpFilter);
					CurrentWrapperList.Add(grpFilter);
				}
				ShowChildWrappers(AllGroupFilter);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public void SetGroupFilterSortingOnMainList()
		{
			if (CurrentWrapper is GroupFilterVM)
			{
				GroupFilterVM gf = CurrentWrapper as GroupFilterVM;
				ViewGroups.SortDescriptions.Clear();
				if (gf != null)
				{
					List<SortDescription> sortlist = GroupFilterHelper.GetSortDescriptions(gf);
					foreach (SortDescription sd in sortlist)
						ViewGroups.SortDescriptions.Add(sd);
				}
			}
			else
				ViewGroups.SortDescriptions.Clear();
			
			
		}

		public void SetGroupFilterSortingOnForms(GroupFilterVM gf)
		{
			ViewGroupsForms.SortDescriptions.Clear();
			if (gf != null)
			{

				List<SortDescription> sortlist = GroupFilterHelper.GetSortDescriptions(gf);
				foreach (SortDescription sd in sortlist)
					ViewGroupsForms.SortDescriptions.Add(sd);
			}
		}

		

		public void ShowChildWrappers(MainListWrapper wrapper)
		{

			try
			{
				CurrentWrapper = wrapper;
				CurrentWrapperIsGroup = wrapper is GroupFilterVM;
				CurrentListWrapperIsGroup = wrapper is AnimeGroupVM;

				if (wrapper is AnimeGroupVM) LastAnimeGroupID = ((AnimeGroupVM)wrapper).AnimeGroupID.Value;
				if (wrapper is GroupFilterVM)
				{
					CurrentGroupFilter = (GroupFilterVM)wrapper;
					LastGroupFilterID = ((GroupFilterVM)wrapper).GroupFilterID.Value;
				}
				if (wrapper is AnimeSeriesVM)
				{
					CurrentSeries = wrapper as AnimeSeriesVM;
					LastAnimeSeriesID = ((AnimeSeriesVM)wrapper).AnimeSeriesID.Value;
				}





				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					// update wrappers
					// check where this wrapper sits in the heirarchy and remove anything below it
					int pos = -1;

					


					if (wrapper != null)
					{
						for (int i = 0; i < BreadCrumbs.Count; i++)
						{
							if (wrapper is GroupFilterVM && BreadCrumbs[i] is GroupFilterVM)
							{
								GroupFilterVM wrapObj = wrapper as GroupFilterVM;
								GroupFilterVM bcObj = BreadCrumbs[i] as GroupFilterVM;
								if (wrapObj.FilterName == bcObj.FilterName) pos = i;
							}
							if (wrapper is AnimeGroupVM && BreadCrumbs[i] is AnimeGroupVM)
							{
								AnimeGroupVM wrapObj = wrapper as AnimeGroupVM;
								AnimeGroupVM bcObj = BreadCrumbs[i] as AnimeGroupVM;
								if (wrapObj.AnimeGroupID == bcObj.AnimeGroupID) pos = i;
							}
							if (wrapper is AnimeSeriesVM && BreadCrumbs[i] is AnimeSeriesVM)
							{
								AnimeSeriesVM wrapObj = wrapper as AnimeSeriesVM;
								AnimeSeriesVM bcObj = BreadCrumbs[i] as AnimeSeriesVM;
								if (wrapObj.AnimeSeriesID == bcObj.AnimeSeriesID) pos = i;
							}
							if (wrapper is AnimeEpisodeTypeVM && BreadCrumbs[i] is AnimeEpisodeTypeVM)
							{
								AnimeEpisodeTypeVM wrapObj = wrapper as AnimeEpisodeTypeVM;
								AnimeEpisodeTypeVM bcObj = BreadCrumbs[i] as AnimeEpisodeTypeVM;
								if (wrapObj.EpisodeTypeDescription == bcObj.EpisodeTypeDescription) pos = i;
							}
							if (wrapper is AnimeEpisodeVM && BreadCrumbs[i] is AnimeEpisodeVM)
							{
								AnimeEpisodeVM wrapObj = wrapper as AnimeEpisodeVM;
								AnimeEpisodeVM bcObj = BreadCrumbs[i] as AnimeEpisodeVM;
								if (wrapObj.AnimeEpisodeID == bcObj.AnimeEpisodeID) pos = i;
							}
						}
					}
					else pos = 0;

					if (pos >= 0)
					{
						for (int i = BreadCrumbs.Count - 1; i >= 0; i--)
						{
							if (i >= pos) BreadCrumbs.RemoveAt(i);
						}
					}


					BreadCrumbs.Add(wrapper);

					if (wrapper is GroupFilterVM)
					{
						if (AllGroups.Count == 0)
							RefreshGroupsSeriesData();

						// apply sorting
						// get default sorting from the group filter
					}


					// means we are at the top level
					if (wrapper == null)
					{
						CurrentWrapperList.Clear();
						AllGroupFilters.Clear();
						foreach (GroupFilterVM grpFilter in GroupFilterHelper.AllGroupFilters)
						{
							AllGroupFilters.Add(grpFilter);
							CurrentWrapperList.Add(grpFilter);
						}
					}
					else
					{
						CurrentWrapperList.Clear();
						foreach (MainListWrapper wp in wrapper.GetDirectChildren())
						{
							CurrentWrapperList.Add(wp);
						}
					}

					SetGroupFilterSortingOnMainList();
					ViewGroups.Refresh();

					//new FilterMainListBox(ViewGroups, SearchTextBox, CurrentGroupFilter);
				});
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}


		void AllGroups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			//ViewGroups.Refresh();
		}

		

		public int GroupCount
		{
			get
			{
				int i = 0;
				foreach (object obj in ViewGroups)
					i++;
				return i;
			}
		}

		private void LoadTestData()
		{
			System.Windows.Application.Current.Dispatcher.Invoke( System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate() 
			{ 
				AllGroups.Clear();
				AllSeries.Clear();

				AnimeGroupVM grpNew = new AnimeGroupVM();
				grpNew.GroupName = grpNew.SortName = "Bleach";
				AllGroups.Add(grpNew);

				grpNew = new AnimeGroupVM();
				grpNew.GroupName = grpNew.SortName = "Naruto";
				AllGroups.Add(grpNew);

				grpNew = new AnimeGroupVM();
				grpNew.GroupName = grpNew.SortName = "High School of the Dead";
				AllGroups.Add(grpNew);

				grpNew = new AnimeGroupVM();
				grpNew.GroupName = grpNew.SortName = "Gundam";
				AllGroups.Add(grpNew);
			});
		}

		public void RefreshBookmarkedAnime()
		{
			try
			{
				// set this to null so that it will be refreshed the next time it is needed

				List<JMMServerBinary.Contract_BookmarkedAnime> baRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllBookmarkedAnime();

				//if (baRaw.Count == 0) return;

				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					BookmarkedAnime.Clear();

					// must series before groups the binding is based on the groups, and will refresh when that is changed
					foreach (JMMServerBinary.Contract_BookmarkedAnime contract in baRaw)
					{

						BookmarkedAnimeVM ba = new BookmarkedAnimeVM(contract);

						if (ba.DownloadingBool && BookmarkFilter_Downloading)
							BookmarkedAnime.Add(ba);

						if (ba.NotDownloadingBool && BookmarkFilter_NotDownloading)
							BookmarkedAnime.Add(ba);
					}
					ViewBookmarkedAnime.Refresh();

				});
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public void RefreshGroupsSeriesData()
		{
			//LoadTestData();
			//return;

			try
			{
				// set this to null so that it will be refreshed the next time it is needed
				AllAnimeDetailedDictionary = null;

				List<JMMServerBinary.Contract_AnimeGroup> grpsRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllGroups(JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				List<JMMServerBinary.Contract_AnimeSeries> seriesRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllSeries(JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

				if (grpsRaw.Count == 0 || seriesRaw.Count == 0) return;

				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					AllGroups.Clear();
					AllSeries.Clear();
					AllGroupsDictionary.Clear();
					AllSeriesDictionary.Clear();
					AllAnimeDictionary.Clear();

					// must series before groups the binding is based on the groups, and will refresh when that is changed
					foreach (JMMServerBinary.Contract_AnimeSeries ser in seriesRaw)
					{
						AnimeSeriesVM serNew = new AnimeSeriesVM(ser);
						AllSeries.Add(serNew);
						AllSeriesDictionary[serNew.AnimeSeriesID.Value] = serNew;
					}

					ViewSeriesSearch.Refresh();

					foreach (JMMServerBinary.Contract_AnimeGroup grp in grpsRaw)
					{
						AnimeGroupVM grpNew = new AnimeGroupVM(grp);
						AllGroups.Add(grpNew);
						AllGroupsDictionary[grpNew.AnimeGroupID.Value] = grpNew;
					}


				});
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			

		}

		public void InitGroupsSeriesData()
		{
			//LoadTestData();
			//return;

			try
			{
				// set this to null so that it will be refreshed the next time it is needed
				AllAnimeDetailedDictionary = null;

				List<JMMServerBinary.Contract_AnimeGroup> grpsRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllGroups(JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				List<JMMServerBinary.Contract_AnimeSeries> seriesRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllSeries(JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

				if (grpsRaw.Count == 0 || seriesRaw.Count == 0) return;


					AllGroups.Clear();
					AllSeries.Clear();
					AllGroupsDictionary.Clear();
					AllSeriesDictionary.Clear();
					AllAnimeDictionary.Clear();

					// must series before groups the binding is based on the groups, and will refresh when that is changed
					foreach (JMMServerBinary.Contract_AnimeSeries ser in seriesRaw)
					{
						AnimeSeriesVM serNew = new AnimeSeriesVM(ser);
						AllSeries.Add(serNew);
						AllSeriesDictionary[serNew.AnimeSeriesID.Value] = serNew;
					}

					foreach (JMMServerBinary.Contract_AnimeGroup grp in grpsRaw)
					{
						AnimeGroupVM grpNew = new AnimeGroupVM(grp);
						AllGroups.Add(grpNew);
						AllGroupsDictionary[grpNew.AnimeGroupID.Value] = grpNew;
					}

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}


		}

		public AnimeSeriesVM GetSeriesForEpisode(AnimeEpisodeVM ep)
		{
			try
			{
				AnimeSeriesVM thisSeries = null;
				foreach (AnimeSeriesVM ser in MainListHelperVM.Instance.AllSeries)
				{
					if (ser.AnimeSeriesID == ep.AnimeSeriesID)
					{
						thisSeries = ser;
						break;
					}
				}
				return thisSeries;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

			return null;
		}

		public AnimeSeriesVM GetSeriesForVideo(int videoLocalID)
		{
			try
			{
				// get the episodes that this file applies to
				List<JMMServerBinary.Contract_AnimeEpisode> eps = JMMServerVM.Instance.clientBinaryHTTP.GetEpisodesForFile(videoLocalID, 
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				foreach (JMMServerBinary.Contract_AnimeEpisode epcontract in eps)
				{
					AnimeSeriesVM thisSeries = null;
					foreach (AnimeSeriesVM ser in MainListHelperVM.Instance.AllSeries)
					{
						if (ser.AnimeSeriesID == epcontract.AnimeSeriesID)
						{
							thisSeries = ser;
							break;
						}
					}
					return thisSeries;
				}
				
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

			return null;
		}

		public AnimeEpisodeVM GetEpisodeForVideo(VideoDetailedVM vid, EpisodeList epList)
		{
			// get the episodes that this file applies to
			

			try
			{
				List<JMMServerBinary.Contract_AnimeEpisode> eps = JMMServerVM.Instance.clientBinaryHTTP.GetEpisodesForFile(vid.VideoLocalID, 
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

				foreach (JMMServerBinary.Contract_AnimeEpisode epcontract in eps)
				{
					foreach (object epObj in epList.lbEpisodes.ItemsSource)
					{
						AnimeEpisodeVM epItem = epObj as AnimeEpisodeVM;
						if (epItem.AnimeEpisodeID == vid.AnimeEpisodeID)
							return epItem;
					}
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

			return null;
		}

		public void UpdateHeirarchy(AnimeEpisodeVM ep)
		{
			try
			{
				// update the episode first
				JMMServerBinary.Contract_AnimeEpisode contract = JMMServerVM.Instance.clientBinaryHTTP.GetEpisode(ep.AnimeEpisodeID, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				UpdateHeirarchy(contract);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public void UpdateHeirarchy(JMMServerBinary.Contract_AnimeEpisode epcontract)
		{
			try
			{
				foreach (AnimeSeriesVM ser in AllSeries)
				{
					if (ser.AnimeSeriesID.HasValue && epcontract.AnimeSeriesID == ser.AnimeSeriesID.Value)
					{
						foreach (AnimeEpisodeVM ep in ser.AllEpisodes)
						{
							if (ep.AnimeEpisodeID == epcontract.AnimeEpisodeID)
							{
								ep.Populate(epcontract);

								// update the attached videos if they are visible
								List<JMMServerBinary.Contract_VideoDetailed> contracts = JMMServerVM.Instance.clientBinaryHTTP.GetFilesForEpisode(ep.AnimeEpisodeID, 
									JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
								foreach (JMMServerBinary.Contract_VideoDetailed vidcontract in contracts)
								{
									foreach (VideoDetailedVM vid in ep.FilesForEpisode)
									{
										if (vid.VideoLocalID == vidcontract.VideoLocalID)
										{
											vid.Populate(vidcontract);
											break;
										}
									}
								}

								// update all the attached groups
								UpdateGroupAndSeriesForEpisode(ep);
							}
						}
						break;
					}
				}

				// update the episodes
				/*if (CurrentSeries != null && CurrentSeries.AnimeSeriesID.HasValue && CurrentSeries.AnimeSeriesID.Value == epcontract.AnimeSeriesID)
				{

					foreach (AnimeEpisodeVM ep in CurrentSeries.AllEpisodes)
					{
						if (ep.AnimeEpisodeID == epcontract.AnimeEpisodeID)
						{
							ep.Populate(epcontract);

							// update the attached videos if they are visible
							List<JMMServerBinary.Contract_VideoDetailed> contracts = JMMServerVM.Instance.clientBinaryHTTP.GetFilesForEpisode(ep.AnimeEpisodeID);
							foreach (JMMServerBinary.Contract_VideoDetailed vidcontract in contracts)
							{
								foreach (VideoDetailedVM vid in ep.FilesForEpisode)
								{
									if (vid.VideoLocalID == vidcontract.VideoLocalID)
									{
										vid.Populate(vidcontract);
										break;
									}
								}
							}

							// update all the attached groups
							UpdateGroupAndSeriesForEpisode(ep);
						}
					}
				}*/
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public void UpdateHeirarchy(VideoDetailedVM vid)
		{
			try
			{
				// get the episodes that this file applies to
				List<JMMServerBinary.Contract_AnimeEpisode> eps = JMMServerVM.Instance.clientBinaryHTTP.GetEpisodesForFile(vid.VideoLocalID, 
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				foreach (JMMServerBinary.Contract_AnimeEpisode epcontract in eps)
				{
					AnimeSeriesVM thisSeries = null;
					foreach (AnimeSeriesVM ser in MainListHelperVM.Instance.AllSeries)
					{
						if (ser.AnimeSeriesID == epcontract.AnimeSeriesID)
						{
							thisSeries = ser;
							break;
						}
					}

					// update the episodes
					if (thisSeries != null && thisSeries.AnimeSeriesID.HasValue && thisSeries.AnimeSeriesID.Value == epcontract.AnimeSeriesID)
					{
						foreach (AnimeEpisodeVM ep in thisSeries.AllEpisodes)
						{
							if (ep.AnimeEpisodeID == epcontract.AnimeEpisodeID)
							{
								ep.Populate(epcontract);

								// update the attached videos
								List<JMMServerBinary.Contract_VideoDetailed> contracts = JMMServerVM.Instance.clientBinaryHTTP.GetFilesForEpisode(ep.AnimeEpisodeID, 
									JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
								foreach (JMMServerBinary.Contract_VideoDetailed vidcontract in contracts)
								{
									if (vid.VideoLocalID == vidcontract.VideoLocalID)
									{
										vid.Populate(vidcontract);
										break;
									}
								}

								// update all the attached groups
								UpdateGroupAndSeriesForEpisode(ep);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void UpdateGroupAndSeriesForEpisode(AnimeEpisodeVM ep)
		{
			try
			{
				// update the attached series
				JMMServerBinary.Contract_AnimeSeries serContract = JMMServerVM.Instance.clientBinaryHTTP.GetSeries(ep.AnimeSeriesID,
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				foreach (AnimeSeriesVM ser in MainListHelperVM.Instance.AllSeries)
				{
					if (ser.AnimeSeriesID == serContract.AnimeSeriesID)
					{
						ser.Populate(serContract);

						// TODO update the episode list
						break;
					}
				}

				List<JMMServerBinary.Contract_AnimeGroup> grps = JMMServerVM.Instance.clientBinaryHTTP.GetAllGroupsAboveSeries(ep.AnimeSeriesID,
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				foreach (AnimeGroupVM grp in MainListHelperVM.Instance.AllGroups)
				{
					foreach (JMMServerBinary.Contract_AnimeGroup grpContract in grps)
					{
						if (grp.AnimeGroupID.Value == grpContract.AnimeGroupID)
						{
							grp.Populate(grpContract);
							break;
						}
					}

				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		public void UpdateHeirarchy(AnimeSeriesVM ser)
		{
			try
			{
				// update the attached series
				// refresh the data
				ser.RefreshBase();
				ser.AniDB_Anime.Detail.RefreshBase();

				List<JMMServerBinary.Contract_AnimeGroup> grps = JMMServerVM.Instance.clientBinaryHTTP.GetAllGroupsAboveSeries(ser.AnimeSeriesID.Value,
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				foreach (AnimeGroupVM grp in MainListHelperVM.Instance.AllGroups)
				{
					foreach (JMMServerBinary.Contract_AnimeGroup grpContract in grps)
					{
						if (grp.AnimeGroupID.Value == grpContract.AnimeGroupID)
						{
							grp.Populate(grpContract);
							break;
						}
					}

				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}
	}
}

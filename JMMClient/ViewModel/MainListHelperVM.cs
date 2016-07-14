using JMMClient.UserControls;
using JMMClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using JMMClient.JMMServerBinary;

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

        public ObservableCollection<AVDumpVM> AVDumpFiles { get; set; }
        public ObservableCollection<BookmarkedAnimeVM> BookmarkedAnime { get; set; }

        public DateTime LastChange { get; set; } = DateTime.MinValue;

        public Dictionary<int, AnimeGroupVM> AllGroupsDictionary { get; set; }
        public Dictionary<int, AnimeSeriesVM> AllSeriesDictionary { get; set; }
        public Dictionary<int, AniDB_AnimeVM> AllAnimeDictionary { get; set; }
        public Dictionary<int, GroupFilterVM> AllGroupFiltersDictionary { get; set; }

        public SeriesSearchType SerSearchType { get; set; }
        public int SearchResultCount = 0;

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

        //public int LastAnimeGroupID { get; set; }
        //public int LastAnimeSeriesID { get; set; }
        //public int LastGroupFilterID { get; set; }
        public string CurrentOpenGroupFilter { get; set; }

        public Dictionary<string, int> LastGroupForGF { get; set; } // GroupFilterID, position in list of last selected group

        public GroupFilterVM AllGroupFilter
        {
            get
            {
                return MainListHelperVM.Instance.AllGroupFiltersDictionary.Values.FirstOrDefault(a => a.FilterType == 4);
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

        private double fullScrollerHeight = 500;
        public double FullScrollerHeight
        {
            get { return fullScrollerHeight; }
            set
            {
                fullScrollerHeight = value;
                OnPropertyChanged(new PropertyChangedEventArgs("FullScrollerHeight"));
            }
        }

        private double downloadRecScrollerWidth = 10;
        public double DownloadRecScrollerWidth
        {
            get { return downloadRecScrollerWidth; }
            set
            {
                downloadRecScrollerWidth = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DownloadRecScrollerWidth"));
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
                    DateTime start = DateTime.Now;

                    List<JMMServerBinary.Contract_AniDB_AnimeDetailed> allAnimeDetailed = JMMServerVM.Instance.clientBinaryHTTP.GetAllAnimeDetailed();
                    allAnimeDetailedDictionary = new Dictionary<int, AniDB_AnimeDetailedVM>();
                    foreach (JMMServerBinary.Contract_AniDB_AnimeDetailed aniDetail in allAnimeDetailed)
                    {
                        AniDB_AnimeDetailedVM anid = new AniDB_AnimeDetailedVM();
                        anid.Populate(aniDetail, aniDetail.AniDBAnime.AnimeID);
                        allAnimeDetailedDictionary[anid.AniDB_Anime.AnimeID] = anid;
                    }

                    TimeSpan ts = DateTime.Now - start;
                    NLog.LogManager.GetCurrentClassLogger().Trace("Got all anime detailed in {0} ms", ts.TotalMilliseconds);
                }
                return allAnimeDetailedDictionary;
            }
            set
            {
                allAnimeDetailedDictionary = value;
            }
        }

        public event EventHandler Refreshed;

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
            SerSearchType = SeriesSearchType.TitleOnly;

            CurrentWrapperList = new ObservableCollection<MainListWrapper>();
            EpisodesForSeries = new ObservableCollection<AnimeEpisodeVM>();
            AVDumpFiles = new ObservableCollection<AVDumpVM>();
            BookmarkedAnime = new ObservableCollection<BookmarkedAnimeVM>();


            AllGroupsDictionary = new Dictionary<int, AnimeGroupVM>();
            AllSeriesDictionary = new Dictionary<int, AnimeSeriesVM>();
            AllAnimeDictionary = new Dictionary<int, AniDB_AnimeVM>();
            AllGroupFiltersDictionary = new Dictionary<int, GroupFilterVM>();
            ViewGroups = CollectionViewSource.GetDefaultView(CurrentWrapperList);
            ViewGroupsForms = CollectionViewSource.GetDefaultView(AllGroupsDictionary.Values);
            ViewAVDumpFiles = CollectionViewSource.GetDefaultView(AVDumpFiles);
            ViewBookmarkedAnime = CollectionViewSource.GetDefaultView(BookmarkedAnime);

            ViewSeriesSearch = CollectionViewSource.GetDefaultView(AllSeriesDictionary.Values);
            ViewSeriesSearch.SortDescriptions.Add(new SortDescription("SeriesName", ListSortDirection.Ascending));
            ViewSeriesSearch.Filter = SeriesSearchFilter;

            BreadCrumbs = new ObservableCollection<MainListWrapper>();

            //LastAnimeGroupID = 0;
            //LastAnimeSeriesID = 0;
            //LastGroupFilterID = 0;
            CurrentOpenGroupFilter = "";
            LastGroupForGF = new Dictionary<string, int>();
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
            System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
            {
                SearchResultCount = 0;
                ViewSeriesSearch.Refresh();
            });
        }

        private bool SeriesSearchFilter(object obj)
        {
            AnimeSeriesVM ser = obj as AnimeSeriesVM;
            if (ser == null) return false;

            if (SearchResultCount > 100) return false;

            bool passed = false;
            System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
            {
                passed = GroupSearchFilterHelper.EvaluateSeriesTextSearch(ser, SeriesSearchTextBox.Text.Replace("'", "`"), SerSearchType);
            });

            if (passed)
                SearchResultCount++;

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
                    AnimeSeriesVM ser = AllSeriesDictionary.Values.FirstOrDefault(a => a.AniDB_ID == anime.AnimeID);
                    if (ser != null)
                    {
                        ser.RefreshBase();
                        ser.AniDB_Anime.Detail.RefreshBase();
                        AllSeriesDictionary[ser.AnimeSeriesID.Value] = ser;
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


                foreach (GroupFilterVM grpFilter in AllGroupFiltersDictionary.Values)
                {
                    //if (grpFilter.FilterConditions.Count == 0)
                    //	AllGroupFilter = grpFilter;
                    if (!grpFilter.GroupFilterParentId.HasValue)
                        CurrentWrapperList.Add(grpFilter);
                }
                ShowChildWrappers(AllGroupFilter);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        //[LogExecutionTime]
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


        //[LogExecutionTime]
        public void ShowChildWrappers(MainListWrapper wrapper)
        {

            try
            {
                CurrentWrapper = wrapper;
                CurrentWrapperIsGroup = wrapper is GroupFilterVM;
                CurrentListWrapperIsGroup = wrapper is AnimeGroupVM;

                if (wrapper is AnimeGroupVM)
                {
                    //LastAnimeGroupID = ((AnimeGroupVM)wrapper).AnimeGroupID.Value;
                    MainListHelperVM.Instance.CurrentOpenGroupFilter = "AnimeGroupVM|" + ((AnimeGroupVM)wrapper).AnimeGroupID.Value;
                }
                if (wrapper is GroupFilterVM)
                {
                    CurrentGroupFilter = (GroupFilterVM)wrapper;
                    //LastGroupFilterID = ((GroupFilterVM)wrapper).GroupFilterID.Value;

                    MainListHelperVM.Instance.CurrentOpenGroupFilter = "GroupFilterVM|" + ((GroupFilterVM)wrapper).GroupFilterID.Value;
                }
                if (wrapper is AnimeSeriesVM)
                {
                    CurrentSeries = wrapper as AnimeSeriesVM;
                    //LastAnimeSeriesID = ((AnimeSeriesVM)wrapper).AnimeSeriesID.Value;

                    MainListHelperVM.Instance.CurrentOpenGroupFilter = "NoGroup";
                }

                if (wrapper == null)
                    MainListHelperVM.Instance.CurrentOpenGroupFilter = "Init";


                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
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
                        if (AllGroupsDictionary.Count == 0)
                            RefreshGroupsSeriesData();

                        // apply sorting
                        // get default sorting from the group filter
                    }


                    // means we are at the top level
                    if (wrapper == null)
                    {
                        CurrentWrapperList.Clear();
                        foreach (GroupFilterVM grpFilter in AllGroupFiltersDictionary.Values.Where(a=>!a.GroupFilterParentId.HasValue).OrderBy(a=>a.FilterName))
                        {
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



        public void RefreshBookmarkedAnime()
        {
            try
            {
                // set this to null so that it will be refreshed the next time it is needed

                List<JMMServerBinary.Contract_BookmarkedAnime> baRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllBookmarkedAnime();

                //if (baRaw.Count == 0) return;

                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
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

        public void ClearData()
        {
            AllGroupsDictionary.Clear();
            AllSeriesDictionary.Clear();
            AllAnimeDictionary.Clear();
            AllGroupFiltersDictionary.Clear();
            ViewSeriesSearch.Refresh();
            LastChange=DateTime.MinValue;
            OnRefreshed();
        }



        public void RefreshGroupsSeriesData()
        {
            //LoadTestData();
            //return;

            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
                {
                    UpdateAll();                    
                    OnRefreshed();

                });
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }


        }
        public void RefreshGroupFiltersOnly()
        {
            //LoadTestData();
            //return;

            try
            {
                // set this to null so that it will be refreshed the next time it is needed
                List<JMMServerBinary.Contract_GroupFilter> gfRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllGroupFilters();


                if (gfRaw.Count == 0) return;

                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
                {
                    object p = CurrentWrapper;

                  
                    GroupFilterVM vms = AllGroupFiltersDictionary.Values.Where(a => a.GroupFilterID == 0).FirstOrDefault();
                    AllGroupFiltersDictionary.Clear();
                    if (vms!=null)
                        AllGroupFiltersDictionary.Add(0, vms);
                    foreach (JMMServerBinary.Contract_GroupFilter gf_con in gfRaw.OrderBy(a => a.GroupFilterName))
                    {
                        GroupFilterVM gf = new GroupFilterVM(gf_con);
                        gf.AllowEditing = !gf.IsLocked;
                        AllGroupFiltersDictionary[gf.GroupFilterID.Value] = gf;
                    }
                  
                    //Restore previous condition
                    if (p is GroupFilterVM)
                    {
                        int id = ((GroupFilterVM)p).GroupFilterID.Value;
                        if (AllGroupFiltersDictionary.ContainsKey(id))
                        {
                            CurrentWrapper = AllGroupFiltersDictionary[id];
                        }
                    }
                    else if (p is AnimeGroupVM)
                    {
                        int id = ((AnimeGroupVM)p).AnimeGroupID.Value;
                        if (AllGroupsDictionary.ContainsKey(id))
                        {
                            CurrentWrapper = AllGroupsDictionary[id];
                        }

                    }
                    else if (p is AnimeSeriesVM)
                    {
                        int id = ((AnimeSeriesVM)p).AnimeSeriesID.Value;
                        if (AllSeriesDictionary.ContainsKey(id))
                        {
                            CurrentWrapper = AllSeriesDictionary[id];
                        }
                    }
                    OnRefreshed();

                });
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }


        public void UpdateAll()
        {
            try
            {
                Contract_MainChanges changes= JMMServerVM.Instance.clientBinaryHTTP.GetAllChanges(LastChange, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                AllAnimeDetailedDictionary = null;
                //Update Anime Series
                foreach (int sr in changes.Series.RemovedItems)
                {
                    if (AllSeriesDictionary.ContainsKey(sr))
                    {
                        AnimeSeriesVM vm = AllSeriesDictionary[sr];
                        AllSeriesDictionary.Remove(sr);
                    }
                }
                foreach (Contract_AnimeSeries s in changes.Series.ChangedItems)
                {
                    if (AllSeriesDictionary.ContainsKey(s.AnimeSeriesID))
                    {
                        AnimeSeriesVM v = AllSeriesDictionary[s.AnimeSeriesID];
                        v.Populate(s);
                    }
                    else
                    {
                        AnimeSeriesVM v = new AnimeSeriesVM(s);
                        AllSeriesDictionary[s.AnimeSeriesID] = v;
                    }
                }
                //Update Anime Groups
                foreach (int gr in changes.Groups.RemovedItems)
                {
                    if (AllGroupsDictionary.ContainsKey(gr))
                    {
                        AnimeGroupVM vm = AllGroupsDictionary[gr];
                        AllGroupsDictionary.Remove(gr);
                    }
                }
                foreach (Contract_AnimeGroup g in changes.Groups.ChangedItems)
                {
                    AnimeGroupVM v;
                    if (AllGroupsDictionary.ContainsKey(g.AnimeGroupID))
                    {
                        v = AllGroupsDictionary[g.AnimeGroupID];
                        v.Populate(g);
                    }
                    else
                    {
                        v = new AnimeGroupVM(g);
                        AllGroupsDictionary[g.AnimeGroupID] = v;
                    }

                }
                foreach (AnimeGroupVM v in AllGroupsDictionary.Values)
                {
                    v.PopulateSerieInfo(AllGroupsDictionary, AllSeriesDictionary);
                }

                //Update Group Filters
                foreach (int gfr in changes.Filters.RemovedItems)
                {
                    if (AllGroupFiltersDictionary.ContainsKey(gfr))
                    {
                        GroupFilterVM vm = AllGroupFiltersDictionary[gfr];
                        AllGroupFiltersDictionary.Remove(gfr);
                    }
                }
                foreach (Contract_GroupFilter gf in changes.Filters.ChangedItems)
                {
                    if (AllGroupFiltersDictionary.ContainsKey(gf.GroupFilterID.Value))
                    {
                        GroupFilterVM v = AllGroupFiltersDictionary[gf.GroupFilterID.Value];
                        v.Populate(gf);
                    }
                    else
                    {
                        GroupFilterVM v=new GroupFilterVM(gf);
                        AllGroupFiltersDictionary[gf.GroupFilterID.Value] = v;
                    }
                }

                foreach (int gf in changes.Filters.ChangedItems.Select(a => a.GroupFilterID.Value))
                {
                    GroupFilterVM v = AllGroupFiltersDictionary[gf];
                    //Recalculate Groups Count
                    v.GetDirectChildren();
                }
                LastChange = changes.LastChange;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
        public void InitGroupsSeriesData()
        {
            LastChange = DateTime.MinValue;
            UpdateAll();
        }

        public AnimeSeriesVM GetSeriesForEpisode(AnimeEpisodeVM ep)
        {
            try
            {
                return AllSeriesDictionary.SureGet(ep.AnimeSeriesID);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            return null;
        }

        public AnimeSeriesVM GetSeriesForAnime(int animeID)
        {
            try
            {
                return MainListHelperVM.Instance.AllSeriesDictionary.Values.FirstOrDefault(a => a.AniDB_ID == animeID);                
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            return null;
        }

        public AnimeSeriesVM GetSeries(int animeSeriesID)
        {
            try
            {
                if (AllSeriesDictionary.ContainsKey(animeSeriesID))
                    return AllSeriesDictionary[animeSeriesID];
                else
                    return null;
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
                List<JMMServerBinary.Contract_AnimeEpisode> eps = JMMServerVM.Instance.clientBinaryHTTP.GetEpisodesForFile(videoLocalID,JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                foreach (JMMServerBinary.Contract_AnimeEpisode epcontract in eps)
                {
                    AnimeSeriesVM thisSeries=AllSeriesDictionary.SureGet(epcontract.AnimeSeriesID);
                    if (thisSeries != null)
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
                foreach (AnimeSeriesVM ser in AllSeriesDictionary.Values)
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
                    AnimeSeriesVM thisSeries = AllSeriesDictionary.SureGet(epcontract.AnimeSeriesID);

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
                JMMServerBinary.Contract_AnimeSeries serContract = JMMServerVM.Instance.clientBinaryHTTP.GetSeries(ep.AnimeSeriesID, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                AnimeSeriesVM ser = AllSeriesDictionary.SureGet(serContract.AnimeSeriesID);
                if (ser != null)
                {
                    ser.Populate(serContract);
                    // TODO update the episode list
                }
                List<JMMServerBinary.Contract_AnimeGroup> grps = JMMServerVM.Instance.clientBinaryHTTP.GetAllGroupsAboveSeries(ep.AnimeSeriesID,JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                foreach (JMMServerBinary.Contract_AnimeGroup grpContract in grps)
                { 
                    AnimeGroupVM agrp = AllGroupsDictionary.SureGet(grpContract.AnimeGroupID);
                    agrp?.Populate(grpContract);
                    agrp?.PopulateSerieInfo(AllGroupsDictionary, AllSeriesDictionary);
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
                foreach (JMMServerBinary.Contract_AnimeGroup grpContract in grps)
                {
                    AnimeGroupVM agrp = AllGroupsDictionary.SureGet(grpContract.AnimeGroupID);
                    agrp?.Populate(grpContract);
                    agrp?.PopulateSerieInfo(AllGroupsDictionary, AllSeriesDictionary);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
        private void OnRefreshed()
        {
            var handler = this.Refreshed;
            if (handler != null)
                handler(null, EventArgs.Empty);
        }
    }
}

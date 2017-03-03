using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using NLog;
using Shoko.Commons.Extensions;
using Shoko.Commons.Notification;
using Shoko.Desktop.Utilities;
using Shoko.Models.Enums;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel
{
    public class VM_MainListHelper : INotifyPropertyChangedExt
    {
        private static VM_MainListHelper _instance;
        public ICollectionView ViewGroups { get; set; }
        public ICollectionView ViewGroupsForms { get; set; }
        public ICollectionView ViewAVDumpFiles { get; set; }
        public ICollectionView ViewBookmarkedAnime { get; set; }

        // contains a value for each AnimeSeries and the last highlighted episode for that series
        public Dictionary<int, int> LastEpisodeForSeries { get; set; }

        public ObservableCollection<IListWrapper> BreadCrumbs { get; set; }
        public ObservableCollection<IListWrapper> CurrentWrapperList { get; set; }

        public ObservableCollection<VM_AVDump> AVDumpFiles { get; set; }
        public ObservableCollection<VM_BookmarkedAnime> BookmarkedAnime { get; set; }

        public DateTime LastChange { get; set; } = DateTime.MinValue;

        public ObservableListDictionary<int, VM_AnimeGroup_User> AllGroupsDictionary { get; set; }
        public ObservableListDictionary<int, VM_AnimeSeries_User> AllSeriesDictionary { get; set; }
        public Dictionary<int, VM_AniDB_Anime> AllAnimeDictionary { get; set; }
        public Dictionary<int, VM_GroupFilter> AllGroupFiltersDictionary { get; set; }


        public SeriesSearchType SerSearchType { get; set; }
        public int SearchResultCount;

        public ObservableCollection<VM_AnimeEpisode_User> EpisodesForSeries { get; set; }
        //public ObservableCollection<FileDetailedVM> FilesForSeries { get; set; }

        //public MainListWrapper CurrentWrapper { get; set; }

        public VM_GroupFilter CurrentGroupFilter { get; set; }

        public TextBox SearchTextBox { get; set; }

        public ICollectionView ViewSeriesSearch { get; set; }
        private Timer searchTimer;



        public bool BookmarkFilter_Downloading = true;
        public bool BookmarkFilter_NotDownloading = true;

        private TextBox seriesSearchTextBox;
        public TextBox SeriesSearchTextBox
        {
            get { return seriesSearchTextBox; }
            set
            {
                seriesSearchTextBox = value;
                seriesSearchTextBox.TextChanged += SearchTextBox_TextChanged;
            }
        }

        //public int LastAnimeGroupID { get; set; }
        //public int LastAnimeSeriesID { get; set; }
        //public int LastGroupFilterID { get; set; }
        public string CurrentOpenGroupFilter { get; set; }

        public Dictionary<string, int> LastGroupForGF { get; set; } // GroupFilterID, position in list of last selected group

        public VM_GroupFilter AllGroupFilter
        {
            get
            {
                return Instance.AllGroupFiltersDictionary.Values.FirstOrDefault(a => a.FilterType == 4);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        private IListWrapper currentWrapper;
        public IListWrapper CurrentWrapper
        {
            get { return currentWrapper; }
            set
            {
                this.SetField(()=>currentWrapper,value);
            }
        }

        private double fullScrollerWidth = 10;
        public double FullScrollerWidth
        {
            get { return fullScrollerWidth; }
            set
            {
                this.SetField(()=>fullScrollerWidth,value);
            }
        }

        private double fullScrollerHeight = 500;
        public double FullScrollerHeight
        {
            get { return fullScrollerHeight; }
            set
            {
                this.SetField(()=>fullScrollerHeight,value);
            }
        }

        private double downloadRecScrollerWidth = 10;
        public double DownloadRecScrollerWidth
        {
            get { return downloadRecScrollerWidth; }
            set
            {
                this.SetField(()=>downloadRecScrollerWidth,value);
            }
        }

        private double mainScrollerWidth = 10;
        public double MainScrollerWidth
        {
            get { return mainScrollerWidth; }
            set
            {
                this.SetField(()=>mainScrollerWidth,value);


                double temp = mainScrollerWidth - 10;
                if (temp < 10) temp = 10;
                MainScrollerChildrenWidth = temp;


                temp = mainScrollerWidth - 80;
                if (temp < 10) temp = 10;
                MainScrollerSeriesChildrenWidth = temp;
            }
        }

        private double playlistWidth = 10;
        public double PlaylistWidth
        {
            get { return playlistWidth; }
            set
            {
                this.SetField(()=>playlistWidth,value);

            }
        }

        private double mainScrollerChildrenWidth = 10;
        public double MainScrollerChildrenWidth
        {
            get { return mainScrollerChildrenWidth; }
            set
            {
                this.SetField(()=>mainScrollerChildrenWidth,value);
            }
        }

        private double mainScrollerSeriesChildrenWidth = 10;
        public double MainScrollerSeriesChildrenWidth
        {
            get { return mainScrollerSeriesChildrenWidth; }
            set
            {
                this.SetField(()=>mainScrollerSeriesChildrenWidth,value);
            }
        }

        private bool currentWrapperIsGroup;
        public bool CurrentWrapperIsGroup
        {
            get { return currentWrapperIsGroup; }
            set
            {
                this.SetField(()=>currentWrapperIsGroup,value);
            }
        }

        private bool currentListWrapperIsGroup;
        public bool CurrentListWrapperIsGroup
        {
            get { return currentListWrapperIsGroup; }
            set
            {
                this.SetField(()=>currentListWrapperIsGroup,value);
            }
        }

        private bool showEpisodes;
        public bool ShowEpisodes
        {
            get { return showEpisodes; }
            set
            {
                this.SetField(()=>showEpisodes,value);
            }
        }

        private VM_AnimeSeries_User currentSeries;
        public VM_AnimeSeries_User CurrentSeries
        {
            get { return currentSeries; }
            set
            {
                this.SetField(()=>currentSeries,value);
            }
        }

        private Dictionary<int, VM_AniDB_AnimeDetailed> allAnimeDetailedDictionary;
        public Dictionary<int, VM_AniDB_AnimeDetailed> AllAnimeDetailedDictionary
        {
            get
            {
                if (allAnimeDetailedDictionary == null)                {
                    DateTime start = DateTime.Now;
                    allAnimeDetailedDictionary = VM_ShokoServer.Instance.ShokoServices.GetAllAnimeDetailed().Cast<VM_AniDB_AnimeDetailed>().ToDictionary(a => a.AniDBAnime.AnimeID, a => a);                    
                    TimeSpan ts = DateTime.Now - start;
                    LogManager.GetCurrentClassLogger().Trace("Got all anime detailed in {0} ms", ts.TotalMilliseconds);
                }
                return allAnimeDetailedDictionary;
            }
            set
            {
                allAnimeDetailedDictionary = value;
            }
        }

        public event EventHandler Refreshed;

        public static VM_MainListHelper Instance => _instance ?? (_instance = new VM_MainListHelper());

        private VM_MainListHelper()
        {
            SerSearchType = SeriesSearchType.TitleOnly;

            CurrentWrapperList = new ObservableCollection<IListWrapper>();
            EpisodesForSeries = new ObservableCollection<VM_AnimeEpisode_User>();
            AVDumpFiles = new ObservableCollection<VM_AVDump>();
            BookmarkedAnime = new ObservableCollection<VM_BookmarkedAnime>();


            AllGroupsDictionary = new ObservableListDictionary<int, VM_AnimeGroup_User>();
            AllSeriesDictionary = new ObservableListDictionary<int, VM_AnimeSeries_User>();
            AllAnimeDictionary = new Dictionary<int, VM_AniDB_Anime>();
            AllGroupFiltersDictionary = new Dictionary<int, VM_GroupFilter>();
            ViewGroups = CollectionViewSource.GetDefaultView(CurrentWrapperList);
            ViewGroupsForms = CollectionViewSource.GetDefaultView(AllGroupsDictionary.Values);
            ViewAVDumpFiles = CollectionViewSource.GetDefaultView(AVDumpFiles);
            ViewBookmarkedAnime = CollectionViewSource.GetDefaultView(BookmarkedAnime);

            ViewSeriesSearch = CollectionViewSource.GetDefaultView(AllSeriesDictionary.Values);
            ViewSeriesSearch.SortDescriptions.Add(new SortDescription("SeriesName", ListSortDirection.Ascending));
            ViewSeriesSearch.Filter = SeriesSearchFilter;

            BreadCrumbs = new ObservableCollection<IListWrapper>();

            //LastAnimeGroupID = 0;
            //LastAnimeSeriesID = 0;
            //LastGroupFilterID = 0;
            CurrentOpenGroupFilter = "";
            LastGroupForGF = new Dictionary<string, int>();
            LastEpisodeForSeries = new Dictionary<int, int>();
        }

        void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchTimer?.Stop();

            searchTimer = new Timer
            {
                AutoReset = false,
                Interval = 500
            };
            // 500ms
            searchTimer.Elapsed += searchTimer_Elapsed;
            searchTimer.Enabled = true;
        }

        void searchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
            {
                SearchResultCount = 0;
                ViewSeriesSearch.Refresh();
            });
        }

        private bool SeriesSearchFilter(object obj)
        {
            VM_AnimeSeries_User ser = obj as VM_AnimeSeries_User;
            if (ser == null) return false;

            if (SearchResultCount > 100) return false;

            var passed = GroupSearchFilterHelper.EvaluateSeriesTextSearch(ser, SeriesSearchTextBox.Text.Replace("'", "`"), SerSearchType);

            if (passed)
                SearchResultCount++;

            return passed;
        }

        public void UpdateAnime(int animeID)
        {
            try
            {
                VM_AniDB_Anime anime = (VM_AniDB_Anime)VM_ShokoServer.Instance.ShokoServices.GetAnime(animeID);
                if (anime != null)
                {
                    AllAnimeDictionary[anime.AnimeID] = anime;

                    // update the series
                    VM_AnimeSeries_User ser = AllSeriesDictionary.Values.FirstOrDefault(a => a.AniDB_ID == anime.AnimeID);
                    if (ser != null)
                    {
                        VM_MainListHelper.Instance.UpdateAll();
                        AllSeriesDictionary[ser.AnimeSeriesID] = ser;
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
                IListWrapper wrapper = BreadCrumbs[BreadCrumbs.Count - 2];
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


                foreach (VM_GroupFilter grpFilter in AllGroupFiltersDictionary.Values)
                {
                    //if (grpFilter.FilterConditions.Count == 0)
                    //	AllGroupFilter = grpFilter;
                    if (!grpFilter.ParentGroupFilterID.HasValue)
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
            if (CurrentWrapper is VM_GroupFilter)
            {
                VM_GroupFilter gf = (VM_GroupFilter) CurrentWrapper;
                ViewGroups.SortDescriptions.Clear();              
                List<SortDescription> sortlist = gf.GetSortDescriptions();
                foreach (SortDescription sd in sortlist)
                    ViewGroups.SortDescriptions.Add(sd);
            }
            else
                ViewGroups.SortDescriptions.Clear();


        }

        public void SetGroupFilterSortingOnForms(VM_GroupFilter gf)
        {
            ViewGroupsForms.SortDescriptions.Clear();
            if (gf != null)
            {

                List<SortDescription> sortlist = gf.GetSortDescriptions();
                foreach (SortDescription sd in sortlist)
                    ViewGroupsForms.SortDescriptions.Add(sd);
            }
        }


        //[LogExecutionTime]
        public void ShowChildWrappers(IListWrapper wrapper)
        {

            try
            {
                CurrentWrapper = wrapper;
                CurrentWrapperIsGroup = wrapper is VM_GroupFilter;
                CurrentListWrapperIsGroup = wrapper is VM_AnimeGroup_User;

                if (wrapper is VM_AnimeGroup_User)
                {
                    //LastAnimeGroupID = ((VM_AnimeGroup_User)wrapper).AnimeGroupID.Value;
                    Instance.CurrentOpenGroupFilter = "VM_AnimeGroup_User|" + ((VM_AnimeGroup_User)wrapper).AnimeGroupID;
                }
                if (wrapper is VM_GroupFilter)
                {
                    CurrentGroupFilter = (VM_GroupFilter)wrapper;
                    //LastGroupFilterID = ((GroupFilterVM)wrapper).GroupFilterID.Value;

                    Instance.CurrentOpenGroupFilter = "GroupFilterVM|" + ((VM_GroupFilter)wrapper).GroupFilterID;
                }
                if (wrapper is VM_AnimeSeries_User)
                {
                    CurrentSeries = (VM_AnimeSeries_User) wrapper;
                    //LastAnimeSeriesID = ((VM_AnimeSeries_User)wrapper).AnimeSeriesID.Value;

                    Instance.CurrentOpenGroupFilter = "NoGroup";
                }

                if (wrapper == null)
                    Instance.CurrentOpenGroupFilter = "Init";


                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                {
                    // update wrappers
                    // check where this wrapper sits in the heirarchy and remove anything below it
                    int pos = -1;




                    if (wrapper != null)
                    {
                        for (int i = 0; i < BreadCrumbs.Count; i++)
                        {
                            if (wrapper is VM_GroupFilter && BreadCrumbs[i] is VM_GroupFilter)
                            {
                                VM_GroupFilter wrapObj = (VM_GroupFilter) wrapper;
                                VM_GroupFilter bcObj = (VM_GroupFilter) BreadCrumbs[i];
                                if (wrapObj.GroupFilterName == bcObj.GroupFilterName) pos = i;
                            }
                            if (wrapper is VM_AnimeGroup_User && BreadCrumbs[i] is VM_AnimeGroup_User)
                            {
                                VM_AnimeGroup_User wrapObj = (VM_AnimeGroup_User) wrapper;
                                VM_AnimeGroup_User bcObj = (VM_AnimeGroup_User) BreadCrumbs[i];
                                if (wrapObj.AnimeGroupID == bcObj.AnimeGroupID) pos = i;
                            }
                            if (wrapper is VM_AnimeSeries_User && BreadCrumbs[i] is VM_AnimeSeries_User)
                            {
                                VM_AnimeSeries_User wrapObj = (VM_AnimeSeries_User) wrapper;
                                VM_AnimeSeries_User bcObj = (VM_AnimeSeries_User) BreadCrumbs[i];
                                if (wrapObj.AnimeSeriesID == bcObj.AnimeSeriesID) pos = i;
                            }
                            if (wrapper is VM_AnimeEpisodeType && BreadCrumbs[i] is VM_AnimeEpisodeType)
                            {
                                VM_AnimeEpisodeType wrapObj = (VM_AnimeEpisodeType) wrapper;
                                VM_AnimeEpisodeType bcObj = (VM_AnimeEpisodeType) BreadCrumbs[i];
                                if (wrapObj.EpisodeTypeDescription == bcObj.EpisodeTypeDescription) pos = i;
                            }
                            if (wrapper is VM_AnimeEpisode_User && BreadCrumbs[i] is VM_AnimeEpisode_User)
                            {
                                VM_AnimeEpisode_User wrapObj = (VM_AnimeEpisode_User) wrapper;
                                VM_AnimeEpisode_User bcObj = (VM_AnimeEpisode_User) BreadCrumbs[i];
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

                    if (wrapper is VM_GroupFilter)
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
                        foreach (VM_GroupFilter grpFilter in AllGroupFiltersDictionary.Values.Where(a => !a.ParentGroupFilterID.HasValue).OrderBy(a => a.GroupFilterName))
                        {
                            CurrentWrapperList.Add(grpFilter);
                        }
                    }
                    else
                    {
                        CurrentWrapperList.Clear();
                        foreach (IListWrapper wp in wrapper.GetDirectChildren())
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

        /*
        void AllGroups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //ViewGroups.Refresh();
        }
        */


        public int GroupCount
        {
            get
            {

                int i = 0;
                // ReSharper disable once UnusedVariable
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

                List<VM_BookmarkedAnime> baRaw = VM_ShokoServer.Instance.ShokoServices.GetAllBookmarkedAnime().CastList<VM_BookmarkedAnime>();

                //if (baRaw.Count == 0) return;

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                {
                    BookmarkedAnime.Clear();

                    // must series before groups the binding is based on the groups, and will refresh when that is changed
                    foreach (VM_BookmarkedAnime ba in baRaw)
                    {


                        if (ba.DownloadingBool && BookmarkFilter_Downloading)
                            BookmarkedAnime.Add(ba);

                        if (!ba.DownloadingBool && BookmarkFilter_NotDownloading)
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
            LastChange = DateTime.MinValue;
            OnRefreshed();
        }



        public void RefreshGroupsSeriesData()
        {
            //LoadTestData();
            //return;

            try
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
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
                List<VM_GroupFilter> gfRaw = VM_ShokoServer.Instance.ShokoServices.GetAllGroupFilters().CastList<VM_GroupFilter>();


                if (gfRaw.Count == 0) return;

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                {
                    object p = CurrentWrapper;


                    VM_GroupFilter vms = AllGroupFiltersDictionary.Values.FirstOrDefault(a => a.GroupFilterID == 0);
                    AllGroupFiltersDictionary.Clear();
                    if (vms != null)
                        AllGroupFiltersDictionary.Add(0, vms);
                    foreach (VM_GroupFilter gf in gfRaw.OrderBy(a => a.GroupFilterName))
                    {
                        AllGroupFiltersDictionary[gf.GroupFilterID] = gf;
                    }

                    //Restore previous condition
                    if (p is VM_GroupFilter)
                    {
                        int id = ((VM_GroupFilter)p).GroupFilterID;
                        if (AllGroupFiltersDictionary.ContainsKey(id))
                        {
                            CurrentWrapper = AllGroupFiltersDictionary[id];
                        }
                    }
                    else if (p is VM_AnimeGroup_User)
                    {
                        int id = ((VM_AnimeGroup_User)p).AnimeGroupID;
                        if (AllGroupsDictionary.ContainsKey(id))
                        {
                            CurrentWrapper = AllGroupsDictionary[id];
                        }

                    }
                    else if (p is VM_AnimeSeries_User)
                    {
                        int id = ((VM_AnimeSeries_User)p).AnimeSeriesID;
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
                CL_MainChanges changes = VM_ShokoServer.Instance.ShokoServices.GetAllChanges(LastChange, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                AllAnimeDetailedDictionary = null;

                //Update Anime Groups (Must be done before Series)
                foreach (int gr in changes.Groups.RemovedItems)
                {
                    AllGroupsDictionary.Remove(gr);
                }

                foreach (VM_AnimeGroup_User g in changes.Groups.ChangedItems.CastList<VM_AnimeGroup_User>())
                {
                    VM_AnimeGroup_User v;

                    if (AllGroupsDictionary.TryGetValue(g.AnimeGroupID, out v))
                    {
                        v.Populate(g);
                    }
                    else
                    {
                        AllGroupsDictionary[g.AnimeGroupID] = g;
                    }
                }

                //Update Anime Series (NOTE: relies on AllGroupsDictionary being up to date)
                foreach (int sr in changes.Series.RemovedItems)
                {
                    AllSeriesDictionary.Remove(sr);
                }

                foreach (VM_AnimeSeries_User s in changes.Series.ChangedItems.CastList<VM_AnimeSeries_User>())
                {
                    VM_AnimeSeries_User v;

                    if (AllSeriesDictionary.TryGetValue(s.AnimeSeriesID, out v))
                    {
                        v.Populate(s);
                    }
                    else
                    {
                        AllSeriesDictionary[s.AnimeSeriesID] = s;
                    }
                }

                foreach (VM_AnimeGroup_User v in AllGroupsDictionary.Values)
                {
                    v.PopulateSerieInfo(AllGroupsDictionary, AllSeriesDictionary);
                }

                //Update Group Filters
                foreach (int gfr in changes.Filters.RemovedItems)
                {
                    AllGroupFiltersDictionary.Remove(gfr);
                }

                foreach (VM_GroupFilter gf in changes.Filters.ChangedItems.CastList<VM_GroupFilter>())
                {
                    VM_GroupFilter v;

                    if (AllGroupFiltersDictionary.TryGetValue(gf.GroupFilterID, out v))
                    {
                        v.Populate(gf);
                    }
                    else
                    {
                        AllGroupFiltersDictionary[gf.GroupFilterID] = gf;
                    }
                }

                foreach (int gf in changes.Filters.ChangedItems.Select(a => a.GroupFilterID))
                {
                    VM_GroupFilter v = AllGroupFiltersDictionary[gf];
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

        public void UpdateAnimeGroups()
        {
            CL_MainChanges changes = VM_ShokoServer.Instance.ShokoServices.GetAllChanges(LastChange,
                VM_ShokoServer.Instance.CurrentUser.JMMUserID);

            //Update Anime Groups
            foreach (int gr in changes.Groups.RemovedItems)
            {
                AllGroupsDictionary.Remove(gr);
            }
            foreach (CL_AnimeGroup_User g in changes.Groups.ChangedItems)
            {
                VM_AnimeGroup_User v;

                if (AllGroupsDictionary.TryGetValue(g.AnimeGroupID, out v))
                {
                    v.Populate(g);
                }
                else
                {
                    AllGroupsDictionary[g.AnimeGroupID] = (VM_AnimeGroup_User)g;
                }
            }
            foreach (VM_AnimeGroup_User v in AllGroupsDictionary.Values)
            {
                v.PopulateSerieInfo(AllGroupsDictionary, AllSeriesDictionary);
            }
        }

        public void InitGroupsSeriesData()
        {
            LastChange = DateTime.MinValue;
            UpdateAll();
        }

        public VM_AnimeSeries_User GetSeriesForEpisode(VM_AnimeEpisode_User ep)
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

        public VM_AnimeSeries_User GetSeriesForAnime(int animeID)
        {
            try
            {
                return AllSeriesDictionary.Values.FirstOrDefault(a => a.AniDB_ID == animeID);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            return null;
        }

        public VM_AnimeSeries_User GetSeries(int animeSeriesID)
        {
            try
            {
                VM_AnimeSeries_User vm;

                AllSeriesDictionary.TryGetValue(animeSeriesID, out vm);

                return vm;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            return null;
        }

        public VM_AnimeSeries_User GetSeriesForVideo(int videoLocalID)
        {
            try
            {
                // get the episodes that this file applies to
                foreach (CL_AnimeEpisode_User epcontract in VM_ShokoServer.Instance.ShokoServices.GetEpisodesForFile(videoLocalID, VM_ShokoServer.Instance.CurrentUser.JMMUserID))
                {
                    VM_AnimeSeries_User thisSeries = AllSeriesDictionary.SureGet(epcontract.AnimeSeriesID);
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
        /*
        public VM_AnimeEpisode_User GetEpisodeForVideo(VM_VideoDetailed vid, EpisodeList epList)
        {
            // get the episodes that this file applies to


            try
            {
                List<JMMServerBinary.Contract_AnimeEpisode> eps = VM_ShokoServer.Instance.ShokoServices.GetEpisodesForFile(vid.VideoLocalID,
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID.Value);

                foreach (JMMServerBinary.Contract_AnimeEpisode epcontract in eps)
                {
                    foreach (object epObj in epList.lbEpisodes.ItemsSource)
                    {
                        VM_AnimeEpisode_User epItem = epObj as VM_AnimeEpisode_User;
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
        */
        public void RefreshHeirarchy(VM_AnimeEpisode_User ep)
        {
            try
            {
                // update the episode first
                VM_AnimeEpisode_User contract = (VM_AnimeEpisode_User)VM_ShokoServer.Instance.ShokoServices.GetEpisode(ep.AnimeEpisodeID, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                UpdateHeirarchy(contract);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void UpdateHeirarchy(VM_AnimeEpisode_User epcontract)
        {
            try
            {
                foreach (VM_AnimeSeries_User ser in AllSeriesDictionary.Values)
                {
                    if (ser.AnimeSeriesID!=0 && epcontract.AnimeSeriesID == ser.AnimeSeriesID)
                    {
                        foreach (VM_AnimeEpisode_User ep in ser.AllEpisodes)
                        {
                            if (ep.AnimeEpisodeID == epcontract.AnimeEpisodeID)
                            {
                                ep.Populate(epcontract);

                                // update the attached videos if they are visible
                                IEnumerable<VM_VideoDetailed> vids = VM_ShokoServer.Instance.ShokoServices.GetFilesForEpisode(ep.AnimeEpisodeID,
                                    VM_ShokoServer.Instance.CurrentUser.JMMUserID).Cast<VM_VideoDetailed>();
                                foreach (VM_VideoDetailed vidcontract in vids)
                                {
                                    foreach (VM_VideoDetailed vid in ep.FilesForEpisode)
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

					foreach (VM_AnimeEpisode_User ep in CurrentSeries.AllEpisodes)
					{
						if (ep.AnimeEpisodeID == epcontract.AnimeEpisodeID)
						{
							ep.Populate(epcontract);

							// update the attached videos if they are visible
							List<JMMServerBinary.Contract_VideoDetailed> contracts = VM_ShokoServer.Instance.clientBinaryHTTP.GetFilesForEpisode(ep.AnimeEpisodeID);
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

        public void UpdateHeirarchy(VM_VideoDetailed vid)
        {
            try
            {
                // get the episodes that this file applies to
                List<VM_AnimeEpisode_User> eps = VM_ShokoServer.Instance.ShokoServices.GetEpisodesForFile(vid.VideoLocalID,
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeEpisode_User>();
                foreach (VM_AnimeEpisode_User epcontract in eps)
                {
                    VM_AnimeSeries_User thisSeries = AllSeriesDictionary.SureGet(epcontract.AnimeSeriesID);

                    // update the episodes
                    if (thisSeries != null && thisSeries.AnimeSeriesID!=0 && thisSeries.AnimeSeriesID == epcontract.AnimeSeriesID)
                    {
                        foreach (VM_AnimeEpisode_User ep in thisSeries.AllEpisodes)
                        {
                            if (ep.AnimeEpisodeID == epcontract.AnimeEpisodeID)
                            {
                                ep.Populate(epcontract);

                                // update the attached videos
                                IEnumerable<VM_VideoDetailed> vids = VM_ShokoServer.Instance.ShokoServices.GetFilesForEpisode(ep.AnimeEpisodeID, VM_ShokoServer.Instance.CurrentUser.JMMUserID).Cast<VM_VideoDetailed>();
                                foreach (VM_VideoDetailed vidcontract in vids)
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

        private void UpdateGroupAndSeriesForEpisode(VM_AnimeEpisode_User ep)
        {
            try
            {
                UpdateAll();
                VM_AnimeSeries_User ser = AllSeriesDictionary.SureGet(ep.AnimeSeriesID);
                if (ser != null)
                    UpdateAboveGroups(ser);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void UpdateAboveGroups(VM_AnimeSeries_User ser)
        {
            int? groupID = ser.AnimeGroupID;
            List<CL_AnimeGroup_User> grps = new List<CL_AnimeGroup_User>();
            while (groupID.HasValue)
            {
                VM_AnimeGroup_User grp = AllGroupsDictionary.SureGet(groupID.Value);
                if (grp != null)
                {
                    grp.PopulateSerieInfo(AllGroupsDictionary, AllSeriesDictionary);
                }
                else
                {
                    groupID = null;
                }
                groupID = grp.AnimeGroupParentID;
            }
        }

        public void UpdateHeirarchy(VM_AnimeSeries_User ser)
        {
            try
            {
                UpdateAll();
                UpdateAboveGroups(ser);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
        private void OnRefreshed()
        {
            var handler = Refreshed;
            handler?.Invoke(null, EventArgs.Empty);
        }
    }
}
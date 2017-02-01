using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using NLog;
using Shoko.Commons.Extensions;
using Shoko.Commons.Notification;
using Shoko.Models.Enums;
using Shoko.Desktop.UserControls;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Desktop.ViewModel.Server;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel
{
    public class VM_Dashboard : INotifyPropertyChangedExt
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static VM_Dashboard _instance;
        //public ICollectionView ViewGroups { get; set; }
        public ObservableCollection<VM_AnimeEpisode_User> EpsWatchNext_Recent { get; set; }
        public ICollectionView ViewEpsWatchNext_Recent { get; set; }

        public ObservableCollection<VM_AnimeEpisode_User> EpsWatchedRecently { get; set; }
        public ICollectionView ViewEpsWatchedRecently { get; set; }

        public ObservableCollection<VM_AnimeSeries_User> SeriesMissingEps { get; set; }
        public ICollectionView ViewSeriesMissingEps { get; set; }

        public ObservableCollection<VM_AniDB_Anime> MiniCalendar { get; set; }
        public ICollectionView ViewMiniCalendar { get; set; }

        public ObservableCollection<object> RecommendationsWatch { get; set; }
        public ICollectionView ViewRecommendationsWatch { get; set; }

        public ObservableCollection<object> RecommendationsDownload { get; set; }
        public ICollectionView ViewRecommendationsDownload { get; set; }

        public ObservableCollection<object> RecentAdditions { get; set; }
        public ICollectionView ViewRecentAdditions { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        public static VM_Dashboard Instance => _instance ?? (_instance = new VM_Dashboard());

        private Boolean isReadOnly = true;
        public Boolean IsReadOnly
        {
            get { return isReadOnly; }
            set
            {
                this.SetField(()=>isReadOnly,value);
            }
        }

        private Boolean isBeingEdited;
        public Boolean IsBeingEdited
        {
            get { return isBeingEdited; }
            set
            {
                this.SetField(()=>isBeingEdited,value);
            }
        }

        private Boolean isLoadingData = true;
        public Boolean IsLoadingData
        {
            get { return isLoadingData; }
            set
            {
                this.SetField(()=>isLoadingData,value);
            }
        }

        private VM_Dashboard()
        {
            IsLoadingData = false;

            EpsWatchNext_Recent = new ObservableCollection<VM_AnimeEpisode_User>();
            ViewEpsWatchNext_Recent = CollectionViewSource.GetDefaultView(EpsWatchNext_Recent);

            EpsWatchedRecently = new ObservableCollection<VM_AnimeEpisode_User>();
            ViewEpsWatchedRecently = CollectionViewSource.GetDefaultView(EpsWatchedRecently);

            SeriesMissingEps = new ObservableCollection<VM_AnimeSeries_User>();
            ViewSeriesMissingEps = CollectionViewSource.GetDefaultView(SeriesMissingEps);

            MiniCalendar = new ObservableCollection<VM_AniDB_Anime>();
            ViewMiniCalendar = CollectionViewSource.GetDefaultView(MiniCalendar);

            RecommendationsWatch = new ObservableCollection<object>();
            ViewRecommendationsWatch = CollectionViewSource.GetDefaultView(RecommendationsWatch);

            RecommendationsDownload = new ObservableCollection<object>();
            ViewRecommendationsDownload = CollectionViewSource.GetDefaultView(RecommendationsDownload);

            RecentAdditions = new ObservableCollection<object>();
            ViewRecentAdditions = CollectionViewSource.GetDefaultView(RecentAdditions);

        }



        public void RefreshData(bool refreshContinueWatching, bool refreshRecentAdditions, bool refreshOtherWidgets, RecentAdditionsType addType)
        {
            try
            {
                IsLoadingData = true;

                // clear all displayed data
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
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
                    }

                    if (refreshOtherWidgets)
                    {
                        ViewEpsWatchedRecently.Refresh();
                        ViewSeriesMissingEps.Refresh();
                        ViewMiniCalendar.Refresh();
                        ViewRecommendationsWatch.Refresh();
                        ViewRecommendationsDownload.Refresh();
                        ViewRecentAdditions.Refresh();
                    }

                    if (refreshContinueWatching) ViewEpsWatchNext_Recent.Refresh();
                    if (refreshRecentAdditions) ViewRecentAdditions.Refresh();
                });

                DateTime start = DateTime.Now;
                VM_MainListHelper.Instance.RefreshGroupsSeriesData();
                TimeSpan ts = DateTime.Now - start;

                logger.Trace("Dashboard Time: RefreshGroupsSeriesData: {0}", ts.TotalMilliseconds);

                if (refreshContinueWatching && VM_UserSettings.Instance.DashWatchNextEpExpanded)
                    RefreshEpsWatchNext_Recent();

                if (refreshRecentAdditions && VM_UserSettings.Instance.DashRecentAdditionsExpanded)
                    RefreshRecentAdditions(addType);

                if (refreshOtherWidgets)
                {
                    if (VM_UserSettings.Instance.DashRecentlyWatchEpsExpanded)
                        RefreshRecentlyWatchedEps();

                    if (VM_UserSettings.Instance.DashSeriesMissingEpisodesExpanded)
                        RefreshSeriesMissingEps();

                    if (VM_UserSettings.Instance.DashMiniCalendarExpanded)
                        RefreshMiniCalendar();

                    if (VM_UserSettings.Instance.DashRecommendationsWatchExpanded)
                        RefreshRecommendationsWatch();

                    if (VM_UserSettings.Instance.DashRecommendationsDownloadExpanded)
                        RefreshRecommendationsDownload();

                }

                IsLoadingData = false;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void RefreshRecentAdditions(RecentAdditionsType addType)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate {
                    RecentAdditions.Clear();
                });

                if (addType == RecentAdditionsType.Episode)
                {
                    List<VM_AnimeEpisode_User> epContracts =
                        VM_ShokoServer.Instance.ShokoServices.GetEpisodesRecentlyAdded(VM_UserSettings.Instance.Dash_RecentAdditions_Items, VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeEpisode_User>();

                    System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                    {
                        foreach (VM_AnimeEpisode_User ep in epContracts)
                        {
                            ep.RefreshAnime();

                            if (ep.AniDB_Anime == null)
                                ep.RefreshAnime(true); // this might be a new series

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
                    List<VM_AnimeSeries_User> serContracts =
                        VM_ShokoServer.Instance.ShokoServices.GetSeriesRecentlyAdded(VM_UserSettings.Instance.Dash_RecentAdditions_Items, VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeSeries_User>();

                    System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                    {
                        foreach (VM_AnimeSeries_User ser in serContracts)
                        {
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
        }


        public void RefreshRecommendationsWatch()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate {
                    RecommendationsWatch.Clear();
                });

                List<VM_Recommendation> contracts =
                    VM_ShokoServer.Instance.ShokoServices.GetRecommendations(VM_UserSettings.Instance.Dash_RecWatch_Items, VM_ShokoServer.Instance.CurrentUser.JMMUserID,
                    (int)RecommendationType.Watch).CastList<VM_Recommendation>();

                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                {
                    foreach (VM_Recommendation contract in contracts)
                    {
                        RecommendationsWatch.Add(contract);
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
        }

        public void RefreshRecommendationsDownload()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate {
                    RecommendationsDownload.Clear();
                });

                List<VM_Recommendation> contracts =
                    VM_ShokoServer.Instance.ShokoServices.GetRecommendations(VM_UserSettings.Instance.Dash_RecDownload_Items, VM_ShokoServer.Instance.CurrentUser.JMMUserID,
                    (int)RecommendationType.Download).CastList<VM_Recommendation>();

                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                {
                    foreach (VM_Recommendation contract in contracts)
                    {

                        RecommendationsDownload.Add(contract);
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
        }

        public void GetMissingRecommendationsDownload()
        {
            try
            {
                IsLoadingData = true;

                foreach (object obj in RecommendationsDownload)
                {
                    VM_Recommendation rec = obj as VM_Recommendation;
                    if (rec == null) continue;

                    if (!rec.Recommended_AnimeInfoExists)
                    {
                        string result = VM_ShokoServer.Instance.ShokoServices.UpdateAnimeData(rec.RecommendedAnimeID);
                        if (string.IsNullOrEmpty(result))
                        {
                            VM_AniDB_Anime anime=(VM_AniDB_Anime)VM_ShokoServer.Instance.ShokoServices.GetAnime(rec.RecommendedAnimeID);
                            System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, (Action)delegate
                            {
                                rec.Recommended_AniDB_Anime = anime;
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
        }

        public void RefreshSeriesMissingEps()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate {
                    SeriesMissingEps.Clear();
                });

                List<VM_AnimeSeries_User> epSeries =
                    VM_ShokoServer.Instance.ShokoServices.GetSeriesWithMissingEpisodes(VM_UserSettings.Instance.Dash_MissingEps_Items, VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeSeries_User>();

                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                {

                    foreach (VM_AnimeSeries_User ser  in epSeries)
                    {
                        if (VM_ShokoServer.Instance.CurrentUser.EvaluateSeries(ser))
                            SeriesMissingEps.Add(ser);
                    }
                    ViewSeriesMissingEps.Refresh();
                });
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void RefreshEpsWatchNext_Recent()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate {
                    EpsWatchNext_Recent.Clear();
                });

                DateTime start = DateTime.Now;

                List<VM_AnimeEpisode_User> epContracts =
                    VM_ShokoServer.Instance.ShokoServices.GetContinueWatchingFilter(VM_ShokoServer.Instance.CurrentUser.JMMUserID, VM_UserSettings.Instance.Dash_WatchNext_Items).CastList<VM_AnimeEpisode_User>();

                TimeSpan ts = DateTime.Now - start;
                logger.Trace("Dashboard Time: RefreshEpsWatchNext_Recent: contracts: {0}", ts.TotalMilliseconds);

                start = DateTime.Now;
                List<VM_AnimeEpisode_User> epList = new List<VM_AnimeEpisode_User>();
                foreach (VM_AnimeEpisode_User ep in epContracts)
                {
                    
                    if (ep.AniDB_Anime == null)
                        ep.RefreshAnime(true); // this might be a new series

                    ts = DateTime.Now - start;
                    logger.Trace("Dashboard Time: RefreshEpsWatchNext_Recent: episode details: Stage 1: {0}", ts.TotalMilliseconds);

                    ep.RefreshAnime();

                    ts = DateTime.Now - start;
                    logger.Trace("Dashboard Time: RefreshEpsWatchNext_Recent: episode details: Stage 2: {0}", ts.TotalMilliseconds);

                    ep.SetTvDBInfo();

                    ts = DateTime.Now - start;
                    logger.Trace("Dashboard Time: RefreshEpsWatchNext_Recent: episode details: Stage 3: {0}", ts.TotalMilliseconds);

                    epList.Add(ep);
                }
                ts = DateTime.Now - start;
                logger.Trace("Dashboard Time: RefreshEpsWatchNext_Recent: episode details: {0}", ts.TotalMilliseconds);

                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                {
                    foreach (VM_AnimeEpisode_User ep in epList)
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

        }

        public void RefreshRecentlyWatchedEps()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate {
                    EpsWatchedRecently.Clear();
                });

                List<VM_AnimeEpisode_User> epContracts =
                    VM_ShokoServer.Instance.ShokoServices.GetEpisodesRecentlyWatched(VM_UserSettings.Instance.Dash_RecentlyWatchedEp_Items, VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeEpisode_User>();

                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                {
                    foreach (VM_AnimeEpisode_User ep in epContracts)
                    {
                        ep.RefreshAnime();
                        if (ep.AniDB_Anime == null)
                            ep.RefreshAnime(true); // this might be a new series
                        if (ep.AniDB_Anime != null && VM_ShokoServer.Instance.CurrentUser.EvaluateAnime(ep.AniDB_Anime))
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

        }

        public void RefreshMiniCalendar()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                {
                    MiniCalendar.Clear();
                    ViewMiniCalendar.SortDescriptions.Clear();
                    ViewMiniCalendar.SortDescriptions.Add(VM_UserSettings.Instance.Dash_MiniCalendarUpcomingOnly ? new SortDescription("AirDate", ListSortDirection.Ascending) : new SortDescription("AirDate", ListSortDirection.Descending));
                });

                List<VM_AniDB_Anime> contracts = VM_ShokoServer.Instance.ShokoServices.GetMiniCalendar(VM_ShokoServer.Instance.CurrentUser.JMMUserID, VM_UserSettings.Instance.Dash_MiniCalendarDays).CastList<VM_AniDB_Anime>();

                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                {
                    DateTime yesterday = DateTime.Now.AddDays(-1);
                    foreach (VM_AniDB_Anime contract in contracts)
                    {
                        bool useAnime = !(VM_UserSettings.Instance.Dash_MiniCalendarUpcomingOnly && contract.AirDate < yesterday);

                        if (useAnime)
                        {
                            if (VM_ShokoServer.Instance.CurrentUser.EvaluateAnime(contract))
                                MiniCalendar.Add(contract);
                        }
                    }

                    ViewMiniCalendar.Refresh();
                });
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }


    }


}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using NLog;
using Shoko.Commons.Extensions;
using Shoko.Commons.Notification;
using Shoko.Desktop.Enums;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Desktop.ViewModel.Metro;
using Shoko.Desktop.ViewModel.Server;
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantStringFormatCall

namespace Shoko.Desktop.ViewModel
{
    public class VM_DashboardMetro :INotifyPropertyChanged, INotifyPropertyChangedExt
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private MainWindow mainWdw;
        private readonly List<NavContainer> NavigationHistory = new List<NavContainer>();


        public ObservableCollection<ContinueWatchingTile> ContinueWatching { get; set; }
        public ICollectionView ViewContinueWatching { get; set; }

        public ObservableCollection<RandomSeriesTile> RandomSeries { get; set; }
        public ICollectionView ViewRandomSeries { get; set; }

        public ObservableCollection<NewEpisodeTile> NewEpisodes { get; set; }


        private static VM_DashboardMetro _instance;

        public static VM_DashboardMetro Instance => _instance ?? (_instance = new VM_DashboardMetro());

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        public delegate void FinishedProcessHandler(FinishedProcessEventArgs ev);
        public event FinishedProcessHandler OnFinishedProcessEvent;
        protected void OnFinishedProcess(FinishedProcessEventArgs ev)
        {
            OnFinishedProcessEvent?.Invoke(ev);
        }

        private Boolean isReadOnly = true;
        public Boolean IsReadOnly
        {
            get { return isReadOnly; }
            set
            {
                isReadOnly = this.SetField(isReadOnly, value);
            }
        }

        private Boolean isBeingEdited;
        public Boolean IsBeingEdited
        {
            get { return isBeingEdited; }
            set
            {
                isBeingEdited = this.SetField(isBeingEdited, value);
            }
        }

        private Boolean isLoadingData = true;
        public Boolean IsLoadingData
        {
            get { return isLoadingData; }
            set
            {
                isLoadingData = this.SetField(isLoadingData, value);
            }
        }

        private VM_DashboardMetro()
        {
            IsLoadingData = false;

            ContinueWatching = new ObservableCollection<ContinueWatchingTile>();
            ViewContinueWatching = CollectionViewSource.GetDefaultView(ContinueWatching);

            RandomSeries = new ObservableCollection<RandomSeriesTile>();
            ViewRandomSeries = CollectionViewSource.GetDefaultView(RandomSeries);

            NewEpisodes = new ObservableCollection<NewEpisodeTile>();
        }

        public void InitNavigator(MainWindow wdw)
        {
            mainWdw = wdw;

        }

        public void NavigateForward(MetroViews viewType, object content)
        {
            NavigationHistory.Add(new NavContainer() { NavView = viewType, NavContent = content });
            mainWdw.ShowDashMetroView(MetroViews.ContinueWatching, content);
        }

        public void NavigateBack()
        {
            if (NavigationHistory.Count == 0)
            {
                mainWdw.ShowDashMetroView(MetroViews.MainMetro);
                return;
            }

            NavigationHistory.RemoveAt(NavigationHistory.Count - 1);

            if (NavigationHistory.Count > 0)
                mainWdw.ShowDashMetroView(NavigationHistory[NavigationHistory.Count - 1].NavView, NavigationHistory[NavigationHistory.Count - 1].NavContent);
            else
                mainWdw.ShowDashMetroView(MetroViews.MainMetro);
        }

        public void RefreshAllData(bool contWatching, bool randomSeries, bool newEps)
        {
            try
            {
                RefreshBaseData();
                if (contWatching) RefreshContinueWatching();
                if (randomSeries) RefreshRandomSeries();
                if (newEps) RefreshNewEpisodes();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        public void RefreshBaseData()
        {
            try
            {
                IsLoadingData = true;

                DateTime start = DateTime.Now;
                VM_MainListHelper.Instance.RefreshGroupsSeriesData();
                TimeSpan ts = DateTime.Now - start;

                logger.Trace("Dashboard Time: RefreshGroupsSeriesData: {0}", ts.TotalMilliseconds);


                IsLoadingData = false;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

        }

        public void RefreshContinueWatching()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate {
                    ContinueWatching.Clear();
                });

                DateTime start = DateTime.Now;

                List<VM_AnimeEpisode_User> epContracts =
                    VM_ShokoServer.Instance.ShokoServices.GetContinueWatchingFilter(VM_ShokoServer.Instance.CurrentUser.JMMUserID, VM_UserSettings.Instance.DashMetro_WatchNext_Items).CastList<VM_AnimeEpisode_User>();

                TimeSpan ts = DateTime.Now - start;
                logger.Trace("Dashboard Time: RefreshEpsWatchNext_Recent: contracts: {0}", ts.TotalMilliseconds);

                start = DateTime.Now;
                List<VM_AnimeEpisode_User> epList = new List<VM_AnimeEpisode_User>();
                foreach (VM_AnimeEpisode_User ep in epContracts)
                {
                    ep.RefreshAnime();
                    if (ep.AniDB_Anime == null)
                        ep.RefreshAnime(true); // this might be a new series
                    epList.Add(ep);
                }
                ts = DateTime.Now - start;
                logger.Trace("Dashboard Time: RefreshEpsWatchNext_Recent: episode details: {0}", ts.TotalMilliseconds);

                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                {
                    foreach (VM_AnimeEpisode_User ep in epList)
                    {
                        var imageName = AppSettings.DashMetroImageType == DashboardMetroImageType.Fanart ? ep.AniDB_Anime.FanartPath : ep.AniDB_Anime.PosterPathWithRandoms;

                        ContinueWatching.Add(new ContinueWatchingTile()
                        {
                            EpisodeDetails = ep.EpisodeNumberAndName,
                            AnimeName = ep.AnimeSeries.SeriesName,
                            Picture = imageName,
                            AnimeSeries = ep.AnimeSeries,
                            UnwatchedEpisodes = ep.AnimeSeries.UnwatchedEpisodeCount,
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

        }

        public void RefreshRandomSeries()
        {
            try
            {
                logger.Trace("XXX1 RefreshRandomSeries");
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate {
                    RandomSeries.Clear();
                });

                logger.Trace("XXX2 RefreshRandomSeries");
                List<VM_AnimeSeries_User> serList = new List<VM_AnimeSeries_User>();
                logger.Trace("XXX3 RefreshRandomSeries");

                foreach (VM_AnimeGroup_User grp in VM_MainListHelper.Instance.AllGroupsDictionary.Values)
                {
                    // ignore sub groups
                    if (grp.AnimeGroupParentID.HasValue) continue;

                    foreach (VM_AnimeSeries_User ser in grp.AllAnimeSeries)
                    {
                        if (!ser.IsComplete) continue;
                        if (ser.AllFilesWatched) continue;
                        if (!VM_ShokoServer.Instance.CurrentUser.EvaluateSeries(ser)) continue;

                        serList.Add(ser);
                    }
                }

                DateTime start = DateTime.Now;
                logger.Trace("XXX4 RefreshRandomSeries");

                var serShuffledList = serList.OrderBy(a => Guid.NewGuid());

                //serList.Shuffle();

                TimeSpan ts = DateTime.Now - start;
                logger.Trace($"XXX5 Shuffled {serList.Count} series list in {ts.TotalMilliseconds} ms");

                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                {
                    foreach (VM_AnimeSeries_User ser in serShuffledList.Take(AppSettings.DashMetro_RandomSeries_Items))
                    {
                        var imageName = AppSettings.DashMetroImageType == DashboardMetroImageType.Fanart ? ser.AniDBAnime.AniDBAnime.FanartPath : ser.AniDBAnime.AniDBAnime.PosterPathWithRandoms;

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
                logger.Error(ex, ex.ToString());
            }

        }

        public void RefreshNewEpisodes()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate {
                    NewEpisodes.Clear();
                });

                List<VM_AnimeEpisode_User> epContracts =
                        VM_ShokoServer.Instance.ShokoServices.GetEpisodesRecentlyAddedSummary(VM_UserSettings.Instance.DashMetro_NewEpisodes_Items, VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeEpisode_User>();

                foreach (VM_AnimeEpisode_User ep in epContracts)
                {
                    ep.RefreshAnime();
                    if (ep.AniDB_Anime == null)
                        ep.RefreshAnime(true); // this might be a new series
                    if (ep.AniDB_Anime != null)
                    {
                        //ep.SetTvDBInfo();

                        var imageName = AppSettings.DashMetroImageType == DashboardMetroImageType.Fanart ? ep.AnimeSeries.AniDBAnime.AniDBAnime.FanartPath : ep.AnimeSeries.AniDBAnime.AniDBAnime.PosterPathWithRandoms;

                        NewEpisodeTile tile = new NewEpisodeTile()
                        {
                            EpisodeDetails = ep.EpisodeNumberAndName,
                            AnimeName = ep.AnimeSeries.SeriesName,
                            Picture = imageName,
                            AnimeSeries = ep.AnimeSeries,
                            TileSize = "Large",
                            Height = 100
                        };


                        System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate {
                            NewEpisodes.Add(tile);
                        });
                    }
                }

                OnFinishedProcess(new FinishedProcessEventArgs(DashboardMetroProcessType.NewEpisodes));
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
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

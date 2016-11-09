using JMMClient.Metro;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace JMMClient
{
    public class DashboardMetroVM : INotifyPropertyChanged
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private MainWindow mainWdw;
        private List<NavContainer> NavigationHistory = new List<NavContainer>();


        public ObservableCollection<ContinueWatchingTile> ContinueWatching { get; set; }
        public ICollectionView ViewContinueWatching { get; set; }

        public ObservableCollection<RandomSeriesTile> RandomSeries { get; set; }
        public ICollectionView ViewRandomSeries { get; set; }

        public ObservableCollection<NewEpisodeTile> NewEpisodes { get; set; }


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
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
                {
                    ContinueWatching.Clear();
                });

                DateTime start = DateTime.Now;

                List<JMMServerBinary.Contract_AnimeEpisode> epContracts =
                    JMMServerVM.Instance.clientBinaryHTTP.GetContinueWatchingFilter(JMMServerVM.Instance.CurrentUser.JMMUserID.Value, UserSettingsVM.Instance.DashMetro_WatchNext_Items);

                TimeSpan ts = DateTime.Now - start;
                logger.Trace("Dashboard Time: RefreshEpsWatchNext_Recent: contracts: {0}", ts.TotalMilliseconds);

                start = DateTime.Now;
                List<AnimeEpisodeVM> epList = new List<AnimeEpisodeVM>();
                foreach (JMMServerBinary.Contract_AnimeEpisode contract in epContracts)
                {
                    AnimeEpisodeVM ep = new AnimeEpisodeVM(contract);
                    string animename = ep.AnimeName; // just do this to force anidb anime detail record to be loaded
                    ep.RefreshAnime();
                    if (ep.AniDB_Anime == null)
                        ep.RefreshAnime(true); // this might be a new series
                    epList.Add(ep);
                }
                ts = DateTime.Now - start;
                logger.Trace("Dashboard Time: RefreshEpsWatchNext_Recent: episode details: {0}", ts.TotalMilliseconds);

                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
                {
                    foreach (AnimeEpisodeVM ep in epList)
                    {
                        string imageName = "";
                        if (AppSettings.DashMetroImageType == DashboardMetroImageType.Fanart)
                            imageName = ep.AniDB_Anime.FanartPath;
                        else
                            imageName = ep.AniDB_Anime.PosterPathWithRandoms;

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
            finally
            {
            }
        }

        public void RefreshRandomSeries()
        {
            try
            {
                logger.Trace("XXX1 RefreshRandomSeries");
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
                {
                    RandomSeries.Clear();
                });

                logger.Trace("XXX2 RefreshRandomSeries");
                List<AnimeSeriesVM> serList = new List<AnimeSeriesVM>();
                logger.Trace("XXX3 RefreshRandomSeries");

                foreach (AnimeGroupVM grp in MainListHelperVM.Instance.AllGroupsDictionary.Values)
                {
                    // ignore sub groups
                    if (grp.AnimeGroupParentID.HasValue) continue;

                    foreach (AnimeSeriesVM ser in grp.AllAnimeSeries)
                    {
                        if (!ser.IsComplete) continue;
                        if (ser.AllFilesWatched) continue;
                        if (!JMMServerVM.Instance.CurrentUser.EvaluateSeries(ser)) continue;

                        serList.Add(ser);
                    }
                }

                DateTime start = DateTime.Now;
                logger.Trace("XXX4 RefreshRandomSeries");

                var serShuffledList = serList.OrderBy(a => Guid.NewGuid());

                //serList.Shuffle();

                TimeSpan ts = DateTime.Now - start;
                logger.Trace(string.Format("XXX5 Shuffled {0} series list in {1} ms", serList.Count, ts.TotalMilliseconds));

                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
                {
                    foreach (AnimeSeriesVM ser in serShuffledList.Take(AppSettings.DashMetro_RandomSeries_Items))
                    {
                        string imageName = "";
                        if (AppSettings.DashMetroImageType == DashboardMetroImageType.Fanart)
                            imageName = ser.AniDB_Anime.FanartPath;
                        else
                            imageName = ser.AniDB_Anime.PosterPathWithRandoms;

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
            finally
            {
            }
        }

        public void RefreshNewEpisodes()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
                {
                    NewEpisodes.Clear();
                });

                List<JMMServerBinary.Contract_AnimeEpisode> epContracts =
                        JMMServerVM.Instance.clientBinaryHTTP.GetEpisodesRecentlyAddedSummary(UserSettingsVM.Instance.DashMetro_NewEpisodes_Items, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

                foreach (JMMServerBinary.Contract_AnimeEpisode contract in epContracts)
                {
                    AnimeEpisodeVM ep = new AnimeEpisodeVM(contract);
                    ep.RefreshAnime();
                    if (ep.AniDB_Anime == null)
                        ep.RefreshAnime(true); // this might be a new series
                    if (ep.AniDB_Anime != null)
                    {
                        //ep.SetTvDBInfo();

                        string imageName = "";
                        if (AppSettings.DashMetroImageType == DashboardMetroImageType.Fanart)
                            imageName = ep.AnimeSeries.AniDB_Anime.FanartPath;
                        else
                            imageName = ep.AnimeSeries.AniDB_Anime.PosterPathWithRandoms;

                        NewEpisodeTile tile = new NewEpisodeTile()
                        {
                            EpisodeDetails = ep.EpisodeNumberAndName,
                            AnimeName = ep.AnimeSeries.SeriesName,
                            Picture = imageName,
                            AnimeSeries = ep.AnimeSeries,
                            TileSize = "Large",
                            Height = 100
                        };


                        System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
                        {
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

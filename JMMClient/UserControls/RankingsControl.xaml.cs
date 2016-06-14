using JMMClient.ViewModel;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for RankingsControl.xaml
    /// </summary>
    public partial class RankingsControl : UserControl
    {
        public List<AniDB_AnimeDetailedVM> AllAnime = null;

        public ObservableCollection<AnimeRatingVM> AllRankings = null;
        public ListCollectionView ViewUserRankings { get; set; }

        private static Logger logger = LogManager.GetCurrentClassLogger();

        BackgroundWorker workerFiles = new BackgroundWorker();

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading",
            typeof(bool), typeof(RankingsControl), new UIPropertyMetadata(false, null));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set
            {
                SetValue(IsLoadingProperty, value);
                IsNotLoading = !IsLoading;
            }
        }

        public static readonly DependencyProperty IsNotLoadingProperty = DependencyProperty.Register("IsNotLoading",
            typeof(bool), typeof(RankingsControl), new UIPropertyMetadata(true, null));

        public bool IsNotLoading
        {
            get { return (bool)GetValue(IsNotLoadingProperty); }
            set { SetValue(IsNotLoadingProperty, value); }
        }

        public static readonly DependencyProperty StatusMessageProperty = DependencyProperty.Register("StatusMessage",
            typeof(string), typeof(RankingsControl), new UIPropertyMetadata("", null));

        public string StatusMessage
        {
            get { return (string)GetValue(StatusMessageProperty); }
            set { SetValue(StatusMessageProperty, value); }
        }

        public static readonly DependencyProperty AnimeHasSeriesProperty = DependencyProperty.Register("AnimeHasSeries",
            typeof(bool), typeof(RankingsControl), new UIPropertyMetadata(false, null));

        public bool AnimeHasSeries
        {
            get { return (bool)GetValue(AnimeHasSeriesProperty); }
            set { SetValue(AnimeHasSeriesProperty, value); }
        }

        public static readonly DependencyProperty ShowAnimeDetailsProperty = DependencyProperty.Register("ShowAnimeDetails",
            typeof(bool), typeof(RankingsControl), new UIPropertyMetadata(false, null));

        public bool ShowAnimeDetails
        {
            get { return (bool)GetValue(ShowAnimeDetailsProperty); }
            set { SetValue(ShowAnimeDetailsProperty, value); }
        }


        public RankingsControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            AllAnime = new List<AniDB_AnimeDetailedVM>();
            AllRankings = new ObservableCollection<AnimeRatingVM>();

            ViewUserRankings = new ListCollectionView(AllRankings);
            GroupByYearUserRating(ViewUserRankings);

            btnSortOverall.Click += new RoutedEventHandler(btnSortOverall_Click);
            btnSortYear.Click += new RoutedEventHandler(btnSortYear_Click);

            dgRankings.SelectionChanged += new SelectionChangedEventHandler(dgRankings_SelectionChanged);
            cRating.OnRatingValueChangedEvent += new RatingControl.RatingValueChangedHandler(cRating_OnRatingValueChangedEvent);

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            cboCollection.Items.Add(JMMClient.Properties.Resources.Random_All);
            cboCollection.Items.Add(JMMClient.Properties.Resources.Rankings_InCollection);
            cboCollection.Items.Add(JMMClient.Properties.Resources.Rankings_AllEpisodes);
            cboCollection.Items.Add(JMMClient.Properties.Resources.Rankings_NotWatched);
            cboCollection.SelectedIndex = 1;

            cboWatched.Items.Add(JMMClient.Properties.Resources.Random_All);
            cboWatched.Items.Add(JMMClient.Properties.Resources.Rankings_AllWatched);
            cboWatched.Items.Add(JMMClient.Properties.Resources.Rankings_NotWatched);
            cboWatched.SelectedIndex = 0;

            cboVoted.Items.Add(JMMClient.Properties.Resources.Random_All);
            cboVoted.Items.Add(JMMClient.Properties.Resources.Rankings_Voted);
            cboVoted.Items.Add(JMMClient.Properties.Resources.Rankings_NotVoted);
            cboVoted.SelectedIndex = 0;

            workerFiles.DoWork += new DoWorkEventHandler(workerFiles_DoWork);
            workerFiles.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerFiles_RunWorkerCompleted);

            btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            btnViewSeries.Click += new RoutedEventHandler(btnViewSeries_Click);

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(RankingsControl_DataContextChanged);
        }

        void RankingsControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                AnimeRatingVM animeRanking = this.DataContext as AnimeRatingVM;
                if (animeRanking == null) return;

                AnimeHasSeries = animeRanking.AnimeSeries != null;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnViewSeries_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AnimeRatingVM animeRanking = this.DataContext as AnimeRatingVM;
                if (animeRanking == null) return;

                if (animeRanking.AnimeSeries == null)
                {
                    MessageBox.Show(JMMClient.Properties.Resources.Rankings_AnimeNotInCollection);
                    return;
                }

                MainWindow mainwdw = (MainWindow)Window.GetWindow(this);

                if (mainwdw == null) return;
                mainwdw.ShowPinnedSeries(animeRanking.AnimeSeries);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void RefreshData()
        {
            if (workerFiles.IsBusy) return;

            ShowAnimeDetails = false;
            IsLoading = true;
            btnRefresh.IsEnabled = false;
            AllRankings.Clear();

            StatusMessage = JMMClient.Properties.Resources.Loading;

            RankingRefreshOptions opt = new RankingRefreshOptions()
            {
                CollectionState = (RatingCollectionState)cboCollection.SelectedIndex,
                WatchedState = (RatingWatchedState)cboWatched.SelectedIndex,
                VotedState = (RatingVotedState)cboVoted.SelectedIndex
            };

            workerFiles.RunWorkerAsync(opt);
        }

        void workerFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                List<AnimeRatingVM> ratings = e.Result as List<AnimeRatingVM>;
                foreach (AnimeRatingVM rating in ratings)
                    AllRankings.Add(rating);
                ViewUserRankings.Refresh();

                IsLoading = false;
                btnRefresh.IsEnabled = true;
                this.Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void workerFiles_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                RankingRefreshOptions opt = e.Argument as RankingRefreshOptions;


                List<JMMServerBinary.Contract_AnimeRating> rawRatings = JMMServerVM.Instance.clientBinaryHTTP.GetAnimeRatings(
                    (int)opt.CollectionState, (int)opt.WatchedState, (int)opt.VotedState, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

                List<AnimeRatingVM> ratings = new List<AnimeRatingVM>();
                foreach (JMMServerBinary.Contract_AnimeRating contract in rawRatings)
                {
                    AnimeRatingVM rating = new AnimeRatingVM(contract);
                    ratings.Add(rating);
                }

                e.Result = ratings;

                /*AllAnime = MainListHelperVM.Instance.AllAnimeDetailedDictionary.Values.ToList();

				List<AnimeRanking> rankings = new List<AnimeRanking>();

				int i = 0;
				foreach (AniDB_AnimeDetailedVM anime in AllAnime)
				{
					i++;
					AnimeRanking ranking = new AnimeRanking()
					{
						AnimeName = anime.AniDB_Anime.MainTitle,
						Ranking = 1,
						Rating = String.Format("{0:0.00}", anime.AniDB_Anime.AniDBRating),
						UserRating = anime.UserRating,
						Year = anime.AniDB_Anime.BeginYear,
						AnimeDetailed = anime
					};
					AllRankings.Add(ranking);
					//if (i == 50) break;
				}*/
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void cRating_OnRatingValueChangedEvent(RatingValueEventArgs ev)
        {
            AnimeRatingVM animeRating = this.DataContext as AnimeRatingVM;
            if (animeRating == null) return;

            AnimeSeriesVM ser = animeRating.AnimeSeries;
            if (ser == null) return;

            try
            {
                decimal rating = (decimal)ev.RatingValue;

                int voteType = 1;
                if (ser.AniDB_Anime.FinishedAiring) voteType = 2;

                animeRating.UserRating = rating;
                animeRating.AnimeDetailed.UserRating = rating;

                JMMServerVM.Instance.VoteAnime(ser.AniDB_ID, rating, voteType);

                // refresh the data
                MainListHelperVM.Instance.UpdateHeirarchy(ser);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void dgRankings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid _DataGrid = sender as DataGrid;

            AnimeRatingVM animeRanking = _DataGrid.SelectedItem as AnimeRatingVM;
            if (animeRanking == null)
            {
                ShowAnimeDetails = false;
                return;
            }

            ShowDetails(animeRanking);
            ShowAnimeDetails = true;
        }

        private void ShowDetails(AnimeRatingVM ranking)
        {
            this.DataContext = ranking;
        }

        void btnSortYear_Click(object sender, RoutedEventArgs e)
        {
            GroupByYearUserRating(ViewUserRankings);
            ViewUserRankings.Refresh();
        }

        void btnSortOverall_Click(object sender, RoutedEventArgs e)
        {
            SortByOverallUserRating(ViewUserRankings);
            ViewUserRankings.Refresh();
        }

        private void SortByOverallUserRating(ListCollectionView view)
        {
            view.SortDescriptions.Clear();
            view.GroupDescriptions.Clear();

            foreach (DataGridColumn column in dgRankings.Columns)
                column.SortDirection = null;

            view.SortDescriptions.Add(new SortDescription("UserRating", ListSortDirection.Descending));
            view.SortDescriptions.Add(new SortDescription("Year", ListSortDirection.Descending));
            view.SortDescriptions.Add(new SortDescription("AnimeName", ListSortDirection.Ascending));
        }

        private void GroupByYearUserRating(ListCollectionView view)
        {
            view.SortDescriptions.Clear();
            view.GroupDescriptions.Clear();

            foreach (DataGridColumn column in dgRankings.Columns)
                column.SortDirection = null;

            view.SortDescriptions.Add(new SortDescription("Year", ListSortDirection.Descending));
            view.SortDescriptions.Add(new SortDescription("UserRating", ListSortDirection.Descending));
            view.SortDescriptions.Add(new SortDescription("AnimeName", ListSortDirection.Ascending));

            view.GroupDescriptions.Add(new PropertyGroupDescription("Year"));
        }

        public void Init()
        {
        }
    }

    public class RankingRefreshOptions
    {
        public RatingCollectionState CollectionState { get; set; }
        public RatingWatchedState WatchedState { get; set; }
        public RatingVotedState VotedState { get; set; }
    }
}

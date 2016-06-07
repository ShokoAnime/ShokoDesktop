using JMMClient.ViewModel;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JMMClient.Forms
{
    /// <summary>
    /// Interaction logic for SelectTvDBSeasonForm.xaml
    /// </summary>
    public partial class SelectTvDBSeasonForm : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private AniDB_AnimeVM Anime = null;
        private List<AniDB_EpisodeVM> AniDBEpisodes = null;
        private bool FirstLoad = true;
        private int? CrossRef_AniDB_TvDBV2ID = null;
        private TvDBDetails TvDetails = null;

        public static readonly DependencyProperty AnimeIDProperty = DependencyProperty.Register("AnimeID",
            typeof(int), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata(0, null));

        public int AnimeID
        {
            get { return (int)GetValue(AnimeIDProperty); }
            set { SetValue(AnimeIDProperty, value); }
        }

        public static readonly DependencyProperty AnimeNameProperty = DependencyProperty.Register("AnimeName",
            typeof(string), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata("", null));

        public string AnimeName
        {
            get { return (string)GetValue(AnimeNameProperty); }
            set { SetValue(AnimeNameProperty, value); }
        }


        public static readonly DependencyProperty AnimeEpisodeTypeProperty = DependencyProperty.Register("AnimeEpisodeType",
            typeof(int), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata(0, null));

        public int AnimeEpisodeType
        {
            get { return (int)GetValue(AnimeEpisodeTypeProperty); }
            set { SetValue(AnimeEpisodeTypeProperty, value); }
        }

        public static readonly DependencyProperty AnimeEpisodeNumberProperty = DependencyProperty.Register("AnimeEpisodeNumber",
            typeof(int), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata(0, null));

        public int AnimeEpisodeNumber
        {
            get { return (int)GetValue(AnimeEpisodeNumberProperty); }
            set { SetValue(AnimeEpisodeNumberProperty, value); }
        }

        public static readonly DependencyProperty AnimeURLProperty = DependencyProperty.Register("AnimeURL",
            typeof(string), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata("", null));

        public string AnimeURL
        {
            get { return (string)GetValue(AnimeURLProperty); }
            set { SetValue(AnimeURLProperty, value); }
        }

        public static readonly DependencyProperty TvDBIDProperty = DependencyProperty.Register("TvDBID",
            typeof(int), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata(0, null));

        public int TvDBID
        {
            get { return (int)GetValue(TvDBIDProperty); }
            set { SetValue(TvDBIDProperty, value); }
        }

        public static readonly DependencyProperty TvDBSeasonProperty = DependencyProperty.Register("TvDBSeason",
            typeof(int), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata(0, null));

        public int TvDBSeason
        {
            get { return (int)GetValue(TvDBSeasonProperty); }
            set { SetValue(TvDBSeasonProperty, value); }
        }

        public static readonly DependencyProperty TvDBEpisodeNumberProperty = DependencyProperty.Register("TvDBEpisodeNumber",
            typeof(int), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata(0, null));

        public int TvDBEpisodeNumber
        {
            get { return (int)GetValue(TvDBEpisodeNumberProperty); }
            set { SetValue(TvDBEpisodeNumberProperty, value); }
        }

        public static readonly DependencyProperty TvDBSeriesNameProperty = DependencyProperty.Register("TvDBSeriesName",
            typeof(string), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata("", null));

        public string TvDBSeriesName
        {
            get { return (string)GetValue(TvDBSeriesNameProperty); }
            set { SetValue(TvDBSeriesNameProperty, value); }
        }

        public static readonly DependencyProperty TvDBURLProperty = DependencyProperty.Register("TvDBURL",
            typeof(string), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata("", null));

        public string TvDBURL
        {
            get { return (string)GetValue(TvDBURLProperty); }
            set { SetValue(TvDBURLProperty, value); }
        }


        public static readonly DependencyProperty SelectedEpisodeProperty = DependencyProperty.Register("SelectedEpisode",
            typeof(TvDB_EpisodeVM), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata(null, null));

        public TvDB_EpisodeVM SelectedEpisode
        {
            get { return (TvDB_EpisodeVM)GetValue(SelectedEpisodeProperty); }
            set { SetValue(SelectedEpisodeProperty, value); }
        }

        public SelectTvDBSeasonForm()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            btnClose.Click += new RoutedEventHandler(btnClose_Click);
            btnUpdate.Click += new RoutedEventHandler(btnUpdate_Click);
        }

        void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboSeasonNumber.Items.Count == 0)
                {
                    MessageBox.Show(JMMClient.Properties.Resources.TvDB_NoSeasons, JMMClient.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                AniDB_EpisodeVM aniEp = cboAniDBEpisodeNumber.SelectedItem as AniDB_EpisodeVM;
                if (aniEp == null)
                {
                    MessageBox.Show(JMMClient.Properties.Resources.TvDB_NoAniDB, JMMClient.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                AnimeEpisodeNumber = aniEp.EpisodeNumber;

                TvDB_EpisodeVM tvep = cboEpisodeNumber.SelectedItem as TvDB_EpisodeVM;
                if (tvep == null)
                {
                    MessageBox.Show(JMMClient.Properties.Resources.TvDB_NoTvDB, JMMClient.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                TvDBEpisodeNumber = tvep.EpisodeNumber;

                int aniEpType = (int)EpisodeType.Episode;
                if (cboEpisodeType.SelectedIndex == 1) aniEpType = (int)EpisodeType.Special;

                AnimeEpisodeType = aniEpType;
                TvDBSeason = int.Parse(cboSeasonNumber.SelectedItem.ToString());

                this.Cursor = Cursors.Wait;

                string res = JMMServerVM.Instance.clientBinaryHTTP.LinkAniDBTvDB(AnimeID, AnimeEpisodeType, AnimeEpisodeNumber,
                    TvDBID, TvDBSeason, TvDBEpisodeNumber, CrossRef_AniDB_TvDBV2ID);
                if (res.Length > 0)
                    MessageBox.Show(res, JMMClient.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                else
                {
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        public void Init(int animeID, string animeName, EpisodeType aniEpType, int aniEpNumber, int tvDBID, int tvSeason, int tvEpNumber, string tvSeriesName,
            AniDB_AnimeVM anime, int? crossRef_AniDB_TvDBV2ID)
        {
            Anime = anime;
            AnimeID = animeID;
            AnimeName = animeName;
            AnimeEpisodeType = (int)aniEpType;
            AnimeEpisodeNumber = aniEpNumber;
            TvDBID = tvDBID;
            TvDBSeason = tvSeason;
            TvDBEpisodeNumber = tvEpNumber;
            TvDBSeriesName = tvSeriesName;
            CrossRef_AniDB_TvDBV2ID = crossRef_AniDB_TvDBV2ID;

            AnimeURL = string.Format(Constants.URLS.AniDB_Series, AnimeID);
            TvDBURL = string.Format(Constants.URLS.TvDB_Series, TvDBID);

            bool hasSpecials = false;
            AniDBEpisodes = new List<AniDB_EpisodeVM>();
            List<JMMClient.JMMServerBinary.Contract_AniDB_Episode> contracts = JMMServerVM.Instance.clientBinaryHTTP.GetAniDBEpisodesForAnime(AnimeID);
            foreach (JMMClient.JMMServerBinary.Contract_AniDB_Episode contract in contracts)
            {
                AniDBEpisodes.Add(new AniDB_EpisodeVM(contract));
                if (contract.EpisodeType == (int)EpisodeType.Special) hasSpecials = true;
            }

            cboEpisodeType.Items.Clear();
            cboEpisodeType.Items.Add(JMMClient.Properties.Resources.Episodes);
            if (hasSpecials) cboEpisodeType.Items.Add(JMMClient.Properties.Resources.Specials);

            cboEpisodeType.SelectionChanged += new SelectionChangedEventHandler(cboEpisodeType_SelectionChanged);

            if (aniEpType == EpisodeType.Episode)
                cboEpisodeType.SelectedIndex = 0;
            else
                cboEpisodeType.SelectedIndex = 1;



            // get the seasons

            try
            {
                cboSeasonNumber.Items.Clear();

                List<int> seasons = null;
                if (anime.TvSummary.TvDetails.ContainsKey(tvDBID))
                {
                    TvDetails = anime.TvSummary.TvDetails[tvDBID];
                    seasons = anime.TvSummary.TvDetails[tvDBID].DictTvDBSeasons.Keys.ToList();
                }
                else
                {
                    JMMServerVM.Instance.clientBinaryHTTP.UpdateTvDBData(tvDBID);
                    TvDetails = new TvDBDetails(tvDBID);
                    seasons = TvDetails.DictTvDBSeasons.Keys.ToList();
                    //seasons = JMMServerVM.Instance.clientBinaryHTTP.GetSeasonNumbersForSeries(tvDBID);
                }

                int i = 0;
                int idx = 0;
                foreach (int season in seasons)
                {
                    cboSeasonNumber.Items.Add(season.ToString());
                    if (season == tvSeason) idx = i;
                    i++;
                }

                cboSeasonNumber.SelectionChanged += new SelectionChangedEventHandler(cboSeasonNumber_SelectionChanged);
                cboEpisodeNumber.SelectionChanged += new SelectionChangedEventHandler(cboEpisodeNumber_SelectionChanged);

                cboSeasonNumber.SelectedIndex = idx;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void cboEpisodeType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                cboAniDBEpisodeNumber.Items.Clear();

                int i = 0;
                int idx = 0;
                int epType = cboEpisodeType.SelectedIndex == 0 ? (int)EpisodeType.Episode : (int)EpisodeType.Special;


                foreach (AniDB_EpisodeVM ep in AniDBEpisodes)
                {
                    if (ep.EpisodeType == epType)
                    {
                        cboAniDBEpisodeNumber.Items.Add(ep);

                        if (AnimeEpisodeNumber == ep.EpisodeNumber) idx = i;
                        i++;
                    }
                }

                if (cboAniDBEpisodeNumber.Items.Count > 0)
                {
                    if (FirstLoad)
                    {
                        cboAniDBEpisodeNumber.SelectedIndex = idx;
                        FirstLoad = false;
                    }
                    else
                        cboAniDBEpisodeNumber.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void cboEpisodeNumber_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedEpisode = cboEpisodeNumber.SelectedItem as TvDB_EpisodeVM;
        }

        void cboSeasonNumber_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = 0;
            int idxCount = 0;
            cboEpisodeNumber.Items.Clear();
            if (TvDetails != null)
            {
                foreach (TvDB_EpisodeVM ep in TvDetails.DictTvDBEpisodes.Values)
                {
                    if (ep.SeasonNumber == int.Parse(cboSeasonNumber.SelectedItem.ToString()))
                    {
                        cboEpisodeNumber.Items.Add(ep);

                        if (ep.EpisodeNumber == TvDBEpisodeNumber)
                            idx = idxCount;

                        idxCount++;
                    }
                }
            }
            if (cboEpisodeNumber.Items.Count > 0)
            {
                cboEpisodeNumber.SelectedIndex = idx;
            }
        }
    }
}

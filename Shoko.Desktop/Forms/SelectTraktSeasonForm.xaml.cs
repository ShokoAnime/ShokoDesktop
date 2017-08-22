using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models;
using Shoko.Models.Enums;
using Shoko.Models.Server;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for SelectTraktSeasonForm.xaml
    /// </summary>
    public partial class SelectTraktSeasonForm : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private VM_AniDB_Anime Anime = null;
        private List<VM_AniDB_Episode> AniDBEpisodes = null;
        private bool FirstLoad = true;
        private int? CrossRef_AniDB_TraktV2ID = null;
        private VM_TraktDetails traktDetails = null;

        public static readonly DependencyProperty AnimeIDProperty = DependencyProperty.Register("AnimeID",
            typeof(int), typeof(SelectTraktSeasonForm), new UIPropertyMetadata(0, null));

        public int AnimeID
        {
            get { return (int)GetValue(AnimeIDProperty); }
            set { SetValue(AnimeIDProperty, value); }
        }

        public static readonly DependencyProperty AnimeNameProperty = DependencyProperty.Register("AnimeName",
            typeof(string), typeof(SelectTraktSeasonForm), new UIPropertyMetadata("", null));

        public string AnimeName
        {
            get { return (string)GetValue(AnimeNameProperty); }
            set { SetValue(AnimeNameProperty, value); }
        }

        public static readonly DependencyProperty AnimeEpisodeTypeProperty = DependencyProperty.Register("AnimeEpisodeType",
            typeof(int), typeof(SelectTraktSeasonForm), new UIPropertyMetadata(0, null));

        public int AnimeEpisodeType
        {
            get { return (int)GetValue(AnimeEpisodeTypeProperty); }
            set { SetValue(AnimeEpisodeTypeProperty, value); }
        }

        public static readonly DependencyProperty AnimeEpisodeNumberProperty = DependencyProperty.Register("AnimeEpisodeNumber",
            typeof(int), typeof(SelectTraktSeasonForm), new UIPropertyMetadata(0, null));

        public int AnimeEpisodeNumber
        {
            get { return (int)GetValue(AnimeEpisodeNumberProperty); }
            set { SetValue(AnimeEpisodeNumberProperty, value); }
        }

        public static readonly DependencyProperty AnimeURLProperty = DependencyProperty.Register("AnimeURL",
            typeof(string), typeof(SelectTraktSeasonForm), new UIPropertyMetadata("", null));

        public string AnimeURL
        {
            get { return (string)GetValue(AnimeURLProperty); }
            set { SetValue(AnimeURLProperty, value); }
        }

        public static readonly DependencyProperty TraktIDProperty = DependencyProperty.Register("TraktID",
            typeof(string), typeof(SelectTraktSeasonForm), new UIPropertyMetadata("", null));

        public string TraktID
        {
            get { return (string)GetValue(TraktIDProperty); }
            set { SetValue(TraktIDProperty, value); }
        }

        public static readonly DependencyProperty TraktSeasonProperty = DependencyProperty.Register("TraktSeason",
            typeof(int), typeof(SelectTraktSeasonForm), new UIPropertyMetadata(0, null));

        public int TraktSeason
        {
            get { return (int)GetValue(TraktSeasonProperty); }
            set { SetValue(TraktSeasonProperty, value); }
        }

        public static readonly DependencyProperty TraktEpisodeNumberProperty = DependencyProperty.Register("TraktEpisodeNumber",
            typeof(int), typeof(SelectTraktSeasonForm), new UIPropertyMetadata(0, null));

        public int TraktEpisodeNumber
        {
            get { return (int)GetValue(TraktEpisodeNumberProperty); }
            set { SetValue(TraktEpisodeNumberProperty, value); }
        }

        public static readonly DependencyProperty TraktSeriesNameProperty = DependencyProperty.Register("TraktSeriesName",
            typeof(string), typeof(SelectTraktSeasonForm), new UIPropertyMetadata("", null));

        public string TraktSeriesName
        {
            get { return (string)GetValue(TraktSeriesNameProperty); }
            set { SetValue(TraktSeriesNameProperty, value); }
        }

        public static readonly DependencyProperty TraktURLProperty = DependencyProperty.Register("TraktURL",
            typeof(string), typeof(SelectTraktSeasonForm), new UIPropertyMetadata("", null));

        public string TraktURL
        {
            get { return (string)GetValue(TraktURLProperty); }
            set { SetValue(TraktURLProperty, value); }
        }

        public static readonly DependencyProperty SelectedEpisodeProperty = DependencyProperty.Register("SelectedEpisode",
            typeof(Trakt_Episode), typeof(SelectTraktSeasonForm), new UIPropertyMetadata(null, null));

        public Trakt_Episode SelectedEpisode
        {
            get { return (Trakt_Episode)GetValue(SelectedEpisodeProperty); }
            set { SetValue(SelectedEpisodeProperty, value); }
        }

        public SelectTraktSeasonForm()
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
                    MessageBox.Show(Shoko.Commons.Properties.Resources.Trakt_NoSeasons, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                VM_AniDB_Episode aniEp = cboAniDBEpisodeNumber.SelectedItem as VM_AniDB_Episode;
                AnimeEpisodeNumber = aniEp.EpisodeNumber;

                Trakt_Episode traktep = cboEpisodeNumber.SelectedItem as Trakt_Episode;
                TraktEpisodeNumber = traktep.EpisodeNumber;

                int aniEpType = (int)EpisodeType.Episode;
                if (cboEpisodeType.SelectedIndex == 1) aniEpType = (int)EpisodeType.Special;

                AnimeEpisodeType = aniEpType;
                TraktSeason = int.Parse(cboSeasonNumber.SelectedItem.ToString());

                Cursor = Cursors.Wait;

                string res = VM_ShokoServer.Instance.ShokoServices.LinkAniDBTrakt(AnimeID, AnimeEpisodeType, AnimeEpisodeNumber,
                    TraktID, TraktSeason, TraktEpisodeNumber, CrossRef_AniDB_TraktV2ID);
                if (res.Length > 0)
                    MessageBox.Show(res, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                else
                {
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public void Init(int animeID, string animeName, EpisodeType aniEpType, int aniEpNumber, string traktID, int traktSeason, int traktEpNumber, string trakSeriesName,
            VM_AniDB_Anime anime, int? crossRef_AniDB_TraktV2ID)
        {
            Anime = anime;
            AnimeID = animeID;
            AnimeName = animeName;
            AnimeEpisodeType = (int)aniEpType;
            AnimeEpisodeNumber = aniEpNumber;
            TraktID = traktID;
            TraktSeason = traktSeason;
            TraktEpisodeNumber = traktEpNumber;
            TraktSeriesName = trakSeriesName;
            CrossRef_AniDB_TraktV2ID = crossRef_AniDB_TraktV2ID;

            AnimeURL = string.Format(Constants.URLS.AniDB_Series, AnimeID);
            TraktURL = string.Format(Constants.URLS.Trakt_Series, TraktID);

            bool hasSpecials = false;
            AniDBEpisodes = new List<VM_AniDB_Episode>();
            foreach (VM_AniDB_Episode contract in VM_ShokoServer.Instance.ShokoServices.GetAniDBEpisodesForAnime(AnimeID).Cast<VM_AniDB_Episode>())

            {
                AniDBEpisodes.Add(contract);
                if (contract.EpisodeType == (int)EpisodeType.Special) hasSpecials = true;
            }

            cboEpisodeType.Items.Clear();
            cboEpisodeType.Items.Add(Shoko.Commons.Properties.Resources.Anime_Episodes);
            if (hasSpecials) cboEpisodeType.Items.Add(Shoko.Commons.Properties.Resources.Anime_Specials);

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
                if (anime.traktSummary.traktDetails.ContainsKey(traktID))
                {
                    traktDetails = anime.traktSummary.traktDetails[traktID];
                    seasons = anime.traktSummary.traktDetails[traktID].DictTraktSeasons.Keys.ToList();
                }
                else
                {
                    VM_ShokoServer.Instance.ShokoServices.UpdateTraktData(traktID);
                    traktDetails = new VM_TraktDetails(traktID);
                    seasons = traktDetails.DictTraktSeasons.Keys.ToList();
                }

                int i = 0;
                int idx = 0;
                foreach (int season in seasons)
                {
                    cboSeasonNumber.Items.Add(season.ToString());
                    if (season == traktSeason) idx = i;
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


                foreach (VM_AniDB_Episode ep in AniDBEpisodes)
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
            SelectedEpisode = cboEpisodeNumber.SelectedItem as Trakt_Episode;
        }

        void cboSeasonNumber_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = 0;
            int idxCount = 0;
            cboEpisodeNumber.Items.Clear();
            if (traktDetails != null)
            {
                foreach (Trakt_Episode ep in traktDetails.DictTraktEpisodes.Values)
                {
                    if (ep.Season == int.Parse(cboSeasonNumber.SelectedItem.ToString()))
                    {
                        cboEpisodeNumber.Items.Add(ep);

                        if (ep.EpisodeNumber == TraktEpisodeNumber)
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

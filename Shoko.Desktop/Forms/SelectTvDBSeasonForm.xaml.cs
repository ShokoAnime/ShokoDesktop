using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NLog;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models;
using Shoko.Models.Enums;
using Shoko.Models.Server;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for SelectTvDBSeasonForm.xaml
    /// </summary>
    public partial class SelectTvDBSeasonForm : Window
    {
        private List<VM_AniDB_Episode> AniDBEpisodes;
        private bool FirstLoad = true;
        private VM_TvDBDetails TvDetails;
        private bool IsAdditive;

        public static readonly DependencyProperty AnimeIDProperty = DependencyProperty.Register("AnimeID",
            typeof(int), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata(0, null));

        public int AnimeID
        {
            get => (int) GetValue(AnimeIDProperty);
            set => SetValue(AnimeIDProperty, value);
        }

        public static readonly DependencyProperty AnimeNameProperty = DependencyProperty.Register("AnimeName",
            typeof(string), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata("", null));

        public string AnimeName
        {
            get => (string) GetValue(AnimeNameProperty);
            set => SetValue(AnimeNameProperty, value);
        }


        public static readonly DependencyProperty AnimeEpisodeTypeProperty = DependencyProperty.Register("AnimeEpisodeType",
            typeof(int), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata(0, null));

        public int AnimeEpisodeType
        {
            get => (int) GetValue(AnimeEpisodeTypeProperty);
            set => SetValue(AnimeEpisodeTypeProperty, value);
        }

        public static readonly DependencyProperty AnimeEpisodeNumberProperty = DependencyProperty.Register("AnimeEpisodeNumber",
            typeof(int), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata(0, null));

        public int AnimeEpisodeNumber
        {
            get => (int) GetValue(AnimeEpisodeNumberProperty);
            set => SetValue(AnimeEpisodeNumberProperty, value);
        }

        public static readonly DependencyProperty AnimeURLProperty = DependencyProperty.Register("AnimeURL",
            typeof(string), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata("", null));

        public string AnimeURL
        {
            get => (string) GetValue(AnimeURLProperty);
            set => SetValue(AnimeURLProperty, value);
        }

        public static readonly DependencyProperty TvDBIDProperty = DependencyProperty.Register("TvDBID",
            typeof(int), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata(0, null));

        public int TvDBID
        {
            get => (int) GetValue(TvDBIDProperty);
            set => SetValue(TvDBIDProperty, value);
        }

        public static readonly DependencyProperty TvDBSeasonProperty = DependencyProperty.Register("TvDBSeason",
            typeof(int), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata(0, null));

        public int TvDBSeason
        {
            get => (int) GetValue(TvDBSeasonProperty);
            set => SetValue(TvDBSeasonProperty, value);
        }

        public static readonly DependencyProperty TvDBEpisodeNumberProperty = DependencyProperty.Register("TvDBEpisodeNumber",
            typeof(int), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata(0, null));

        public int TvDBEpisodeNumber
        {
            get => (int) GetValue(TvDBEpisodeNumberProperty);
            set => SetValue(TvDBEpisodeNumberProperty, value);
        }

        public static readonly DependencyProperty TvDBSeriesNameProperty = DependencyProperty.Register("TvDBSeriesName",
            typeof(string), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata("", null));

        public string TvDBSeriesName
        {
            get => (string)GetValue(TvDBSeriesNameProperty);
            set => SetValue(TvDBSeriesNameProperty, value);
        }

        public static readonly DependencyProperty TvDBURLProperty = DependencyProperty.Register("TvDBURL",
            typeof(string), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata("", null));

        public string TvDBURL
        {
            get => (string)GetValue(TvDBURLProperty);
            set => SetValue(TvDBURLProperty, value);
        }


        public static readonly DependencyProperty SelectedEpisodeProperty = DependencyProperty.Register("SelectedEpisode",
            typeof(VM_TvDB_Episode), typeof(SelectTvDBSeasonForm), new UIPropertyMetadata(null, null));

        public VM_TvDB_Episode SelectedEpisode
        {
            get => (VM_TvDB_Episode)GetValue(SelectedEpisodeProperty);
            set => SetValue(SelectedEpisodeProperty, value);
        }

        public SelectTvDBSeasonForm()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            btnClose.Click += btnClose_Click;
            btnUpdate.Click += btnUpdate_Click;

            cboEpisodeType.SelectionChanged += cboEpisodeType_SelectionChanged;

            cboSeasonNumber.SelectionChanged += cboSeasonNumber_SelectionChanged;
            cboEpisodeNumber.SelectionChanged += cboEpisodeNumber_SelectionChanged;
        }

        void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboSeasonNumber.Items.Count == 0)
                {
                    MessageBox.Show(Commons.Properties.Resources.TvDB_NoSeasons, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!(cboAniDBEpisodeNumber.SelectedItem is VM_AniDB_Episode aniEp))
                {
                    MessageBox.Show(Commons.Properties.Resources.TvDB_NoAniDB, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                AnimeEpisodeNumber = aniEp.EpisodeNumber;

                if (!(cboEpisodeNumber.SelectedItem is VM_TvDB_Episode tvep))
                {
                    MessageBox.Show(Commons.Properties.Resources.TvDB_NoTvDB, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                TvDBEpisodeNumber = tvep.EpisodeNumber;

                int aniEpType = (int)EpisodeType.Episode;
                if (cboEpisodeType.SelectedIndex == 1) aniEpType = (int)EpisodeType.Special;

                AnimeEpisodeType = aniEpType;
                TvDBSeason = int.Parse(cboSeasonNumber.SelectedItem.ToString());

                Cursor = Cursors.Wait;

                var xref = new CrossRef_AniDB_TvDBV2
                {
                    AnimeID = AnimeID,
                    AniDBStartEpisodeType = AnimeEpisodeType,
                    AniDBStartEpisodeNumber = AnimeEpisodeNumber,
                    TvDBID = TvDBID,
                    TvDBSeasonNumber = TvDBSeason,
                    TvDBStartEpisodeNumber = TvDBEpisodeNumber,
                    CrossRef_AniDB_TvDBV2ID = 0,
                    CrossRefSource = (int) CrossRefSource.User,
                    IsAdditive = IsAdditive
                };
                string res = VM_ShokoServer.Instance.ShokoServices.LinkAniDBTvDB(xref);
                if (res.Length > 0)
                    MessageBox.Show(res, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
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

        public void Init(int animeID, string animeName, EpisodeType aniEpType, int aniEpNumber, int tvDBID, int tvSeason, int tvEpNumber, string tvSeriesName,
            VM_AniDB_Anime anime, bool isAdditive)
        {
            AnimeID = animeID;
            AnimeName = animeName;
            AnimeEpisodeType = (int)aniEpType;
            AnimeEpisodeNumber = aniEpNumber;
            TvDBID = tvDBID;
            TvDBSeason = tvSeason;
            TvDBEpisodeNumber = tvEpNumber;
            TvDBSeriesName = tvSeriesName;
            IsAdditive = isAdditive;

            AnimeURL = string.Format(Constants.URLS.AniDB_Series, AnimeID);
            TvDBURL = string.Format(Constants.URLS.TvDB_Series, TvDBID);

            bool hasSpecials = false;
            AniDBEpisodes = new List<VM_AniDB_Episode>();
            foreach (VM_AniDB_Episode contract in VM_ShokoServer.Instance.ShokoServices.GetAniDBEpisodesForAnime(AnimeID).Cast<VM_AniDB_Episode>())
            {
                AniDBEpisodes.Add(contract);
                if (contract.EpisodeType == (int)EpisodeType.Special) hasSpecials = true;
            }

            cboEpisodeType.Items.Clear();
            cboEpisodeType.Items.Add(Commons.Properties.Resources.Anime_Episodes);
            if (hasSpecials) cboEpisodeType.Items.Add(Commons.Properties.Resources.Anime_Specials);

            cboEpisodeType.SelectedIndex = aniEpType == EpisodeType.Episode ? 0 : 1;

            // get the seasons

            try
            {
                cboSeasonNumber.Items.Clear();

                List<int> seasons;
                if (anime.TvSummary.TvDetails.ContainsKey(tvDBID))
                {
                    TvDetails = anime.TvSummary.TvDetails[tvDBID];
                    seasons = anime.TvSummary.TvDetails[tvDBID].DictTvDBSeasons.Keys.ToList();
                }
                else
                {
                    TvDetails = new VM_TvDBDetails(tvDBID);
                    seasons = TvDetails.DictTvDBSeasons.Keys.ToList();
                }

                int i = 0;
                int idx = 0;
                foreach (int season in seasons)
                {
                    cboSeasonNumber.Items.Add(season.ToString());
                    if (season == tvSeason) idx = i;
                    i++;
                }

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
                    if (ep.EpisodeType == epType)
                    {
                        cboAniDBEpisodeNumber.Items.Add(ep);

                        if (AnimeEpisodeNumber == ep.EpisodeNumber) idx = i;
                        i++;
                    }

                if (cboAniDBEpisodeNumber.Items.Count > 0)
                    if (FirstLoad)
                    {
                        cboAniDBEpisodeNumber.SelectedIndex = idx;
                        FirstLoad = false;
                    }
                    else
                        cboAniDBEpisodeNumber.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void cboEpisodeNumber_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedEpisode = cboEpisodeNumber.SelectedItem as VM_TvDB_Episode;
        }

        void cboSeasonNumber_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = 0;
            int idxCount = 0;
            cboEpisodeNumber.Items.Clear();
            if (TvDetails != null)
                foreach (VM_TvDB_Episode ep in TvDetails.DictTvDBEpisodes.Values)
                    if (ep.SeasonNumber == int.Parse(cboSeasonNumber.SelectedItem.ToString()))
                    {
                        cboEpisodeNumber.Items.Add(ep);

                        if (ep.EpisodeNumber == TvDBEpisodeNumber)
                            idx = idxCount;

                        idxCount++;
                    }
            if (cboEpisodeNumber.Items.Count > 0)
                cboEpisodeNumber.SelectedIndex = idx;
        }
    }
}

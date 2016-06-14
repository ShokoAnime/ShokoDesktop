using JMMClient.ViewModel;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JMMClient.Forms
{
    /// <summary>
    /// Interaction logic for SelectTvDBEpisodeForm.xaml
    /// </summary>
    public partial class SelectTvDBEpisodeForm : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private int AnimeID = 0;
        private AniDB_AnimeVM Anime = null;
        private AnimeEpisodeVM AnimeEpisode = null;
        private TvDBDetails TvDetails = null;

        public ObservableCollection<TvDB_EpisodeVM> CurrentEpisodes { get; set; }
        public ObservableCollection<int> SeasonNumbers { get; set; }

        /*public static readonly DependencyProperty CurrentEpisodesProperty = DependencyProperty.Register("CurrentEpisodes",
			typeof(List<TvDB_EpisodeVM>), typeof(SelectTvDBEpisodeForm), new UIPropertyMetadata(null, null));

		public List<TvDB_EpisodeVM> CurrentEpisodes
		{
			get { return (List<TvDB_EpisodeVM>)GetValue(CurrentEpisodesProperty); }
			set { SetValue(CurrentEpisodesProperty, value); }
		}*/

        public SelectTvDBEpisodeForm()
        {
            InitializeComponent();

            CurrentEpisodes = new ObservableCollection<TvDB_EpisodeVM>();
            SeasonNumbers = new ObservableCollection<int>();

            btnClose.Click += new RoutedEventHandler(btnClose_Click);
            cboSeason.SelectionChanged += new SelectionChangedEventHandler(cboSeason_SelectionChanged);
        }


        void cboSeason_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // refresh episode list
            try
            {
                CurrentEpisodes.Clear();
                foreach (TvDB_EpisodeVM tvep in TvDetails.TvDBEpisodes)
                {
                    if (tvep.SeasonNumber == int.Parse(cboSeason.SelectedItem.ToString()))
                        CurrentEpisodes.Add(tvep);
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
            //SelectedTvDBID = null;
            this.Close();
        }

        private void CommandBinding_UseThis(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(TvDB_EpisodeVM))
                {
                    this.Cursor = Cursors.Wait;
                    TvDB_EpisodeVM tvEp = obj as TvDB_EpisodeVM;

                    string res = JMMServerVM.Instance.clientBinaryHTTP.LinkAniDBTvDBEpisode(AnimeEpisode.AniDB_EpisodeID, tvEp.Id, AnimeID);
                    this.Cursor = Cursors.Arrow;

                    if (res.Length > 0)
                    {
                        Utils.ShowErrorMessage(res);
                        return;
                    }

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

        public void Init(AnimeEpisodeVM ep, AniDB_AnimeVM anime)
        {
            AnimeID = anime.AnimeID;
            Anime = anime;
            AnimeEpisode = ep;

            List<int> uids = new List<int>();
            cboSeries.Items.Clear();
            foreach (CrossRef_AniDB_TvDBVMV2 xref in Anime.TvSummary.CrossRefTvDBV2)
            {
                if (!uids.Contains(xref.TvDBID))
                    cboSeries.Items.Add(xref);

                uids.Add(xref.TvDBID);
            }

            cboSeries.SelectionChanged += new SelectionChangedEventHandler(cboSeries_SelectionChanged);

            if (cboSeries.Items.Count > 0)
                cboSeries.SelectedIndex = 0;
        }

        private void PopulateSeasons(CrossRef_AniDB_TvDBVMV2 xref)
        {
            cboSeason.SelectionChanged -= new SelectionChangedEventHandler(cboSeason_SelectionChanged);

            SeasonNumbers.Clear();
            cboSeason.Items.Clear();
            TvDetails = null;
            if (Anime.TvSummary.TvDetails.ContainsKey(xref.TvDBID))
                TvDetails = Anime.TvSummary.TvDetails[xref.TvDBID];

            if (TvDetails == null) return;

            foreach (int season in TvDetails.DictTvDBSeasons.Keys)
                cboSeason.Items.Add(season);

            cboSeason.SelectionChanged += new SelectionChangedEventHandler(cboSeason_SelectionChanged);

            if (cboSeason.Items.Count > 0)
                cboSeason.SelectedIndex = 0;
        }

        void cboSeries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CrossRef_AniDB_TvDBVMV2 xref = cboSeries.SelectedItem as CrossRef_AniDB_TvDBVMV2;
            PopulateSeasons(xref);
        }
    }
}

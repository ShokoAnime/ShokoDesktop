using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for SelectTvDBEpisodeForm.xaml
    /// </summary>
    public partial class SelectTvDBEpisodeForm : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private int AnimeID = 0;
        private VM_AniDB_Anime Anime = null;
        private VM_AnimeEpisode_User AnimeEpisode = null;
        private VM_TvDBDetails TvDetails = null;

        public ObservableCollectionEx<VM_TvDB_Episode> CurrentEpisodes { get; set; }


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

            CurrentEpisodes = new ObservableCollectionEx<VM_TvDB_Episode>();


            btnClose.Click += new RoutedEventHandler(btnClose_Click);
            cboSeason.SelectionChanged += new SelectionChangedEventHandler(cboSeason_SelectionChanged);
        }


        void cboSeason_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // refresh episode list
            try
            {
                int season = int.Parse(cboSeason.SelectedItem.ToString());
                CurrentEpisodes.ReplaceRange(TvDetails.TvDBEpisodes.Where(a=>a.SeasonNumber==season));
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
            //SelectedTvDBID = null;
            Close();
        }

        private void CommandBinding_UseThis(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_TvDB_Episode))
                {
                    Cursor = Cursors.Wait;
                    VM_TvDB_Episode tvEp = obj as VM_TvDB_Episode;

                    string res = VM_ShokoServer.Instance.ShokoServices.LinkAniDBTvDBEpisode(AnimeEpisode.AniDB_EpisodeID, tvEp.Id);
                    Cursor = Cursors.Arrow;

                    if (res.Length > 0)
                    {
                        Utils.ShowErrorMessage(res);
                        return;
                    }

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

        public void Init(VM_AnimeEpisode_User ep, VM_AniDB_Anime anime)
        {
            AnimeID = anime.AnimeID;
            Anime = anime;
            AnimeEpisode = ep;

            List<int> uids = new List<int>();
            cboSeries.Items.Clear();
            foreach (VM_CrossRef_AniDB_TvDBV2 xref in Anime.TvSummary.CrossRefTvDBV2)
            {
                if (!uids.Contains(xref.TvDBID))
                    cboSeries.Items.Add(xref);

                uids.Add(xref.TvDBID);
            }

            cboSeries.SelectionChanged += new SelectionChangedEventHandler(cboSeries_SelectionChanged);

            if (cboSeries.Items.Count > 0)
                cboSeries.SelectedIndex = 0;
        }

        private void PopulateSeasons(VM_CrossRef_AniDB_TvDBV2 xref)
        {
            cboSeason.SelectionChanged -= new SelectionChangedEventHandler(cboSeason_SelectionChanged);


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
            VM_CrossRef_AniDB_TvDBV2 xref = cboSeries.SelectedItem as VM_CrossRef_AniDB_TvDBV2;
            PopulateSeasons(xref);
        }
    }
}

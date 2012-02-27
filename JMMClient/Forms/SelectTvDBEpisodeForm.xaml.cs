using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NLog;
using JMMClient.ViewModel;

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

		public static readonly DependencyProperty CurrentEpisodesProperty = DependencyProperty.Register("CurrentEpisodes",
			typeof(List<TvDB_EpisodeVM>), typeof(SelectTvDBEpisodeForm), new UIPropertyMetadata(null, null));

		public List<TvDB_EpisodeVM> CurrentEpisodes
		{
			get { return (List<TvDB_EpisodeVM>)GetValue(CurrentEpisodesProperty); }
			set { SetValue(CurrentEpisodesProperty, value); }
		}

		public SelectTvDBEpisodeForm()
		{
			InitializeComponent();

			btnClose.Click += new RoutedEventHandler(btnClose_Click);
			cboSeason.SelectionChanged += new SelectionChangedEventHandler(cboSeason_SelectionChanged);
		}


		void cboSeason_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// refresh episode list
			try
			{
				CurrentEpisodes = new List<TvDB_EpisodeVM>();
				foreach (TvDB_EpisodeVM tvep in Anime.TvDBEpisodes)
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

			cboSeason.Items.Clear();
			foreach (int season in Anime.DictTvDBSeasons.Keys)
				cboSeason.Items.Add(season);

			if (cboSeason.Items.Count > 0) cboSeason.SelectedIndex = 0;
		}
	}
}

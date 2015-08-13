using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using NLog;
using JMMClient.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.ComponentModel;

namespace JMMClient.Forms
{
    /// <summary>
    /// Interaction logic for SearchTraktForm.xaml
    /// </summary>
    public partial class SearchTraktForm : Window
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

        public ICollectionView ViewTraktSeriesSearchResults { get; set; }
        public ObservableCollection<TraktTVShowResponseVM> TraktSeriesSearchResults { get; set; }

        public ICollectionView ViewCrossRef_AniDB_TraktResult { get; set; }
        public ObservableCollection<CrossRef_AniDB_TraktVMV2> CrossRef_AniDB_TraktResult { get; set; }

        public static readonly DependencyProperty IsSearchProperty = DependencyProperty.Register("IsSearch",
			typeof(bool), typeof(SearchTraktForm), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty IsExistingProperty = DependencyProperty.Register("IsExisting",
			typeof(bool), typeof(SearchTraktForm), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty HasWebCacheRecProperty = DependencyProperty.Register("HasWebCacheRec",
			typeof(bool), typeof(SearchTraktForm), new UIPropertyMetadata(false, null));


		public bool IsSearch
		{
			get { return (bool)GetValue(IsSearchProperty); }
			set { SetValue(IsSearchProperty, value); }
		}

		public bool IsExisting
		{
			get { return (bool)GetValue(IsExistingProperty); }
			set { SetValue(IsExistingProperty, value); }
		}

		public bool HasWebCacheRec
		{
			get { return (bool)GetValue(HasWebCacheRecProperty); }
			set { SetValue(HasWebCacheRecProperty, value); }
		}

		

		public static readonly DependencyProperty AnimeNameProperty = DependencyProperty.Register("AnimeName",
			typeof(string), typeof(SearchTraktForm), new UIPropertyMetadata("", null));

		public string AnimeName
		{
			get { return (string)GetValue(AnimeNameProperty); }
			set { SetValue(AnimeNameProperty, value); }
		}

		private int AnimeID = 0;
		private string ExistingTraktID = "";
		public string SelectedTraktID = "";
        private AniDB_AnimeVM Anime = null;

		public SearchTraktForm()
		{
			InitializeComponent();

            TraktSeriesSearchResults = new ObservableCollection<TraktTVShowResponseVM>();
            ViewTraktSeriesSearchResults = CollectionViewSource.GetDefaultView(TraktSeriesSearchResults);

            CrossRef_AniDB_TraktResult = new ObservableCollection<CrossRef_AniDB_TraktVMV2>();
            ViewCrossRef_AniDB_TraktResult = CollectionViewSource.GetDefaultView(CrossRef_AniDB_TraktResult);

            rbExisting.Checked += new RoutedEventHandler(rbExisting_Checked);
			rbSearch.Checked += new RoutedEventHandler(rbSearch_Checked);

			hlURL.Click += new RoutedEventHandler(hlURL_Click);
			//hlURLWebCache.Click += new RoutedEventHandler(hlURLWebCache_Click);

			rbSearch.IsChecked = true;
			rbExisting.IsChecked = false;

			btnSearch.Click += new RoutedEventHandler(btnSearch_Click);
			btnClose.Click += new RoutedEventHandler(btnClose_Click);
			btnUseThis.Click += new RoutedEventHandler(btnUseThis_Click);
			btnUseThisExisting.Click += new RoutedEventHandler(btnUseThisExisting_Click);
		}

		void btnUseThisExisting_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// prompt to select season
				Window wdw = Window.GetWindow(this);

				this.Cursor = Cursors.Wait;
				SelectTraktSeasonForm frm = new SelectTraktSeasonForm();
				frm.Owner = wdw;
                frm.Init(AnimeID, AnimeName, EpisodeType.Episode, 1, txtSeriesID.Text.Trim(), 1, 1, AnimeName, Anime, null);
				bool? result = frm.ShowDialog();
				if (result.Value)
				{
					SelectedTraktID = txtSeriesID.Text.Trim();
					this.DialogResult = true;
					this.Cursor = Cursors.Arrow;
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

		void btnUseThis_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.Cursor = Cursors.Wait;

                // remove any existing links
                string res = JMMServerVM.Instance.clientBinaryHTTP.RemoveLinkAniDBTraktForAnime(this.AnimeID);
                if (res.Length > 0)
                {
                    MessageBox.Show(res, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Cursor = Cursors.Arrow;
                    return;
                }

                // add links
                foreach (CrossRef_AniDB_TraktVMV2 xref in CrossRef_AniDB_TraktResult)
                {
                    res = JMMServerVM.Instance.clientBinaryHTTP.LinkAniDBTrakt(xref.AnimeID, xref.AniDBStartEpisodeType, xref.AniDBStartEpisodeNumber,
                        xref.TraktID, xref.TraktSeasonNumber, xref.TraktStartEpisodeNumber, null);
                    if (res.Length > 0)
                    {
                        MessageBox.Show(res, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        this.Cursor = Cursors.Arrow;
                        return;
                    }

                }

                this.DialogResult = true;
                this.Cursor = Cursors.Arrow;
                this.Close();
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
			SelectedTraktID = "";
			this.Close();
		}

		private void CommandBinding_UseThis(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(TraktTVShowResponseVM))
				{
					this.Cursor = Cursors.Wait;
					TraktTVShowResponseVM searchResult = obj as TraktTVShowResponseVM;

					// prompt to select season
					Window wdw = Window.GetWindow(this);

					this.Cursor = Cursors.Wait;
					SelectTraktSeasonForm frm = new SelectTraktSeasonForm();
					frm.Owner = wdw;
                    frm.Init(AnimeID, AnimeName, EpisodeType.Episode, 1, searchResult.TraktID, 1, 1, AnimeName, Anime, null);
					bool? result = frm.ShowDialog();
					if (result.Value)
					{
						SelectedTraktID = searchResult.TraktID;
						this.DialogResult = true;
						this.Cursor = Cursors.Arrow;
						this.Close();
					}
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

		void btnSearch_Click(object sender, RoutedEventArgs e)
		{
            TraktSeriesSearchResults.Clear();
            CrossRef_AniDB_TraktResult.Clear();

            HasWebCacheRec = false;
			if (!JMMServerVM.Instance.ServerOnline) return;

			this.Cursor = Cursors.Wait;
			try
			{
                
                // first find what the community recommends
                List<JMMServerBinary.Contract_Azure_CrossRef_AniDB_Trakt> xrefs = JMMServerVM.Instance.clientBinaryHTTP.GetTraktCrossRefWebCache(AnimeID, false);
                if (xrefs != null && xrefs.Count > 0)
                {
                    foreach (JMMServerBinary.Contract_Azure_CrossRef_AniDB_Trakt xref in xrefs)
                    {
                        CrossRef_AniDB_TraktVMV2 xrefAzure = new CrossRef_AniDB_TraktVMV2(xref);
                        CrossRef_AniDB_TraktResult.Add(xrefAzure);
                    }

                    HasWebCacheRec = true;
                }

                // now search Trakt
                

                List<JMMServerBinary.Contract_TraktTVShowResponse> traktResults = JMMServerVM.Instance.clientBinaryHTTP.SearchTrakt(txtSearch.Text.Trim());
				foreach (JMMServerBinary.Contract_TraktTVShowResponse traktResult in traktResults)
					TraktSeriesSearchResults.Add(new TraktTVShowResponseVM(traktResult));
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

		void hlURL_Click(object sender, RoutedEventArgs e)
		{
			Uri uri = new Uri(string.Format(Constants.URLS.Trakt_Series, txtSeriesID.Text.Trim()));
			Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
		}

		void hlURLWebCache_Click(object sender, RoutedEventArgs e)
		{
			//Uri uri = new Uri(string.Format(Constants.URLS.Trakt_Series, CrossRef_AniDB_TraktResult.TraktID));
			//Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
		}

		void rbSearch_Checked(object sender, RoutedEventArgs e)
		{
			EvaluateRadioButtons();
		}

		void rbExisting_Checked(object sender, RoutedEventArgs e)
		{
			EvaluateRadioButtons();
		}

		private void EvaluateRadioButtons()
		{
			IsSearch = rbSearch.IsChecked.Value;
			IsExisting = rbExisting.IsChecked.Value;

            HasWebCacheRec = IsSearch && CrossRef_AniDB_TraktResult != null && CrossRef_AniDB_TraktResult.Count > 0;
		}

        public void Init(int animeID, string animeName, string searchCriteria, string existingTraktID, AniDB_AnimeVM anime)
		{
            Anime = anime;
			AnimeID = animeID;
			AnimeName = animeName;
			ExistingTraktID = existingTraktID;
			txtSearch.Text = searchCriteria;
		}
	}
}

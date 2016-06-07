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
using System.Diagnostics;
using NLog;
using JMMClient.ViewModel;
using System.Threading;
using System.Globalization;

namespace JMMClient.Forms
{
	/// <summary>
	/// Interaction logic for SearchTvDBForm.xaml
	/// </summary>
	public partial class SearchTvDBForm : Window
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static readonly DependencyProperty IsSearchProperty = DependencyProperty.Register("IsSearch",
			typeof(bool), typeof(SearchTvDBForm), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty IsExistingProperty = DependencyProperty.Register("IsExisting",
			typeof(bool), typeof(SearchTvDBForm), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty HasWebCacheRecProperty = DependencyProperty.Register("HasWebCacheRec",
			typeof(bool), typeof(SearchTvDBForm), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty CrossRef_AniDB_TvDBResultProperty = DependencyProperty.Register("CrossRef_AniDB_TvDBResult",
			typeof(List<CrossRef_AniDB_TvDBVMV2>), typeof(SearchTvDBForm), new UIPropertyMetadata(null, null));

		public static readonly DependencyProperty TVDBSeriesSearchResultsProperty = DependencyProperty.Register("TVDBSeriesSearchResults",
			typeof(List<TVDBSeriesSearchResultVM>), typeof(SearchTvDBForm), new UIPropertyMetadata(null, null));

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

		public List<CrossRef_AniDB_TvDBVMV2> CrossRef_AniDB_TvDBResult
		{
			get { return (List<CrossRef_AniDB_TvDBVMV2>)GetValue(CrossRef_AniDB_TvDBResultProperty); }
			set { SetValue(CrossRef_AniDB_TvDBResultProperty, value); }
		}

		public List<TVDBSeriesSearchResultVM> TVDBSeriesSearchResults
		{
			get { return (List<TVDBSeriesSearchResultVM>)GetValue(TVDBSeriesSearchResultsProperty); }
			set { SetValue(TVDBSeriesSearchResultsProperty, value); }
		}

		public static readonly DependencyProperty AnimeNameProperty = DependencyProperty.Register("AnimeName",
			typeof(string), typeof(SearchTvDBForm), new UIPropertyMetadata("", null));

		public string AnimeName
		{
			get { return (string)GetValue(AnimeNameProperty); }
			set { SetValue(AnimeNameProperty, value); }
		}

		private int AnimeID = 0;
		private int? ExistingTVDBID = null;
		public int? SelectedTvDBID = null;
		private AniDB_AnimeVM Anime = null;

		public SearchTvDBForm()
		{
			InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            CrossRef_AniDB_TvDBResult = new List<CrossRef_AniDB_TvDBVMV2>();

			rbExisting.Checked += new RoutedEventHandler(rbExisting_Checked);
			rbSearch.Checked += new RoutedEventHandler(rbSearch_Checked);

			hlURL.Click += new RoutedEventHandler(hlURL_Click);

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
				int id = 0;
				int.TryParse(txtSeriesID.Text, out id);
				if (id <= 0)
				{
					MessageBox.Show(Properties.Resources.Search_InvalidTvDB, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
					txtSeriesID.Focus();
					return;
				}

				this.Cursor = Cursors.Wait;

				// prompt to select season
				Window wdw = Window.GetWindow(this);

				this.Cursor = Cursors.Wait;
				SelectTvDBSeasonForm frm = new SelectTvDBSeasonForm();
				frm.Owner = wdw;
				frm.Init(AnimeID, AnimeName, EpisodeType.Episode, 1, id, 1, 1, AnimeName, Anime, null);
				bool? result = frm.ShowDialog();
				if (result.Value)
				{
					SelectedTvDBID = id;
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
				string res = JMMServerVM.Instance.clientBinaryHTTP.RemoveLinkAniDBTvDBForAnime(this.AnimeID);
				if (res.Length > 0)
				{
					MessageBox.Show(res, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
					this.Cursor = Cursors.Arrow;
					return;
				}


				// add links
				foreach (CrossRef_AniDB_TvDBVMV2 xref in CrossRef_AniDB_TvDBResult)
				{
					res = JMMServerVM.Instance.clientBinaryHTTP.LinkAniDBTvDB(xref.AnimeID, xref.AniDBStartEpisodeType, xref.AniDBStartEpisodeNumber,
						xref.TvDBID, xref.TvDBSeasonNumber, xref.TvDBStartEpisodeNumber, null);
					if (res.Length > 0)
					{
						MessageBox.Show(res, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
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
			SelectedTvDBID = null;
			this.Close();
		}

		private void CommandBinding_UseThis(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(TVDBSeriesSearchResultVM))
				{
					this.Cursor = Cursors.Wait;
					TVDBSeriesSearchResultVM searchResult = obj as TVDBSeriesSearchResultVM;

					// prompt to select season
					Window wdw = Window.GetWindow(this);

					this.Cursor = Cursors.Wait;
					SelectTvDBSeasonForm frm = new SelectTvDBSeasonForm();
					frm.Owner = wdw;
					frm.Init(AnimeID, AnimeName, EpisodeType.Episode, 1, searchResult.SeriesID, 1,1, searchResult.SeriesName, Anime, null);
					bool? result = frm.ShowDialog();
					if (result.Value)
					{
						SelectedTvDBID = searchResult.SeriesID;
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
			HasWebCacheRec = false;
			if (!JMMServerVM.Instance.ServerOnline) return;

			this.Cursor = Cursors.Wait;
			try
			{
				CrossRef_AniDB_TvDBResult.Clear();
				// first find what the community recommends
				List<JMMServerBinary.Contract_Azure_CrossRef_AniDB_TvDB> xrefs = JMMServerVM.Instance.clientBinaryHTTP.GetTVDBCrossRefWebCache(AnimeID, false);
				if (xrefs != null && xrefs.Count > 0)
				{
					foreach (JMMServerBinary.Contract_Azure_CrossRef_AniDB_TvDB xref in xrefs)
					{
						CrossRef_AniDB_TvDBVMV2 xrefAzure = new CrossRef_AniDB_TvDBVMV2(xref);
						CrossRef_AniDB_TvDBResult.Add(xrefAzure);
					}
					
					HasWebCacheRec = true;
				}

				// now search the TvDB
				TVDBSeriesSearchResults = new List<TVDBSeriesSearchResultVM>();
				List<JMMServerBinary.Contract_TVDBSeriesSearchResult> tvResults = JMMServerVM.Instance.clientBinaryHTTP.SearchTheTvDB(txtSearch.Text.Replace("`", "'").Trim());
				foreach (JMMServerBinary.Contract_TVDBSeriesSearchResult tvResult in tvResults)
					TVDBSeriesSearchResults.Add(new TVDBSeriesSearchResultVM(tvResult));
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
			int id = 0;
			int.TryParse(txtSeriesID.Text, out id);
			if (id <= 0) return;

			Uri uri = new Uri(string.Format(Constants.URLS.TvDB_Series, id));
			Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
		}

		void hlURLWebCache_Click(object sender, RoutedEventArgs e)
		{
			//Uri uri = new Uri(string.Format(Constants.URLS.TvDB_Series, CrossRef_AniDB_TvDBResult.TvDBID));
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

			HasWebCacheRec = IsSearch && CrossRef_AniDB_TvDBResult != null && CrossRef_AniDB_TvDBResult.Count > 0;
		}

		public void Init(int animeID, string animeName, string searchCriteria, int? existingTVDBID, AniDB_AnimeVM anime)
		{
			Anime = anime;
			AnimeID = animeID;
			AnimeName = animeName;
			ExistingTVDBID = existingTVDBID;
			txtSearch.Text = searchCriteria;
		}
	}
}

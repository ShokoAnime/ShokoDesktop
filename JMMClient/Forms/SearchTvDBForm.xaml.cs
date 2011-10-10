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
			typeof(CrossRef_AniDB_TvDBResultVM), typeof(SearchTvDBForm), new UIPropertyMetadata(null, null));

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

		public CrossRef_AniDB_TvDBResultVM CrossRef_AniDB_TvDBResult
		{
			get { return (CrossRef_AniDB_TvDBResultVM)GetValue(CrossRef_AniDB_TvDBResultProperty); }
			set { SetValue(CrossRef_AniDB_TvDBResultProperty, value); }
		}

		public List<TVDBSeriesSearchResultVM> TVDBSeriesSearchResults
		{
			get { return (List<TVDBSeriesSearchResultVM>)GetValue(TVDBSeriesSearchResultsProperty); }
			set { SetValue(TVDBSeriesSearchResultsProperty, value); }
		}

		private int AnimeID = 0;
		private int? ExistingTVDBID = null;
		public int? SelectedTvDBID = null;

		public SearchTvDBForm()
		{
			InitializeComponent();

			rbExisting.Checked += new RoutedEventHandler(rbExisting_Checked);
			rbSearch.Checked += new RoutedEventHandler(rbSearch_Checked);

			hlURL.Click += new RoutedEventHandler(hlURL_Click);
			hlURLWebCache.Click += new RoutedEventHandler(hlURLWebCache_Click);

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
					MessageBox.Show("Please enter a valid number as the series ID", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					txtSeriesID.Focus();
					return;
				}

				this.Cursor = Cursors.Wait;
				LinkAniDBToTVDB(id, 1);
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
				LinkAniDBToTVDB(CrossRef_AniDB_TvDBResult.TvDBID, CrossRef_AniDB_TvDBResult.TvDBSeasonNumber);
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

		private void LinkAniDBToTVDB(int tvDBID, int season)
		{
			string res = JMMServerVM.Instance.clientBinaryHTTP.LinkAniDBTvDB(AnimeID, tvDBID, season);
			if (res.Length > 0)
				MessageBox.Show(res, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			else
			{
				this.DialogResult = true;
				SelectedTvDBID = tvDBID;
				this.Close();
			}
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
					LinkAniDBToTVDB(searchResult.SeriesID, 1);
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
				// first find what the community recommends
				JMMServerBinary.Contract_CrossRef_AniDB_TvDBResult xref = JMMServerVM.Instance.clientBinaryHTTP.GetTVDBCrossRefWebCache(AnimeID);
				if (xref != null)
				{
					CrossRef_AniDB_TvDBResult = new CrossRef_AniDB_TvDBResultVM(xref);
					HasWebCacheRec = true;
				}

				// now search the TvDB
				TVDBSeriesSearchResults = new List<TVDBSeriesSearchResultVM>();
				List<JMMServerBinary.Contract_TVDBSeriesSearchResult> tvResults = JMMServerVM.Instance.clientBinaryHTTP.SearchTheTvDB(txtSearch.Text.Trim());
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
			Uri uri = new Uri(string.Format(Constants.URLS.TvDB_Series, CrossRef_AniDB_TvDBResult.TvDBID));
			Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
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

			HasWebCacheRec = IsSearch && CrossRef_AniDB_TvDBResult != null;
		}

		public void Init(int animeID, string searchCriteria, int? existingTVDBID)
		{
			AnimeID = animeID;
			ExistingTVDBID = existingTVDBID;
			txtSearch.Text = searchCriteria;
		}
	}
}

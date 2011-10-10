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
	/// Interaction logic for SearchTraktForm.xaml
	/// </summary>
	public partial class SearchTraktForm : Window
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static readonly DependencyProperty IsSearchProperty = DependencyProperty.Register("IsSearch",
			typeof(bool), typeof(SearchTraktForm), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty IsExistingProperty = DependencyProperty.Register("IsExisting",
			typeof(bool), typeof(SearchTraktForm), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty HasWebCacheRecProperty = DependencyProperty.Register("HasWebCacheRec",
			typeof(bool), typeof(SearchTraktForm), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty CrossRef_AniDB_TraktResultProperty = DependencyProperty.Register("CrossRef_AniDB_TraktResult",
			typeof(CrossRef_AniDB_TraktResultVM), typeof(SearchTraktForm), new UIPropertyMetadata(null, null));

		public static readonly DependencyProperty TraktSeriesSearchResultsProperty = DependencyProperty.Register("TraktSeriesSearchResults",
			typeof(List<TraktTVShowResponseVM>), typeof(SearchTraktForm), new UIPropertyMetadata(null, null));

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

		public CrossRef_AniDB_TraktResultVM CrossRef_AniDB_TraktResult
		{
			get { return (CrossRef_AniDB_TraktResultVM)GetValue(CrossRef_AniDB_TraktResultProperty); }
			set { SetValue(CrossRef_AniDB_TraktResultProperty, value); }
		}

		public List<TraktTVShowResponseVM> TraktSeriesSearchResults
		{
			get { return (List<TraktTVShowResponseVM>)GetValue(TraktSeriesSearchResultsProperty); }
			set { SetValue(TraktSeriesSearchResultsProperty, value); }
		}

		private int AnimeID = 0;
		private string ExistingTraktID = "";
		public string SelectedTraktID = "";

		public SearchTraktForm()
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
				this.Cursor = Cursors.Wait;
				LinkAniDBToTrakt(txtSeriesID.Text.Trim(), 1);
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
				LinkAniDBToTrakt(CrossRef_AniDB_TraktResult.TraktID, CrossRef_AniDB_TraktResult.TraktSeasonNumber);
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

		private void LinkAniDBToTrakt(string traktID, int season)
		{
			string res = JMMServerVM.Instance.clientBinaryHTTP.LinkAniDBTrakt(AnimeID, traktID, season);
			if (res.Length > 0)
				MessageBox.Show(res, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			else
			{
				this.DialogResult = true;
				SelectedTraktID = traktID;
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
				if (obj.GetType() == typeof(TraktTVShowResponseVM))
				{
					this.Cursor = Cursors.Wait;
					TraktTVShowResponseVM searchResult = obj as TraktTVShowResponseVM;
					LinkAniDBToTrakt(searchResult.TraktID, 1);
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
				JMMServerBinary.Contract_CrossRef_AniDB_TraktResult xref = JMMServerVM.Instance.clientBinaryHTTP.GetTraktCrossRefWebCache(AnimeID);
				if (xref != null)
				{
					CrossRef_AniDB_TraktResult = new CrossRef_AniDB_TraktResultVM(xref);
					HasWebCacheRec = true;
				}

				// now search the TvDB
				TraktSeriesSearchResults = new List<TraktTVShowResponseVM>();
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
			Uri uri = new Uri(string.Format(Constants.URLS.Trakt_Series, CrossRef_AniDB_TraktResult.TraktID));
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

			HasWebCacheRec = IsSearch && CrossRef_AniDB_TraktResult != null;
		}

		public void Init(int animeID, string searchCriteria, string existingTraktID)
		{
			AnimeID = animeID;
			ExistingTraktID = existingTraktID;
			txtSearch.Text = searchCriteria;
		}
	}
}

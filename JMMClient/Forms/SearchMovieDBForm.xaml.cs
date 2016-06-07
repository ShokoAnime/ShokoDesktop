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
	/// Interaction logic for SearchMovieDBForm.xaml
	/// </summary>
	public partial class SearchMovieDBForm : Window
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static readonly DependencyProperty IsSearchProperty = DependencyProperty.Register("IsSearch",
			typeof(bool), typeof(SearchMovieDBForm), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty IsExistingProperty = DependencyProperty.Register("IsExisting",
			typeof(bool), typeof(SearchMovieDBForm), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty HasWebCacheRecProperty = DependencyProperty.Register("HasWebCacheRec",
			typeof(bool), typeof(SearchMovieDBForm), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty CrossRef_AniDB_OtherResultProperty = DependencyProperty.Register("CrossRef_AniDB_OtherResult",
			typeof(CrossRef_AniDB_OtherResultVM), typeof(SearchMovieDBForm), new UIPropertyMetadata(null, null));

		public static readonly DependencyProperty MovieDBSearchResultsProperty = DependencyProperty.Register("MovieDBSearchResults",
			typeof(List<MovieDBMovieSearchResultVM>), typeof(SearchMovieDBForm), new UIPropertyMetadata(null, null));

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

		public CrossRef_AniDB_OtherResultVM CrossRef_AniDB_OtherResult
		{
			get { return (CrossRef_AniDB_OtherResultVM)GetValue(CrossRef_AniDB_OtherResultProperty); }
			set { SetValue(CrossRef_AniDB_OtherResultProperty, value); }
		}

		public List<MovieDBMovieSearchResultVM> MovieDBSearchResults
		{
			get { return (List<MovieDBMovieSearchResultVM>)GetValue(MovieDBSearchResultsProperty); }
			set { SetValue(MovieDBSearchResultsProperty, value); }
		}

		private int AnimeID = 0;
		public int? SelectedMovieID = null;

		public SearchMovieDBForm()
		{
			InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

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
				int.TryParse(txtMovieID.Text, out id);
				if (id <= 0)
				{
					MessageBox.Show(Properties.Resources.Search_InvalidMovieID, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
					txtMovieID.Focus();
					return;
				}

				this.Cursor = Cursors.Wait;
				LinkAniDBToMovieDB(id);
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
				LinkAniDBToMovieDB(int.Parse(CrossRef_AniDB_OtherResult.CrossRefID));
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
			SelectedMovieID = null;
			this.Close();
		}

		private void LinkAniDBToMovieDB(int movieID)
		{
			string res = JMMServerVM.Instance.clientBinaryHTTP.LinkAniDBOther(AnimeID, movieID, (int)CrossRefType.MovieDB);
			if (res.Length > 0)
				MessageBox.Show(res, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
			else
			{
				this.DialogResult = true;
				SelectedMovieID = movieID;
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
				if (obj.GetType() == typeof(MovieDBMovieSearchResultVM))
				{
					this.Cursor = Cursors.Wait;
					MovieDBMovieSearchResultVM searchResult = obj as MovieDBMovieSearchResultVM;
					LinkAniDBToMovieDB(searchResult.MovieID);
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
				JMMServerBinary.Contract_CrossRef_AniDB_OtherResult xref = JMMServerVM.Instance.clientBinaryHTTP.GetOtherAnimeCrossRefWebCache(AnimeID, (int)CrossRefType.MovieDB);
				if (xref != null)
				{
					CrossRef_AniDB_OtherResult = new CrossRef_AniDB_OtherResultVM(xref);
					HasWebCacheRec = true;
				}

				// now search the TvDB
				MovieDBSearchResults = new List<MovieDBMovieSearchResultVM>();
				List<JMMServerBinary.Contract_MovieDBMovieSearchResult> movieResults = JMMServerVM.Instance.clientBinaryHTTP.SearchTheMovieDB(txtSearch.Text.Replace("`", "'").Trim());
				foreach (JMMServerBinary.Contract_MovieDBMovieSearchResult movieResult in movieResults)
					MovieDBSearchResults.Add(new MovieDBMovieSearchResultVM(movieResult));
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
			int.TryParse(txtMovieID.Text, out id);
			if (id <= 0) return;

			Uri uri = new Uri(string.Format(Constants.URLS.MovieDB_Series, id));
			Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
		}

		void hlURLWebCache_Click(object sender, RoutedEventArgs e)
		{
			Uri uri = new Uri(string.Format(Constants.URLS.MovieDB_Series, CrossRef_AniDB_OtherResult.CrossRefID));
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

			HasWebCacheRec = IsSearch && CrossRef_AniDB_OtherResult != null;
		}

		public void Init(int animeID, string searchCriteria)
		{
			AnimeID = animeID;
			txtSearch.Text = searchCriteria;
		}
	}
}

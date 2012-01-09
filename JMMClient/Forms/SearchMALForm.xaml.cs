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
using System.Diagnostics;

namespace JMMClient.Forms
{
	/// <summary>
	/// Interaction logic for SearchMALForm.xaml
	/// </summary>
	public partial class SearchMALForm : Window
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static readonly DependencyProperty IsSearchProperty = DependencyProperty.Register("IsSearch",
			typeof(bool), typeof(SearchMALForm), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty IsExistingProperty = DependencyProperty.Register("IsExisting",
			typeof(bool), typeof(SearchMALForm), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty HasWebCacheRecProperty = DependencyProperty.Register("HasWebCacheRec",
			typeof(bool), typeof(SearchMALForm), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty CrossRef_AniDB_MALResultProperty = DependencyProperty.Register("CrossRef_AniDB_MALResult",
			typeof(CrossRef_AniDB_MALResultVM), typeof(SearchMALForm), new UIPropertyMetadata(null, null));

		public static readonly DependencyProperty MALSearchResultsProperty = DependencyProperty.Register("MALSearchResults",
			typeof(List<MALSearchResultVM>), typeof(SearchMALForm), new UIPropertyMetadata(null, null));

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

		public CrossRef_AniDB_MALResultVM CrossRef_AniDB_MALResult
		{
			get { return (CrossRef_AniDB_MALResultVM)GetValue(CrossRef_AniDB_MALResultProperty); }
			set { SetValue(CrossRef_AniDB_MALResultProperty, value); }
		}

		public List<MALSearchResultVM> MALSearchResults
		{
			get { return (List<MALSearchResultVM>)GetValue(MALSearchResultsProperty); }
			set { SetValue(MALSearchResultsProperty, value); }
		}

		private int AnimeID = 0;
		public int? SelectedMALID = null;

		public SearchMALForm()
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
				int.TryParse(txtMALID.Text, out id);
				if (id <= 0)
				{
					MessageBox.Show("Please enter a valid number as the MAL ID", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					txtMALID.Focus();
					return;
				}

				if (string.IsNullOrEmpty(txtMALTitle.Text.Trim()))
				{
					MessageBox.Show("Please enter a title for the show", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					txtMALTitle.Focus();
					return;
				}

				this.Cursor = Cursors.Wait;
				LinkAniDBToMAL(id, txtMALTitle.Text.Trim());
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
				LinkAniDBToMAL(CrossRef_AniDB_MALResult.MALID, CrossRef_AniDB_MALResult.MALTitle);
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
			SelectedMALID = null;
			this.Close();
		}

		private void LinkAniDBToMAL(int malID, string malTitle)
		{
			string res = JMMServerVM.Instance.clientBinaryHTTP.LinkAniDBMAL(AnimeID, malID, malTitle);
			if (res.Length > 0)
				MessageBox.Show(res, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			else
			{
				this.DialogResult = true;
				SelectedMALID = malID;
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
				if (obj.GetType() == typeof(MALSearchResultVM))
				{
					this.Cursor = Cursors.Wait;
					MALSearchResultVM searchResult = obj as MALSearchResultVM;
					LinkAniDBToMAL(searchResult.id, searchResult.title);
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
				JMMServerBinary.Contract_CrossRef_AniDB_MALResult xref = JMMServerVM.Instance.clientBinaryHTTP.GetMALCrossRefWebCache(AnimeID);
				if (xref != null)
				{
					CrossRef_AniDB_MALResult = new CrossRef_AniDB_MALResultVM(xref);
					HasWebCacheRec = true;
				}

				// now search the TvDB
				MALSearchResults = new List<MALSearchResultVM>();
				List<JMMServerBinary.Contract_MALAnimeResponse> malResults = JMMServerVM.Instance.clientBinaryHTTP.SearchMAL(txtSearch.Text.Trim());
				foreach (JMMServerBinary.Contract_MALAnimeResponse malResult in malResults)
					MALSearchResults.Add(new MALSearchResultVM(malResult));
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
			int.TryParse(txtMALID.Text, out id);
			if (id <= 0) return;

			Uri uri = new Uri(string.Format(Constants.URLS.MAL_Series, id));
			Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
		}

		void hlURLWebCache_Click(object sender, RoutedEventArgs e)
		{
			Uri uri = new Uri(string.Format(Constants.URLS.MAL_Series, CrossRef_AniDB_MALResult.MALID));
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

			HasWebCacheRec = IsSearch && CrossRef_AniDB_MALResult != null;
		}

		public void Init(int animeID, string searchCriteria)
		{
			AnimeID = animeID;
			txtSearch.Text = searchCriteria;
		}
	}
}

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
using System.Globalization;
using System.Threading;

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
			typeof(List<CrossRef_AniDB_MALResultVM>), typeof(SearchMALForm), new UIPropertyMetadata(null, null));

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

		public List<CrossRef_AniDB_MALResultVM> CrossRef_AniDB_MALResult
		{
			get { return (List<CrossRef_AniDB_MALResultVM>)GetValue(CrossRef_AniDB_MALResultProperty); }
			set { SetValue(CrossRef_AniDB_MALResultProperty, value); }
		}

		public List<MALSearchResultVM> MALSearchResults
		{
			get { return (List<MALSearchResultVM>)GetValue(MALSearchResultsProperty); }
			set { SetValue(MALSearchResultsProperty, value); }
		}

		public static readonly DependencyProperty AnimeNameProperty = DependencyProperty.Register("AnimeName_MAL",
			typeof(string), typeof(SearchTraktForm), new UIPropertyMetadata("", null));

		public string AnimeName_MAL
		{
			get { return (string)GetValue(AnimeNameProperty); }
			set { SetValue(AnimeNameProperty, value); }
		}

		private int AnimeID = 0;
		public int? SelectedMALID = null;

		public SearchMALForm()
		{
			InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            rbExisting.Checked += new RoutedEventHandler(rbExisting_Checked);
			rbSearch.Checked += new RoutedEventHandler(rbSearch_Checked);

			hlURL.Click += new RoutedEventHandler(hlURL_Click);

			rbSearch.IsChecked = true;
			rbExisting.IsChecked = false;

			btnSearch.Click += new RoutedEventHandler(btnSearch_Click);
			btnClose.Click += new RoutedEventHandler(btnClose_Click);
			btnUseThis.Click += new RoutedEventHandler(btnUseThis_Click);
			btnUseThisExisting.Click += new RoutedEventHandler(btnUseThisExisting_Click);

            btnChkCred.Click += new RoutedEventHandler(btnChkCred_Click);

            CrossRef_AniDB_MALResult = new List<CrossRef_AniDB_MALResultVM>();
		}

		void btnUseThisExisting_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				int id = 0;
				int.TryParse(txtMALID.Text, out id);
				if (id <= 0)
				{
					MessageBox.Show(Properties.Resources.Search_InvalidMAL, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
					txtMALID.Focus();
					return;
				}

				if (string.IsNullOrEmpty(txtMALTitle.Text.Trim()))
				{
					MessageBox.Show(Properties.Resources.Search_EnterTitle, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
					txtMALTitle.Focus();
					return;
				}

				// prompt to select details
				Window wdw = Window.GetWindow(this);

				this.Cursor = Cursors.Wait;
				SelectMALStartForm frm = new SelectMALStartForm();
				frm.Owner = wdw;
				frm.Init(AnimeID, AnimeName_MAL, txtMALTitle.Text.Trim(), id, null, null);
				bool? result = frm.ShowDialog();
				if (result.Value)
				{
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

				foreach (CrossRef_AniDB_MALResultVM xref in CrossRef_AniDB_MALResult)
				{
					LinkAniDBToMAL(xref.MALID, xref.MALTitle, xref.StartEpisodeType, xref.StartEpisodeNumber);
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
			SelectedMALID = null;
			this.Close();
		}

		private void LinkAniDBToMAL(int malID, string malTitle, int epType, int epNumber)
		{
			string res = JMMServerVM.Instance.clientBinaryHTTP.LinkAniDBMAL(AnimeID, malID, malTitle, epType, epNumber);
			if (res.Length > 0)
				MessageBox.Show(res, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
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

					// prompt to select details
					Window wdw = Window.GetWindow(this);

					this.Cursor = Cursors.Wait;
					SelectMALStartForm frm = new SelectMALStartForm();
					frm.Owner = wdw;
					frm.Init(AnimeID, AnimeName_MAL, searchResult.title, searchResult.id, null, null);
					bool? result = frm.ShowDialog();
					if (result.Value)
					{
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
			CrossRef_AniDB_MALResult.Clear();

			if (!JMMServerVM.Instance.ServerOnline) return;

			this.Cursor = Cursors.Wait;
			try
			{
				// first find what the community recommends
				
				JMMServerBinary.Contract_CrossRef_AniDB_MALResult xref = JMMServerVM.Instance.clientBinaryHTTP.GetMALCrossRefWebCache(AnimeID);
				if (xref != null)
				{
					CrossRef_AniDB_MALResult.Add(new CrossRef_AniDB_MALResultVM(xref));
					HasWebCacheRec = true;
				}

				// now search MAL
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
			//TODO
			//Uri uri = new Uri(string.Format(Constants.URLS.MAL_Series, CrossRef_AniDB_MALResult.MALID));
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

			HasWebCacheRec = IsSearch && CrossRef_AniDB_MALResult != null;
		}

		public void Init(int animeID, string animeName, string searchCriteria)
		{
			AnimeID = animeID;
			AnimeName_MAL = animeName;
			txtSearch.Text = searchCriteria;
		}

        private void btnChkCred_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            UserControls.MALSettings OP = new UserControls.MALSettings();
            var host = new Window();
            host.Content = OP;
            host.Title = Properties.Resources.MAL_CheckCred;
            host.Width = 465;
            host.Height = 180;
            host.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            host.ShowDialog();
        }
    }
}

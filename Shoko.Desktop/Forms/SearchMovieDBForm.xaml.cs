using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Models.Enums;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;

namespace Shoko.Desktop.Forms
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

        public static readonly DependencyProperty CrossRef_AniDB_Other_ResponseProperty = DependencyProperty.Register("CrossRef_AniDB_Other_Response",
            typeof(CL_CrossRef_AniDB_Other_Response), typeof(SearchMovieDBForm), new UIPropertyMetadata(null, null));

        public static readonly DependencyProperty MovieDBSearchResultsProperty = DependencyProperty.Register("MovieDBSearchResults",
            typeof(List<VM_MovieDBMovieSearch_Response>), typeof(SearchMovieDBForm), new UIPropertyMetadata(null, null));

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

        public CL_CrossRef_AniDB_Other_Response CrossRef_AniDB_Other_Response
        {
            get { return (CL_CrossRef_AniDB_Other_Response)GetValue(CrossRef_AniDB_Other_ResponseProperty); }
            set { SetValue(CrossRef_AniDB_Other_ResponseProperty, value); }
        }

        public List<VM_MovieDBMovieSearch_Response> MovieDBSearchResults
        {
            get { return (List<VM_MovieDBMovieSearch_Response>)GetValue(MovieDBSearchResultsProperty); }
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

                Cursor = Cursors.Wait;
                LinkAniDBToMovieDB(id);
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

        void btnUseThis_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;
                LinkAniDBToMovieDB(int.Parse(CrossRef_AniDB_Other_Response.CrossRefID));
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
            SelectedMovieID = null;
            Close();
        }

        private void LinkAniDBToMovieDB(int movieID)
        {
            string res = VM_ShokoServer.Instance.ShokoServices.LinkAniDBOther(AnimeID, movieID, (int)CrossRefType.MovieDB);
            if (res.Length > 0)
                MessageBox.Show(res, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                DialogResult = true;
                SelectedMovieID = movieID;
                Close();
            }
        }

        private void CommandBinding_UseThis(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_MovieDBMovieSearch_Response))
                {
                    Cursor = Cursors.Wait;
                    VM_MovieDBMovieSearch_Response searchResult = obj as VM_MovieDBMovieSearch_Response;
                    LinkAniDBToMovieDB(searchResult.MovieID);
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


        void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            HasWebCacheRec = false;
            if (!VM_ShokoServer.Instance.ServerOnline) return;

            Cursor = Cursors.Wait;
            try
            {
                // first find what the community recommends
                CrossRef_AniDB_Other_Response = VM_ShokoServer.Instance.ShokoServices.GetOtherAnimeCrossRefWebCache(AnimeID, (int)CrossRefType.MovieDB);
                if (CrossRef_AniDB_Other_Response != null)
                    HasWebCacheRec = true;

                // now search the TvDB
                MovieDBSearchResults = VM_ShokoServer.Instance.ShokoServices.SearchTheMovieDB(txtSearch.Text.Replace("`", "'").Trim()).CastList<VM_MovieDBMovieSearch_Response>();
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

        void hlURL_Click(object sender, RoutedEventArgs e)
        {
            int id = 0;
            int.TryParse(txtMovieID.Text, out id);
            if (id <= 0) return;

            Uri uri = new Uri(string.Format(Models.Constants.URLS.MovieDB_Series, id));
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
        }

        void hlURLWebCache_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri(string.Format(Models.Constants.URLS.MovieDB_Series, CrossRef_AniDB_Other_Response.CrossRefID));
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

            HasWebCacheRec = IsSearch && CrossRef_AniDB_Other_Response != null;
        }

        public void Init(int animeID, string searchCriteria)
        {
            AnimeID = animeID;
            txtSearch.Text = searchCriteria;
        }
    }
}

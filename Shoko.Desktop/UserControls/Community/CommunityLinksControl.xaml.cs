using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Azure;
using Shoko.Models.Enums;

namespace Shoko.Desktop.UserControls.Community
{
    /// <summary>
    /// Interaction logic for CommunityLinksControl.xaml
    /// </summary>
    public partial class CommunityLinksControl : UserControl
    {
        BackgroundWorker searchTvDBWorker = new BackgroundWorker();
        BackgroundWorker searchTraktWorker = new BackgroundWorker();

        public ObservableCollection<VM_CrossRef_AniDB_TvDBV2> TVDBResults { get; set; }
        public ICollectionView ViewTVDBResults { get; set; }


        public ObservableCollection<VM_CrossRef_AniDB_TraktV2> TraktResults { get; set; }
        public ICollectionView ViewTraktResults { get; set; }


        public static readonly DependencyProperty SearchStatusProperty = DependencyProperty.Register("SearchStatus",
            typeof(string), typeof(CommunityLinksControl), new UIPropertyMetadata("", null));

        public string SearchStatus
        {
            get { return (string)GetValue(SearchStatusProperty); }
            set { SetValue(SearchStatusProperty, value); }
        }

        private string CurrentAniDBURL = string.Empty;
        private string CurrentOtherURL = string.Empty;

        public CommunityLinksControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            TVDBResults = new ObservableCollection<VM_CrossRef_AniDB_TvDBV2>();
            ViewTVDBResults = CollectionViewSource.GetDefaultView(TVDBResults);

            TraktResults = new ObservableCollection<VM_CrossRef_AniDB_TraktV2>();
            ViewTraktResults = CollectionViewSource.GetDefaultView(TraktResults);

            ViewTVDBResults.SortDescriptions.Add(new SortDescription("IsAdminApproved", ListSortDirection.Descending));
            ViewTVDBResults.SortDescriptions.Add(new SortDescription("Username", ListSortDirection.Ascending));
            ViewTVDBResults.SortDescriptions.Add(new SortDescription("TvDBID", ListSortDirection.Ascending));
            ViewTVDBResults.SortDescriptions.Add(new SortDescription("TvDBSeasonNumber", ListSortDirection.Ascending));
            ViewTVDBResults.SortDescriptions.Add(new SortDescription("TvDBStartEpisodeNumber", ListSortDirection.Ascending));

            ViewTraktResults.SortDescriptions.Add(new SortDescription("IsAdminApproved", ListSortDirection.Descending));
            ViewTraktResults.SortDescriptions.Add(new SortDescription("Username", ListSortDirection.Ascending));
            ViewTraktResults.SortDescriptions.Add(new SortDescription("TvDBID", ListSortDirection.Ascending));
            ViewTraktResults.SortDescriptions.Add(new SortDescription("TvDBSeasonNumber", ListSortDirection.Ascending));
            ViewTraktResults.SortDescriptions.Add(new SortDescription("TvDBStartEpisodeNumber", ListSortDirection.Ascending));

            btnSearch.Click += btnSearch_Click;
            btnRandomAnime.Click += btnRandomAnime_Click;

            searchTvDBWorker.DoWork += new DoWorkEventHandler(searchTvDBWorker_DoWork);
            searchTvDBWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(searchTvDBWorker_RunWorkerCompleted);

            searchTraktWorker.DoWork += new DoWorkEventHandler(searchTraktWorker_DoWork);
            searchTraktWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(searchTraktWorker_RunWorkerCompleted);

            dgTvDBResults.SelectionChanged += dgTvDBResults_SelectionChanged;
            dgTraktResults.SelectionChanged += dgTraktResults_SelectionChanged;

            webAniDB.Navigated += webAniDB_Navigated;
            webOther.Navigated += WebOther_Navigated;
        }

        private void WebOther_Navigated(object sender, NavigationEventArgs e)
        {
            HideScriptErrors(webOther, true);

            CurrentOtherURL = e.Uri.ToString();
        }

        void webAniDB_Navigated(object sender, NavigationEventArgs e)
        {
            HideScriptErrors(webAniDB, true);

            CurrentAniDBURL = e.Uri.ToString();
        }

        void dgTvDBResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            VM_CrossRef_AniDB_TvDBV2 xref = ((DataGrid)sender).SelectedItem as VM_CrossRef_AniDB_TvDBV2;
            if (xref == null) return;

            ShowAniDBWebPage(xref.GetAniDBURL());
            ShowOtherWebPage(xref.GetSeriesURL());
        }

        void dgTraktResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            VM_CrossRef_AniDB_TraktV2 xref = ((DataGrid)sender).SelectedItem as VM_CrossRef_AniDB_TraktV2;
            if (xref == null) return;

            ShowAniDBWebPage(xref.GetAniDBURL());
            ShowOtherWebPage(xref.GetShowURL());
        }

        private void ShowAniDBWebPage(string url)
        {
            if (CurrentAniDBURL.Equals(url, StringComparison.InvariantCultureIgnoreCase)) return;
            webAniDB.Navigate(url);
        }

        private void ShowOtherWebPage(string url)
        {
            if (CurrentOtherURL.Equals(url, StringComparison.InvariantCultureIgnoreCase)) return;
            webOther.Navigate(url);
        }

        public void HideScriptErrors(WebBrowser wb, bool Hide)
        {
            FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;
            object objComWebBrowser = fiComWebBrowser.GetValue(wb);
            if (objComWebBrowser == null) return;
            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { Hide });
        }

        #region TvDB

        void searchTvDBWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            SearchTvDBResults res = new SearchTvDBResults();
            res.ErrorMessage = string.Empty;
            res.TvDBLinks = new List<VM_CrossRef_AniDB_TvDBV2>();
            res.ExtraInfo = string.Empty;

            try
            {
                SearchCriteria crit = e.Argument as SearchCriteria;
                if (!string.IsNullOrEmpty(crit.ExtraInfo))
                    res.ExtraInfo = crit.ExtraInfo;

                try
                {
                    List<Azure_CrossRef_AniDB_TvDB> xrefs = VM_ShokoServer.Instance.ShokoServices.GetTVDBCrossRefWebCache(crit.AnimeID, true);
                    if (xrefs != null && xrefs.Count > 0)
                    {
                        foreach (Azure_CrossRef_AniDB_TvDB xref in xrefs)
                        {
                            res.TvDBLinks.Add((VM_CrossRef_AniDB_TvDBV2)xref);
                        }
                    }
                }
                catch (Exception ex)
                {
                    res.ErrorMessage = ex.Message;
                }

                e.Result = res;
            }
            catch (Exception ex)
            {
                res.ErrorMessage = ex.Message;
                e.Result = res;
            }
        }

        public void PerformTvDBSearch(SearchCriteria crit)
        {
            if (!VM_ShokoServer.Instance.ServerOnline) return;

            Cursor = Cursors.Wait;
            SearchStatus = string.Format(Properties.Resources.Community_Searching);

            txtSearch.Text = crit.AnimeID.ToString();

            try
            {
                TVDBResults.Clear();
                btnRandomAnime.IsEnabled = false;
                btnSearch.IsEnabled = false;

                Cursor = Cursors.Wait;

                searchTvDBWorker.RunWorkerAsync(crit);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void searchTvDBWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                SearchTvDBResults res = e.Result as SearchTvDBResults;
                if (!string.IsNullOrEmpty(res.ErrorMessage))
                {
                    //MessageBox.Show()
                    SearchStatus = res.ErrorMessage;
                    return;
                }

                foreach (VM_CrossRef_AniDB_TvDBV2 tvxref in res.TvDBLinks)
                    TVDBResults.Add(tvxref);

                SearchStatus = string.Empty;
                if (!string.IsNullOrEmpty(res.ExtraInfo))
                    SearchStatus = res.ExtraInfo;

                //SearchStatus = string.Format("{0} Anime still need TvDB approval", link.AnimeNeedingApproval);

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
                btnRandomAnime.IsEnabled = true;
                btnSearch.IsEnabled = true;
            }
        }

        #endregion

        #region Trakt

        void searchTraktWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            SearchTraktResults res = new SearchTraktResults();
            res.ErrorMessage = string.Empty;
            res.TraktLinks = new List<VM_CrossRef_AniDB_TraktV2>();
            res.ExtraInfo = string.Empty;

            try
            {
                SearchCriteria crit = e.Argument as SearchCriteria;
                if (!string.IsNullOrEmpty(crit.ExtraInfo))
                    res.ExtraInfo = crit.ExtraInfo;

                try
                {
                    List<Azure_CrossRef_AniDB_Trakt> xrefs = VM_ShokoServer.Instance.ShokoServices.GetTraktCrossRefWebCache(crit.AnimeID, true);
                    if (xrefs != null && xrefs.Count > 0)
                    {
                        foreach (Azure_CrossRef_AniDB_Trakt xref in xrefs)
                        {
                            res.TraktLinks.Add((VM_CrossRef_AniDB_TraktV2)xref);
                        }
                    }
                }
                catch (Exception ex)
                {
                    res.ErrorMessage = ex.Message;
                }

                e.Result = res;
            }
            catch (Exception ex)
            {
                res.ErrorMessage = ex.Message;
                e.Result = res;
            }
        }

        public void PerformTraktSearch(SearchCriteria crit)
        {
            if (!VM_ShokoServer.Instance.ServerOnline) return;

            Cursor = Cursors.Wait;
            SearchStatus = string.Format(Properties.Resources.Community_Searching);

            txtSearch.Text = crit.AnimeID.ToString();

            try
            {
                TraktResults.Clear();
                btnRandomAnime.IsEnabled = false;
                btnSearch.IsEnabled = false;

                Cursor = Cursors.Wait;

                searchTraktWorker.RunWorkerAsync(crit);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void searchTraktWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                SearchTraktResults res = e.Result as SearchTraktResults;
                if (!string.IsNullOrEmpty(res.ErrorMessage))
                {
                    //MessageBox.Show()
                    SearchStatus = res.ErrorMessage;
                    return;
                }

                foreach (VM_CrossRef_AniDB_TraktV2 tvxref in res.TraktLinks)
                    TraktResults.Add(tvxref);

                SearchStatus = string.Empty;
                if (!string.IsNullOrEmpty(res.ExtraInfo))
                    SearchStatus = res.ExtraInfo;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
                btnRandomAnime.IsEnabled = true;
                btnSearch.IsEnabled = true;
            }
        }

        #endregion

        void btnRandomAnime_Click(object sender, RoutedEventArgs e)
        {
            if (!VM_ShokoServer.Instance.ServerOnline) return;

            SearchStatus = Properties.Resources.Community_Searching;

            btnRandomAnime.IsEnabled = false;
            btnSearch.IsEnabled = false;

            Cursor = Cursors.Wait;
            try
            {
                if (tabResults.SelectedIndex == 0)
                {
                    TVDBResults.Clear();

                    Azure_AnimeLink link = VM_ShokoServer.Instance.ShokoServices.Admin_GetRandomLinkForApproval((int)AzureLinkType.TvDB);
                    if (link != null)
                    {
                        SearchCriteria crit = new SearchCriteria();
                        crit.AnimeID = link.RandomAnimeID;
                        crit.ExtraInfo = string.Format(Properties.Resources.Community_TvDBApproval, link.AnimeNeedingApproval);

                        PerformTvDBSearch(crit);
                    }
                }
                else if (tabResults.SelectedIndex == 1)
                {
                    TraktResults.Clear();

                    Azure_AnimeLink link = VM_ShokoServer.Instance.ShokoServices.Admin_GetRandomLinkForApproval((int)AzureLinkType.Trakt);
                    if (link != null)
                    {
                        SearchCriteria crit = new SearchCriteria();
                        crit.AnimeID = link.RandomAnimeID;
                        crit.ExtraInfo = string.Format(Properties.Resources.Community_TraktApproval, link.AnimeNeedingApproval);

                        PerformTraktSearch(crit);
                    }
                }
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Arrow;
                btnRandomAnime.IsEnabled = true;
                btnSearch.IsEnabled = true;
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text)) return;

            int animeID = 0;
            if (!int.TryParse(txtSearch.Text, out animeID)) return;

            SearchCriteria crit = new SearchCriteria();
            crit.AnimeID = animeID;
            crit.ExtraInfo = string.Empty;

            if (tabResults.SelectedIndex == 0)
                PerformTvDBSearch(crit);
            else if (tabResults.SelectedIndex == 1)
                PerformTraktSearch(crit);
        }

        private void CommandBinding_Approve(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_CrossRef_AniDB_TvDBV2))
                {
                    VM_CrossRef_AniDB_TvDBV2 xref = obj as VM_CrossRef_AniDB_TvDBV2;

                    string res = VM_ShokoServer.Instance.ShokoServices.ApproveTVDBCrossRefWebCache(xref.CrossRef_AniDB_TvDBV2ID);
                    if (string.IsNullOrEmpty(res))
                        xref.IsAdminApproved = 1;
                    else
                        MessageBox.Show(res, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);

                }
                if (obj.GetType() == typeof(VM_CrossRef_AniDB_TraktV2))
                {
                    VM_CrossRef_AniDB_TraktV2 xref = obj as VM_CrossRef_AniDB_TraktV2;

                    string res = VM_ShokoServer.Instance.ShokoServices.ApproveTraktCrossRefWebCache(xref.CrossRef_AniDB_TraktV2ID);
                    if (string.IsNullOrEmpty(res))
                        xref.IsAdminApproved = 1;
                    else
                        MessageBox.Show(res, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_Revoke(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_CrossRef_AniDB_TvDBV2))
                {
                    VM_CrossRef_AniDB_TvDBV2 xref = obj as VM_CrossRef_AniDB_TvDBV2;

                    string res = VM_ShokoServer.Instance.ShokoServices.RevokeTVDBCrossRefWebCache(xref.CrossRef_AniDB_TvDBV2ID);
                    if (string.IsNullOrEmpty(res))
                        xref.IsAdminApproved = 0;
                    else
                        MessageBox.Show(res, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);

                }
                if (obj.GetType() == typeof(VM_CrossRef_AniDB_TraktV2))
                {
                    VM_CrossRef_AniDB_TraktV2 xref = obj as VM_CrossRef_AniDB_TraktV2;

                    string res = VM_ShokoServer.Instance.ShokoServices.RevokeTraktCrossRefWebCache(xref.CrossRef_AniDB_TraktV2ID);
                    if (string.IsNullOrEmpty(res))
                        xref.IsAdminApproved = 0;
                    else
                        MessageBox.Show(res, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }

    public class SearchTvDBResults
    {
        public string ErrorMessage { get; set; }
        public List<VM_CrossRef_AniDB_TvDBV2> TvDBLinks { get; set; }
        public string ExtraInfo { get; set; }

        public SearchTvDBResults()
        {

        }
    }

    public class SearchTraktResults
    {
        public string ErrorMessage { get; set; }
        public List<VM_CrossRef_AniDB_TraktV2> TraktLinks { get; set; }
        public string ExtraInfo { get; set; }

        public SearchTraktResults()
        {

        }
    }

    public class SearchCriteria
    {
        public int AnimeID { get; set; }
        public string ExtraInfo { get; set; }

        public SearchCriteria()
        {

        }
    }
}

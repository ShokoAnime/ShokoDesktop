using JMMClient.ViewModel;
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

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for CommunityLinksControl.xaml
    /// </summary>
    public partial class CommunityLinksControl : UserControl
    {
        BackgroundWorker searchTvDBWorker = new BackgroundWorker();
        BackgroundWorker searchTraktWorker = new BackgroundWorker();

        public ObservableCollection<CrossRef_AniDB_TvDBVMV2> TVDBResults { get; set; }
        public ICollectionView ViewTVDBResults { get; set; }


        public ObservableCollection<CrossRef_AniDB_TraktVMV2> TraktResults { get; set; }
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

            TVDBResults = new ObservableCollection<CrossRef_AniDB_TvDBVMV2>();
            ViewTVDBResults = CollectionViewSource.GetDefaultView(TVDBResults);

            TraktResults = new ObservableCollection<CrossRef_AniDB_TraktVMV2>();
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
            CrossRef_AniDB_TvDBVMV2 xref = ((DataGrid)sender).SelectedItem as CrossRef_AniDB_TvDBVMV2;
            if (xref == null) return;

            ShowAniDBWebPage(xref.AniDBURL);
            ShowOtherWebPage(xref.SeriesURL);
        }

        void dgTraktResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            CrossRef_AniDB_TraktVMV2 xref = ((DataGrid)sender).SelectedItem as CrossRef_AniDB_TraktVMV2;
            if (xref == null) return;

            ShowAniDBWebPage(xref.AniDBURL);
            ShowOtherWebPage(xref.ShowURL);
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
            res.TvDBLinks = new List<CrossRef_AniDB_TvDBVMV2>();
            res.ExtraInfo = string.Empty;

            try
            {
                SearchCriteria crit = e.Argument as SearchCriteria;
                if (!string.IsNullOrEmpty(crit.ExtraInfo))
                    res.ExtraInfo = crit.ExtraInfo;

                try
                {
                    List<JMMServerBinary.Contract_Azure_CrossRef_AniDB_TvDB> xrefs = JMMServerVM.Instance.clientBinaryHTTP.GetTVDBCrossRefWebCache(crit.AnimeID, true);
                    if (xrefs != null && xrefs.Count > 0)
                    {
                        foreach (JMMServerBinary.Contract_Azure_CrossRef_AniDB_TvDB xref in xrefs)
                        {
                            CrossRef_AniDB_TvDBVMV2 xrefAzure = new CrossRef_AniDB_TvDBVMV2(xref);
                            res.TvDBLinks.Add(xrefAzure);
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
            if (!JMMServerVM.Instance.ServerOnline) return;

            this.Cursor = Cursors.Wait;
            SearchStatus = string.Format(Properties.Resources.Community_Searching);

            txtSearch.Text = crit.AnimeID.ToString();

            try
            {
                TVDBResults.Clear();
                btnRandomAnime.IsEnabled = false;
                btnSearch.IsEnabled = false;

                this.Cursor = Cursors.Wait;

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

                foreach (CrossRef_AniDB_TvDBVMV2 tvxref in res.TvDBLinks)
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
                this.Cursor = Cursors.Arrow;
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
            res.TraktLinks = new List<CrossRef_AniDB_TraktVMV2>();
            res.ExtraInfo = string.Empty;

            try
            {
                SearchCriteria crit = e.Argument as SearchCriteria;
                if (!string.IsNullOrEmpty(crit.ExtraInfo))
                    res.ExtraInfo = crit.ExtraInfo;

                try
                {
                    List<JMMServerBinary.Contract_Azure_CrossRef_AniDB_Trakt> xrefs = JMMServerVM.Instance.clientBinaryHTTP.GetTraktCrossRefWebCache(crit.AnimeID, true);
                    if (xrefs != null && xrefs.Count > 0)
                    {
                        foreach (JMMServerBinary.Contract_Azure_CrossRef_AniDB_Trakt xref in xrefs)
                        {
                            CrossRef_AniDB_TraktVMV2 xrefAzure = new CrossRef_AniDB_TraktVMV2(xref);
                            res.TraktLinks.Add(xrefAzure);
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
            if (!JMMServerVM.Instance.ServerOnline) return;

            this.Cursor = Cursors.Wait;
            SearchStatus = string.Format(Properties.Resources.Community_Searching);

            txtSearch.Text = crit.AnimeID.ToString();

            try
            {
                TraktResults.Clear();
                btnRandomAnime.IsEnabled = false;
                btnSearch.IsEnabled = false;

                this.Cursor = Cursors.Wait;

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

                foreach (CrossRef_AniDB_TraktVMV2 tvxref in res.TraktLinks)
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
                this.Cursor = Cursors.Arrow;
                btnRandomAnime.IsEnabled = true;
                btnSearch.IsEnabled = true;
            }
        }

        #endregion

        void btnRandomAnime_Click(object sender, RoutedEventArgs e)
        {
            if (!JMMServerVM.Instance.ServerOnline) return;

            SearchStatus = Properties.Resources.Community_Searching;

            btnRandomAnime.IsEnabled = false;
            btnSearch.IsEnabled = false;

            this.Cursor = Cursors.Wait;
            try
            {
                if (tabResults.SelectedIndex == 0)
                {
                    TVDBResults.Clear();

                    JMMServerBinary.Contract_Azure_AnimeLink contract = JMMServerVM.Instance.clientBinaryHTTP.Admin_GetRandomLinkForApproval((int)AzureLinkType.TvDB);
                    if (contract != null)
                    {
                        AzureAnimeLink link = new AzureAnimeLink(contract);
                        SearchCriteria crit = new SearchCriteria();
                        crit.AnimeID = link.RandomAnimeID;
                        crit.ExtraInfo = string.Format(Properties.Resources.Community_TvDBApproval, link.AnimeNeedingApproval);

                        PerformTvDBSearch(crit);
                    }
                }
                else if (tabResults.SelectedIndex == 1)
                {
                    TraktResults.Clear();

                    JMMServerBinary.Contract_Azure_AnimeLink contract = JMMServerVM.Instance.clientBinaryHTTP.Admin_GetRandomLinkForApproval((int)AzureLinkType.Trakt);
                    if (contract != null)
                    {
                        AzureAnimeLink link = new AzureAnimeLink(contract);
                        SearchCriteria crit = new SearchCriteria();
                        crit.AnimeID = link.RandomAnimeID;
                        crit.ExtraInfo = string.Format(Properties.Resources.Community_TraktApproval, link.AnimeNeedingApproval);

                        PerformTraktSearch(crit);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Arrow;
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
                if (obj.GetType() == typeof(CrossRef_AniDB_TvDBVMV2))
                {
                    CrossRef_AniDB_TvDBVMV2 xref = obj as CrossRef_AniDB_TvDBVMV2;

                    string res = JMMServerVM.Instance.clientBinaryHTTP.ApproveTVDBCrossRefWebCache(xref.CrossRef_AniDB_TvDBV2ID);
                    if (string.IsNullOrEmpty(res))
                        xref.IsAdminApproved = 1;
                    else
                        MessageBox.Show(res, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);

                }
                if (obj.GetType() == typeof(CrossRef_AniDB_TraktVMV2))
                {
                    CrossRef_AniDB_TraktVMV2 xref = obj as CrossRef_AniDB_TraktVMV2;

                    string res = JMMServerVM.Instance.clientBinaryHTTP.ApproveTraktCrossRefWebCache(xref.CrossRef_AniDB_TraktV2ID);
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
                if (obj.GetType() == typeof(CrossRef_AniDB_TvDBVMV2))
                {
                    CrossRef_AniDB_TvDBVMV2 xref = obj as CrossRef_AniDB_TvDBVMV2;

                    string res = JMMServerVM.Instance.clientBinaryHTTP.RevokeTVDBCrossRefWebCache(xref.CrossRef_AniDB_TvDBV2ID);
                    if (string.IsNullOrEmpty(res))
                        xref.IsAdminApproved = 0;
                    else
                        MessageBox.Show(res, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);

                }
                if (obj.GetType() == typeof(CrossRef_AniDB_TraktVMV2))
                {
                    CrossRef_AniDB_TraktVMV2 xref = obj as CrossRef_AniDB_TraktVMV2;

                    string res = JMMServerVM.Instance.clientBinaryHTTP.RevokeTraktCrossRefWebCache(xref.CrossRef_AniDB_TraktV2ID);
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
        public List<CrossRef_AniDB_TvDBVMV2> TvDBLinks { get; set; }
        public string ExtraInfo { get; set; }

        public SearchTvDBResults()
        {

        }
    }

    public class SearchTraktResults
    {
        public string ErrorMessage { get; set; }
        public List<CrossRef_AniDB_TraktVMV2> TraktLinks { get; set; }
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

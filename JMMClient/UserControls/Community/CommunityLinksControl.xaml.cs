using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JMMClient.ViewModel;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for CommunityLinksControl.xaml
    /// </summary>
    public partial class CommunityLinksControl : UserControl
    {
        BackgroundWorker searchWorker = new BackgroundWorker();

        public ObservableCollection<CrossRef_AniDB_TvDBVMV2> TVDBResults { get; set; }
        public ICollectionView ViewTVDBResults { get; set; }

        public static readonly DependencyProperty SearchStatusProperty = DependencyProperty.Register("SearchStatus",
            typeof(string), typeof(CommunityLinksControl), new UIPropertyMetadata("", null));

        public string SearchStatus
        {
            get { return (string)GetValue(SearchStatusProperty); }
            set { SetValue(SearchStatusProperty, value); }
        }

        public CommunityLinksControl()
        {
            InitializeComponent();

            TVDBResults = new ObservableCollection<CrossRef_AniDB_TvDBVMV2>();
            ViewTVDBResults = CollectionViewSource.GetDefaultView(TVDBResults);

            ViewTVDBResults.SortDescriptions.Add(new SortDescription("IsAdminApproved", ListSortDirection.Descending));
            ViewTVDBResults.SortDescriptions.Add(new SortDescription("Username", ListSortDirection.Ascending));

            ViewTVDBResults.SortDescriptions.Add(new SortDescription("TvDBID", ListSortDirection.Ascending));
            ViewTVDBResults.SortDescriptions.Add(new SortDescription("TvDBSeasonNumber", ListSortDirection.Ascending));
            ViewTVDBResults.SortDescriptions.Add(new SortDescription("TvDBStartEpisodeNumber", ListSortDirection.Ascending));

            btnSearch.Click += btnSearch_Click;
            btnRandomAnime.Click += btnRandomAnime_Click;

            searchWorker.DoWork += new DoWorkEventHandler(searchWorker_DoWork);
            searchWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(searchWorker_RunWorkerCompleted);
        }

        void searchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            SearchResults res = new SearchResults();
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

        public void PerformSearch(SearchCriteria crit)
        {
            if (!JMMServerVM.Instance.ServerOnline) return;

            this.Cursor = Cursors.Wait;
            SearchStatus = string.Format("Searching...");

            try
            {
                TVDBResults.Clear();
                btnRandomAnime.IsEnabled = false;
                btnSearch.IsEnabled = false;

                this.Cursor = Cursors.Wait;

                searchWorker.RunWorkerAsync(crit);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void searchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                SearchResults res = e.Result as SearchResults;
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

        void btnRandomAnime_Click(object sender, RoutedEventArgs e)
        {
            if (!JMMServerVM.Instance.ServerOnline) return;

            SearchStatus = "Searching...";

            btnRandomAnime.IsEnabled = false;
            btnSearch.IsEnabled = false;

            this.Cursor = Cursors.Wait;
            try
            {
                TVDBResults.Clear();
                
                JMMServerBinary.Contract_Azure_AnimeLink contract = JMMServerVM.Instance.clientBinaryHTTP.Admin_GetRandomLinkForApproval((int)AzureLinkType.TvDB);
                if (contract != null)
                {
                    AzureAnimeLink link = new AzureAnimeLink(contract);
                    SearchCriteria crit = new SearchCriteria();
                    crit.AnimeID = link.RandomAnimeID;
                    crit.ExtraInfo = string.Format("{0} Anime still need TvDB approval", link.AnimeNeedingApproval);

                    PerformSearch(crit); 
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

            PerformSearch(crit);
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
                        MessageBox.Show(res, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    
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
                        MessageBox.Show(res, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }

    public class SearchResults
    {
        public string ErrorMessage { get; set; }
        public List<CrossRef_AniDB_TvDBVMV2> TvDBLinks { get; set; }
        public string ExtraInfo { get; set; }

        public SearchResults()
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

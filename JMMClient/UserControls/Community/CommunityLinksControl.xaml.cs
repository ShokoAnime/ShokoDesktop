using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
        public ObservableCollection<CrossRef_AniDB_TvDBVMV2> TVDBResults { get; set; }
        public ICollectionView ViewTVDBResults { get; set; }

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
        }

        void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!JMMServerVM.Instance.ServerOnline) return;

            if (string.IsNullOrEmpty(txtSearch.Text)) return;

            int animeID = 0;
            if (!int.TryParse(txtSearch.Text, out animeID)) return;

            this.Cursor = Cursors.Wait;
            try
            {
                TVDBResults.Clear();

                List<JMMServerBinary.Contract_Azure_CrossRef_AniDB_TvDB> xrefs = JMMServerVM.Instance.clientBinaryHTTP.GetTVDBCrossRefWebCache(animeID, true);
                if (xrefs != null && xrefs.Count > 0)
                {
                    foreach (JMMServerBinary.Contract_Azure_CrossRef_AniDB_TvDB xref in xrefs)
                    {
                        CrossRef_AniDB_TvDBVMV2 xrefAzure = new CrossRef_AniDB_TvDBVMV2(xref);
                        TVDBResults.Add(xrefAzure);
                    }

                    //HasWebCacheRec = true;
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
}

using JMMClient.Forms;
using JMMClient.ViewModel;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for TvDBAndOtherLinks.xaml
    /// </summary>
    public partial class TvDBAndOtherLinks : UserControl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        BackgroundWorker communityWorker = new BackgroundWorker();

        private ContextMenu commTvDBMenu;
        private ContextMenu commTraktMenu;

        public static readonly DependencyProperty AniDB_AnimeCrossRefsProperty = DependencyProperty.Register("AniDB_AnimeCrossRefs",
            typeof(AniDB_AnimeCrossRefsVM), typeof(TvDBAndOtherLinks), new UIPropertyMetadata(null, null));

        public AniDB_AnimeCrossRefsVM AniDB_AnimeCrossRefs
        {
            get { return (AniDB_AnimeCrossRefsVM)GetValue(AniDB_AnimeCrossRefsProperty); }
            set { SetValue(AniDB_AnimeCrossRefsProperty, value); }
        }


        public static readonly DependencyProperty CommTvDBButtonTextProperty = DependencyProperty.Register("CommTvDBButtonText",
            typeof(string), typeof(TvDBAndOtherLinks), new UIPropertyMetadata("", null));

        public string CommTvDBButtonText
        {
            get { return (string)GetValue(CommTvDBButtonTextProperty); }
            set { SetValue(CommTvDBButtonTextProperty, value); }
        }

        public static readonly DependencyProperty CommTraktButtonTextProperty = DependencyProperty.Register("CommTraktButtonText",
            typeof(string), typeof(TvDBAndOtherLinks), new UIPropertyMetadata("", null));

        public string CommTraktButtonText
        {
            get { return (string)GetValue(CommTraktButtonTextProperty); }
            set { SetValue(CommTraktButtonTextProperty, value); }
        }


        public ObservableCollection<CrossRef_AniDB_TvDBVMV2> CommunityTVDBLinks { get; set; }
        public ICollectionView ViewCommunityTVDBLinks { get; set; }

        public ObservableCollection<CrossRef_AniDB_TraktVMV2> CommunityTraktLinks { get; set; }
        public ICollectionView ViewCommunityTraktLinks { get; set; }

        public TvDBAndOtherLinks()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            commTvDBMenu = new ContextMenu();
            commTraktMenu = new ContextMenu();

            btnTvDBCommLinks1.ContextMenu = commTvDBMenu;
            btnTvDBCommLinks2.ContextMenu = commTvDBMenu;
            btnTvDBCommLinks1.Click += new RoutedEventHandler(btnTvDBCommLinks_Click);
            btnTvDBCommLinks2.Click += new RoutedEventHandler(btnTvDBCommLinks_Click);

            btnTraktCommLinks1.ContextMenu = commTraktMenu;
            btnTraktCommLinks2.ContextMenu = commTraktMenu;
            btnTraktCommLinks1.Click += new RoutedEventHandler(btnTraktCommLinks_Click);
            btnTraktCommLinks2.Click += new RoutedEventHandler(btnTraktCommLinks_Click);

            CommunityTVDBLinks = new ObservableCollection<CrossRef_AniDB_TvDBVMV2>();
            CommunityTraktLinks = new ObservableCollection<CrossRef_AniDB_TraktVMV2>();

            ViewCommunityTVDBLinks = CollectionViewSource.GetDefaultView(CommunityTVDBLinks);
            ViewCommunityTraktLinks = CollectionViewSource.GetDefaultView(CommunityTraktLinks);

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(TvDBAndOtherLinks_DataContextChanged);

            btnSearchTvDB.Click += new RoutedEventHandler(btnSearch_Click);
            btnSearchExistingTvDB.Click += new RoutedEventHandler(btnSearchExisting_Click);

            btnSearchExistingMovieDB.Click += new RoutedEventHandler(btnSearchExistingMovieDB_Click);
            btnSearchMovieDB.Click += new RoutedEventHandler(btnSearchMovieDB_Click);
            btnDeleteMovieDBLink.Click += new RoutedEventHandler(btnDeleteMovieDBLink_Click);
            btnUpdateMovieDBInfo.Click += btnUpdateMovieDBInfo_Click;

            btnSearchExistingTrakt.Click += new RoutedEventHandler(btnSearchExistingTrakt_Click);
            btnSearchTrakt.Click += new RoutedEventHandler(btnSearchTrakt_Click);

            btnSearchExistingMAL.Click += new RoutedEventHandler(btnSearchExistingMAL_Click);
            btnSearchMAL.Click += new RoutedEventHandler(btnSearchMAL_Click);

            communityWorker.WorkerSupportsCancellation = false;
            communityWorker.WorkerReportsProgress = true;
            communityWorker.DoWork += new DoWorkEventHandler(communityWorker_DoWork);
            communityWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(communityWorker_RunWorkerCompleted);
        }

        void btnTvDBCommLinks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CommTvDBTraktMenuCommand cmd = null;

                // get all playlists
                commTvDBMenu.Items.Clear();

                MenuItem itemSeriesAdmin = new MenuItem();
                itemSeriesAdmin.Header = Properties.Resources.CommunityLinks_ShowAdmin;
                itemSeriesAdmin.Click += new RoutedEventHandler(commTvDBMenuItem_Click);
                cmd = new CommTvDBTraktMenuCommand(CommTvDBTraktItemType.ShowCommAdmin, -1); // new playlist
                itemSeriesAdmin.CommandParameter = cmd;
                commTvDBMenu.Items.Add(itemSeriesAdmin);

                if (AniDB_AnimeCrossRefs.TvDBCrossRefExists)
                {
                    MenuItem itemSeriesLinks = new MenuItem();
                    itemSeriesLinks.Header = Properties.Resources.CommunityLins_UseMyLinks;
                    itemSeriesLinks.Click += new RoutedEventHandler(commTvDBMenuItem_Click);
                    cmd = new CommTvDBTraktMenuCommand(CommTvDBTraktItemType.UseMyLinks, -1); // new playlist
                    itemSeriesLinks.CommandParameter = cmd;
                    commTvDBMenu.Items.Add(itemSeriesLinks);
                }

                commTvDBMenu.PlacementTarget = this;
                commTvDBMenu.IsOpen = true;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void commTvDBMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem item = e.Source as MenuItem;
                MenuItem itemSender = sender as MenuItem;

                if (item == null || itemSender == null) return;
                if (!item.Header.ToString().Equals(itemSender.Header.ToString())) return;

                if (item != null && item.CommandParameter != null)
                {
                    CommTvDBTraktMenuCommand cmd = item.CommandParameter as CommTvDBTraktMenuCommand;
                    Debug.Write("Comm TvDB Menu: " + cmd.ToString() + Environment.NewLine);

                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;


                    this.Cursor = Cursors.Wait;

                    if (cmd.MenuType == CommTvDBTraktItemType.ShowCommAdmin)
                    {
                        MainWindow mainwdw = (MainWindow)Window.GetWindow(this);
                        if (mainwdw == null) return;
                        mainwdw.ShowWebCacheAdmin(anime);
                    }

                    if (cmd.MenuType == CommTvDBTraktItemType.UseMyLinks)
                    {
                        if (!AniDB_AnimeCrossRefs.TvDBCrossRefExists)
                        {
                            MessageBox.Show(Properties.Resources.CommunityLinks_NoLinks, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        string res = JMMServerVM.Instance.clientBinaryHTTP.UseMyTvDBLinksWebCache(anime.AnimeID);
                        this.Cursor = Cursors.Arrow;
                        MessageBox.Show(res, Properties.Resources.Result, MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    this.Cursor = Cursors.Arrow;

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }


        void btnTraktCommLinks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CommTvDBTraktMenuCommand cmd = null;

                // get all playlists
                commTraktMenu.Items.Clear();

                MenuItem itemSeriesAdmin = new MenuItem();
                itemSeriesAdmin.Header = Properties.Resources.CommunityLinks_ShowAdmin;
                itemSeriesAdmin.Click += new RoutedEventHandler(commTraktMenuItem_Click);
                cmd = new CommTvDBTraktMenuCommand(CommTvDBTraktItemType.ShowCommAdmin, -1); // new playlist
                itemSeriesAdmin.CommandParameter = cmd;
                commTraktMenu.Items.Add(itemSeriesAdmin);

                if (AniDB_AnimeCrossRefs.TraktCrossRefExists)
                {
                    MenuItem itemSeriesLinks = new MenuItem();
                    itemSeriesLinks.Header = Properties.Resources.CommunityLins_UseMyLinks;
                    itemSeriesLinks.Click += new RoutedEventHandler(commTraktMenuItem_Click);
                    cmd = new CommTvDBTraktMenuCommand(CommTvDBTraktItemType.UseMyLinks, -1); // new playlist
                    itemSeriesLinks.CommandParameter = cmd;
                    commTraktMenu.Items.Add(itemSeriesLinks);
                }

                commTraktMenu.PlacementTarget = this;
                commTraktMenu.IsOpen = true;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void commTraktMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem item = e.Source as MenuItem;
                MenuItem itemSender = sender as MenuItem;

                if (item == null || itemSender == null) return;
                if (!item.Header.ToString().Equals(itemSender.Header.ToString())) return;

                if (item != null && item.CommandParameter != null)
                {
                    CommTvDBTraktMenuCommand cmd = item.CommandParameter as CommTvDBTraktMenuCommand;
                    Debug.Write("Comm TvDB Menu: " + cmd.ToString() + Environment.NewLine);

                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;


                    this.Cursor = Cursors.Wait;

                    if (cmd.MenuType == CommTvDBTraktItemType.ShowCommAdmin)
                    {
                        MainWindow mainwdw = (MainWindow)Window.GetWindow(this);
                        if (mainwdw == null) return;
                        mainwdw.ShowWebCacheAdmin(anime);
                    }

                    if (cmd.MenuType == CommTvDBTraktItemType.UseMyLinks)
                    {
                        if (!AniDB_AnimeCrossRefs.TraktCrossRefExists)
                        {
                            MessageBox.Show(Properties.Resources.CommunityLinks_NoLinks, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        string res = JMMServerVM.Instance.clientBinaryHTTP.UseMyTraktLinksWebCache(anime.AnimeID);
                        this.Cursor = Cursors.Arrow;
                        MessageBox.Show(res, Properties.Resources.Result, MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    this.Cursor = Cursors.Arrow;

                    RefreshAdminData();

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void communityWorker_DoWork(object sender, DoWorkEventArgs e)
        {



            SearchCommunityResults res = new SearchCommunityResults();
            res.ErrorMessage = string.Empty;
            res.TvDBLinks = new List<CrossRef_AniDB_TvDBVMV2>();
            res.TraktLinks = new List<CrossRef_AniDB_TraktVMV2>();
            res.ExtraInfo = string.Empty;
            res.AnimeID = -1;

            try
            {
                AniDB_AnimeVM anime = e.Argument as AniDB_AnimeVM;
                if (anime == null) return;

                res.AnimeID = anime.AnimeID;

                SearchCriteria crit = new SearchCriteria();
                crit.AnimeID = anime.AnimeID;
                crit.ExtraInfo = string.Empty;

                // search for TvDB links
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

                // search for Trakt links
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

        void communityWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                SearchCommunityResults res = e.Result as SearchCommunityResults;
                if (res == null) return;

                AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                if (anime == null) return;

                if (anime.AnimeID != res.AnimeID) return;

                if (!string.IsNullOrEmpty(res.ErrorMessage))
                {
                    return;
                }

                foreach (CrossRef_AniDB_TvDBVMV2 tvxref in res.TvDBLinks)
                    CommunityTVDBLinks.Add(tvxref);

                foreach (CrossRef_AniDB_TraktVMV2 traktxref in res.TraktLinks)
                    CommunityTraktLinks.Add(traktxref);

                btnTvDBCommLinks1.IsEnabled = true;
                btnTvDBCommLinks2.IsEnabled = true;

                CommTvDBButtonText = Properties.Resources.CommunityLinks_NoLinksAvailable;
                if (CommunityTVDBLinks.Count > 0)
                {
                    CommTvDBButtonText = Properties.Resources.CommunityLinks_NeedApproval;
                    foreach (CrossRef_AniDB_TvDBVMV2 xref in CommunityTVDBLinks)
                    {
                        if (xref.IsAdminApprovedBool)
                        {
                            CommTvDBButtonText = Properties.Resources.CommunityLinks_ApprovalExists;
                            break;
                        }
                    }
                }

                btnTraktCommLinks1.IsEnabled = true;
                btnTraktCommLinks2.IsEnabled = true;

                CommTraktButtonText = Properties.Resources.CommunityLinks_NoLinksAvailable;
                if (CommunityTraktLinks.Count > 0)
                {
                    CommTraktButtonText = Properties.Resources.CommunityLinks_NeedApproval;
                    foreach (CrossRef_AniDB_TraktVMV2 xref in CommunityTraktLinks)
                    {
                        if (xref.IsAdminApprovedBool)
                        {
                            CommTraktButtonText = Properties.Resources.CommunityLinks_ApprovalExists;
                            break;
                        }
                    }
                }

                //SearchStatus = string.Format("{0} Anime still need TvDB approval", link.AnimeNeedingApproval);

            }
            catch (Exception ex)
            {
                logger.ErrorException(ex.ToString(), ex);
            }
        }





        #region MAL

        void btnSearchMAL_Click(object sender, RoutedEventArgs e)
        {
            SearchMAL();
        }

        void btnSearchExistingMAL_Click(object sender, RoutedEventArgs e)
        {
            SearchMAL();
        }

        private void SearchMAL()
        {
            try
            {
                AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                if (anime == null) return;

                Window wdw = Window.GetWindow(this);

                this.Cursor = Cursors.Wait;
                SearchMALForm frm = new SearchMALForm();
                frm.Owner = wdw;
                frm.Init(anime.AnimeID, anime.FormattedTitle, anime.MainTitle);
                bool? result = frm.ShowDialog();
                if (result.Value)
                {
                    // update info
                    RefreshData();
                }

                this.Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnEditMALDetails_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // prompt to select details
                Window wdw = Window.GetWindow(this);

                this.Cursor = Cursors.Wait;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_EditMALLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                if (anime == null) return;

                if (obj.GetType() == typeof(CrossRef_AniDB_MALVM))
                {
                    this.Cursor = Cursors.Wait;
                    CrossRef_AniDB_MALVM malLink = obj as CrossRef_AniDB_MALVM;

                    // prompt to select details
                    Window wdw = Window.GetWindow(this);

                    SelectMALStartForm frm = new SelectMALStartForm();
                    frm.Owner = wdw;
                    frm.Init(malLink.AnimeID, anime.FormattedTitle, malLink.MALTitle, malLink.MALID, malLink.StartEpisodeType, malLink.StartEpisodeNumber);
                    bool? result = frm.ShowDialog();
                    if (result.Value)
                    {
                        // update info
                        RefreshData();
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

        private void CommandBinding_DeleteMALLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                if (anime == null) return;

                if (obj.GetType() == typeof(CrossRef_AniDB_MALVM))
                {
                    this.Cursor = Cursors.Wait;
                    CrossRef_AniDB_MALVM malLink = obj as CrossRef_AniDB_MALVM;

                    // prompt to select details
                    Window wdw = Window.GetWindow(this);

                    string msg = string.Format(Properties.Resources.CommunityLinks_DeleteLink);
                    MessageBoxResult result = MessageBox.Show(msg, Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        this.Cursor = Cursors.Wait;

                        string res = JMMServerVM.Instance.clientBinaryHTTP.RemoveLinkAniDBMAL(anime.AnimeID, malLink.StartEpisodeType, malLink.StartEpisodeNumber);
                        if (res.Length > 0)
                            MessageBox.Show(res, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                        {
                            // update info
                            RefreshData();
                        }

                        this.Cursor = Cursors.Arrow;
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





        private void CommandBinding_ToggleAutoLinkMovieDB(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                if (anime == null) return;

                this.Cursor = Cursors.Wait;

                anime.IsMovieDBLinkDisabled = !anime.IsMovieDBLinkDisabled;
                anime.UpdateDisableExternalLinksFlag();
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

        private void CommandBinding_ToggleAutoLinkMAL(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                if (anime == null) return;

                this.Cursor = Cursors.Wait;

                anime.IsMALLinkDisabled = !anime.IsMALLinkDisabled;
                anime.UpdateDisableExternalLinksFlag();
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

        #endregion

        #region Trakt

        private void CommandBinding_ReportTraktLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(CrossRef_AniDB_TvDBVMV2))
                {
                    CrossRef_AniDB_TraktVMV2 link = obj as CrossRef_AniDB_TraktVMV2;

                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;

                    this.Cursor = Cursors.Wait;

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

        private void CommandBinding_DeleteTraktLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(CrossRef_AniDB_TraktVMV2))
                {
                    CrossRef_AniDB_TraktVMV2 link = obj as CrossRef_AniDB_TraktVMV2;

                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;

                    this.Cursor = Cursors.Wait;

                    Window wdw = Window.GetWindow(this);

                    string msg = string.Format(Properties.Resources.CommunityLinks_DeleteLink);
                    MessageBoxResult result = MessageBox.Show(msg, Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        this.Cursor = Cursors.Wait;
                        string res = JMMServerVM.Instance.clientBinaryHTTP.RemoveLinkAniDBTrakt(link.AnimeID, link.AniDBStartEpisodeType, link.AniDBStartEpisodeNumber,
                            link.TraktID, link.TraktSeasonNumber, link.TraktStartEpisodeNumber);
                        if (res.Length > 0)
                            MessageBox.Show(res, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                        {
                            // update info
                            RefreshData();
                        }

                        this.Cursor = Cursors.Arrow;
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

        private void CommandBinding_EditTraktLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(CrossRef_AniDB_TraktVMV2))
                {
                    CrossRef_AniDB_TraktVMV2 link = obj as CrossRef_AniDB_TraktVMV2;

                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;

                    Window wdw = Window.GetWindow(this);

                    this.Cursor = Cursors.Wait;
                    SelectTraktSeasonForm frm = new SelectTraktSeasonForm();
                    frm.Owner = wdw;
                    frm.Init(anime.AnimeID, anime.FormattedTitle, (EpisodeType)link.AniDBStartEpisodeType, link.AniDBStartEpisodeNumber, link.TraktID,
                        link.TraktSeasonNumber, link.TraktStartEpisodeNumber, link.TraktTitle, anime, link.CrossRef_AniDB_TraktV2ID);
                    bool? result = frm.ShowDialog();
                    if (result.Value)
                    {
                        // update info
                        RefreshData();
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

        private void CommandBinding_UpdateTraktLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(CrossRef_AniDB_TraktVMV2))
                {
                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;

                    this.Cursor = Cursors.Wait;

                    foreach (CrossRef_AniDB_TraktVMV2 xref in AniDB_AnimeCrossRefs.CrossRef_AniDB_TraktV2)
                        JMMServerVM.Instance.clientBinaryHTTP.UpdateTraktData(xref.TraktID);

                    anime.ClearTraktData();

                    // find the series for this anime
                    foreach (AnimeSeriesVM ser in MainListHelperVM.Instance.AllSeries)
                    {
                        if (anime.AnimeID == ser.AniDB_ID)
                        {
                            MainListHelperVM.Instance.UpdateHeirarchy(ser);
                            break;
                        }
                    }

                    RefreshData();
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

        private void CommandBinding_SyncTraktLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(CrossRef_AniDB_TraktVMV2))
                {
                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;

                    string response = JMMServerVM.Instance.clientBinaryHTTP.SyncTraktSeries(anime.AnimeID);
                    if (!string.IsNullOrEmpty(response))
                        MessageBox.Show(response, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        MessageBox.Show(Properties.Resources.CommunityLinks_CommandQueued, Properties.Resources.Done, MessageBoxButton.OK, MessageBoxImage.Information);

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

        private void CommandBinding_ToggleAutoLinkTrakt(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                if (anime == null) return;

                this.Cursor = Cursors.Wait;

                anime.IsTraktLinkDisabled = !anime.IsTraktLinkDisabled;
                anime.UpdateDisableExternalLinksFlag();
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

        void btnSearchTrakt_Click(object sender, RoutedEventArgs e)
        {
            SearchTrakt("");
        }

        void btnSearchExistingTrakt_Click(object sender, RoutedEventArgs e)
        {
            SearchTrakt("");
        }

        private void SearchTrakt(string ExistingTraktID)
        {
            try
            {
                AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                if (anime == null) return;

                Window wdw = Window.GetWindow(this);

                this.Cursor = Cursors.Wait;
                SearchTraktForm frm = new SearchTraktForm();
                frm.Owner = wdw;
                frm.Init(anime.AnimeID, anime.FormattedTitle, anime.FormattedTitle, ExistingTraktID, anime);
                bool? result = frm.ShowDialog();
                if (result.Value)
                {
                    // update info
                    RefreshData();
                }

                this.Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        #endregion

        #region TvDB

        void btnSearchExisting_Click(object sender, RoutedEventArgs e)
        {
            SearchTvDB(null);
        }

        void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTvDB(null);
        }

        private void SearchTvDB(int? ExistingSeriesID)
        {
            try
            {
                AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                if (anime == null) return;

                Window wdw = Window.GetWindow(this);

                this.Cursor = Cursors.Wait;
                SearchTvDBForm frm = new SearchTvDBForm();
                frm.Owner = wdw;
                frm.Init(anime.AnimeID, anime.FormattedTitle, anime.FormattedTitle, ExistingSeriesID, anime);
                bool? result = frm.ShowDialog();
                if (result.Value)
                {
                    // update info
                    RefreshData();
                }

                this.Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_ToggleAutoLinkTvDB(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                if (anime == null) return;

                this.Cursor = Cursors.Wait;

                anime.IsTvDBLinkDisabled = !anime.IsTvDBLinkDisabled;
                anime.UpdateDisableExternalLinksFlag();
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

        private void CommandBinding_ReportTvDBLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(CrossRef_AniDB_TvDBVMV2))
                {
                    CrossRef_AniDB_TvDBVMV2 link = obj as CrossRef_AniDB_TvDBVMV2;

                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;

                    this.Cursor = Cursors.Wait;

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

        private void CommandBinding_EditTvDBLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(CrossRef_AniDB_TvDBVMV2))
                {
                    CrossRef_AniDB_TvDBVMV2 link = obj as CrossRef_AniDB_TvDBVMV2;

                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;

                    Window wdw = Window.GetWindow(this);

                    this.Cursor = Cursors.Wait;
                    SelectTvDBSeasonForm frm = new SelectTvDBSeasonForm();
                    frm.Owner = wdw;
                    //TODO
                    frm.Init(anime.AnimeID, anime.FormattedTitle, (EpisodeType)link.AniDBStartEpisodeType, link.AniDBStartEpisodeNumber, link.TvDBID,
                        link.TvDBSeasonNumber, link.TvDBStartEpisodeNumber, link.TvDBTitle, anime, link.CrossRef_AniDB_TvDBV2ID);
                    bool? result = frm.ShowDialog();
                    if (result.Value)
                    {
                        // update info
                        RefreshData();
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

        private void CommandBinding_DeleteTvDBLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(CrossRef_AniDB_TvDBVMV2))
                {
                    CrossRef_AniDB_TvDBVMV2 link = obj as CrossRef_AniDB_TvDBVMV2;

                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;

                    this.Cursor = Cursors.Wait;

                    Window wdw = Window.GetWindow(this);

                    string msg = string.Format(Properties.Resources.CommunityLinks_DeleteLink);
                    MessageBoxResult result = MessageBox.Show(msg, Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        this.Cursor = Cursors.Wait;
                        string res = JMMServerVM.Instance.clientBinaryHTTP.RemoveLinkAniDBTvDB(link.AnimeID, link.AniDBStartEpisodeType, link.AniDBStartEpisodeNumber,
                            link.TvDBID, link.TvDBSeasonNumber, link.TvDBStartEpisodeNumber);
                        if (res.Length > 0)
                            MessageBox.Show(res, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                        {
                            // update info
                            RefreshData();
                        }

                        this.Cursor = Cursors.Arrow;
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

        private void CommandBinding_UpdateTvDBLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(CrossRef_AniDB_TvDBVMV2))
                {
                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;

                    this.Cursor = Cursors.Wait;

                    foreach (CrossRef_AniDB_TvDBVMV2 xref in AniDB_AnimeCrossRefs.CrossRef_AniDB_TvDBV2)
                        JMMServerVM.Instance.clientBinaryHTTP.UpdateTvDBData(xref.TvDBID);

                    anime.ClearTvDBData();

                    // find the series for this anime
                    foreach (AnimeSeriesVM ser in MainListHelperVM.Instance.AllSeries)
                    {
                        if (anime.AnimeID == ser.AniDB_ID)
                        {
                            MainListHelperVM.Instance.UpdateHeirarchy(ser);
                            break;
                        }
                    }

                    RefreshData();
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

        #endregion

        #region MovieDB

        private void SearchMovieDB()
        {
            try
            {
                AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                if (anime == null) return;

                Window wdw = Window.GetWindow(this);

                this.Cursor = Cursors.Wait;
                SearchMovieDBForm frm = new SearchMovieDBForm();
                frm.Owner = wdw;
                frm.Init(anime.AnimeID, anime.FormattedTitle);
                bool? result = frm.ShowDialog();
                if (result.Value)
                {
                    // update info
                    RefreshData();
                }

                this.Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnSearchMovieDB_Click(object sender, RoutedEventArgs e)
        {
            SearchMovieDB();
        }

        void btnSearchExistingMovieDB_Click(object sender, RoutedEventArgs e)
        {
            SearchMovieDB();
        }

        void btnUpdateMovieDBInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                if (anime == null) return;

                Window wdw = Window.GetWindow(this);

                this.Cursor = Cursors.Wait;
                string res = JMMServerVM.Instance.clientBinaryHTTP.UpdateMovieDBData(AniDB_AnimeCrossRefs.MovieDB_Movie.MovieId);
                if (res.Length > 0)
                    MessageBox.Show(res, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                else
                {
                    // update info
                    RefreshData();
                }

                this.Cursor = Cursors.Arrow;
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

        void btnDeleteMovieDBLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                if (anime == null) return;

                Window wdw = Window.GetWindow(this);

                string msg = string.Format(Properties.Resources.CommunityLinks_DeleteLink);
                MessageBoxResult result = MessageBox.Show(msg, Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    this.Cursor = Cursors.Wait;
                    string res = JMMServerVM.Instance.clientBinaryHTTP.RemoveLinkAniDBOther(anime.AnimeID, (int)CrossRefType.MovieDB);
                    if (res.Length > 0)
                        MessageBox.Show(res, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                    {
                        // update info
                        RefreshData();
                    }

                    this.Cursor = Cursors.Arrow;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        #endregion

        void TvDBAndOtherLinks_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (this.DataContext == null)
                {
                    AniDB_AnimeCrossRefs = null;
                    return;
                }

                RefreshData();


            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void RefreshAdminData()
        {
            try
            {
                if (JMMServerVM.Instance.ShowCommunity)
                {
                    AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                    if (anime == null) return;

                    CommunityTVDBLinks.Clear();
                    CommunityTraktLinks.Clear();

                    btnTvDBCommLinks1.IsEnabled = false;
                    btnTvDBCommLinks2.IsEnabled = false;
                    CommTvDBButtonText = Properties.Resources.CommunityLinks_CheckingOnline;

                    btnTraktCommLinks1.IsEnabled = false;
                    btnTraktCommLinks2.IsEnabled = false;
                    CommTraktButtonText = Properties.Resources.CommunityLinks_CheckingOnline;

                    if (!communityWorker.IsBusy)
                        communityWorker.RunWorkerAsync(anime);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void RefreshData()
        {
            try
            {
                AniDB_AnimeCrossRefs = null;

                AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                if (anime == null) return;

                JMMServerBinary.Contract_AniDB_AnimeCrossRefs xrefDetails = JMMServerVM.Instance.clientBinaryHTTP.GetCrossRefDetails(anime.AnimeID);
                if (xrefDetails == null) return;

                AniDB_AnimeCrossRefs = new AniDB_AnimeCrossRefsVM();
                AniDB_AnimeCrossRefs.Populate(xrefDetails);

                MainListHelperVM.Instance.UpdateAnime(anime.AnimeID);

                RefreshAdminData();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }

    public class CommTvDBTraktMenuCommand
    {
        public CommTvDBTraktItemType MenuType { get; set; }
        public int AnimeID { get; set; }

        public CommTvDBTraktMenuCommand()
        {
        }

        public CommTvDBTraktMenuCommand(CommTvDBTraktItemType menuType, int animeID)
        {
            MenuType = menuType;
            AnimeID = animeID;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", MenuType, AnimeID);
        }
    }

    public enum CommTvDBTraktItemType
    {
        ShowCommAdmin = 1,
        UseMyLinks = 2
    }

    public class SearchCommunityResults
    {
        // Use this so we know which Anime we were actually searching for
        // In case the user changes which anime they are looking at
        public int AnimeID { get; set; }

        public string ErrorMessage { get; set; }
        public List<CrossRef_AniDB_TvDBVMV2> TvDBLinks { get; set; }

        public List<CrossRef_AniDB_TraktVMV2> TraktLinks { get; set; }
        public string ExtraInfo { get; set; }

        public SearchCommunityResults()
        {

        }
    }
}

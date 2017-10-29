using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Forms;
using Shoko.Desktop.UserControls.Community;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Azure;
using Shoko.Models.Enums;
using Shoko.Models.Server;

namespace Shoko.Desktop.UserControls
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
            typeof(VM_AniDB_AnimeCrossRefs), typeof(TvDBAndOtherLinks), new UIPropertyMetadata(null, null));

        public VM_AniDB_AnimeCrossRefs AniDB_AnimeCrossRefs
        {
            get { return (VM_AniDB_AnimeCrossRefs)GetValue(AniDB_AnimeCrossRefsProperty); }
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


        public ObservableCollection<VM_CrossRef_AniDB_TvDBV2> CommunityTVDBLinks { get; set; }
        public ICollectionView ViewCommunityTVDBLinks { get; set; }

        public ObservableCollection<VM_CrossRef_AniDB_TraktV2> CommunityTraktLinks { get; set; }
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

            CommunityTVDBLinks = new ObservableCollection<VM_CrossRef_AniDB_TvDBV2>();
            CommunityTraktLinks = new ObservableCollection<VM_CrossRef_AniDB_TraktV2>();

            ViewCommunityTVDBLinks = CollectionViewSource.GetDefaultView(CommunityTVDBLinks);
            ViewCommunityTraktLinks = CollectionViewSource.GetDefaultView(CommunityTraktLinks);

            DataContextChanged += new DependencyPropertyChangedEventHandler(TvDBAndOtherLinks_DataContextChanged);

            btnSearchTvDB.Click += new RoutedEventHandler(btnSearch_Click);
            btnSearchExistingTvDB.Click += new RoutedEventHandler(btnSearchExisting_Click);

            btnSearchExistingMovieDB.Click += new RoutedEventHandler(btnSearchExistingMovieDB_Click);
            btnSearchMovieDB.Click += new RoutedEventHandler(btnSearchMovieDB_Click);

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
                itemSeriesAdmin.Header = Shoko.Commons.Properties.Resources.CommunityLinks_ShowAdmin;
                itemSeriesAdmin.Click += new RoutedEventHandler(commTvDBMenuItem_Click);
                cmd = new CommTvDBTraktMenuCommand(CommTvDBTraktItemType.ShowCommAdmin, -1); // new playlist
                itemSeriesAdmin.CommandParameter = cmd;
                commTvDBMenu.Items.Add(itemSeriesAdmin);

                if (AniDB_AnimeCrossRefs.TvDBCrossRefExists)
                {
                    MenuItem itemSeriesLinks = new MenuItem();
                    itemSeriesLinks.Header = Shoko.Commons.Properties.Resources.CommunityLins_UseMyLinks;
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

                    VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                    if (anime == null) return;


                    Cursor = Cursors.Wait;

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
                            MessageBox.Show(Shoko.Commons.Properties.Resources.CommunityLinks_NoLinks, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        string res = VM_ShokoServer.Instance.ShokoServices.UseMyTvDBLinksWebCache(anime.AnimeID);
                        Cursor = Cursors.Arrow;
                        MessageBox.Show(res, Shoko.Commons.Properties.Resources.Result, MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    Cursor = Cursors.Arrow;

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
                itemSeriesAdmin.Header = Shoko.Commons.Properties.Resources.CommunityLinks_ShowAdmin;
                itemSeriesAdmin.Click += new RoutedEventHandler(commTraktMenuItem_Click);
                cmd = new CommTvDBTraktMenuCommand(CommTvDBTraktItemType.ShowCommAdmin, -1); // new playlist
                itemSeriesAdmin.CommandParameter = cmd;
                commTraktMenu.Items.Add(itemSeriesAdmin);

                if (AniDB_AnimeCrossRefs.TraktCrossRefExists)
                {
                    MenuItem itemSeriesLinks = new MenuItem();
                    itemSeriesLinks.Header = Shoko.Commons.Properties.Resources.CommunityLins_UseMyLinks;
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

                    VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                    if (anime == null) return;


                    Cursor = Cursors.Wait;

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
                            MessageBox.Show(Shoko.Commons.Properties.Resources.CommunityLinks_NoLinks, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        string res = VM_ShokoServer.Instance.ShokoServices.UseMyTraktLinksWebCache(anime.AnimeID);
                        Cursor = Cursors.Arrow;
                        MessageBox.Show(res, Shoko.Commons.Properties.Resources.Result, MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    Cursor = Cursors.Arrow;

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
            res.TvDBLinks = new List<VM_CrossRef_AniDB_TvDBV2>();
            res.TraktLinks = new List<VM_CrossRef_AniDB_TraktV2>();
            res.ExtraInfo = string.Empty;
            res.AnimeID = -1;

            try
            {
                VM_AniDB_Anime anime = e.Argument as VM_AniDB_Anime;
                if (anime == null) return;

                res.AnimeID = anime.AnimeID;

                SearchCriteria crit = new SearchCriteria();
                crit.AnimeID = anime.AnimeID;
                crit.ExtraInfo = string.Empty;

                // search for TvDB links
                try
                {
                    List<Azure_CrossRef_AniDB_TvDB> xrefs = VM_ShokoServer.Instance.ShokoServices.GetTVDBCrossRefWebCache(crit.AnimeID, true).CastList<Azure_CrossRef_AniDB_TvDB>();
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

                // search for Trakt links
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


        void communityWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                SearchCommunityResults res = e.Result as SearchCommunityResults;
                if (res == null) return;

                VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                if (anime == null) return;

                if (anime.AnimeID != res.AnimeID) return;

                if (!string.IsNullOrEmpty(res.ErrorMessage))
                {
                    return;
                }

                foreach (VM_CrossRef_AniDB_TvDBV2 tvxref in res.TvDBLinks)
                    CommunityTVDBLinks.Add(tvxref);

                foreach (VM_CrossRef_AniDB_TraktV2 traktxref in res.TraktLinks)
                    CommunityTraktLinks.Add(traktxref);

                btnTvDBCommLinks1.IsEnabled = true;
                btnTvDBCommLinks2.IsEnabled = true;

                CommTvDBButtonText = Shoko.Commons.Properties.Resources.CommunityLinks_NoLinksAvailable;
                if (CommunityTVDBLinks.Count > 0)
                {
                    CommTvDBButtonText = Shoko.Commons.Properties.Resources.CommunityLinks_NeedApproval;
                    foreach (VM_CrossRef_AniDB_TvDBV2 xref in CommunityTVDBLinks)
                    {
                        if (xref.IsAdminApprovedBool)
                        {
                            CommTvDBButtonText = Shoko.Commons.Properties.Resources.CommunityLinks_ApprovalExists;
                            break;
                        }
                    }
                }

                btnTraktCommLinks1.IsEnabled = true;
                btnTraktCommLinks2.IsEnabled = true;

                CommTraktButtonText = Shoko.Commons.Properties.Resources.CommunityLinks_NoLinksAvailable;
                if (CommunityTraktLinks.Count > 0)
                {
                    CommTraktButtonText = Shoko.Commons.Properties.Resources.CommunityLinks_NeedApproval;
                    foreach (VM_CrossRef_AniDB_TraktV2 xref in CommunityTraktLinks)
                    {
                        if (xref.IsAdminApprovedBool)
                        {
                            CommTraktButtonText = Shoko.Commons.Properties.Resources.CommunityLinks_ApprovalExists;
                            break;
                        }
                    }
                }

                //SearchStatus = string.Format("{0} Anime still need TvDB approval", link.AnimeNeedingApproval);

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
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
                VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                if (anime == null) return;

                Window wdw = Window.GetWindow(this);

                Cursor = Cursors.Wait;
                SearchMALForm frm = new SearchMALForm();
                frm.Owner = wdw;
                frm.Init(anime.AnimeID, anime.FormattedTitle, anime.MainTitle);
                bool? result = frm.ShowDialog();
                if (result.Value)
                {
                    // update info
                    RefreshData();
                }

                Cursor = Cursors.Arrow;
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

                Cursor = Cursors.Wait;

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
                VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                if (anime == null) return;

                if (obj is CrossRef_AniDB_MAL)
                {
                    Cursor = Cursors.Wait;
                    CrossRef_AniDB_MAL malLink = obj as CrossRef_AniDB_MAL;

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
                Cursor = Cursors.Arrow;
            }
        }

        private void CommandBinding_DeleteMALLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                if (anime == null) return;

                if (obj is CrossRef_AniDB_MAL)
                {
                    Cursor = Cursors.Wait;
                    CrossRef_AniDB_MAL malLink = obj as CrossRef_AniDB_MAL;

                    // prompt to select details
                    Window wdw = Window.GetWindow(this);

                    string msg = string.Format(Shoko.Commons.Properties.Resources.CommunityLinks_DeleteLink);
                    MessageBoxResult result = MessageBox.Show(msg, Shoko.Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        Cursor = Cursors.Wait;

                        string res = VM_ShokoServer.Instance.ShokoServices.RemoveLinkAniDBMAL(anime.AnimeID, malLink.StartEpisodeType, malLink.StartEpisodeNumber);
                        if (res.Length > 0)
                            MessageBox.Show(res, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                        {
                            // update info
                            RefreshData();
                        }

                        Cursor = Cursors.Arrow;
                    }
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

        #endregion

        #region Trakt

        private void CommandBinding_ReportTraktLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_CrossRef_AniDB_TvDBV2))
                {
                    VM_CrossRef_AniDB_TraktV2 link = obj as VM_CrossRef_AniDB_TraktV2;

                    VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                    if (anime == null) return;

                    Cursor = Cursors.Wait;

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

        private void CommandBinding_DeleteTraktLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_CrossRef_AniDB_TraktV2))
                {
                    VM_CrossRef_AniDB_TraktV2 link = obj as VM_CrossRef_AniDB_TraktV2;

                    VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                    if (anime == null) return;

                    Cursor = Cursors.Wait;

                    Window wdw = Window.GetWindow(this);

                    string msg = string.Format(Shoko.Commons.Properties.Resources.CommunityLinks_DeleteLink);
                    MessageBoxResult result = MessageBox.Show(msg, Shoko.Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        Cursor = Cursors.Wait;
                        string res = VM_ShokoServer.Instance.ShokoServices.RemoveLinkAniDBTrakt(link.AnimeID, link.AniDBStartEpisodeType, link.AniDBStartEpisodeNumber,
                            link.TraktID, link.TraktSeasonNumber, link.TraktStartEpisodeNumber);
                        if (res.Length > 0)
                            MessageBox.Show(res, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                        {
                            // update info
                            RefreshData();
                        }

                        Cursor = Cursors.Arrow;
                    }

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

        private void CommandBinding_EditTraktLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_CrossRef_AniDB_TraktV2))
                {
                    VM_CrossRef_AniDB_TraktV2 link = obj as VM_CrossRef_AniDB_TraktV2;

                    VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                    if (anime == null) return;

                    Window wdw = Window.GetWindow(this);

                    Cursor = Cursors.Wait;
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
                Cursor = Cursors.Arrow;
            }
        }

        private void CommandBinding_UpdateTraktLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_CrossRef_AniDB_TraktV2))
                {
                    VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                    if (anime == null) return;

                    Cursor = Cursors.Wait;

                    foreach (VM_CrossRef_AniDB_TraktV2 xref in AniDB_AnimeCrossRefs.Obs_CrossRef_AniDB_Trakt)
                        VM_ShokoServer.Instance.ShokoServices.UpdateTraktData(xref.TraktID);

                    anime.ClearTraktData();

                    VM_AnimeSeries_User ser = VM_MainListHelper.Instance.AllSeriesDictionary.Values.FirstOrDefault(a => a.AniDB_ID == anime.AnimeID);
                    if (ser!=null)
                        VM_MainListHelper.Instance.UpdateHeirarchy(ser);

                    RefreshData();
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

        private void CommandBinding_SyncTraktLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_CrossRef_AniDB_TraktV2))
                {
                    VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                    if (anime == null) return;

                    string response = VM_ShokoServer.Instance.ShokoServices.SyncTraktSeries(anime.AnimeID);
                    if (!string.IsNullOrEmpty(response))
                        MessageBox.Show(response, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        MessageBox.Show(Shoko.Commons.Properties.Resources.CommunityLinks_CommandQueued, Shoko.Commons.Properties.Resources.Done, MessageBoxButton.OK, MessageBoxImage.Information);

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
                VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                if (anime == null) return;

                Window wdw = Window.GetWindow(this);

                Cursor = Cursors.Wait;
                SearchTraktForm frm = new SearchTraktForm();
                frm.Owner = wdw;
                frm.Init(anime.AnimeID, anime.FormattedTitle, anime.FormattedTitle, ExistingTraktID, anime);
                bool? result = frm.ShowDialog();
                if (result.Value)
                {
                    // update info
                    RefreshData();
                }

                Cursor = Cursors.Arrow;
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
                VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                if (anime == null) return;

                Window wdw = Window.GetWindow(this);

                Cursor = Cursors.Wait;
                SearchTvDBForm frm = new SearchTvDBForm();
                frm.Owner = wdw;
                frm.Init(anime.AnimeID, anime.FormattedTitle, anime.FormattedTitle, ExistingSeriesID, anime);
                bool? result = frm.ShowDialog();
                if (result.Value)
                {
                    // update info
                    RefreshData();
                }

                Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_ReportTvDBLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_CrossRef_AniDB_TvDBV2))
                {
                    VM_CrossRef_AniDB_TvDBV2 link = obj as VM_CrossRef_AniDB_TvDBV2;

                    VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                    if (anime == null) return;

                    Cursor = Cursors.Wait;

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

        private void CommandBinding_EditTvDBLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_CrossRef_AniDB_TvDBV2))
                {
                    VM_CrossRef_AniDB_TvDBV2 link = obj as VM_CrossRef_AniDB_TvDBV2;

                    VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                    if (anime == null) return;

                    Window wdw = Window.GetWindow(this);

                    Cursor = Cursors.Wait;
                    SelectTvDBSeasonForm frm = new SelectTvDBSeasonForm();
                    frm.Owner = wdw;
                    //TODO
                    frm.Init(anime.AnimeID, anime.FormattedTitle, (EpisodeType)link.AniDBStartEpisodeType, link.AniDBStartEpisodeNumber, link.TvDBID,
                        link.TvDBSeasonNumber, link.TvDBStartEpisodeNumber, link.TvDBTitle, anime);
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
                Cursor = Cursors.Arrow;
            }
        }

        private void CommandBinding_DeleteTvDBLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_CrossRef_AniDB_TvDBV2))
                {
                    VM_CrossRef_AniDB_TvDBV2 link = obj as VM_CrossRef_AniDB_TvDBV2;

                    VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                    if (anime == null) return;

                    Cursor = Cursors.Wait;

                    Window wdw = Window.GetWindow(this);

                    string msg = string.Format(Shoko.Commons.Properties.Resources.CommunityLinks_DeleteLink);
                    MessageBoxResult result = MessageBox.Show(msg, Shoko.Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        Cursor = Cursors.Wait;
                        string res = VM_ShokoServer.Instance.ShokoServices.RemoveLinkAniDBTvDB(link);
                        if (res.Length > 0)
                            MessageBox.Show(res, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                        {
                            // update info
                            RefreshData();
                        }

                        Cursor = Cursors.Arrow;
                    }

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

        private void CommandBinding_UpdateTvDBLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_CrossRef_AniDB_TvDBV2))
                {
                    VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                    if (anime == null) return;

                    Cursor = Cursors.Wait;

                    foreach (VM_CrossRef_AniDB_TvDBV2 xref in AniDB_AnimeCrossRefs.Obs_CrossRef_AniDB_TvDB)
                        VM_ShokoServer.Instance.ShokoServices.UpdateTvDBData(xref.TvDBID);

                    anime.ClearTvDBData();

                    // find the series for this anime
                    VM_AnimeSeries_User ser = VM_MainListHelper.Instance.AllSeriesDictionary.Values.FirstOrDefault(a => a.AniDB_ID == anime.AnimeID);
                    if (ser != null)
                        VM_MainListHelper.Instance.UpdateHeirarchy(ser);

                    RefreshData();
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

        #endregion

        #region MovieDB

        private void SearchMovieDB()
        {
            try
            {
                VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                if (anime == null) return;

                Window wdw = Window.GetWindow(this);

                Cursor = Cursors.Wait;
                SearchMovieDBForm frm = new SearchMovieDBForm();
                frm.Owner = wdw;
                frm.Init(anime.AnimeID, anime.FormattedTitle);
                bool? result = frm.ShowDialog();
                if (result.Value)
                {
                    // update info
                    RefreshData();
                }

                Cursor = Cursors.Arrow;
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

        void CommandBinding_UpdateMovieDBInfo(object sender, RoutedEventArgs e)
        {
            try
            {
                VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                if (anime == null) return;

                Window wdw = Window.GetWindow(this);

                Cursor = Cursors.Wait;
                string res = VM_ShokoServer.Instance.ShokoServices.UpdateMovieDBData(AniDB_AnimeCrossRefs.MovieDBMovie.MovieId);
                if (res.Length > 0)
                    MessageBox.Show(res, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                else
                {
                    // update info
                    RefreshData();
                }

                Cursor = Cursors.Arrow;
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

        void CommandBinding_DeleteMovieDBLink(object sender, RoutedEventArgs e)
        {
            try
            {
                VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                if (anime == null) return;

                Window wdw = Window.GetWindow(this);

                string msg = string.Format(Shoko.Commons.Properties.Resources.CommunityLinks_DeleteLink);
                MessageBoxResult result = MessageBox.Show(msg, Shoko.Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Cursor = Cursors.Wait;
                    string res = VM_ShokoServer.Instance.ShokoServices.RemoveLinkAniDBOther(anime.AnimeID, (int)CrossRefType.MovieDB);
                    if (res.Length > 0)
                        MessageBox.Show(res, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                    {
                        // update info
                        RefreshData();
                    }

                    Cursor = Cursors.Arrow;
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
                if (DataContext == null)
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
                if (VM_ShokoServer.Instance.ShowCommunity)
                {
                    VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                    if (anime == null) return;

                    CommunityTVDBLinks.Clear();
                    CommunityTraktLinks.Clear();

                    btnTvDBCommLinks1.IsEnabled = false;
                    btnTvDBCommLinks2.IsEnabled = false;
                    CommTvDBButtonText = Shoko.Commons.Properties.Resources.CommunityLinks_CheckingOnline;

                    btnTraktCommLinks1.IsEnabled = false;
                    btnTraktCommLinks2.IsEnabled = false;
                    CommTraktButtonText = Shoko.Commons.Properties.Resources.CommunityLinks_CheckingOnline;

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

                VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                if (anime == null) return;

                AniDB_AnimeCrossRefs = (VM_AniDB_AnimeCrossRefs)VM_ShokoServer.Instance.ShokoServices.GetCrossRefDetails(anime.AnimeID);
                if (AniDB_AnimeCrossRefs == null) return;

                VM_MainListHelper.Instance.UpdateAnime(anime.AnimeID);

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
            return $"{MenuType} - {AnimeID}";
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
        public List<VM_CrossRef_AniDB_TvDBV2> TvDBLinks { get; set; }

        public List<VM_CrossRef_AniDB_TraktV2> TraktLinks { get; set; }
        public string ExtraInfo { get; set; }

        public SearchCommunityResults()
        {

        }
    }
}

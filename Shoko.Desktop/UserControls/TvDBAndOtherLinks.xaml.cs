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
using NLog;
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

        public static readonly DependencyProperty DistinctTvDbLinksProperty = DependencyProperty.Register("DistinctTvDbLinks",
            typeof(List<VM_CrossRef_AniDB_TvDBV2>), typeof(TvDBAndOtherLinks), new UIPropertyMetadata(null, null));

        public List<VM_CrossRef_AniDB_TvDBV2> DistinctTvDbLinks
        {
            get => (List<VM_CrossRef_AniDB_TvDBV2>) GetValue(DistinctTvDbLinksProperty);
            set => SetValue(DistinctTvDbLinksProperty, value);
        }

        public TvDBAndOtherLinks()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            commTvDBMenu = new ContextMenu();
            commTraktMenu = new ContextMenu();

            btnTvDBCommLinks1.ContextMenu = commTvDBMenu;
            btnTvDBCommLinks2.ContextMenu = commTvDBMenu;
            btnTvDBCommLinks1.Click += btnTvDBCommLinks_Click;
            btnTvDBCommLinks2.Click += btnTvDBCommLinks_Click;

            btnTraktCommLinks1.ContextMenu = commTraktMenu;
            btnTraktCommLinks2.ContextMenu = commTraktMenu;
            btnTraktCommLinks1.Click += btnTraktCommLinks_Click;
            btnTraktCommLinks2.Click += btnTraktCommLinks_Click;

            CommunityTVDBLinks = new ObservableCollection<VM_CrossRef_AniDB_TvDBV2>();
            CommunityTraktLinks = new ObservableCollection<VM_CrossRef_AniDB_TraktV2>();

            ViewCommunityTVDBLinks = CollectionViewSource.GetDefaultView(CommunityTVDBLinks);
            ViewCommunityTraktLinks = CollectionViewSource.GetDefaultView(CommunityTraktLinks);

            DataContextChanged += TvDBAndOtherLinks_DataContextChanged;

            btnSearchTvDB.Click += btnSearch_Click;
            btnSearchExistingTvDB.Click += btnSearchExisting_Click;

            btnSearchExistingMovieDB.Click += btnSearchExistingMovieDB_Click;
            btnSearchMovieDB.Click += btnSearchMovieDB_Click;

            btnSearchExistingTrakt.Click += btnSearchExistingTrakt_Click;
            btnSearchTrakt.Click += btnSearchTrakt_Click;

            communityWorker.WorkerSupportsCancellation = false;
            communityWorker.WorkerReportsProgress = true;
            communityWorker.DoWork += communityWorker_DoWork;
            communityWorker.RunWorkerCompleted += communityWorker_RunWorkerCompleted;
        }

        void btnTvDBCommLinks_Click(object sender, RoutedEventArgs e)
        {
            MenuItem itemSeriesAdmin = new MenuItem();
            try
            {
                // get all playlists
                commTvDBMenu.Items.Clear();

                itemSeriesAdmin.Header = Commons.Properties.Resources.CommunityLinks_ShowAdmin;
                itemSeriesAdmin.Click += commTvDBMenuItem_Click;
                var cmd = new CommTvDBTraktMenuCommand(CommTvDBTraktItemType.ShowCommAdmin, -1);
                itemSeriesAdmin.CommandParameter = cmd;
                commTvDBMenu.Items.Add(itemSeriesAdmin);

                if (AniDB_AnimeCrossRefs.TvDBCrossRefExists)
                {
                    MenuItem itemSeriesLinks = new MenuItem();
                    itemSeriesLinks.Header = Commons.Properties.Resources.CommunityLins_UseMyLinks;
                    itemSeriesLinks.Click += commTvDBMenuItem_Click;
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
                if (!(e.Source is MenuItem item) || !(sender is MenuItem itemSender)) return;
                if (!item.Header.ToString().Equals(itemSender.Header.ToString())) return;

                if (item.CommandParameter != null)
                {
                    CommTvDBTraktMenuCommand cmd = item.CommandParameter as CommTvDBTraktMenuCommand;
                    Debug.Write("Comm TvDB Menu: " + cmd + Environment.NewLine);

                    if (!(DataContext is VM_AniDB_Anime anime)) return;


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
                            MessageBox.Show(Commons.Properties.Resources.CommunityLinks_NoLinks, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        string res = VM_ShokoServer.Instance.ShokoServices.UseMyTvDBLinksWebCache(anime.AnimeID);
                        Cursor = Cursors.Arrow;
                        MessageBox.Show(res, Commons.Properties.Resources.Result, MessageBoxButton.OK, MessageBoxImage.Information);
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
                // get all playlists
                commTraktMenu.Items.Clear();

                MenuItem itemSeriesAdmin =
                    new MenuItem {Header = Commons.Properties.Resources.CommunityLinks_ShowAdmin};
                itemSeriesAdmin.Click += commTraktMenuItem_Click;
                var cmd = new CommTvDBTraktMenuCommand(CommTvDBTraktItemType.ShowCommAdmin, -1);
                itemSeriesAdmin.CommandParameter = cmd;
                commTraktMenu.Items.Add(itemSeriesAdmin);

                if (AniDB_AnimeCrossRefs.TraktCrossRefExists)
                {
                    MenuItem itemSeriesLinks = new MenuItem();
                    itemSeriesLinks.Header = Commons.Properties.Resources.CommunityLins_UseMyLinks;
                    itemSeriesLinks.Click += commTraktMenuItem_Click;
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
                if (!(e.Source is MenuItem item) || !(sender is MenuItem itemSender)) return;
                if (!item.Header.ToString().Equals(itemSender.Header.ToString())) return;

                if (item.CommandParameter != null)
                {
                    CommTvDBTraktMenuCommand cmd = item.CommandParameter as CommTvDBTraktMenuCommand;
                    Debug.Write("Comm TvDB Menu: " + cmd + Environment.NewLine);

                    if (!(DataContext is VM_AniDB_Anime anime)) return;


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
                            MessageBox.Show(Commons.Properties.Resources.CommunityLinks_NoLinks, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        string res = VM_ShokoServer.Instance.ShokoServices.UseMyTraktLinksWebCache(anime.AnimeID);
                        Cursor = Cursors.Arrow;
                        MessageBox.Show(res, Commons.Properties.Resources.Result, MessageBoxButton.OK, MessageBoxImage.Information);
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
            SearchCommunityResults res = new SearchCommunityResults
            {
                ErrorMessage = string.Empty,
                TvDBLinks = new List<VM_CrossRef_AniDB_TvDBV2>(),
                TraktLinks = new List<VM_CrossRef_AniDB_TraktV2>(),
                ExtraInfo = string.Empty,
                AnimeID = -1
            };

            try
            {
                if (!(e.Argument is VM_AniDB_Anime anime)) return;

                res.AnimeID = anime.AnimeID;

                SearchCriteria crit = new SearchCriteria
                {
                    AnimeID = anime.AnimeID,
                    ExtraInfo = string.Empty
                };

                // search for TvDB links
                try
                {
                    List<Azure_CrossRef_AniDB_TvDB> xrefs = VM_ShokoServer.Instance.ShokoServices.GetTVDBCrossRefWebCache(crit.AnimeID, true).CastList<Azure_CrossRef_AniDB_TvDB>();
                    if (xrefs != null && xrefs.Count > 0)
                        foreach (Azure_CrossRef_AniDB_TvDB xref in xrefs)
                            res.TvDBLinks.Add((VM_CrossRef_AniDB_TvDBV2)xref);
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
                        foreach (Azure_CrossRef_AniDB_Trakt xref in xrefs)
                            res.TraktLinks.Add((VM_CrossRef_AniDB_TraktV2)xref);
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
                if (!(e.Result is SearchCommunityResults res)) return;

                if (!(DataContext is VM_AniDB_Anime anime)) return;

                if (anime.AnimeID != res.AnimeID) return;

                if (!string.IsNullOrEmpty(res.ErrorMessage)) return;

                foreach (VM_CrossRef_AniDB_TvDBV2 tvxref in res.TvDBLinks)
                    CommunityTVDBLinks.Add(tvxref);

                foreach (VM_CrossRef_AniDB_TraktV2 traktxref in res.TraktLinks)
                    CommunityTraktLinks.Add(traktxref);

                btnTvDBCommLinks1.IsEnabled = true;
                btnTvDBCommLinks2.IsEnabled = true;

                CommTvDBButtonText = Commons.Properties.Resources.CommunityLinks_NoLinksAvailable;
                if (CommunityTVDBLinks.Count > 0)
                {
                    CommTvDBButtonText = Commons.Properties.Resources.CommunityLinks_NeedApproval;
                    foreach (VM_CrossRef_AniDB_TvDBV2 xref in CommunityTVDBLinks)
                        if (xref.IsAdminApprovedBool)
                        {
                            CommTvDBButtonText = Commons.Properties.Resources.CommunityLinks_ApprovalExists;
                            break;
                        }
                }

                btnTraktCommLinks1.IsEnabled = true;
                btnTraktCommLinks2.IsEnabled = true;

                CommTraktButtonText = Commons.Properties.Resources.CommunityLinks_NoLinksAvailable;
                if (CommunityTraktLinks.Count > 0)
                {
                    CommTraktButtonText = Commons.Properties.Resources.CommunityLinks_NeedApproval;
                    foreach (VM_CrossRef_AniDB_TraktV2 xref in CommunityTraktLinks)
                        if (xref.IsAdminApprovedBool)
                        {
                            CommTraktButtonText = Commons.Properties.Resources.CommunityLinks_ApprovalExists;
                            break;
                        }
                }

                //SearchStatus = string.Format("{0} Anime still need TvDB approval", link.AnimeNeedingApproval);

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }
        }


        #region Trakt

        private void CommandBinding_ReportTraktLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_CrossRef_AniDB_TvDBV2))
                {
                    if (!(DataContext is VM_AniDB_Anime)) return;

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

                    if (!(DataContext is VM_AniDB_Anime)) return;

                    Cursor = Cursors.Wait;

                    string msg = string.Format(Commons.Properties.Resources.CommunityLinks_DeleteLink);
                    MessageBoxResult result = MessageBox.Show(msg, Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        Cursor = Cursors.Wait;
                        string res = VM_ShokoServer.Instance.ShokoServices.RemoveLinkAniDBTrakt(link.AnimeID, link.AniDBStartEpisodeType, link.AniDBStartEpisodeNumber,
                            link.TraktID, link.TraktSeasonNumber, link.TraktStartEpisodeNumber);
                        if (res.Length > 0)
                            MessageBox.Show(res, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                            RefreshData();

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

                    if (!(DataContext is VM_AniDB_Anime anime)) return;

                    Window wdw = Window.GetWindow(this);

                    Cursor = Cursors.Wait;
                    SelectTraktSeasonForm frm = new SelectTraktSeasonForm {Owner = wdw};
                    frm.Init(anime.AnimeID, anime.FormattedTitle, (EpisodeType)link.AniDBStartEpisodeType, link.AniDBStartEpisodeNumber, link.TraktID,
                        link.TraktSeasonNumber, link.TraktStartEpisodeNumber, link.TraktTitle, anime, link.CrossRef_AniDB_TraktV2ID);
                    bool? result = frm.ShowDialog();
                    if (result != null && result.Value) RefreshData();
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
                    if (!(DataContext is VM_AniDB_Anime anime)) return;

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
                    if (!(DataContext is VM_AniDB_Anime anime)) return;

                    string response = VM_ShokoServer.Instance.ShokoServices.SyncTraktSeries(anime.AnimeID);
                    if (!string.IsNullOrEmpty(response))
                        MessageBox.Show(response, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        MessageBox.Show(Commons.Properties.Resources.CommunityLinks_CommandQueued, Commons.Properties.Resources.Done, MessageBoxButton.OK, MessageBoxImage.Information);

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
                if (!(DataContext is VM_AniDB_Anime anime)) return;

                Window wdw = Window.GetWindow(this);

                Cursor = Cursors.Wait;
                SearchTraktForm frm = new SearchTraktForm();
                frm.Owner = wdw;
                frm.Init(anime.AnimeID, anime.FormattedTitle, anime.FormattedTitle, ExistingTraktID, anime);
                bool? result = frm.ShowDialog();
                if (result != null && result.Value) RefreshData();

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
                if (!(DataContext is VM_AniDB_Anime anime)) return;

                Window wdw = Window.GetWindow(this);

                Cursor = Cursors.Wait;
                SearchTvDBForm frm = new SearchTvDBForm {Owner = wdw};
                frm.Init(anime.AnimeID, anime.FormattedTitle, anime.FormattedTitle, ExistingSeriesID, anime);
                bool? result = frm.ShowDialog();
                if (result != null && result.Value) RefreshData();

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
                    if (!(DataContext is VM_AniDB_Anime)) return;

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

        private void CommandBinding_PreviewTvDB_Matches(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_CrossRef_AniDB_TvDBV2))
                {
                    VM_CrossRef_AniDB_TvDBV2 link = obj as VM_CrossRef_AniDB_TvDBV2;

                    if (!(DataContext is VM_AniDB_Anime anime)) return;
                    if (link == null) return;

                    Window wdw = Window.GetWindow(this);

                    Cursor = Cursors.Wait;
                    TvDBMatchPreview frm = new TvDBMatchPreview {Owner = wdw};
                    frm.Init(link.AnimeID, link.AnimeName, link.TvDBID, link.TvDBTitle, anime, false, true);
                    bool? result = frm.ShowDialog();
                    if (result != null && result.Value) RefreshData();
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

                    if (!(DataContext is VM_AniDB_Anime)) return;

                    Cursor = Cursors.Wait;

                    Window wdw = Window.GetWindow(this);

                    string msg = string.Format(Commons.Properties.Resources.CommunityLinks_DeleteLink);
                    MessageBoxResult result = MessageBox.Show(msg, Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        Cursor = Cursors.Wait;
                        string res = VM_ShokoServer.Instance.ShokoServices.RemoveLinkAniDBTvDB(link);
                        if (res.Length > 0)
                            MessageBox.Show(res, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                            RefreshData();

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
                    if (!(DataContext is VM_AniDB_Anime anime)) return;

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
                if (!(DataContext is VM_AniDB_Anime anime)) return;

                Window wdw = Window.GetWindow(this);

                Cursor = Cursors.Wait;
                SearchMovieDBForm frm = new SearchMovieDBForm();
                frm.Owner = wdw;
                frm.Init(anime.AnimeID, anime.FormattedTitle);
                bool? result = frm.ShowDialog();
                if (result != null && result.Value) RefreshData();

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
                if (!(DataContext is VM_AniDB_Anime)) return;

                Window wdw = Window.GetWindow(this);

                Cursor = Cursors.Wait;
                string res = VM_ShokoServer.Instance.ShokoServices.UpdateMovieDBData(AniDB_AnimeCrossRefs.MovieDBMovie.MovieId);
                if (res.Length > 0)
                    MessageBox.Show(res, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    RefreshData();

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
                if (!(DataContext is VM_AniDB_Anime anime)) return;

                Window wdw = Window.GetWindow(this);

                string msg = string.Format(Commons.Properties.Resources.CommunityLinks_DeleteLink);
                MessageBoxResult result = MessageBox.Show(msg, Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Cursor = Cursors.Wait;
                    string res = VM_ShokoServer.Instance.ShokoServices.RemoveLinkAniDBOther(anime.AnimeID, (int)CrossRefType.MovieDB);
                    if (res.Length > 0)
                        MessageBox.Show(res, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        RefreshData();

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
                    if (!(DataContext is VM_AniDB_Anime anime)) return;

                    CommunityTVDBLinks.Clear();
                    CommunityTraktLinks.Clear();

                    btnTvDBCommLinks1.IsEnabled = false;
                    btnTvDBCommLinks2.IsEnabled = false;
                    CommTvDBButtonText = Commons.Properties.Resources.CommunityLinks_CheckingOnline;

                    btnTraktCommLinks1.IsEnabled = false;
                    btnTraktCommLinks2.IsEnabled = false;
                    CommTraktButtonText = Commons.Properties.Resources.CommunityLinks_CheckingOnline;

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

                if (!(DataContext is VM_AniDB_Anime anime)) return;

                AniDB_AnimeCrossRefs = (VM_AniDB_AnimeCrossRefs)VM_ShokoServer.Instance.ShokoServices.GetCrossRefDetails(anime.AnimeID);
                if (AniDB_AnimeCrossRefs == null) return;

                VM_MainListHelper.Instance.UpdateAnime(anime.AnimeID);

                RefreshAdminData();

                DistinctTvDbLinks = AniDB_AnimeCrossRefs.Obs_CrossRef_AniDB_TvDB.DistinctBy(a => a.TvDBID).ToList();
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
    }
}

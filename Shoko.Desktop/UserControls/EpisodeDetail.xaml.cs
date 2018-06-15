using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Enums;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for EpisodeDetail.xaml
    /// </summary>
    public partial class EpisodeDetail : UserControl
    {
        private ContextMenu playlistMenu;

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded",
            typeof(bool), typeof(EpisodeDetail), new UIPropertyMetadata(false, isExpandedCallback));


        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set
            {
                SetValue(IsExpandedProperty, value);
                SetVisibility();
            }
        }

        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register("IsCollapsed",
            typeof(bool), typeof(EpisodeDetail), new UIPropertyMetadata(true, isCollapsedCallback));


        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set
            {
                SetValue(IsCollapsedProperty, value);
                SetVisibility();
            }
        }

        public static readonly DependencyProperty ShowEpisodeImageInSummaryProperty = DependencyProperty.Register("ShowEpisodeImageInSummary",
            typeof(bool), typeof(EpisodeDetail), new UIPropertyMetadata(true, null));


        public bool ShowEpisodeImageInSummary
        {
            get { return (bool)GetValue(ShowEpisodeImageInSummaryProperty); }
            set { SetValue(ShowEpisodeImageInSummaryProperty, value); }
        }

        public static readonly DependencyProperty ShowEpisodeOverviewInSummaryProperty = DependencyProperty.Register("ShowEpisodeOverviewInSummary",
            typeof(bool), typeof(EpisodeDetail), new UIPropertyMetadata(true, null));


        public bool ShowEpisodeOverviewInSummary
        {
            get { return (bool)GetValue(ShowEpisodeOverviewInSummaryProperty); }
            set { SetValue(ShowEpisodeOverviewInSummaryProperty, value); }
        }

        public static readonly DependencyProperty ShowEpisodeImageInExpandedProperty = DependencyProperty.Register("ShowEpisodeImageInExpanded",
            typeof(bool), typeof(EpisodeDetail), new UIPropertyMetadata(true, null));


        public bool ShowEpisodeImageInExpanded
        {
            get { return (bool)GetValue(ShowEpisodeImageInExpandedProperty); }
            set { SetValue(ShowEpisodeImageInExpandedProperty, value); }
        }

        public static readonly DependencyProperty ShowEpisodeOverviewInExpandedProperty = DependencyProperty.Register("ShowEpisodeOverviewInExpanded",
            typeof(bool), typeof(EpisodeDetail), new UIPropertyMetadata(true, null));


        public bool ShowEpisodeOverviewInExpanded
        {
            get { return (bool)GetValue(ShowEpisodeOverviewInExpandedProperty); }
            set { SetValue(ShowEpisodeOverviewInExpandedProperty, value); }
        }
        private static void isExpandedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EpisodeDetail input = (EpisodeDetail)d;
            //input.tbTest.Text = e.NewValue as string;
        }

        private static void isCollapsedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EpisodeDetail input = (EpisodeDetail)d;
            //input.tbTest.Text = e.NewValue as string;
        }

        private void SetVisibility()
        {
            VM_AnimeEpisode_User ep = DataContext as VM_AnimeEpisode_User;
            if (ep != null)
            {
                ShowEpisodeImageInSummary = IsCollapsed && ep.ShowEpisodeImageInSummary;
                ShowEpisodeOverviewInSummary = IsCollapsed && ep.ShowEpisodeOverviewInSummary;

                ShowEpisodeImageInExpanded = IsExpanded && ep.ShowEpisodeImageInExpanded;
                ShowEpisodeOverviewInExpanded = IsExpanded && ep.ShowEpisodeOverviewInExpanded;
            }
        }

        public EpisodeDetail()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            playlistMenu = new ContextMenu();

            btnToggleExpander.Click += new RoutedEventHandler(btnToggleExpander_Click);
            DataContextChanged += new DependencyPropertyChangedEventHandler(EpisodeDetail_DataContextChanged);
            Loaded += new RoutedEventHandler(EpisodeDetail_Loaded);

            btnPlaylistAdd.ContextMenu = playlistMenu;
            btnPlaylistAdd.Click += new RoutedEventHandler(btnPlaylistAdd_Click);

            btnTvDBLinkAdd.Click += new RoutedEventHandler(btnTvDBLinkAdd_Click);
            btnTvDBLinkRemove.Click += new RoutedEventHandler(btnTvDBLinkRemove_Click);
        }

        void btnTvDBLinkRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // get the current tvdb link
                VM_AnimeEpisode_User ep = DataContext as VM_AnimeEpisode_User;

                Window wdw = Window.GetWindow(this);

                Cursor = Cursors.Wait;

                string res =
                    VM_ShokoServer.Instance.ShokoServices.RemoveLinkAniDBTvDBEpisode(ep.AniDB_EpisodeID,
                        ep.GetTvDBEpisodeID());
                if (res.Length > 0)
                {
                    Cursor = Cursors.Arrow;
                    Utils.ShowErrorMessage(res);
                    return;
                }

                // update info
                VM_AnimeEpisode_User contract = (VM_AnimeEpisode_User) VM_ShokoServer.Instance.ShokoServices.GetEpisode(
                    ep.AnimeEpisodeID, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                if (contract != null)
                {
                    ep.RefreshAnime(true);
                    ep.Populate(contract);
                    ep.SetTvDBInfo();
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

        void btnTvDBLinkAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // get the current tvdb link
                VM_AnimeEpisode_User ep = DataContext as VM_AnimeEpisode_User;
                ep.RefreshAnime();
                if (ep.AniDBAnime == null || ep.AniDBAnime.TvSummary == null || ep.AniDBAnime.TvSummary.CrossRefTvDBV2 == null ||
                    ep.AniDBAnime.TvSummary.CrossRefTvDBV2.Count == 0)
                {
                    Utils.ShowErrorMessage(Shoko.Commons.Properties.Resources.EpisodeDetail_TvDBLink);
                    return;
                }

                Window wdw = Window.GetWindow(this);

                Cursor = Cursors.Wait;
                SelectTvDBEpisodeForm frm = new SelectTvDBEpisodeForm();
                frm.Owner = wdw;
                frm.Init(ep, ep.AniDBAnime);
                bool? result = frm.ShowDialog();
                if (result.Value)
                {
                    // update info
                    CL_AnimeEpisode_User contract = VM_ShokoServer.Instance.ShokoServices.GetEpisode(
                        ep.AnimeEpisodeID, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    if (contract != null)
                    {
                        ep.RefreshAnime(true);
                        ep.Populate(contract);
                        ep.SetTvDBInfo();
                    }

                }

                this.DataContext = ep;
                Cursor = Cursors.Arrow;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnPlaylistAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // get all playlists
                List<VM_Playlist> rawPlaylists = VM_ShokoServer.Instance.ShokoServices.GetAllPlaylists().CastList<VM_Playlist>();
                PlaylistMenuCommand cmd = null;

                playlistMenu.Items.Clear();

                Separator sep = new Separator();

                MenuItem itemNew = new MenuItem();
                itemNew.Header = Shoko.Commons.Properties.Resources.EpisodeDetail_NewPlaylist;
                itemNew.Click += new RoutedEventHandler(playlistMenuItem_Click);
                cmd = new PlaylistMenuCommand(PlaylistItemType.SingleEpisode, -1); // new playlist
                itemNew.CommandParameter = cmd;
                playlistMenu.Items.Add(itemNew);
                playlistMenu.Items.Add(sep);

                foreach (VM_Playlist contract in rawPlaylists)
                {
                    MenuItem itemSeriesPL = new MenuItem();
                    itemSeriesPL.Header = contract.PlaylistName;
                    itemSeriesPL.Click += new RoutedEventHandler(playlistMenuItem_Click);
                    cmd = new PlaylistMenuCommand(PlaylistItemType.SingleEpisode, contract.PlaylistID);
                    itemSeriesPL.CommandParameter = cmd;
                    playlistMenu.Items.Add(itemSeriesPL);
                }

                playlistMenu.PlacementTarget = this;
                playlistMenu.IsOpen = true;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void playlistMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem item = e.Source as MenuItem;
                MenuItem itemSender = sender as MenuItem;

                if (item == null || itemSender == null) return;
                if (!item.Header.ToString().Equals(itemSender.Header.ToString())) return;

                if (item != null && item.CommandParameter != null)
                {
                    PlaylistMenuCommand cmd = item.CommandParameter as PlaylistMenuCommand;
                    Debug.Write(Shoko.Commons.Properties.Resources.EpisodeDetail_PlaylistMenu + cmd.ToString() + Environment.NewLine);

                    VM_AnimeEpisode_User ep = DataContext as VM_AnimeEpisode_User;
                    if (ep == null) return;

                    // get the playlist
                    VM_Playlist pl = null;
                    if (cmd.PlaylistID < 0)
                    {
                        pl = VM_PlaylistHelper.CreatePlaylist(Window.GetWindow(this));
                        if (pl == null) return;
                    }
                    else
                    {
                        pl = (VM_Playlist)VM_ShokoServer.Instance.ShokoServices.GetPlaylist(cmd.PlaylistID);
                        if (pl == null)
                        {
                            MessageBox.Show(Shoko.Commons.Properties.Resources.EpisodeDetail_PlaylistMissing, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    Cursor = Cursors.Wait;

                    pl.AddEpisode(ep.AnimeEpisodeID);
                    pl.Save();

                    VM_PlaylistHelper.Instance.RefreshData();

                    Cursor = Cursors.Arrow;

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void EpisodeDetail_Loaded(object sender, RoutedEventArgs e)
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(this);
            while (parentObject != null)
            {
                parentObject = VisualTreeHelper.GetParent(parentObject);
                AnimeSeries seriesControl = parentObject as AnimeSeries;
                if (seriesControl != null)
                {
                    SetBinding(WidthProperty, new Binding("ActualWidth") { Source = seriesControl, Converter = ArithmeticConverter.Instance, ConverterParameter = 40, IsAsync = true });
                    return;
                }
            }
        }

        void EpisodeDetail_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetVisibility();
        }

        private void CommandBinding_OpenFolder(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    VM_VideoDetailed vid = obj as VM_VideoDetailed;

                    if (File.Exists(vid.GetFullPath()))
                    {
                        Utils.OpenFolderAndSelectFile(vid.GetFullPath());
                    }
                    else
                    {
                        MessageBox.Show(Shoko.Commons.Properties.Resources.MSG_ERR_FileNotFound, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_DeleteLink(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    VM_VideoDetailed vid = obj as VM_VideoDetailed;
                    VM_AnimeEpisode_User ep = DataContext as VM_AnimeEpisode_User;
                    //VM_AnimeEpisode_User ep = VM_MainListHelper.Instance.GetEpisodeForVideo(vid, ((MainWindow)parentWindow).epListMain);

                    string res = VM_ShokoServer.Instance.ShokoServices.RemoveAssociationOnFile(vid.VideoLocalID, ep.AniDB_EpisodeID);
                    if (res.Length > 0)
                    {
                        MessageBox.Show(res, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        if (ep != null)
                        {
                            VM_MainListHelper.Instance.RefreshHeirarchy(ep);
                            DisplayFiles();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_DeleteFile(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    VM_VideoDetailed vid = obj as VM_VideoDetailed;
                    VM_AnimeEpisode_User ep = DataContext as VM_AnimeEpisode_User;
                    AskDeleteFile dlg=new AskDeleteFile(string.Format(Shoko.Commons.Properties.Resources.DeleteFile_Title,vid.GetFileName()), Shoko.Commons.Properties.Resources.EpisodeDetail_ConfirmDelete+"\r\n\r\n"+ Shoko.Commons.Properties.Resources.DeleteFile_Confirm,vid.Places);
                    dlg.Owner = Window.GetWindow(this);
                    bool? res=dlg.ShowDialog();
                    if (res.HasValue && res.Value)
                    {
                        string tresult = string.Empty;
                        Cursor = Cursors.Wait;
                        foreach (CL_VideoLocal_Place lv in dlg.Selected)
                        {
                            string result = VM_ShokoServer.Instance.ShokoServices.DeleteVideoLocalPlaceAndFile(lv.VideoLocal_Place_ID);
                            if (result.Length > 0)
                                tresult += result + "\r\n";
                        }
                        if (!string.IsNullOrEmpty(tresult))
                            MessageBox.Show(tresult, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        if (ep != null)
                        {
                            VM_MainListHelper.Instance.RefreshHeirarchy(ep);
                            DisplayFiles();
                        }

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

        private void CommandBinding_PlayVideo(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    VM_VideoDetailed vid = obj as VM_VideoDetailed;
                    //VM_AnimeEpisode_User ep = this.DataContext as VM_AnimeEpisode_User;
                    bool force = true;
                    if (MainWindow.videoHandler.DefaultPlayer.Player.ToString() !=
                        Enum.GetName(typeof(VideoPlayer), VideoPlayer.WindowsDefault))
                    {
                        if (vid.VideoLocal_ResumePosition > 0)
                        {
                            AskResumeVideo ask = new AskResumeVideo(vid.VideoLocal_ResumePosition);
                            ask.Owner = Window.GetWindow(this);
                            if (ask.ShowDialog() == true)
                                force = false;
                        }
                    }
                    MainWindow.videoHandler.PlayVideo(vid, force);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_PlayEpisode(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            try
            {
                VM_AnimeEpisode_User ep = DataContext as VM_AnimeEpisode_User;
                ep.RefreshFilesForEpisode();


                if (ep.FilesForEpisode.Count > 0)
                {
                    bool force = true;
                    if (MainWindow.videoHandler.DefaultPlayer.Player.ToString() !=
                        Enum.GetName(typeof(VideoPlayer), VideoPlayer.WindowsDefault))
                    {
                        if (ep.FilesForEpisode[0].VideoLocal_ResumePosition > 0)
                        {
                            AskResumeVideo ask = new AskResumeVideo(ep.FilesForEpisode[0].VideoLocal_ResumePosition);
                            ask.Owner = Window.GetWindow(this);
                            if (ask.ShowDialog() == true)
                                force = false;
                        }
                    }
                    MainWindow.videoHandler.PlayVideo(ep.FilesForEpisode[0], force);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_ToggleFileDetails(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    VM_VideoDetailed vid = obj as VM_VideoDetailed;
                    vid.ShowMoreDetails = !vid.ShowMoreDetails;
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

        private void CommandBinding_RehashFile(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Window parentWindow = Window.GetWindow(this);

                object obj = e.Parameter;
                if (obj == null) return;

                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    VM_VideoDetailed vid = obj as VM_VideoDetailed;
                    VM_ShokoServer.Instance.ShokoServices.RehashFile(vid.VideoLocalID);
                }


                MessageBox.Show(Shoko.Commons.Properties.Resources.MSG_INFO_AddedQueueCmds, Shoko.Commons.Properties.Resources.Done, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_ForceAddMyList(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    VM_VideoDetailed vid = obj as VM_VideoDetailed;

                    VM_ShokoServer.Instance.ShokoServices.ForceAddFileToMyList(vid.VideoLocal_Hash);

                    MessageBox.Show(Shoko.Commons.Properties.Resources.EpisodeDetail_CommandQueued, Shoko.Commons.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);

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



        private void CommandBinding_ForceUpdate(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    VM_VideoDetailed vid = obj as VM_VideoDetailed;

                    VM_ShokoServer.Instance.ShokoServices.UpdateFileData(vid.VideoLocalID);

                    MessageBox.Show(Shoko.Commons.Properties.Resources.EpisodeDetail_CommandQueued, Shoko.Commons.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);

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

        private void CommandBinding_ToggleVariation(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    VM_VideoDetailed vid = obj as VM_VideoDetailed;

                    vid.VideoLocal_IsVariation = vid.Variation ? 0 : 1;

                    string result = VM_ShokoServer.Instance.ShokoServices.SetVariationStatusOnFile(vid.VideoLocalID, vid.Variation);
                    if (result.Length > 0)
                        MessageBox.Show(result, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
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

        public void DisplayFiles()
        {
            try
            {
                VM_AnimeEpisode_User ep = DataContext as VM_AnimeEpisode_User;
                if (ep != null)
                {
                    ep.RefreshFilesForEpisode();
                    lbFiles.ItemsSource = ep.FilesForEpisode;

                    if (!ep.HasFiles)
                    {
                        List<CL_AniDB_GroupStatus> relGroups = ep.ReleaseGroups;
                        if (relGroups.Count > 0)
                        {
                            string grpList = "";
                            foreach (CL_AniDB_GroupStatus rg in relGroups)
                            {
                                if (grpList.Length > 0)
                                    grpList += ", ";
                                grpList += rg.GroupName;
                            }
                            tbFileDetailsGroups.Text = Shoko.Commons.Properties.Resources.GroupsAvailableFrom + " " + grpList;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnToggleExpander_Click(object sender, RoutedEventArgs e)
        {
            IsExpanded = !IsExpanded;
            IsCollapsed = !IsCollapsed;

            if (IsExpanded)
            {
                DisplayFiles();
            }
        }
    }

    public class ArithmeticConverter : IValueConverter
    {
        private double _parsedParameter = 0;
        private bool _isParsedParameter = false;
        private static ArithmeticConverter _instance = new ArithmeticConverter();

        public static ArithmeticConverter Instance => _instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return value;

            try
            {
                var doubleValue = (double)value;

                if (_isParsedParameter == false)
                {
                    _isParsedParameter = true;
                    var strParameter = System.Convert.ToString(parameter);
                    double.TryParse(strParameter, out _parsedParameter);
                }

                return doubleValue - _parsedParameter;
            }
            catch (Exception)
            {
                // ignore
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

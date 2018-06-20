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
using MahApps.Metro.Controls;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Enums;
using Shoko.Desktop.Forms;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;
using Utils = Shoko.Desktop.Utilities.Utils;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for EpisodeDetail.xaml
    /// </summary>
    public partial class EpisodeDetail
    {
        private readonly ContextMenu playlistMenu;

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded",
            typeof(bool), typeof(EpisodeDetail), new UIPropertyMetadata(false, isExpandedCallback));


        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
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
            get => (bool)GetValue(IsCollapsedProperty);
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
            get => (bool)GetValue(ShowEpisodeImageInSummaryProperty);
            set => SetValue(ShowEpisodeImageInSummaryProperty, value);
        }

        public static readonly DependencyProperty ShowEpisodeOverviewInSummaryProperty = DependencyProperty.Register("ShowEpisodeOverviewInSummary",
            typeof(bool), typeof(EpisodeDetail), new UIPropertyMetadata(true, null));


        public bool ShowEpisodeOverviewInSummary
        {
            get => (bool)GetValue(ShowEpisodeOverviewInSummaryProperty);
            set => SetValue(ShowEpisodeOverviewInSummaryProperty, value);
        }

        public static readonly DependencyProperty ShowEpisodeImageInExpandedProperty = DependencyProperty.Register("ShowEpisodeImageInExpanded",
            typeof(bool), typeof(EpisodeDetail), new UIPropertyMetadata(true, null));


        public bool ShowEpisodeImageInExpanded
        {
            get => (bool)GetValue(ShowEpisodeImageInExpandedProperty);
            set => SetValue(ShowEpisodeImageInExpandedProperty, value);
        }

        public static readonly DependencyProperty ShowEpisodeOverviewInExpandedProperty = DependencyProperty.Register("ShowEpisodeOverviewInExpanded",
            typeof(bool), typeof(EpisodeDetail), new UIPropertyMetadata(true, null));


        public bool ShowEpisodeOverviewInExpanded
        {
            get => (bool)GetValue(ShowEpisodeOverviewInExpandedProperty);
            set => SetValue(ShowEpisodeOverviewInExpandedProperty, value);
        }
        private static void isExpandedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        private static void isCollapsedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        private void SetVisibility()
        {
            if (!(DataContext is VM_AnimeEpisode_User ep)) return;
            ShowEpisodeImageInSummary = IsCollapsed && ep.ShowEpisodeImageInSummary;
            ShowEpisodeOverviewInSummary = IsCollapsed && ep.ShowEpisodeOverviewInSummary;

            ShowEpisodeImageInExpanded = IsExpanded && ep.ShowEpisodeImageInExpanded;
            ShowEpisodeOverviewInExpanded = IsExpanded && ep.ShowEpisodeOverviewInExpanded;
        }

        public EpisodeDetail()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            playlistMenu = new ContextMenu();

            btnToggleExpander.Click += btnToggleExpander_Click;
            DataContextChanged += EpisodeDetail_DataContextChanged;
            Loaded += EpisodeDetail_Loaded;

            btnPlaylistAdd.ContextMenu = playlistMenu;
            btnPlaylistAdd.Click += btnPlaylistAdd_Click;

            btnTvDBLinkAdd.Click += btnTvDBLinkAdd_Click;
            btnTvDBLinkRemove.Click += btnTvDBLinkRemove_Click;
        }

        void btnTvDBLinkRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // get the current tvdb link
                if (!(DataContext is VM_AnimeEpisode_User ep))
                {
                    Utils.ShowErrorMessage("DataContext is null");
                    return;
                }
                ep.RefreshAnime();

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
                CL_AnimeEpisode_User contract = VM_ShokoServer.Instance.ShokoServices.GetEpisode(
                    ep.AnimeEpisodeID, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                if (contract == null) return;

                ep.Populate(contract);
                ep.RefreshAnime(true);
                ep.SetTvDBInfo();

                var p = this.GetParentObject();
                AnimeSeries s = null;
                while (true)
                {
                    if (p == null) break;
                    if (p is AnimeSeries series)
                    {
                        s = series;
                        break;
                    }

                    p = p.GetParentObject();
                }

                if (s != null)
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        s.LoadSeries();
                        s.tabEpisodes.IsSelected = true;
                    });
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

        void btnTvDBLinkAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // get the current tvdb link
                if (!(DataContext is VM_AnimeEpisode_User ep))
                {
                    Utils.ShowErrorMessage("DataContext is null");
                    return;
                }
                ep.RefreshAnime();
                if (ep.AniDBAnime?.TvSummary?.CrossRefTvDBV2 == null || ep.AniDBAnime.TvSummary.CrossRefTvDBV2.Count == 0)
                {
                    Utils.ShowErrorMessage(Commons.Properties.Resources.EpisodeDetail_TvDBLink);
                    return;
                }

                Window wdw = Window.GetWindow(this);

                Cursor = Cursors.Wait;
                SelectTvDBEpisodeForm frm = new SelectTvDBEpisodeForm {Owner = wdw};
                frm.Init(ep, ep.AniDBAnime);
                bool? result = frm.ShowDialog();
                if (result == null || !result.Value) return;
                // update info
                CL_AnimeEpisode_User contract = VM_ShokoServer.Instance.ShokoServices.GetEpisode(
                    ep.AnimeEpisodeID, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                if (contract == null) return;

                ep.Populate(contract);
                ep.RefreshAnime(true);
                ep.SetTvDBInfo();
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

        void btnPlaylistAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // get all playlists
                List<VM_Playlist> rawPlaylists = VM_ShokoServer.Instance.ShokoServices.GetAllPlaylists().CastList<VM_Playlist>();

                playlistMenu.Items.Clear();

                Separator sep = new Separator();

                MenuItem itemNew = new MenuItem {Header = Commons.Properties.Resources.EpisodeDetail_NewPlaylist};
                itemNew.Click += playlistMenuItem_Click;
                var cmd = new PlaylistMenuCommand(PlaylistItemType.SingleEpisode, -1);
                itemNew.CommandParameter = cmd;
                playlistMenu.Items.Add(itemNew);
                playlistMenu.Items.Add(sep);

                foreach (VM_Playlist contract in rawPlaylists)
                {
                    MenuItem itemSeriesPL = new MenuItem {Header = contract.PlaylistName};
                    itemSeriesPL.Click += playlistMenuItem_Click;
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

                if (item.CommandParameter != null)
                {
                    PlaylistMenuCommand cmd = item.CommandParameter as PlaylistMenuCommand;
                    if (cmd == null) return;
                    Debug.Write(Commons.Properties.Resources.EpisodeDetail_PlaylistMenu + cmd + Environment.NewLine);

                    if (!(DataContext is VM_AnimeEpisode_User ep)) return;

                    // get the playlist
                    VM_Playlist pl;
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
                            MessageBox.Show(Commons.Properties.Resources.EpisodeDetail_PlaylistMissing, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
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
                if (parentObject is AnimeSeries seriesControl)
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
                if (obj.GetType() != typeof(VM_VideoDetailed)) return;
                VM_VideoDetailed vid = obj as VM_VideoDetailed;

                if (File.Exists(vid.GetFullPath()))
                    Utils.OpenFolderAndSelectFile(vid.GetFullPath());
                else
                    MessageBox.Show(Commons.Properties.Resources.MSG_ERR_FileNotFound, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_DeleteLink(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() != typeof(VM_VideoDetailed)) return;
                VM_VideoDetailed vid = obj as VM_VideoDetailed;
                VM_AnimeEpisode_User ep = DataContext as VM_AnimeEpisode_User;
                //VM_AnimeEpisode_User ep = VM_MainListHelper.Instance.GetEpisodeForVideo(vid, ((MainWindow)parentWindow).epListMain);

                if (vid == null || ep == null) return;
                string res = VM_ShokoServer.Instance.ShokoServices.RemoveAssociationOnFile(vid.VideoLocalID, ep.AniDB_EpisodeID);
                if (res.Length > 0)
                {
                    MessageBox.Show(res, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    VM_MainListHelper.Instance.RefreshHeirarchy(ep);
                    DisplayFiles();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_DeleteFile(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() != typeof(VM_VideoDetailed)) return;
                VM_VideoDetailed vid = obj as VM_VideoDetailed;
                VM_AnimeEpisode_User ep = DataContext as VM_AnimeEpisode_User;
                if (vid == null) return;
                AskDeleteFile dlg = new AskDeleteFile(
                    string.Format(Commons.Properties.Resources.DeleteFile_Title, vid.GetFileName()),
                    Commons.Properties.Resources.EpisodeDetail_ConfirmDelete + "\r\n\r\n" +
                    Commons.Properties.Resources.DeleteFile_Confirm, vid.Places) {Owner = Window.GetWindow(this)};
                bool? res=dlg.ShowDialog();
                if (!res.HasValue || !res.Value) return;
                string tresult = string.Empty;
                Cursor = Cursors.Wait;
                foreach (CL_VideoLocal_Place lv in dlg.Selected)
                {
                    string result = VM_ShokoServer.Instance.ShokoServices.DeleteVideoLocalPlaceAndFile(lv.VideoLocal_Place_ID);
                    if (result.Length > 0)
                        tresult += result + "\r\n";
                }
                if (!string.IsNullOrEmpty(tresult))
                    MessageBox.Show(tresult, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);

                if (ep == null) return;
                VM_MainListHelper.Instance.RefreshHeirarchy(ep);
                DisplayFiles();
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
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() != typeof(VM_VideoDetailed)) return;
                VM_VideoDetailed vid = obj as VM_VideoDetailed;
                //VM_AnimeEpisode_User ep = this.DataContext as VM_AnimeEpisode_User;
                bool force = true;
                if (vid == null) return;
                if (MainWindow.videoHandler.DefaultPlayer.Player.ToString() !=
                    Enum.GetName(typeof(VideoPlayer), VideoPlayer.WindowsDefault))
                    if (vid.VideoLocal_ResumePosition > 0)
                    {
                        AskResumeVideo ask =
                            new AskResumeVideo(vid.VideoLocal_ResumePosition) {Owner = Window.GetWindow(this)};
                        if (ask.ShowDialog() == true)
                            force = false;
                    }

                MainWindow.videoHandler.PlayVideo(vid, force);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_PlayEpisode(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                VM_AnimeEpisode_User ep = DataContext as VM_AnimeEpisode_User;
                if (ep == null) return;
                ep.RefreshFilesForEpisode();


                if (ep.FilesForEpisode.Count <= 0) return;
                bool force = true;
                if (MainWindow.videoHandler.DefaultPlayer.Player.ToString() !=
                    Enum.GetName(typeof(VideoPlayer), VideoPlayer.WindowsDefault))
                    if (ep.FilesForEpisode[0].VideoLocal_ResumePosition > 0)
                    {
                        AskResumeVideo ask =
                            new AskResumeVideo(ep.FilesForEpisode[0].VideoLocal_ResumePosition)
                            {
                                Owner = Window.GetWindow(this)
                            };
                        if (ask.ShowDialog() == true)
                            force = false;
                    }

                MainWindow.videoHandler.PlayVideo(ep.FilesForEpisode[0], force);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_ToggleFileDetails(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() != typeof(VM_VideoDetailed)) return;
                if (obj is VM_VideoDetailed vid) vid.ShowMoreDetails = !vid.ShowMoreDetails;
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
                object obj = e.Parameter;
                if (obj == null) return;

                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    if (!(obj is VM_VideoDetailed vid)) return;
                    VM_ShokoServer.Instance.ShokoServices.RehashFile(vid.VideoLocalID);
                }


                MessageBox.Show(Commons.Properties.Resources.MSG_INFO_AddedQueueCmds, Commons.Properties.Resources.Done, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_ForceAddMyList(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() != typeof(VM_VideoDetailed)) return;
                if (!(obj is VM_VideoDetailed vid)) return;

                VM_ShokoServer.Instance.ShokoServices.ForceAddFileToMyList(vid.VideoLocal_Hash);

                MessageBox.Show(Commons.Properties.Resources.EpisodeDetail_CommandQueued, Commons.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
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
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() != typeof(VM_VideoDetailed)) return;
                if (!(obj is VM_VideoDetailed vid)) return;

                VM_ShokoServer.Instance.ShokoServices.UpdateFileData(vid.VideoLocalID);

                MessageBox.Show(Commons.Properties.Resources.EpisodeDetail_CommandQueued,
                    Commons.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
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
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() != typeof(VM_VideoDetailed)) return;
                if (!(obj is VM_VideoDetailed vid)) return;

                vid.VideoLocal_IsVariation = vid.Variation ? 0 : 1;

                string result =
                    VM_ShokoServer.Instance.ShokoServices.SetVariationStatusOnFile(vid.VideoLocalID, vid.Variation);
                if (result.Length > 0)
                    MessageBox.Show(result, Commons.Properties.Resources.Error, MessageBoxButton.OK,
                        MessageBoxImage.Error);
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
                if (!(DataContext is VM_AnimeEpisode_User ep)) return;
                ep.RefreshFilesForEpisode();
                lbFiles.ItemsSource = ep.FilesForEpisode;

                if (ep.HasFiles) return;
                List<CL_AniDB_GroupStatus> relGroups = ep.ReleaseGroups;
                if (relGroups.Count <= 0) return;
                string grpList = "";
                foreach (CL_AniDB_GroupStatus rg in relGroups)
                {
                    if (grpList.Length > 0)
                        grpList += ", ";
                    grpList += rg.GroupName;
                }

                tbFileDetailsGroups.Text = Commons.Properties.Resources.GroupsAvailableFrom + " " + grpList;
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

            if (IsExpanded) DisplayFiles();
        }
    }

    public class ArithmeticConverter : IValueConverter
    {
        private double _parsedParameter;
        private bool _isParsedParameter;

        public static ArithmeticConverter Instance { get; } = new ArithmeticConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return value;

            try
            {
                if (value != null)
                {
                    var doubleValue = (double)value;

                    if (_isParsedParameter) return doubleValue - _parsedParameter;
                    _isParsedParameter = true;
                    var strParameter = System.Convert.ToString(parameter);
                    double.TryParse(strParameter, out _parsedParameter);

                    return doubleValue - _parsedParameter;
                }
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Desktop.Enums;
using Shoko.Models.Enums;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for PlaylistControl.xaml
    /// </summary>
    public partial class PlaylistControl : UserControl
    {
        public static readonly DependencyProperty HasPlaylistsProperty = DependencyProperty.Register("HasPlaylists",
            typeof(bool), typeof(PlaylistControl), new UIPropertyMetadata(false, null));


        public bool HasPlaylists
        {
            get { return (bool)GetValue(HasPlaylistsProperty); }
            set
            {
                SetValue(HasPlaylistsProperty, value);
            }
        }

        public PlaylistControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            VM_PlaylistHelper.Instance.OnPlaylistModifiedEvent += new VM_PlaylistHelper.PlaylistModifiedHandler(Instance_OnPlaylistModifiedEvent);
            btnEditPlaylist.Click += new RoutedEventHandler(btnEditPlaylist_Click);
            btnEditPlaylistFinish.Click += new RoutedEventHandler(btnEditPlaylistFinish_Click);

            btnIncreaseHeaderImageSize.Click += new RoutedEventHandler(btnIncreaseHeaderImageSize_Click);
            btnDecreaseHeaderImageSize.Click += new RoutedEventHandler(btnDecreaseHeaderImageSize_Click);

            togFanart.IsChecked = VM_UserSettings.Instance.UseFanartOnPlaylistHeader;
            togFanart.Click += new RoutedEventHandler(togFanart_Click);

            cboPlayOrderEdit.Items.Clear();
            cboPlayOrderEdit.Items.Add(Properties.Resources.PlaylistPlayOrderSeq);
            cboPlayOrderEdit.Items.Add(Properties.Resources.PlaylistPlayOrderRandom);
            cboPlayOrderEdit.SelectedIndex = 0;


            cboPlayOrderLocked.Items.Clear();
            cboPlayOrderLocked.Items.Add(Properties.Resources.PlaylistPlayOrderSeq);
            cboPlayOrderLocked.Items.Add(Properties.Resources.PlaylistPlayOrderRandom);
            cboPlayOrderLocked.SelectedIndex = 0;

            btnRandomEpisode.Click += new RoutedEventHandler(btnRandomEpisode_Click);

            DataContextChanged += new DependencyPropertyChangedEventHandler(PlaylistControl_DataContextChanged);
            MainWindow.videoHandler.VideoWatchedEvent += new VideoPlayers.VideoHandler.VideoWatchedEventHandler(videoHandler_VideoWatchedEvent);

            btnPlayAll.Click += new RoutedEventHandler(btnPlayAll_Click);
        }

        void btnPlayAll_Click(object sender, RoutedEventArgs e)
        {
            VM_Playlist pl = DataContext as VM_Playlist;
            if (pl == null) return;

            Cursor = Cursors.Wait;
            List<VM_AnimeEpisode_User> eps = pl.GetAllEpisodes(false);
            Cursor = Cursors.Arrow;

            MainWindow.videoHandler.PlayEpisodes(eps);
        }

        void videoHandler_VideoWatchedEvent(VideoPlayers.VideoWatchedEventArgs ev)
        {
            if (MainWindow.CurrentMainTabIndex == MainWindow.TAB_MAIN_Playlists)
            {
                VM_Playlist pl = DataContext as VM_Playlist;
                if (pl == null) return;

                pl.PopulatePlaylistObjects();
                ShowNextEpisode();
            }
        }

        void btnRandomEpisode_Click(object sender, RoutedEventArgs e)
        {
            VM_Playlist pl = DataContext as VM_Playlist;
            if (pl == null) return;

            Cursor = Cursors.Wait;
            pl.SetNextEpisode(true);
            ShowNextEpisode();
            Cursor = Cursors.Arrow;
        }

        void togFanart_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.UseFanartOnPlaylistHeader = !VM_UserSettings.Instance.UseFanartOnPlaylistHeader;

            VM_Playlist pl = DataContext as VM_Playlist;
            if (pl == null) return;

            pl.SetDependendProperties();
        }

        void btnDecreaseHeaderImageSize_Click(object sender, RoutedEventArgs e)
        {

            VM_UserSettings.Instance.PlaylistHeader_Image_Height = VM_UserSettings.Instance.PlaylistHeader_Image_Height - 10;
        }

        void btnIncreaseHeaderImageSize_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.PlaylistHeader_Image_Height = VM_UserSettings.Instance.PlaylistHeader_Image_Height + 10;
        }

        void btnEditPlaylistFinish_Click(object sender, RoutedEventArgs e)
        {
            VM_Playlist pl = DataContext as VM_Playlist;
            if (pl == null) return;

            pl.PlaylistName = txtPlaylistName.Text.Trim();
            pl.PlayUnwatched = chkPlayUnwatchedEdit.IsChecked.Value ? 1 : 0;
            pl.PlayWatched = chkPlayWatchedEdit.IsChecked.Value ? 1 : 0;

            if (cboPlayOrderEdit.SelectedIndex == 0) pl.DefaultPlayOrder = (int)PlaylistPlayOrder.Sequential;
            if (cboPlayOrderEdit.SelectedIndex == 1) pl.DefaultPlayOrder = (int)PlaylistPlayOrder.Random;

            pl.Save();

            pl.IsBeingEdited = false;
            pl.IsReadOnly = true;


            if (pl.DefaultPlayOrder == (int)PlaylistPlayOrder.Sequential)
            {
                cboPlayOrderEdit.SelectedIndex = 0;
                cboPlayOrderLocked.SelectedIndex = 0;
            }
            if (pl.DefaultPlayOrder == (int)PlaylistPlayOrder.Random)
            {
                cboPlayOrderEdit.SelectedIndex = 1;
                cboPlayOrderLocked.SelectedIndex = 1;
            }

            pl.SetDependendProperties();
        }

        void btnEditPlaylist_Click(object sender, RoutedEventArgs e)
        {
            VM_Playlist pl = DataContext as VM_Playlist;
            if (pl == null) return;

            pl.IsBeingEdited = true;
            pl.IsReadOnly = false;
        }

        void Instance_OnPlaylistModifiedEvent(PlaylistModifiedEventArgs ev)
        {
            ShowNextEpisode();


        }

        void PlaylistControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            HasPlaylists = false;

            VM_Playlist pl = DataContext as VM_Playlist;
            if (pl == null) return;

            ShowNextEpisode();

            if (pl.DefaultPlayOrder == (int)PlaylistPlayOrder.Sequential)
            {
                cboPlayOrderEdit.SelectedIndex = 0;
                cboPlayOrderLocked.SelectedIndex = 0;
            }
            if (pl.DefaultPlayOrder == (int)PlaylistPlayOrder.Random)
            {
                cboPlayOrderEdit.SelectedIndex = 1;
                cboPlayOrderLocked.SelectedIndex = 1;
            }

            HasPlaylists = true;
        }



        private void ShowNextEpisode()
        {
            ucNextEpisodePlaylist.EpisodeExists = false;
            ucNextEpisodePlaylist.EpisodeMissing = true;
            ucNextEpisodePlaylist.DataContext = null;

            VM_Playlist pl = DataContext as VM_Playlist;
            if (pl == null) return;

            if (pl.NextEpisode == null) return;

            if (pl.NextEpisode != null)
                ucNextEpisodePlaylist.DataContext = pl.NextEpisode;

        }

        private void CommandBinding_PlayEpisode(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_PlaylistItem))
                {
                    VM_PlaylistItem pli = obj as VM_PlaylistItem;

                    VM_AnimeEpisode_User ep = null;

                    if (pli.ItemType == Models.Enums.PlaylistItemType.Episode)
                    {
                        ep = pli.PlaylistItem as VM_AnimeEpisode_User;
                    }
                    if (pli.ItemType == Models.Enums.PlaylistItemType.AnimeSeries)
                    {
                        VM_AnimeSeries_User ser = pli.PlaylistItem as VM_AnimeSeries_User;
                        if (ser == null) return;

                        ep = (VM_AnimeEpisode_User)VM_ShokoServer.Instance.ShokoServices.GetNextUnwatchedEpisode(ser.AnimeSeriesID,
                            VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    }

                    if (ep == null) return;
                    ep.SetTvDBInfo();
                    if (ep.FilesForEpisode.Count == 1)
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
                    else if (ep.FilesForEpisode.Count > 1)
                    {
                        if (AppSettings.AutoFileSingleEpisode)
                        {
                            VM_VideoDetailed vid = MainWindow.videoHandler.GetAutoFileForEpisode(ep);
                            if (vid != null)
                            {
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
                        else
                        {
                            PlayVideosForEpisodeForm frm = new PlayVideosForEpisodeForm();
                            frm.Owner = parentWindow;
                            frm.Init(ep);
                            frm.ShowDialog();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// This event bubbles up from PlayEpisodeControl
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandBinding_ToggleWatchedStatus(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            Cursor = Cursors.Wait;

            try
            {
                Window parentWindow = Window.GetWindow(this);
                VM_AnimeSeries_User serTemp = null;
                bool newStatus = false;

                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    VM_VideoDetailed vid = obj as VM_VideoDetailed;
                    newStatus = !vid.Watched;
                    VM_ShokoServer.Instance.ShokoServices.ToggleWatchedStatusOnVideo(vid.VideoLocalID, newStatus, VM_ShokoServer.Instance.CurrentUser.JMMUserID);

                    VM_MainListHelper.Instance.UpdateHeirarchy(vid);

                    serTemp = VM_MainListHelper.Instance.GetSeriesForVideo(vid.VideoLocalID);
                }

                if (obj.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    VM_AnimeEpisode_User ep = obj as VM_AnimeEpisode_User;
                    newStatus = !ep.Watched;
                    CL_Response<CL_AnimeEpisode_User> response = VM_ShokoServer.Instance.ShokoServices.ToggleWatchedStatusOnEpisode(ep.AnimeEpisodeID,
                        newStatus, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    if (!string.IsNullOrEmpty(response.ErrorMessage))
                    {
                        MessageBox.Show(response.ErrorMessage, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    VM_MainListHelper.Instance.UpdateHeirarchy((VM_AnimeEpisode_User)response.Result);

                    serTemp = VM_MainListHelper.Instance.GetSeriesForEpisode(ep);
                }

                if (obj.GetType() == typeof(VM_PlaylistItem))
                {
                    VM_PlaylistItem pli = obj as VM_PlaylistItem;
                    VM_AnimeEpisode_User ep = pli.PlaylistItem as VM_AnimeEpisode_User;

                    newStatus = !ep.Watched;
                    CL_Response<CL_AnimeEpisode_User> response = VM_ShokoServer.Instance.ShokoServices.ToggleWatchedStatusOnEpisode(ep.AnimeEpisodeID,
                        newStatus, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    if (!string.IsNullOrEmpty(response.ErrorMessage))
                    {
                        MessageBox.Show(response.ErrorMessage, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    VM_MainListHelper.Instance.UpdateHeirarchy((VM_AnimeEpisode_User)response.Result);

                    serTemp = VM_MainListHelper.Instance.GetSeriesForEpisode(ep);
                }

                VM_Playlist pl = DataContext as VM_Playlist;
                if (pl == null) return;

                pl.PopulatePlaylistObjects();
                ShowNextEpisode();

                if (newStatus == true && serTemp != null)
                {
                    Utils.PromptToRateSeries(serTemp, parentWindow);
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
    }
}

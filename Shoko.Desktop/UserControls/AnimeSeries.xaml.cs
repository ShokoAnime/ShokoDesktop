using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NLog;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.VideoPlayers;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;
using Shoko.Models.Enums;
using Shoko.Models.Server;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for AnimeSeries.xaml
    /// </summary>
    public partial class AnimeSeries : UserControl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private ContextMenu playlistMenu;

        public static readonly DependencyProperty AllPostersProperty = DependencyProperty.Register("AllPosters",
            typeof(List<VM_PosterContainer>), typeof(AnimeSeries), new UIPropertyMetadata(null, null));

        public List<VM_PosterContainer> AllPosters
        {
            get { return (List<VM_PosterContainer>)GetValue(AllPostersProperty); }
            set { SetValue(AllPostersProperty, value); }
        }

        public static readonly DependencyProperty AllFanartsProperty = DependencyProperty.Register("AllFanarts",
            typeof(List<VM_FanartContainer>), typeof(AnimeSeries), new UIPropertyMetadata(null, null));

        public List<VM_FanartContainer> AllFanarts
        {
            get { return (List<VM_FanartContainer>)GetValue(AllFanartsProperty); }
            set { SetValue(AllFanartsProperty, value); }
        }

        public static readonly DependencyProperty SeriesTvDBWideBannersProperty = DependencyProperty.Register("SeriesTvDBWideBanners",
            typeof(List<VM_TvDB_ImageWideBanner>), typeof(AnimeSeries), new UIPropertyMetadata(null, null));

        public List<VM_TvDB_ImageWideBanner> SeriesTvDBWideBanners
        {
            get { return (List<VM_TvDB_ImageWideBanner>)GetValue(SeriesTvDBWideBannersProperty); }
            set { SetValue(SeriesTvDBWideBannersProperty, value); }
        }

        public static readonly DependencyProperty ImageListBoxWidthProperty = DependencyProperty.Register("ImageListBoxWidth",
            typeof(double), typeof(AnimeSeries), new UIPropertyMetadata((double)300, null));

        public double ImageListBoxWidth
        {
            get { return (double)GetValue(ImageListBoxWidthProperty); }
            set { SetValue(ImageListBoxWidthProperty, value); }
        }

        public static readonly DependencyProperty ImageListBoxWidthFullProperty = DependencyProperty.Register("ImageListBoxWidthFull",
            typeof(double), typeof(AnimeSeries), new UIPropertyMetadata((double)300, null));

        public double ImageListBoxWidthFull
        {
            get { return (double)GetValue(ImageListBoxWidthFullProperty); }
            set { SetValue(ImageListBoxWidthFullProperty, value); }
        }

        public static readonly DependencyProperty SeriesPos_TvDBLinksProperty = DependencyProperty.Register("SeriesPos_TvDBLinks",
            typeof(int), typeof(AnimeSeries), new UIPropertyMetadata(6, null));

        public int SeriesPos_TvDBLinks
        {
            get { return (int)GetValue(SeriesPos_TvDBLinksProperty); }
            set { SetValue(SeriesPos_TvDBLinksProperty, value); }
        }

        public static readonly DependencyProperty SeriesPos_PlayNextEpisodeProperty = DependencyProperty.Register("SeriesPos_PlayNextEpisode",
            typeof(int), typeof(AnimeSeries), new UIPropertyMetadata(6, null));

        public int SeriesPos_PlayNextEpisode
        {
            get { return (int)GetValue(SeriesPos_PlayNextEpisodeProperty); }
            set { SetValue(SeriesPos_PlayNextEpisodeProperty, value); }
        }

        public static readonly DependencyProperty SeriesPos_TitlesProperty = DependencyProperty.Register("SeriesPos_Titles",
            typeof(int), typeof(AnimeSeries), new UIPropertyMetadata(6, null));

        public int SeriesPos_Titles
        {
            get { return (int)GetValue(SeriesPos_TitlesProperty); }
            set { SetValue(SeriesPos_TitlesProperty, value); }
        }

        public static readonly DependencyProperty SeriesPos_TagsProperty = DependencyProperty.Register("SeriesPos_Tags",
            typeof(int), typeof(AnimeSeries), new UIPropertyMetadata(6, null));

        public int SeriesPos_Tags
        {
            get { return (int)GetValue(SeriesPos_TagsProperty); }
            set { SetValue(SeriesPos_TagsProperty, value); }
        }

        public static readonly DependencyProperty SeriesPos_CustomTagsProperty = DependencyProperty.Register("SeriesPos_CustomTags",
            typeof(int), typeof(AnimeSeries), new UIPropertyMetadata(7, null));

        public int SeriesPos_CustomTags
        {
            get { return (int)GetValue(SeriesPos_CustomTagsProperty); }
            set { SetValue(SeriesPos_CustomTagsProperty, value); }
        }

        public static readonly DependencyProperty TruncatedDescriptionProperty = DependencyProperty.Register("TruncatedDescription",
            typeof(bool), typeof(AnimeSeries), new UIPropertyMetadata(true, null));

        public bool TruncatedDescription
        {
            get { return (bool)GetValue(TruncatedDescriptionProperty); }
            set { SetValue(TruncatedDescriptionProperty, value); }
        }

        public static readonly DependencyProperty FullDescriptionProperty = DependencyProperty.Register("FullDescription",
            typeof(bool), typeof(AnimeSeries), new UIPropertyMetadata(false, null));

        public bool FullDescription
        {
            get { return (bool)GetValue(FullDescriptionProperty); }
            set { SetValue(FullDescriptionProperty, value); }
        }

        public static readonly DependencyProperty PreviousOverrideNameProperty = DependencyProperty.Register("PreviousOverrideName",
            typeof(string), typeof(AnimeSeries), new UIPropertyMetadata("", null));

        public string PreviousOverrideName
        {
            get { return (string)GetValue(PreviousOverrideNameProperty); }
            set { SetValue(PreviousOverrideNameProperty, value); }
        }

        public AnimeSeries()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            playlistMenu = new ContextMenu();

            cRating.OnRatingValueChangedEvent += cRating_OnRatingValueChangedEvent;

            Loaded += AnimeSeries_Loaded;
            btnAnimeGroupShow.Click += btnAnimeGroupShow_Click;
            btnTvDBLinks.Click += btnTvDBLinks_Click;
            //btnPlayNextEpisode.Click += new RoutedEventHandler(btnPlayNextEpisode_Click);

            btnSwitchView.Click += BtnSwitchView_Click;

            DataContextChanged += AnimeSeries_DataContextChanged;

            tabContainer.SelectionChanged += tabContainer_SelectionChanged;

            btnUpdateAniDBInfo.Click += btnUpdateAniDBInfo_Click;
            LayoutUpdated += AnimeSeries_LayoutUpdated;

            btnEditSeries.Click += btnEditSeries_Click;
            btnEditSeriesFinish.Click += btnEditSeriesFinish_Click;

            btnAnimeTitles.Click += btnAnimeTitles_Click;
            btnAnimeTags.Click += btnAnimeTags_Click;
            btnCustomTags.Click += btnCustomTags_Click;
            btnAddCustomTag.Click += btnAddCustomTag_Click;
            btnManageTags.Click += btnManageTags_Click;

            btnPlaylistAdd.ContextMenu = playlistMenu;
            btnPlaylistAdd.Click += btnPlaylistAdd_Click;

            btnRandomEpisode.Click += btnRandomEpisode_Click;

            chkSerNameOverride.Click += chkSerNameOverride_Click;
            btnSelectOverrideName.Click += btnSelectOverrideName_Click;

            MainWindow.videoHandler.VideoWatchedEvent += videoHandler_VideoWatchedEvent;

            Unloaded += (sender, e) => MainWindow.videoHandler.VideoWatchedEvent -= videoHandler_VideoWatchedEvent;

            SetSeriesWidgetOrder();

            PreviewMouseWheel += AnimeSeries_PreviewMouseWheel;
        }

        private void BtnSwitchView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AppSettings.DisplaySeriesSimple = true;
                // check if this control is part of the series container
                DependencyObject parentObject = VisualTreeHelper.GetParent(this);
                while (parentObject != null)
                {
                    parentObject = VisualTreeHelper.GetParent(parentObject);
                    AnimeSeriesContainerControl containerCtrl = parentObject as AnimeSeriesContainerControl;
                    if (containerCtrl != null)
                    {
                        // show the simple view
                        VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
                        if (ser == null) return;

                        AnimeSeriesSimplifiedControl seriesControl = new AnimeSeriesSimplifiedControl();
                        seriesControl.DataContext = ser;

                        containerCtrl.DataContext = seriesControl;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void AnimeSeries_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                foreach (ScrollViewer sv in Utils.GetScrollViewers(this))
                    sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta / 3D);
            }
            catch
            {
                // ignore
            }
        }

        void videoHandler_VideoWatchedEvent(VideoWatchedEventArgs ev)
        {
            if (MainWindow.CurrentMainTabIndex == (int) MainWindow.TAB_MAIN.Collection || MainWindow.CurrentMainTabIndex == (int) MainWindow.TAB_MAIN.Pinned)
                ShowNextEpisode();
        }

        void btnSelectOverrideName_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window wdw = Window.GetWindow(this);

                wdw.Cursor = Cursors.Wait;

                VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
                if (ser == null) return;

                SelectAniDBTitleForm frm = new SelectAniDBTitleForm();
                frm.Owner = wdw;
                frm.Init(ser.AniDBAnime.AnimeTitles);
                wdw.Cursor = Cursors.Arrow;
                bool? result = frm.ShowDialog();
                if (result.Value)
                {
                    // update info
                    txtSeriesName.Text = frm.SelectedTitle;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }


        void chkSerNameOverride_Click(object sender, RoutedEventArgs e)
        {
            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;

            EvaluateEditing();
        }

        private void EvaluateEditing()
        {
            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;

            if ((bool)chkSerNameOverride.IsChecked)
            {
                txtSeriesName.Text = ser.SeriesNameOverride;
                txtSeriesName.IsEnabled = true;
                chkSerNameOverride.IsChecked = true;
                btnSelectOverrideName.IsEnabled = true;
            }
            else
            {
                txtSeriesName.Text = ser.SeriesName;
                txtSeriesName.IsEnabled = false;
                chkSerNameOverride.IsChecked = false;
                btnSelectOverrideName.IsEnabled = false;
            }
        }

        void btnRandomEpisode_Click(object sender, RoutedEventArgs e)
        {
            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;

            MainWindow mainwdw = (MainWindow)Window.GetWindow(this);

            RandomEpisodeForm frm = new RandomEpisodeForm();
            frm.Owner = Window.GetWindow(this); ;
            frm.Init(RandomSeriesEpisodeLevel.Series, ser);
            bool? result = frm.ShowDialog();
        }

        void btnPlaylistAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<VM_Playlist> rawPlaylists = VM_ShokoServer.Instance.ShokoServices.GetAllPlaylists().CastList<VM_Playlist>();
                PlaylistMenuCommand cmd = null;

                // get all playlists
                playlistMenu.Items.Clear();

                MenuItem itemSeries = new MenuItem();
                itemSeries.Header = Commons.Properties.Resources.Anime_AddSeries;
                itemSeries.Click += playlistMenuItem_Click;
                playlistMenu.Items.Add(itemSeries);

                Separator sep = new Separator();

                MenuItem itemSeriesNew = new MenuItem();
                itemSeriesNew.Header = Commons.Properties.Resources.Anime_NewPlaylists;
                itemSeriesNew.Click += playlistMenuItem_Click;
                cmd = new PlaylistMenuCommand(PlaylistItemType.Series, -1); // new playlist
                itemSeriesNew.CommandParameter = cmd;
                itemSeries.Items.Add(itemSeriesNew);
                itemSeries.Items.Add(sep);

                foreach (VM_Playlist contract in rawPlaylists)
                {
                    MenuItem itemSeriesPL = new MenuItem();
                    itemSeriesPL.Header = contract.PlaylistName;
                    itemSeriesPL.Click += playlistMenuItem_Click;
                    cmd = new PlaylistMenuCommand(PlaylistItemType.Series, contract.PlaylistID);
                    itemSeriesPL.CommandParameter = cmd;
                    itemSeries.Items.Add(itemSeriesPL);
                }

                MenuItem itemAllEpisodes = new MenuItem();
                itemAllEpisodes.Header = Commons.Properties.Resources.Anime_AddAllEpisodes;
                playlistMenu.Items.Add(itemAllEpisodes);

                Separator sep2 = new Separator();

                MenuItem itemAllEpisodesNew = new MenuItem();
                itemAllEpisodesNew.Header = Commons.Properties.Resources.Anime_NewPlaylists;
                itemAllEpisodesNew.Click += playlistMenuItem_Click;
                cmd = new PlaylistMenuCommand(PlaylistItemType.AllEpisodes, -1); // new playlist
                itemAllEpisodesNew.CommandParameter = cmd;
                itemAllEpisodes.Items.Add(itemAllEpisodesNew);
                itemAllEpisodes.Items.Add(sep2);

                foreach (VM_Playlist contract in rawPlaylists)
                {
                    MenuItem itemSeriesPL = new MenuItem();
                    itemSeriesPL.Header = contract.PlaylistName;
                    itemSeriesPL.Click += playlistMenuItem_Click;
                    cmd = new PlaylistMenuCommand(PlaylistItemType.AllEpisodes, contract.PlaylistID);
                    itemSeriesPL.CommandParameter = cmd;
                    itemAllEpisodes.Items.Add(itemSeriesPL);
                }


                MenuItem itemUnwatchedEpisodes = new MenuItem();
                itemUnwatchedEpisodes.Header = Commons.Properties.Resources.Anime_AddUnwatched;
                playlistMenu.Items.Add(itemUnwatchedEpisodes);

                Separator sep3 = new Separator();

                MenuItem itemUnwatchedEpisodesNew = new MenuItem();
                itemUnwatchedEpisodesNew.Header = Commons.Properties.Resources.Anime_NewPlaylists;
                itemUnwatchedEpisodesNew.Click += playlistMenuItem_Click;
                cmd = new PlaylistMenuCommand(PlaylistItemType.UnwatchedEpisodes, -1); // new playlist
                itemUnwatchedEpisodesNew.CommandParameter = cmd;
                itemUnwatchedEpisodes.Items.Add(itemUnwatchedEpisodesNew);
                itemUnwatchedEpisodes.Items.Add(sep3);

                foreach (VM_Playlist contract in rawPlaylists)
                {
                    MenuItem itemSeriesPL = new MenuItem();
                    itemSeriesPL.Header = contract.PlaylistName;
                    itemSeriesPL.Click += playlistMenuItem_Click;
                    cmd = new PlaylistMenuCommand(PlaylistItemType.UnwatchedEpisodes, contract.PlaylistID);
                    itemSeriesPL.CommandParameter = cmd;
                    itemUnwatchedEpisodes.Items.Add(itemSeriesPL);
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
                    Debug.Write(Commons.Properties.Resources.Anime_PlaylistMenu + " " + cmd + Environment.NewLine);

                    VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
                    if (ser == null) return;

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
                            MessageBox.Show(Commons.Properties.Resources.Anime_PlaylistMissing, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }                       
                    }

                    Cursor = Cursors.Wait;

                    // get the items to add to the playlist
                    if (cmd.AddType == PlaylistItemType.Series)
                    {
                        pl.AddSeries(ser.AnimeSeriesID);
                    }
                    else
                    {
                        List<VM_AnimeEpisode_User> eps = ser.AllEpisodes.OrderBy(a=>a.EpisodeType).ThenBy(a=>a.EpisodeNumber).ThenBy(a=>a.AniDB_AirDate.HasValue).ThenBy(a=>a.AniDB_AirDate).ToList();

                        if (cmd.AddType == PlaylistItemType.AllEpisodes)
                        {

                            foreach (VM_AnimeEpisode_User ep in eps)
                            {
                                if (ep.EpisodeTypeEnum == EpisodeType.Episode || ep.EpisodeTypeEnum == EpisodeType.Special)
                                    pl.AddEpisode(ep.AnimeEpisodeID);
                            }
                        }

                        if (cmd.AddType == PlaylistItemType.UnwatchedEpisodes)
                        {
                            foreach (VM_AnimeEpisode_User ep in eps)
                            {
                                if (!ep.Watched && (ep.EpisodeTypeEnum == EpisodeType.Episode || ep.EpisodeTypeEnum == EpisodeType.Special))
                                    pl.AddEpisode(ep.AnimeEpisodeID);
                            }
                        }
                    }

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


        private void Handle_Click(object sender, MouseButtonEventArgs e)
        {
            string tag = ((TextBlock)sender).Tag.ToString();

            if (tag.Equals("txtDescription", StringComparison.InvariantCultureIgnoreCase))
            {
                TruncatedDescription = !TruncatedDescription;
                FullDescription = !FullDescription;
            }
        }

        void btnEditSeriesFinish_Click(object sender, RoutedEventArgs e)
        {
            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;

            ser.IsBeingEdited = false;
            ser.IsReadOnly = true;



            string oldName = "";
            if (!string.IsNullOrEmpty(ser.SeriesNameOverride))
                oldName = ser.SeriesNameOverride;

            string newName = txtSeriesName.Text.Trim();
            if (chkSerNameOverride.IsChecked != null && (bool) !chkSerNameOverride.IsChecked)
                newName = "";

	        if (!oldName.Equals(newName))
	        {
		        // override name has changes so lets save to db
		        ser.SeriesNameOverride = newName;

		        ser.Save();

		        // prompt to change parent group name
		        MessageBoxResult res = MessageBox.Show(Commons.Properties.Resources.Anime_RenameParent,
                    Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
		        if (res == MessageBoxResult.Yes)
		        {
			        VM_AnimeGroup_User thisGrp = VM_MainListHelper.Instance.AllGroupsDictionary[ser.AnimeGroupID];
			        if (thisGrp != null)
			        {
				        thisGrp.GroupName = ser.GroupName;
				        thisGrp.SortName = ser.SeriesName;
				        thisGrp.Save();
			        }
		        }
	        }
	        else
	        {
		        ser.Save();
	        }

            //VM_MainListHelper.Instance.RefreshGroupsSeriesData();

            EvaluateEditing();
        }

        void btnEditSeries_Click(object sender, RoutedEventArgs e)
        {
            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;

            if (string.IsNullOrEmpty(ser.SeriesNameOverride))
                PreviousOverrideName = "";
            else
                PreviousOverrideName = ser.SeriesNameOverride;

            ser.IsBeingEdited = true;
            ser.IsReadOnly = false;
        }

        private void SetSeriesWidgetOrder()
        {

            SeriesPos_PlayNextEpisode = VM_UserSettings.Instance.GetSeriesWidgetPosition(SeriesWidgets.PlayNextEpisode) + 4;
            SeriesPos_TvDBLinks = VM_UserSettings.Instance.GetSeriesWidgetPosition(SeriesWidgets.TvDBLinks) + 4;
            SeriesPos_Titles = VM_UserSettings.Instance.GetSeriesWidgetPosition(SeriesWidgets.Titles) + 4;
            SeriesPos_Tags = VM_UserSettings.Instance.GetSeriesWidgetPosition(SeriesWidgets.Tags) + 4;
            SeriesPos_CustomTags = VM_UserSettings.Instance.GetSeriesWidgetPosition(SeriesWidgets.CustomTags) + 4;
        }



        void AnimeSeries_LayoutUpdated(object sender, EventArgs e)
        {
        }

        private void CommandBinding_MoveUpWidget(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            SeriesWidgets swid = (SeriesWidgets)int.Parse(obj.ToString());

            VM_UserSettings.Instance.MoveUpSeriesWidget(swid);
            SetSeriesWidgetOrder();
        }

        private void CommandBinding_MoveDownWidget(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            SeriesWidgets swid = (SeriesWidgets)int.Parse(obj.ToString());

            VM_UserSettings.Instance.MoveDownSeriesWidget(swid);
            SetSeriesWidgetOrder();
        }

        private void CommandBinding_ViewImage(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_PosterContainer))
                {
                    VM_PosterContainer poster = (VM_PosterContainer)obj;
                    Utils.OpenFile(poster.FullImagePath);
                }

                if (obj.GetType() == typeof(VM_FanartContainer))
                {
                    VM_FanartContainer fanart = (VM_FanartContainer)obj;
                    Utils.OpenFile(fanart.FullImagePath);
                }

                /*
				if (obj.GetType() == typeof(TvDB_ImagePosterVM))
				{
					TvDB_ImagePosterVM poster = (TvDB_ImagePosterVM)obj;
					Utils.OpenFile(poster.FullImagePath);
				}

				if (obj.GetType() == typeof(TvDB_ImageFanartVM))
				{
					TvDB_ImageFanartVM fanart = (TvDB_ImageFanartVM)obj;
					Utils.OpenFile(fanart.FullImagePath);
				}*/

                if (obj.GetType() == typeof(VM_TvDB_ImageWideBanner))
                {
                    VM_TvDB_ImageWideBanner banner = (VM_TvDB_ImageWideBanner)obj;
                    Utils.OpenFile(banner.FullImagePath);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_DisableImage(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            EnableDisableImage(false, obj);
        }

        private void CommandBinding_EnableImage(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            EnableDisableImage(true, obj);
        }

        private void CommandBinding_SelectTitleTextAndCopy(object sender, ExecutedRoutedEventArgs e)
        {
            string obj = e.Parameter as string;
            Utils.CopyToClipboard(obj);
        }

        private void EnableDisableImage(bool enabled, object img)
        {
            if (!VM_ShokoServer.Instance.ServerOnline) return;

            try
            {
                VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
                if (ser == null) return;

                Cursor = Cursors.Wait;
                string res = "";


                // NOTE if we are disabling an image we should also make sure it is not the default

                if (img.GetType() == typeof(VM_PosterContainer))
                {
                    VM_PosterContainer poster = (VM_PosterContainer)img;

                    if (!enabled && poster.IsImageDefault)
                        SetDefaultImage(false, img, false);

                    switch (poster.ImageType)
                    {
                        case ImageEntityType.TvDB_Cover:
                            VM_TvDB_ImagePoster tvPoster = poster.PosterObject as VM_TvDB_ImagePoster;
                            res = VM_ShokoServer.Instance.ShokoServices.EnableDisableImage(enabled, tvPoster.TvDB_ImagePosterID, (int)ImageEntityType.TvDB_Cover);
                            tvPoster.Enabled = enabled ? 1 : 0;
                            break;

                        case ImageEntityType.AniDB_Cover:
                            VM_AniDB_Anime anime = poster.PosterObject as VM_AniDB_Anime;
                            res = VM_ShokoServer.Instance.ShokoServices.EnableDisableImage(enabled, anime.AnimeID, (int)ImageEntityType.AniDB_Cover);
                            anime.ImageEnabled = enabled ? 1 : 0;
                            break;

                        case ImageEntityType.MovieDB_Poster:
                            VM_MovieDB_Poster moviePoster = poster.PosterObject as VM_MovieDB_Poster;
                            res = VM_ShokoServer.Instance.ShokoServices.EnableDisableImage(enabled, moviePoster.MovieDB_PosterID, (int)ImageEntityType.MovieDB_Poster);
                            moviePoster.Enabled = enabled ? 1 : 0;
                            break;
                    }
                    poster.IsImageEnabled = enabled;
                }

                if (img.GetType() == typeof(VM_FanartContainer))
                {
                    VM_FanartContainer fanart = (VM_FanartContainer)img;

                    if (!enabled && fanart.IsImageDefault)
                        SetDefaultImage(false, img, false);

                    switch (fanart.ImageType)
                    {
                        case ImageEntityType.TvDB_FanArt:
                            VM_TvDB_ImageFanart tvFanart = fanart.FanartObject as VM_TvDB_ImageFanart;
                            res = VM_ShokoServer.Instance.ShokoServices.EnableDisableImage(enabled, tvFanart.TvDB_ImageFanartID, (int)ImageEntityType.TvDB_FanArt);
                            tvFanart.Enabled = enabled ? 1 : 0;
                            break;

                        case ImageEntityType.MovieDB_FanArt:
                            VM_MovieDB_Fanart movieFanart = fanart.FanartObject as VM_MovieDB_Fanart;
                            res = VM_ShokoServer.Instance.ShokoServices.EnableDisableImage(enabled, movieFanart.MovieDB_FanartID, (int)ImageEntityType.MovieDB_FanArt);
                            movieFanart.Enabled = enabled ? 1 : 0;
                            break;
                    }
                    fanart.IsImageEnabled = enabled;
                }

                if (img.GetType() == typeof(VM_TvDB_ImageWideBanner))
                {
                    VM_TvDB_ImageWideBanner banner = (VM_TvDB_ImageWideBanner)img;

                    if (!enabled && banner.IsImageDefault)
                        SetDefaultImage(false, img, false);

                    res = VM_ShokoServer.Instance.ShokoServices.EnableDisableImage(enabled, banner.TvDB_ImageWideBannerID, (int)ImageEntityType.TvDB_Banner);
                    banner.Enabled = enabled ? 1 : 0;
                }

                if (res.Length > 0)
                {
                    MessageBox.Show(res, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    VM_MainListHelper.Instance.UpdateHeirarchy(ser);
                    VM_MainListHelper.Instance.UpdateAnime(ser.AniDB_ID);
                    RefreshImagesData();
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

        private void CommandBinding_SetDefaultImageON(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            SetDefaultImage(true, obj, true);
        }

        private void CommandBinding_SetDefaultImageOFF(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            SetDefaultImage(false, obj, true);
        }

        private void SetDefaultImage(bool isDefault, object img, bool refreshData)
        {
            if (!VM_ShokoServer.Instance.ServerOnline) return;

            try
            {
                VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
                if (ser == null) return;

                Cursor = Cursors.Wait;
                string res = "";

                string disabledMessage = Commons.Properties.Resources.Anime_DisabledImage;

                if (img.GetType() == typeof(VM_PosterContainer))
                {
                    VM_PosterContainer poster = (VM_PosterContainer)img;

                    if (isDefault && !poster.IsImageEnabled)
                    {
                        MessageBox.Show(disabledMessage, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    switch (poster.ImageType)
                    {
                        case ImageEntityType.TvDB_Cover:
                            VM_TvDB_ImagePoster tvPoster = poster.PosterObject as VM_TvDB_ImagePoster;
                            res = VM_ShokoServer.Instance.ShokoServices.SetDefaultImage(isDefault, ser.AniDB_ID,
                                tvPoster.TvDB_ImagePosterID, (int)ImageEntityType.TvDB_Cover, (int)ImageSizeType.Poster);
                            tvPoster.IsImageDefault = isDefault;
                            break;

                        case ImageEntityType.AniDB_Cover:
                            VM_AniDB_Anime anime = poster.PosterObject as VM_AniDB_Anime;
                            res = VM_ShokoServer.Instance.ShokoServices.SetDefaultImage(isDefault, ser.AniDB_ID,
                                anime.AnimeID, (int)ImageEntityType.AniDB_Cover, (int)ImageSizeType.Poster);
                            anime.IsImageDefault = isDefault;
                            break;

                        case ImageEntityType.MovieDB_Poster:
                            VM_MovieDB_Poster moviePoster = poster.PosterObject as VM_MovieDB_Poster;
                            res = VM_ShokoServer.Instance.ShokoServices.SetDefaultImage(isDefault, ser.AniDB_ID,
                                moviePoster.MovieDB_PosterID, (int)ImageEntityType.MovieDB_Poster, (int)ImageSizeType.Poster);
                            moviePoster.IsImageDefault = isDefault;
                            break;
                    }
                    poster.IsImageDefault = isDefault;
                }

                if (img.GetType() == typeof(VM_FanartContainer))
                {
                    VM_FanartContainer fanart = (VM_FanartContainer)img;

                    if (isDefault && !fanart.IsImageEnabled)
                    {
                        MessageBox.Show(disabledMessage, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    switch (fanart.ImageType)
                    {
                        case ImageEntityType.TvDB_FanArt:
                            VM_TvDB_ImageFanart tvFanart = fanart.FanartObject as VM_TvDB_ImageFanart;
                            res = VM_ShokoServer.Instance.ShokoServices.SetDefaultImage(isDefault, ser.AniDB_ID,
                                tvFanart.TvDB_ImageFanartID, (int)ImageEntityType.TvDB_FanArt, (int)ImageSizeType.Fanart);
                            tvFanart.IsImageDefault = isDefault;
                            break;

                        case ImageEntityType.MovieDB_FanArt:
                            VM_MovieDB_Fanart movieFanart = fanart.FanartObject as VM_MovieDB_Fanart;
                            res = VM_ShokoServer.Instance.ShokoServices.SetDefaultImage(isDefault, ser.AniDB_ID,
                                movieFanart.MovieDB_FanartID, (int)ImageEntityType.MovieDB_FanArt, (int)ImageSizeType.Fanart);
                            movieFanart.IsImageDefault = isDefault;
                            break;
                    }
                    fanart.IsImageDefault = isDefault;

                }

                if (img.GetType() == typeof(VM_TvDB_ImageWideBanner))
                {
                    VM_TvDB_ImageWideBanner banner = (VM_TvDB_ImageWideBanner)img;

                    if (isDefault && !banner.IsImageEnabled)
                    {
                        MessageBox.Show(disabledMessage, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }


                    res = VM_ShokoServer.Instance.ShokoServices.SetDefaultImage(isDefault, ser.AniDB_ID,
                                banner.TvDB_ImageWideBannerID, (int)ImageEntityType.TvDB_Banner, (int)ImageSizeType.WideBanner);
                    banner.IsImageDefault = isDefault;

                }

                if (res.Length > 0)
                {
                    MessageBox.Show(res, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    if (refreshData)
                    {
                        VM_MainListHelper.Instance.UpdateHeirarchy(ser);
                        VM_MainListHelper.Instance.UpdateAnime(ser.AniDB_ID);
                        RefreshImagesData();
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

        void btnUpdateAniDBInfo_Click(object sender, RoutedEventArgs e)
        {
            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;

            try
            {
                Cursor = Cursors.Wait;
                VM_ShokoServer.Instance.ShokoServices.UpdateAnimeData(ser.AniDB_ID);

                // refresh the data
                VM_MainListHelper.Instance.UpdateHeirarchy(ser);
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

        void tabContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                TabControl tab = e.Source as TabControl;
                if (tab.SelectedIndex == 1) // episodes
                {
                    Cursor = Cursors.Wait;
                    epListMain.PopulateToolbars();
                    Cursor = Cursors.Arrow;
                }
                else if (tab.SelectedIndex == 2) // images
                {
                    RefreshImagesData();
                }
                else if (tab.SelectedIndex == 3) // related and similar
                {
                    Cursor = Cursors.Wait;
                    ucSimilarAnime.RefreshData();
                    ucRelatedAnime.RefreshData();
                    Cursor = Cursors.Arrow;
                }
                else if (tab.SelectedIndex == 4) // trakt shouts
                {
                    Cursor = Cursors.Wait;
                    ucTraktShouts.RefreshComments();
                    Cursor = Cursors.Arrow;
                }
                else if (tab.SelectedIndex == 5) // files
                {
                    Cursor = Cursors.Wait;
                    ShowFileSummary();
                    Cursor = Cursors.Arrow;
                }

            }
        }

        private void RefreshImagesData()
        {
            Cursor = Cursors.Wait;

            try
            {
                VM_AniDB_AnimeCrossRefs AniDB_AnimeCrossRefs = null;
                SeriesTvDBWideBanners = null;
                AllPosters = null;
                AllFanarts = null;

                VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
                if (ser == null) return;

                AniDB_AnimeCrossRefs = (VM_AniDB_AnimeCrossRefs)VM_ShokoServer.Instance.ShokoServices.GetCrossRefDetails(ser.AniDB_ID);
                if (AniDB_AnimeCrossRefs == null) return;

                SeriesTvDBWideBanners = AniDB_AnimeCrossRefs.TvDBImageWideBanners;

                AllPosters = AniDB_AnimeCrossRefs.AllPosters;
                AllFanarts = AniDB_AnimeCrossRefs.AllFanarts;
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

        void btnTvDBLinks_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.SeriesTvDBLinksExpanded = !VM_UserSettings.Instance.SeriesTvDBLinksExpanded;

            ShowTvDBLinks();


        }

        void btnAnimeTitles_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.TitlesExpanded = !VM_UserSettings.Instance.TitlesExpanded;
        }

        void btnAnimeTags_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.TagsExpanded = !VM_UserSettings.Instance.TagsExpanded;
        }

        void btnCustomTags_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.CustomTagsExpanded = !VM_UserSettings.Instance.CustomTagsExpanded;
        }

        void btnManageTags_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window wdw = Window.GetWindow(this);

                ManageCustomTags frm = new ManageCustomTags();
                frm.Owner = wdw;
                bool? result = frm.ShowDialog();
                if (result.Value)
                {
                    // refresh
                    //RefreshRecords();
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnAddCustomTag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboCustomTag.SelectedItem == null) return;

                VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
                if (ser == null) return;

                VM_CustomTag tag = cboCustomTag.SelectedItem as VM_CustomTag;

                // check if we already have this tag
                foreach (VM_CustomTag ctag in ser.AniDBAnime.CustomTags)
                {
                    if (ctag.CustomTagID == tag.CustomTagID)
                        return;
                }

                CrossRef_CustomTag xref = new CrossRef_CustomTag();
                xref.CrossRefID = ser.AniDB_ID;
                xref.CrossRefType = (int) CustomTagCrossRefType.Anime;
                xref.CustomTagID = tag.CustomTagID;

                CL_Response<CrossRef_CustomTag>
                    resp = VM_ShokoServer.Instance.ShokoServices.SaveCustomTagCrossRef(xref);
                if (!string.IsNullOrEmpty(resp.ErrorMessage))
                {
                    Utils.ShowErrorMessage(resp.ErrorMessage);
                }
                else
                {
                    ser.AniDBAnime.CustomTags.Add(tag);
                    ser.AniDBAnime.ViewCustomTags.Refresh();
                    VM_MainListHelper.Instance.RefreshGroupsSeriesData();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_DeleteCustomTag(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
                if (ser == null) return;

                Cursor = Cursors.Wait;
                string res = "";


                // NOTE if we are disabling an image we should also make sure it is not the default
                VM_CustomTag tag = null;
                if (obj.GetType() == typeof(VM_CustomTag))
                {
                    tag = (VM_CustomTag)obj;
                    res = VM_ShokoServer.Instance.ShokoServices.DeleteCustomTagCrossRef(
                        tag.CustomTagID, (int)CustomTagCrossRefType.Anime, ser.AniDB_ID);

                }

                if (res.Length > 0)
                {
                    MessageBox.Show(res, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    VM_CustomTag ctagToRemove = null;
                    foreach (VM_CustomTag ctag in ser.AniDBAnime.CustomTags)
                    {
                        if (ctag.CustomTagID == tag.CustomTagID)
                        {
                            ctagToRemove = ctag;
                            break;
                        }

                    }

                    if (ctagToRemove != null)
                    {
                        ser.AniDBAnime.CustomTags.Remove(ctagToRemove);
                        ser.AniDBAnime.ViewCustomTags.Refresh();
                        VM_MainListHelper.Instance.UpdateAll();
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

        /*void btnPlayNextEpisode_Click(object sender, RoutedEventArgs e)
		{
			VM_UserSettings.Instance.SeriesNextEpisodeExpanded = !VM_UserSettings.Instance.SeriesNextEpisodeExpanded;

			ShowNextEpisode();
		}*/

        private void CommandBinding_RefreshSeries(object sender, ExecutedRoutedEventArgs e)
        {
            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;
            VM_MainListHelper.Instance.RefreshGroupsSeriesData();

            LoadSeries();
        }

        public void LoadSeries()
        {
            Cursor = Cursors.Wait;

            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;

            ser.AniDBAnime?.AniDBAnime.ClearTvDBData();
            ser.AniDBAnime?.AniDBAnime.ClearTraktData();

            ser.RefreshEpisodes();

            FullDescription = false;
            TruncatedDescription = true;

            ShowFileSummary();
            ShowTvDBLinks();
            ShowNextEpisode();

            ser.PopulateIsFave();

            epListMain.DataContext = ser;
            ucSimilarAnime.DataContext = ser;
            ucRelatedAnime.DataContext = ser;
            ucTraktShouts.DataContext = ser;

            if (tabContainer.SelectedIndex == 1) // episodes
            {
                epListMain.PopulateToolbars();
            }
            else if (tabContainer.SelectedIndex == 2) // images
            {
                RefreshImagesData();
            }
            else if (tabContainer.SelectedIndex == 3) // related and similar
            {
                Cursor = Cursors.Wait;
                ucSimilarAnime.RefreshData();
                ucRelatedAnime.RefreshData();
                Cursor = Cursors.Arrow;
            }
            else if (tabContainer.SelectedIndex == 4) // trakt shouts
            {
                Cursor = Cursors.Wait;
                ucTraktShouts.RefreshComments();
                Cursor = Cursors.Arrow;
            }
            else if (tabContainer.SelectedIndex == 5) // files
            {
                Cursor = Cursors.Wait;
                ShowFileSummary();
                Cursor = Cursors.Arrow;
            }

            cboVoteType.Items.Clear();
            cboVoteType.Items.Add(Commons.Properties.Resources.VoteTypeAnimeTemporary);
            if (ser.AniDBAnime.AniDBAnime.FinishedAiring)
                cboVoteType.Items.Add(Commons.Properties.Resources.VoteTypeAnimePermanent);

            if (ser.AniDBAnime.AniDBAnime.FinishedAiring && ser.AllFilesWatched)
                cboVoteType.SelectedIndex = 1;
            else
                cboVoteType.SelectedIndex = 0;


            EvaluateEditing();

            Cursor = Cursors.Arrow;
        }

        void AnimeSeries_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LoadSeries();
        }


        private void ShowFileSummary()
        {
            if (tabContainer.SelectedIndex == 5)
            {
                VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
                if (ser == null) return;
                ucFileSummary.DataContext = ser.AniDBAnime.AniDBAnime;
                ucFolderSummary.DataContext = ser.AniDBAnime.AniDBAnime;
            }
        }

        private void ShowTvDBLinks()
        {
            if (VM_UserSettings.Instance.SeriesTvDBLinksExpanded)
            {
                VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
                if (ser == null) return;
                ucTvDBLinks.DataContext = ser.AniDBAnime.AniDBAnime;
            }
        }

        private void ShowNextEpisode()
        {
            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;

            VM_AnimeEpisode_User ep = (VM_AnimeEpisode_User)VM_ShokoServer.Instance.ShokoServices.GetNextUnwatchedEpisode(ser.AnimeSeriesID,
                VM_ShokoServer.Instance.CurrentUser.JMMUserID);
            if (ep != null)
            {
                ep.SetTvDBInfo();
                ucNextEpisode.DataContext = ep;
            }
            else
            {
                ucNextEpisode.EpisodeExists = false;
                ucNextEpisode.EpisodeMissing = true;
                ucNextEpisode.DataContext = null;
            }
        }

        void btnAnimeGroupShow_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.SeriesGroupExpanded = !VM_UserSettings.Instance.SeriesGroupExpanded;
        }



        void AnimeSeries_Loaded(object sender, RoutedEventArgs e)
        {
            if (cboCustomTag.Items != null && cboCustomTag.Items.Count > 0)
                cboCustomTag.SelectedIndex = 0;
        }

        void cRating_OnRatingValueChangedEvent(RatingValueEventArgs ev)
        {
            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;

            try
            {
                decimal rating = (decimal)ev.RatingValue;

                int voteType = 1;
                if (cboVoteType.SelectedItem.ToString() == Commons.Properties.Resources.VoteTypeAnimeTemporary) voteType = 2;
                if (cboVoteType.SelectedItem.ToString() == Commons.Properties.Resources.VoteTypeAnimePermanent) voteType = 1;

                VM_ShokoServer.Instance.VoteAnime(ser.AniDB_ID, rating, voteType);

                // refresh the data
                //ser.RefreshBase();
                //ser.AniDB_Anime.Detail.RefreshBase();
                VM_MainListHelper.Instance.UpdateHeirarchy(ser);
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
                        MessageBox.Show(response.ErrorMessage, Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    VM_MainListHelper.Instance.UpdateHeirarchy((VM_AnimeEpisode_User)response.Result);

                    serTemp = VM_MainListHelper.Instance.GetSeriesForEpisode(ep);
                }

                ShowNextEpisode();

                if (newStatus && serTemp != null)
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

    public class PlaylistMenuCommand
    {
        public PlaylistItemType AddType { get; set; }
        public int PlaylistID { get; set; }

        public PlaylistMenuCommand()
        {
        }

        public PlaylistMenuCommand(PlaylistItemType addType, int playlistID)
        {
            AddType = addType;
            PlaylistID = playlistID;
        }

        public override string ToString()
        {
            return $"{AddType} - {PlaylistID}";
        }
    }

    public enum PlaylistItemType
    {
        Series = 1,
        AllEpisodes = 2,
        UnwatchedEpisodes = 3,
        SingleEpisode = 4
    }


}

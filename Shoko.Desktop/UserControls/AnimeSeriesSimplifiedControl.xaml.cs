using NLog;
using System;
using System.Collections.Generic;
//using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Enums;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Metro;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for AnimeSeriesSimplifiedControl.xaml
    /// </summary>
    public partial class AnimeSeriesSimplifiedControl : UserControl
    {

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public ObservableCollection<VM_AnimeEpisode_User> UnwatchedEpisodes { get; set; }
        public ICollectionView ViewUnwatchedEpisodes { get; set; }

        public ObservableCollectionEx<RecommendationTile> Recommendations { get; set; }

        public ObservableCollection<object> Comments { get; set; }

        public ObservableCollection<VM_AniDB_Character> Characters { get; set; }
        public ICollectionView ViewCharacters { get; set; }


        private readonly BackgroundWorker episodesWorker = new BackgroundWorker();
        private readonly BackgroundWorker recsWorker = new BackgroundWorker();
        private readonly BackgroundWorker commentsWorker = new BackgroundWorker();
        private readonly BackgroundWorker postCommenttWorker = new BackgroundWorker();
        private readonly BackgroundWorker refreshCommentsRecsWorker = new BackgroundWorker();
        private readonly BackgroundWorker charWorker = new BackgroundWorker();

        public static readonly DependencyProperty UnwatchedEpisodeCountProperty = DependencyProperty.Register("UnwatchedEpisodeCount",
            typeof(int), typeof(AnimeSeriesSimplifiedControl), new UIPropertyMetadata(0, null));

        public int UnwatchedEpisodeCount
        {
            get => (int) GetValue(UnwatchedEpisodeCountProperty);
            set => SetValue(UnwatchedEpisodeCountProperty, value);
        }

        public static readonly DependencyProperty PosterWidthProperty = DependencyProperty.Register("PosterWidth",
            typeof(double), typeof(AnimeSeriesSimplifiedControl), new UIPropertyMetadata((double)180, null));

        public double PosterWidth
        {
            get => (double) GetValue(PosterWidthProperty);
            set => SetValue(PosterWidthProperty, value);
        }

        public static readonly DependencyProperty IsLoadingCommentsProperty = DependencyProperty.Register("IsLoadingComments",
            typeof(bool), typeof(AnimeSeriesSimplifiedControl), new UIPropertyMetadata(false, null));

        public bool IsLoadingComments
        {
            get => (bool) GetValue(IsLoadingCommentsProperty);
            set
            {
                SetValue(IsLoadingCommentsProperty, value);
                IsNotLoadingComments = !value;
            }
        }

        public static readonly DependencyProperty IsNotLoadingCommentsProperty = DependencyProperty.Register("IsNotLoadingComments",
            typeof(bool), typeof(AnimeSeriesSimplifiedControl), new UIPropertyMetadata(false, null));

        public bool IsNotLoadingComments
        {
            get => (bool) GetValue(IsNotLoadingCommentsProperty);
            set => SetValue(IsNotLoadingCommentsProperty, value);
        }

        public AnimeSeriesSimplifiedControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            UnwatchedEpisodes = new ObservableCollection<VM_AnimeEpisode_User>();
            ViewUnwatchedEpisodes = CollectionViewSource.GetDefaultView(UnwatchedEpisodes);

            Characters = new ObservableCollection<VM_AniDB_Character>();
            ViewCharacters = CollectionViewSource.GetDefaultView(Characters);

            Recommendations = new ObservableCollectionEx<RecommendationTile>();
            Comments = new ObservableCollection<object>();

            DataContextChanged += AnimeSeriesSimplifiedControl_DataContextChanged;

            btnBack.Click += btnBack_Click;
            btnPlayNextEp.Click += btnPlayNextEp_Click;
            btnPlayAllEps.Click += btnPlayAllEps_Click;
            btnRefresh.Click += BtnRefresh_Click;
            btnSwitchView.Click += BtnSwitchView_Click;

            episodesWorker.DoWork += episodesWorker_DoWork;
            episodesWorker.RunWorkerCompleted += episodesWorker_RunWorkerCompleted;

            charWorker.DoWork += charWorker_DoWork;
            charWorker.RunWorkerCompleted += charWorker_RunWorkerCompleted;

            recsWorker.DoWork += recsWorker_DoWork;
            recsWorker.RunWorkerCompleted += recsWorker_RunWorkerCompleted;

            commentsWorker.DoWork += commentsWorker_DoWork;
            commentsWorker.RunWorkerCompleted += commentsWorker_RunWorkerCompleted;

            refreshCommentsRecsWorker.DoWork += refreshCommentsRecsWorker_DoWork;
            refreshCommentsRecsWorker.RunWorkerCompleted += refreshCommentsRecsWorker_RunWorkerCompleted;

            MainWindow.videoHandler.VideoWatchedEvent += videoHandler_VideoWatchedEvent;
            Unloaded += (sender, e) => MainWindow.videoHandler.VideoWatchedEvent -= videoHandler_VideoWatchedEvent;

            txtCommentNew.GotFocus += txtCommentNew_GotFocus;
            txtCommentNew.LostFocus += txtCommentNew_LostFocus;
            btnSubmitComment.Click += btnSubmitComment_Click;

            btnRefreshComments.Click += btnRefreshComments_Click;

            postCommenttWorker.DoWork += postCommentWorker_DoWork;
            postCommenttWorker.RunWorkerCompleted += postCommentWorker_RunWorkerCompleted;

            cRating.OnRatingValueChangedEvent += cRating_OnRatingValueChangedEvent;

            grdMain.PreviewMouseWheel += grdMain_PreviewMouseWheel;
            lbEpisodes.PreviewMouseWheel += lbEpisodes_PreviewMouseWheel;
            lbComments.PreviewMouseWheel += lbComments_PreviewMouseWheel;
            lbChars.PreviewMouseWheel += lbComments_PreviewMouseWheel;
            lbRecommendations.PreviewMouseWheel += lbComments_PreviewMouseWheel;
            PreviewMouseWheel += AnimeSeriesSimplifiedControl_PreviewMouseWheel;
        }

        private void AnimeSeriesSimplifiedControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                foreach (ScrollViewer sv in Utils.GetScrollViewers(this))
                    sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta / 3D);
            }
            catch
            {
                // ignored
            }
        }

        private void BtnSwitchView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AppSettings.DisplaySeriesSimple = false;
                // check if this control is part of the series container
                DependencyObject parentObject = VisualTreeHelper.GetParent(this);
                while (parentObject != null)
                {
                    parentObject = VisualTreeHelper.GetParent(parentObject);
                    AnimeSeriesContainerControl containerCtrl = parentObject as AnimeSeriesContainerControl;
                    if (containerCtrl != null)
                    {
                        // show the full view
                        VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
                        if (ser == null) return;

                        AnimeSeries seriesControl = new AnimeSeries();
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

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        void lbComments_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                foreach (ScrollViewer sv in Utils.GetScrollViewers(this))
                    sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta / 3D);
            }
            catch
            {
                // ignored
            }
        }

        void lbEpisodes_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                foreach (ScrollViewer sv in Utils.GetScrollViewers(this))
                    sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta / 3D);
            }
            catch
            {
                // ignored
            }
        }

        void grdMain_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                foreach (ScrollViewer sv in Utils.GetScrollViewers(this))
                    sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta / 3D);
            }
            catch
            {
                // ignored
            }
        }

        void cRating_OnRatingValueChangedEvent(RatingValueEventArgs ev)
        {
            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;

            try
            {
                decimal rating = (decimal)ev.RatingValue;

                int voteType = 1;
                if (ser.AniDBAnime.AniDBAnime.FinishedAiring) voteType = 2;

                VM_ShokoServer.Instance.VoteAnime(ser.AniDB_ID, rating, voteType);

                // refresh the data
                VM_MainListHelper.Instance.UpdateHeirarchy(ser);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void postCommentWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string msg = e.Result.ToString();
            Cursor = Cursors.Arrow;

            MessageBox.Show(msg, Shoko.Commons.Properties.Resources.Anime_Message, MessageBoxButton.OK, MessageBoxImage.Information);
            txtCommentNew.Text = "";
            btnSubmitComment.IsEnabled = true;

            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;

            commentsWorker.RunWorkerAsync(ser);
        }

        void postCommentWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            VM_Trakt_CommentPost comment = e.Argument as VM_Trakt_CommentPost;

            string msg;
            try
            {
                CL_Response<bool> r=VM_ShokoServer.Instance.ShokoServices.PostTraktCommentShow(comment.TraktID, comment.CommentText, comment.Spoiler);
                msg = r.ErrorMessage;
            }
            catch (Exception ex)
            {
                e.Result = ex.Message;
                return;
            }

            e.Result = msg;
        }

        void btnSubmitComment_Click(object sender, RoutedEventArgs e)
        {
            if (!VM_ShokoServer.Instance.Trakt_IsEnabled)
            {
                Utils.ShowErrorMessage(Shoko.Commons.Properties.Resources.Anime_TraktNotEnabled);
                txtCommentNew.Focus();
                return;
            }

            if (string.IsNullOrEmpty(VM_ShokoServer.Instance.Trakt_AuthToken))
            {
                Utils.ShowErrorMessage(Shoko.Commons.Properties.Resources.Anime_ShokoAuth);
                txtCommentNew.Focus();
                return;
            }

            VM_AnimeSeries_User animeSeries = (VM_AnimeSeries_User)DataContext;
            if (animeSeries == null)
            {
                Utils.ShowErrorMessage(Shoko.Commons.Properties.Resources.Anime_SeriesNotFound);
                txtCommentNew.Focus();
                return;
            }

            string commentText = txtCommentNew.Text.Trim();
            if (string.IsNullOrEmpty(commentText))
            {
                Utils.ShowErrorMessage(Shoko.Commons.Properties.Resources.Anime_EnterText);
                txtCommentNew.Focus();
                return;
            }

            if (commentText.Length > 2000)
            {
                Utils.ShowErrorMessage(string.Format(Shoko.Commons.Properties.Resources.Anime_CommentText, commentText.Length));
                txtCommentNew.Focus();
                return;
            }

            btnSubmitComment.IsEnabled = false;

            if (animeSeries.AniDBAnime.AniDBAnime.traktSummary != null)
            {
                string traktID = string.Empty;

                // check to  see if this series is linked to more than one Trakt series
                if (animeSeries.AniDBAnime.AniDBAnime.traktSummary.traktDetails == null ||
                    animeSeries.AniDBAnime.AniDBAnime.traktSummary.traktDetails.Count == 0)
                {
                    Utils.ShowErrorMessage(string.Format(Shoko.Commons.Properties.Resources.Anime_NoTrakt));
                    txtCommentNew.Focus();
                    btnSubmitComment.IsEnabled = true;
                    return;
                }

                // check to  see if this series is linked to more than one Trakt series
                if (animeSeries.AniDBAnime.AniDBAnime.traktSummary.traktDetails != null &&
                    animeSeries.AniDBAnime.AniDBAnime.traktSummary.traktDetails.Count > 1)
                {
                    Utils.ShowErrorMessage(string.Format(Shoko.Commons.Properties.Resources.Anime_MultiTrakt));
                    txtCommentNew.Focus();
                    btnSubmitComment.IsEnabled = true;
                    return;
                }

                if (animeSeries.AniDBAnime.AniDBAnime.traktSummary.traktDetails != null &&
                    animeSeries.AniDBAnime.AniDBAnime.traktSummary.traktDetails.Count == 1)
                {

                    Cursor = Cursors.Wait;

                    foreach (KeyValuePair<string, VM_TraktDetails> kvp in animeSeries.AniDBAnime.AniDBAnime.traktSummary.traktDetails)
                    { traktID = kvp.Key; }

                    VM_Trakt_CommentPost comment = new VM_Trakt_CommentPost();
                    comment.TraktID = traktID;
                    comment.AnimeID = animeSeries.AniDB_ID;
                    comment.CommentText = commentText;
                    comment.Spoiler = chkSpoiler.IsChecked.Value;

                    postCommenttWorker.RunWorkerAsync(comment);
                }
            }
            else
            {
                Utils.ShowErrorMessage(string.Format(Shoko.Commons.Properties.Resources.Anime_NoTrakt));
                txtCommentNew.Focus();
                btnSubmitComment.IsEnabled = true;
            }
        }

        void btnRefreshComments_Click(object sender, RoutedEventArgs e)
        {
            VM_AnimeSeries_User animeSeries = (VM_AnimeSeries_User)DataContext;
            if (animeSeries == null) return;

            IsLoadingComments = true;

            refreshCommentsRecsWorker.RunWorkerAsync(animeSeries);
        }



        void refreshCommentsRecsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            VM_AnimeSeries_User animeSeries = (VM_AnimeSeries_User)DataContext;
            if (animeSeries == null) return;

            commentsWorker.RunWorkerAsync(animeSeries);
        }

        void refreshCommentsRecsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                VM_AnimeSeries_User ser = e.Argument as VM_AnimeSeries_User;

                VM_ShokoServer.Instance.ShokoServices.UpdateAnimeData(ser.AniDB_ID);

                // refresh the data
                VM_MainListHelper.Instance.UpdateHeirarchy(ser);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }
        }

        void txtCommentNew_LostFocus(object sender, RoutedEventArgs e)
        {
            txtCommentNew.Height = 30;
            txtCommentNew.Foreground = Brushes.DarkGray;

            if (txtCommentNew.Text.Trim().Length == 0)
                txtCommentNew.Text = Shoko.Commons.Properties.Resources.Anime_YourSay;
        }

        void txtCommentNew_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtCommentNew.Text.Equals(Shoko.Commons.Properties.Resources.Anime_YourSay, StringComparison.InvariantCultureIgnoreCase))
                txtCommentNew.Text = string.Empty;

            txtCommentNew.Foreground = Brushes.Black;
            txtCommentNew.Height = 150;
        }


        void videoHandler_VideoWatchedEvent(VideoPlayers.VideoWatchedEventArgs ev)
        {
            try
            {
                MainWindow mainwdw = (MainWindow) Window.GetWindow(this);

                if (MainWindow.CurrentMainTabIndex == (int) MainWindow.TAB_MAIN.Dashboard &&
                    mainwdw?.tileContinueWatching.Visibility == Visibility.Visible)
                    RefreshData();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        void btnPlayAllEps_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
                if (ser == null) return;

                MainWindow.videoHandler.PlayAllUnwatchedEpisodes(ser.AnimeSeriesID);

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnPlayNextEp_Click(object sender, RoutedEventArgs e)
        {
            if (UnwatchedEpisodes.Count == 0) return;

            try
            {
                VM_AnimeEpisode_User ep = UnwatchedEpisodes[0];

                if (ep.IsWatched == 1)
                {
                    foreach (var episode in UnwatchedEpisodes)
                    {
                        if (episode.IsWatched == 0)
                        {
                            ep = episode;
                            break;
                        }
                    }
                }

                if (ep.FilesForEpisode.Count == 1)
                {
                    bool force = true;
                    if (MainWindow.videoHandler.DefaultPlayer.Player.ToString() !=
                        Enum.GetName(typeof(VideoPlayer), VideoPlayer.WindowsDefault))
                    {

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
                    }
                    MainWindow.videoHandler.PlayVideo(ep.FilesForEpisode[0],force);
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
                        MainWindow mainwdw = (MainWindow)Window.GetWindow(this);

                        PlayVideosForEpisodeForm frm = new PlayVideosForEpisodeForm();
                        frm.Owner = mainwdw;
                        frm.Init(ep);
                        frm.ShowDialog();
                    }
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void episodesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;

            if (!recsWorker.IsBusy)
                recsWorker.RunWorkerAsync(ser);
        }

        void episodesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                VM_AnimeSeries_User ser = e.Argument as VM_AnimeSeries_User;

                List<VM_AnimeEpisode_User> rawEps = VM_ShokoServer.Instance.ShokoServices.GetAllUnwatchedEpisodes(ser.AnimeSeriesID,
                        VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeEpisode_User>();

                // find the last watched episode
                VM_AnimeEpisode_User rawLastEp = (VM_AnimeEpisode_User)VM_ShokoServer.Instance.ShokoServices.GetLastWatchedEpisodeForSeries(ser.AnimeSeriesID,
                        VM_ShokoServer.Instance.CurrentUser.JMMUserID);

                if (rawLastEp != null)
                {
                    rawLastEp.SetTvDBInfo();
                    //if (VM_ShokoServer.Instance.Trakt_IsEnabled)
                    //    ep.SetTraktInfo();
                    rawLastEp.EpisodeOrder = 0;
                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
                    {
                        UnwatchedEpisodes.Add(rawLastEp);
                    });
                }

                int i = 0;
                foreach (VM_AnimeEpisode_User raw in rawEps)
                {
                    // only show episodes and specials - ignore trailers, credits etc for continue watching
                    if ((Models.Enums.EpisodeType)raw.EpisodeType != Models.Enums.EpisodeType.Episode && (Models.Enums.EpisodeType)raw.EpisodeType != Models.Enums.EpisodeType.Special) continue;
                    i++;
                    raw.SetTvDBInfo();
                    //if (i == 1 && VM_ShokoServer.Instance.Trakt_IsEnabled) ep.SetTraktInfo();
                    raw.EpisodeOrder = i;
                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
                    {
                        UnwatchedEpisodes.Add(raw);
                    });

                    if (i == 5) break;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }
        }

        void recsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;

            if (!charWorker.IsBusy)
            {
                charWorker.RunWorkerAsync(ser);
            }
        }

        void recsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            { 
                VM_AnimeSeries_User ser = e.Argument as VM_AnimeSeries_User;

                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
                {
                    Recommendations.Clear();
                });

                List<VM_AniDB_Anime_Similar> tempList = VM_ShokoServer.Instance.ShokoServices.GetSimilarAnimeLinks(ser.AniDB_ID,
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID).Cast<VM_AniDB_Anime_Similar>().OrderByDescending(a => a.AnimeID).ToList();
                foreach (VM_AniDB_Anime_Similar sim in tempList)
                {
                    if (!sim.AnimeInfoExists)
                    {
                        string result = VM_ShokoServer.Instance.ShokoServices.UpdateAnimeData(sim.SimilarAnimeID);
                        if (string.IsNullOrEmpty(result))
                        {
                            sim.PopulateAnime((VM_AniDB_Anime)VM_ShokoServer.Instance.ShokoServices.GetAnime(sim.SimilarAnimeID));
                        }
                    }

                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
                    {
                        Recommendations.Add(new RecommendationTile()
                        {
                            Details = string.Empty,
                            AnimeName = sim.DisplayName,
                            Picture = sim.PosterPath,
                            AnimeSeries = sim.AnimeSeries,
                            TileSize = string.Intern("Large"),
                            Height = 100,
                            Source = string.Intern("AniDB"),
                            AnimeID = sim.AnimeID,
                            URL = sim.AniDB_SiteURL,
                            SimilarAnimeID = sim.SimilarAnimeID,
                            HasSeries = sim.LocalSeriesExists
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }
        }

        void commentsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsLoadingComments = false;
        }

        void commentsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            VM_AnimeSeries_User ser = e.Argument as VM_AnimeSeries_User;
            List<VM_Trakt_CommentUser> tempComments = new List<VM_Trakt_CommentUser>();

            try
            {
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
                {
                    Comments.Clear();
                });

                // get comments from trakt
                List<VM_Trakt_CommentUser> rawComments = VM_ShokoServer.Instance.ShokoServices.GetTraktCommentsForAnime(ser.AniDB_ID).CastList<VM_Trakt_CommentUser>();
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
                {
                    rawComments.ForEach(a=>Comments.Add(a));
                });

                // get recommendations from AniDB
                List<VM_AniDB_Recommendation> rawRecs = VM_ShokoServer.Instance.ShokoServices.GetAniDBRecommendations(ser.AniDB_ID).CastList<VM_AniDB_Recommendation>();

                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
                {
                    rawRecs.ForEach(a=>Comments.Add(a));
                });

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }
        }

        void charWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;

            if (!commentsWorker.IsBusy)
            {
                IsLoadingComments = true;
                commentsWorker.RunWorkerAsync(ser);
            }
        }

        void charWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                VM_AnimeSeries_User ser = e.Argument as VM_AnimeSeries_User;

                List<VM_AniDB_Character> chars = VM_ShokoServer.Instance.ShokoServices.GetCharactersForAnime(ser.AniDB_ID).CastList<VM_AniDB_Character>();

                List<VM_AniDB_Character> chrsToAdd = new List<VM_AniDB_Character>();

                // first add all the main characters
                foreach (VM_AniDB_Character chr in
                    chars.Where(x => x?.CharType != null && x.CharType.Equals(Models.Constants.CharacterType.MAIN,
                                         StringComparison.InvariantCultureIgnoreCase)))
                {
                    chrsToAdd.Add(chr);
                }

                // now add all the character types
                int i = 0;
                foreach (VM_AniDB_Character chr in
                    chars.Where(x => x?.CharType != null && !x.CharType.Equals(Models.Constants.CharacterType.MAIN,
                                         StringComparison.InvariantCultureIgnoreCase)))
                {
                    chrsToAdd.Add(chr);
                    i++;

                    if (i == 25) break;
                }

                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
                {
                    foreach (VM_AniDB_Character chr in chrsToAdd)
                        Characters.Add(chr);
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }
        }

        void btnBack_Click(object sender, RoutedEventArgs e)
        {
            //MainWindow mainwdw = (MainWindow)Window.GetWindow(this);
            //mainwdw.ShowDashMetroView(MetroViews.MainMetro);

            VM_DashboardMetro.Instance.NavigateBack();
        }

        private void RefreshData()
        {
            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;

            //VM_MainListHelper.Instance.UpdateAll();

            UnwatchedEpisodeCount = ser.UnwatchedEpisodeCount;

            UnwatchedEpisodes.Clear();
            Recommendations.Clear();
            Comments.Clear();
            Characters.Clear();

            RefreshUnwatchedEpisodes();
        }

        void AnimeSeriesSimplifiedControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;

            ucExternalLinks.DataContext = ser;

            try
            {
                PosterWidth = 180D;
                if (ser.AniDBAnime.AniDBAnime.UsePosterOnSeries)
                {
                    string imgName = ser.AniDBAnime.AniDBAnime.FanartPathThenPosterPath;
                    if (File.Exists(imgName))
                    {
                        BitmapDecoder decoder = BitmapDecoder.Create(new Uri(imgName), BitmapCreateOptions.None, BitmapCacheOption.None);
                        BitmapFrame frame = decoder.Frames[0];

                        PosterWidth = frame.PixelWidth / (double) frame.PixelHeight * 250D;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }

            RefreshData();
        }

        private void RefreshUnwatchedEpisodes()
        {
            VM_AnimeSeries_User ser = DataContext as VM_AnimeSeries_User;
            if (ser == null) return;

            if (!episodesWorker.IsBusy)
                episodesWorker.RunWorkerAsync(ser);
        }

        private void CommandBinding_ViewComment(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                ViewCommentForm frm = new ViewCommentForm();
                frm.Owner = parentWindow;
                frm.Init(obj);
                frm.ShowDialog();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_PlayEpisode(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    VM_AnimeEpisode_User ep = obj as VM_AnimeEpisode_User;

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
                            bool? result = frm.ShowDialog();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_PlayAllUnwatchedEpisode(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void CommandBinding_ToggleWatchedStatus(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            Cursor = Cursors.Wait;

            try
            {
                Window parentWindow = Window.GetWindow(this);
                VM_AnimeSeries_User ser = null;
                bool newStatus = false;

                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    VM_VideoDetailed vid = obj as VM_VideoDetailed;
                    newStatus = !vid.Watched;
                    VM_ShokoServer.Instance.ShokoServices.ToggleWatchedStatusOnVideo(vid.VideoLocalID, newStatus, VM_ShokoServer.Instance.CurrentUser.JMMUserID);

                    VM_MainListHelper.Instance.UpdateHeirarchy(vid);

                    ser = VM_MainListHelper.Instance.GetSeriesForVideo(vid.VideoLocalID);
                }

                if (obj.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    VM_AnimeEpisode_User ep = obj as VM_AnimeEpisode_User;
                    newStatus = !ep.Watched;

                    CL_Response<CL_AnimeEpisode_User>response = VM_ShokoServer.Instance.ShokoServices.ToggleWatchedStatusOnEpisode(ep.AnimeEpisodeID,
                        newStatus, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    if (!string.IsNullOrEmpty(response.ErrorMessage))
                    {
                        MessageBox.Show(response.ErrorMessage, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    VM_MainListHelper.Instance.UpdateHeirarchy((VM_AnimeEpisode_User)response.Result);

                    ser = VM_MainListHelper.Instance.GetSeriesForEpisode(ep);
                }

                RefreshData();
                if (newStatus && ser != null)
                {
                    Utils.PromptToRateSeries(ser, parentWindow);
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

        private void CommandBinding_VoteUp(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(RecommendationTile))
                {
                    RecommendationTile rec = obj as RecommendationTile;
                    Utils.AniDBVoteRecommendation(rec.AnimeID, rec.SimilarAnimeID, true);

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_VoteDown(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(RecommendationTile))
                {
                    RecommendationTile rec = obj as RecommendationTile;
                    Utils.AniDBVoteRecommendation(rec.AnimeID, rec.SimilarAnimeID, false);

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_RecDetails(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(RecommendationTile))
                {
                    RecommendationTile rec = obj as RecommendationTile;
                    if (rec.AnimeSeries != null)
                    {
                        //MainWindow mainwdw = (MainWindow)Window.GetWindow(this);
                        //mainwdw.ShowDashMetroView(MetroViews.ContinueWatching, rec.AnimeSeries);
                        VM_DashboardMetro.Instance.NavigateForward(MetroViews.ContinueWatching, rec.AnimeSeries);
                    }
                    else
                    {
                        Uri uri = new Uri(rec.URL);
                        Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
                    }

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }
}

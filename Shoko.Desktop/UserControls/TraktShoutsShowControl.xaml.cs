using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for TraktCommentsShowControl.xaml
    /// </summary>
    public partial class TraktCommentsShowControl : UserControl
    {
        public ObservableCollection<object> CurrentComments { get; set; }

        public static readonly DependencyProperty NumberOfCommentsProperty = DependencyProperty.Register("NumberOfComments",
            typeof(int), typeof(TraktCommentsShowControl), new UIPropertyMetadata(0, null));

        public int NumberOfComments
        {
            get { return (int)GetValue(NumberOfCommentsProperty); }
            set { SetValue(NumberOfCommentsProperty, value); }
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading",
            typeof(bool), typeof(TraktCommentsShowControl), new UIPropertyMetadata(true, null));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set
            {
                SetValue(IsLoadingProperty, value);
                IsNotLoading = !value;
            }
        }

        public static readonly DependencyProperty IsNotLoadingProperty = DependencyProperty.Register("IsNotLoading",
            typeof(bool), typeof(TraktCommentsShowControl), new UIPropertyMetadata(true, null));

        public bool IsNotLoading
        {
            get { return (bool)GetValue(IsNotLoadingProperty); }
            set { SetValue(IsNotLoadingProperty, value); }
        }

        private BackgroundWorker refreshDataWorker = new BackgroundWorker();
        private BackgroundWorker postCommentWorker = new BackgroundWorker();

        public TraktCommentsShowControl()
        {
            InitializeComponent();

            CurrentComments = new ObservableCollection<object>();

            DataContextChanged += new DependencyPropertyChangedEventHandler(TraktCommentsShowControl_DataContextChanged);

            btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            btnSubmitComment.Click += new RoutedEventHandler(btnSubmitComment_Click);

            refreshDataWorker.DoWork += new DoWorkEventHandler(refreshDataWorker_DoWork);
            refreshDataWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(refreshDataWorker_RunWorkerCompleted);

            postCommentWorker.DoWork += new DoWorkEventHandler(postCommentWorker_DoWork);
            postCommentWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(postCommentWorker_RunWorkerCompleted);
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
                bool? result = frm.ShowDialog();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void postCommentWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string msg = e.Result.ToString();

            MessageBox.Show(msg, "Message", MessageBoxButton.OK, MessageBoxImage.Information);
            txtCommentNew.Text = "";
            RefreshComments();
        }

        void postCommentWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            VM_Trakt_CommentPost comment = e.Argument as VM_Trakt_CommentPost;

            string msg = "";
            try
            {
                CL_Response<bool> resp=VM_ShokoServer.Instance.ShokoServices.PostTraktCommentShow(comment.TraktID, comment.CommentText, comment.Spoiler);
                msg = resp.ErrorMessage;
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
                Utils.ShowErrorMessage("You have not enabled Trakt, for more info go to 'Settings - Community Sites - Trakt TV'");
                txtCommentNew.Focus();
                return;
            }

            if (string.IsNullOrEmpty(VM_ShokoServer.Instance.Trakt_AuthToken))
            {
                Utils.ShowErrorMessage("You have not authorized JMM to use your Trakt account, for more info go to 'Settings - Community Sites - Trakt TV'");
                txtCommentNew.Focus();
                return;
            }

            VM_AnimeSeries_User animeSeries = (VM_AnimeSeries_User)DataContext;
            if (animeSeries == null)
            {
                Utils.ShowErrorMessage("Anime series info not found");
                txtCommentNew.Focus();
                return;
            }

            string commentText = txtCommentNew.Text.Trim();
            if (string.IsNullOrEmpty(commentText))
            {
                Utils.ShowErrorMessage("Please enter text for your Comment");
                txtCommentNew.Focus();
                return;
            }

            if (commentText.Length > 2000)
            {
                Utils.ShowErrorMessage($"Comment text must be less than 2000 characters ({commentText.Length})");
                txtCommentNew.Focus();
                return;
            }

            btnRefresh.IsEnabled = false;
            btnSubmitComment.IsEnabled = false;

            if (animeSeries.AniDBAnime.AniDBAnime.traktSummary != null)
            {
                string traktID = string.Empty;

                // check to  see if this series is linked to more than one Trakt series
                if (animeSeries.AniDBAnime.AniDBAnime.traktSummary.traktDetails == null ||
                    animeSeries.AniDBAnime.AniDBAnime.traktSummary.traktDetails.Count == 0)
                {
                    Utils.ShowErrorMessage(string.Format("Cannot Comment where a series does not have a Trakt show linked"));
                    txtCommentNew.Focus();
                    return;
                }

                // check to  see if this series is linked to more than one Trakt series
                if (animeSeries.AniDBAnime.AniDBAnime.traktSummary.traktDetails != null &&
                    animeSeries.AniDBAnime.AniDBAnime.traktSummary.traktDetails.Count > 1)
                {
                    Utils.ShowErrorMessage(string.Format("Cannot Comment where a series has more than one Trakt show linked"));
                    txtCommentNew.Focus();
                    return;
                }

                if (animeSeries.AniDBAnime.AniDBAnime.traktSummary.traktDetails != null &&
                    animeSeries.AniDBAnime.AniDBAnime.traktSummary.traktDetails.Count == 1)
                {

                    Cursor = Cursors.Wait;
                    IsLoading = true;

                    foreach (KeyValuePair<string, VM_TraktDetails> kvp in animeSeries.AniDBAnime.AniDBAnime.traktSummary.traktDetails)
                    { traktID = kvp.Key; }

                    VM_Trakt_CommentPost comment = new VM_Trakt_CommentPost();
                    comment.TraktID = traktID;
                    comment.AnimeID = animeSeries.AniDB_ID;
                    comment.CommentText = commentText;
                    comment.Spoiler = chkSpoiler.IsChecked.Value;

                    postCommentWorker.RunWorkerAsync(comment);
                }
            }
            else
            {
                Utils.ShowErrorMessage(string.Format("Cannot Comment where a series does not have a Trakt show linked"));
                txtCommentNew.Focus();
            }
        }

        void refreshDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<object> tempComments = e.Result as List<object>;
            NumberOfComments = tempComments.Count;

            foreach (object comment in tempComments)
                CurrentComments.Add(comment);

            IsLoading = false;
            Cursor = Cursors.Arrow;
            btnRefresh.IsEnabled = true;
            btnSubmitComment.IsEnabled = true;
        }

        void refreshDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<object> tempComments = new List<object>();

            try
            {
                VM_AnimeSeries_User animeSeries = (VM_AnimeSeries_User)e.Argument;
                if (animeSeries == null) return;

                // get comments from Trakt
                foreach (VM_Trakt_CommentUser contract in VM_ShokoServer.Instance.ShokoServices.GetTraktCommentsForAnime(animeSeries.AniDB_ID).Cast<VM_Trakt_CommentUser>())
                {
                    tempComments.Add(contract);
                }

                // get comments from AniDB
                // get recommendations from AniDB
                foreach (VM_AniDB_Recommendation rec in VM_ShokoServer.Instance.ShokoServices.GetAniDBRecommendations(animeSeries.AniDB_ID).Cast<VM_AniDB_Recommendation>())
                {
                    tempComments.Add(rec);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            e.Result = tempComments;
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {

            RefreshComments();

        }

        void TraktCommentsShowControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        public void RefreshComments()
        {
            VM_AnimeSeries_User animeSeries = (VM_AnimeSeries_User)DataContext;
            if (animeSeries == null) return;

            btnRefresh.IsEnabled = false;
            btnSubmitComment.IsEnabled = false;

            Cursor = Cursors.Wait;
            IsLoading = true;
            NumberOfComments = 0;

            CurrentComments.Clear();
            refreshDataWorker.RunWorkerAsync(animeSeries);
        }
    }
}

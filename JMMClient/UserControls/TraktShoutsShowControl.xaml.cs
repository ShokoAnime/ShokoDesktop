using System;
using System.Collections.Generic;
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
using System.Collections.ObjectModel;
using JMMClient.ViewModel;
using System.IO;
using JMMClient.ImageDownload;
using System.ComponentModel;
using JMMClient.Forms;

namespace JMMClient.UserControls
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

			this.DataContextChanged += new DependencyPropertyChangedEventHandler(TraktCommentsShowControl_DataContextChanged);

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
			Trakt_CommentPost comment = e.Argument as Trakt_CommentPost;

			string msg = "";
			try
			{
				JMMServerVM.Instance.clientBinaryHTTP.PostTraktCommentShow(comment.TraktID, comment.CommentText, comment.Spoiler, ref msg);
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
            if (!JMMServerVM.Instance.Trakt_IsEnabled)
            {
                Utils.ShowErrorMessage("You have not enabled Trakt, for more info go to 'Settings - Community Sites - Trakt TV'");
                txtCommentNew.Focus();
                return;
            }

            if (string.IsNullOrEmpty(JMMServerVM.Instance.Trakt_AuthToken))
            {
                Utils.ShowErrorMessage("You have not authorized JMM to use your Trakt account, for more info go to 'Settings - Community Sites - Trakt TV'");
                txtCommentNew.Focus();
                return;
            }

			AnimeSeriesVM animeSeries = (AnimeSeriesVM)this.DataContext;
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
				Utils.ShowErrorMessage(string.Format("Comment text must be less than 2000 characters ({0})", commentText.Length));
				txtCommentNew.Focus();
				return;
			}

			btnRefresh.IsEnabled = false;
			btnSubmitComment.IsEnabled = false;

            if (animeSeries.AniDB_Anime.traktSummary != null)
            {
                string traktID = string.Empty;

                // check to  see if this series is linked to more than one Trakt series
                if (animeSeries.AniDB_Anime.traktSummary.traktDetails == null ||
                    animeSeries.AniDB_Anime.traktSummary.traktDetails.Count == 0)
                {
                    Utils.ShowErrorMessage(string.Format("Cannot Comment where a series does not have a Trakt show linked"));
                    txtCommentNew.Focus();
                    return;
                }

                // check to  see if this series is linked to more than one Trakt series
                if (animeSeries.AniDB_Anime.traktSummary.traktDetails != null &&
                    animeSeries.AniDB_Anime.traktSummary.traktDetails.Count > 1)
                {
                    Utils.ShowErrorMessage(string.Format("Cannot Comment where a series has more than one Trakt show linked"));
                    txtCommentNew.Focus();
                    return;
                }

                if (animeSeries.AniDB_Anime.traktSummary.traktDetails != null &&
                    animeSeries.AniDB_Anime.traktSummary.traktDetails.Count == 1)
                {

                    this.Cursor = Cursors.Wait;
                    IsLoading = true;

                    foreach (KeyValuePair<string, TraktDetails> kvp in animeSeries.AniDB_Anime.traktSummary.traktDetails)
                    { traktID = kvp.Key; }
                    
                    Trakt_CommentPost comment = new Trakt_CommentPost();
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
			this.Cursor = Cursors.Arrow;
			btnRefresh.IsEnabled = true;
			btnSubmitComment.IsEnabled = true;
		}

		void refreshDataWorker_DoWork(object sender, DoWorkEventArgs e)
		{
            List<object> tempComments = new List<object>();

			try
			{
				AnimeSeriesVM animeSeries = (AnimeSeriesVM)e.Argument;
				if (animeSeries == null) return;

                // get comments from Trakt
				List<JMMServerBinary.Contract_Trakt_CommentUser> rawComments = JMMServerVM.Instance.clientBinaryHTTP.GetTraktCommentsForAnime(animeSeries.AniDB_ID);
				foreach (JMMServerBinary.Contract_Trakt_CommentUser contract in rawComments)
				{
					Trakt_CommentUserVM traktComment = new Trakt_CommentUserVM(contract);
					tempComments.Add(traktComment);
				}

				// get comments from AniDB
                // get recommendations from AniDB
                List<JMMServerBinary.Contract_AniDB_Recommendation> rawRecs = JMMServerVM.Instance.clientBinaryHTTP.GetAniDBRecommendations(animeSeries.AniDB_ID);
                foreach (JMMServerBinary.Contract_AniDB_Recommendation contract in rawRecs)
                {
                    AniDB_RecommendationVM rec = new AniDB_RecommendationVM(contract);
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
			AnimeSeriesVM animeSeries = (AnimeSeriesVM)this.DataContext;
			if (animeSeries == null) return;

			btnRefresh.IsEnabled = false;
			btnSubmitComment.IsEnabled = false;

			this.Cursor = Cursors.Wait;
			IsLoading = true;
			NumberOfComments = 0;

			CurrentComments.Clear();
			refreshDataWorker.RunWorkerAsync(animeSeries);
		}
	}
}

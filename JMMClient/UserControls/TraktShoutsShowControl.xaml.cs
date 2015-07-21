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

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for TraktShoutsShowControl.xaml
	/// </summary>
	public partial class TraktShoutsShowControl : UserControl
	{
        public ObservableCollection<object> CurrentShouts { get; set; }

		public static readonly DependencyProperty NumberOfShoutsProperty = DependencyProperty.Register("NumberOfShouts",
			typeof(int), typeof(TraktShoutsShowControl), new UIPropertyMetadata(0, null));

		public int NumberOfShouts
		{
			get { return (int)GetValue(NumberOfShoutsProperty); }
			set { SetValue(NumberOfShoutsProperty, value); }
		}

		public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading",
			typeof(bool), typeof(TraktShoutsShowControl), new UIPropertyMetadata(true, null));

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
			typeof(bool), typeof(TraktShoutsShowControl), new UIPropertyMetadata(true, null));

		public bool IsNotLoading
		{
			get { return (bool)GetValue(IsNotLoadingProperty); }
			set { SetValue(IsNotLoadingProperty, value); }
		}

		private BackgroundWorker refreshDataWorker = new BackgroundWorker();
		private BackgroundWorker postShoutWorker = new BackgroundWorker();

		public TraktShoutsShowControl()
		{
			InitializeComponent();

			CurrentShouts = new ObservableCollection<object>();

			this.DataContextChanged += new DependencyPropertyChangedEventHandler(TraktShoutsShowControl_DataContextChanged);

			btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
			btnSubmitShout.Click += new RoutedEventHandler(btnSubmitShout_Click);

			refreshDataWorker.DoWork += new DoWorkEventHandler(refreshDataWorker_DoWork);
			refreshDataWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(refreshDataWorker_RunWorkerCompleted);

			postShoutWorker.DoWork += new DoWorkEventHandler(postShoutWorker_DoWork);
			postShoutWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(postShoutWorker_RunWorkerCompleted);
		}

		void postShoutWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			string msg = e.Result.ToString();

			MessageBox.Show(msg, "Message", MessageBoxButton.OK, MessageBoxImage.Information);
			txtShoutNew.Text = "";
			RefreshShouts();
		}

		void postShoutWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			Trakt_ShoutPost shout = e.Argument as Trakt_ShoutPost;

			string msg = "";
			try
			{
				JMMServerVM.Instance.clientBinaryHTTP.PostShoutShow(shout.TraktID, shout.ShoutText, shout.Spoiler, ref msg);
			}
			catch (Exception ex)
			{
				e.Result = ex.Message;
				return;
			}

			e.Result = msg;
		}

		void btnSubmitShout_Click(object sender, RoutedEventArgs e)
		{
			AnimeSeriesVM animeSeries = (AnimeSeriesVM)this.DataContext;
			if (animeSeries == null)
			{
				Utils.ShowErrorMessage("Anime series info not found");
				txtShoutNew.Focus();
				return;
			}

			string shoutText = txtShoutNew.Text.Trim();
			if (string.IsNullOrEmpty(shoutText))
			{
				Utils.ShowErrorMessage("Please enter text for your shout");
				txtShoutNew.Focus();
				return;
			}

			if (shoutText.Length > 2000)
			{
				Utils.ShowErrorMessage(string.Format("Shout text must be less than 2000 characters ({0})", shoutText.Length));
				txtShoutNew.Focus();
				return;
			}

			btnRefresh.IsEnabled = false;
			btnSubmitShout.IsEnabled = false;

            if (animeSeries.AniDB_Anime.traktSummary != null)
            {
                string traktID = string.Empty;

                // check to  see if this series is linked to more than one Trakt series
                if (animeSeries.AniDB_Anime.traktSummary.traktDetails == null ||
                    animeSeries.AniDB_Anime.traktSummary.traktDetails.Count == 0)
                {
                    Utils.ShowErrorMessage(string.Format("Cannot shout where a series does not have a Trakt show linked"));
                    txtShoutNew.Focus();
                    return;
                }

                // check to  see if this series is linked to more than one Trakt series
                if (animeSeries.AniDB_Anime.traktSummary.traktDetails != null &&
                    animeSeries.AniDB_Anime.traktSummary.traktDetails.Count > 1)
                {
                    Utils.ShowErrorMessage(string.Format("Cannot shout where a series has more than one Trakt show linked"));
                    txtShoutNew.Focus();
                    return;
                }

                if (animeSeries.AniDB_Anime.traktSummary.traktDetails != null &&
                    animeSeries.AniDB_Anime.traktSummary.traktDetails.Count == 1)
                {

                    this.Cursor = Cursors.Wait;
                    IsLoading = true;

                    foreach (KeyValuePair<string, TraktDetails> kvp in animeSeries.AniDB_Anime.traktSummary.traktDetails)
                    { traktID = kvp.Key; }
                    
                    Trakt_ShoutPost shout = new Trakt_ShoutPost();
                    shout.TraktID = traktID;
                    shout.AnimeID = animeSeries.AniDB_ID;
                    shout.ShoutText = shoutText;
                    shout.Spoiler = chkSpoiler.IsChecked.Value;

                    postShoutWorker.RunWorkerAsync(shout);
                }
            }
            else
            {
                Utils.ShowErrorMessage(string.Format("Cannot shout where a series does not have a Trakt show linked"));
                txtShoutNew.Focus();
            }
		}

		void refreshDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
            List<object> tempShouts = e.Result as List<object>;
			NumberOfShouts = tempShouts.Count;
			
			foreach (object shout in tempShouts)
				CurrentShouts.Add(shout);

			IsLoading = false;
			this.Cursor = Cursors.Arrow;
			btnRefresh.IsEnabled = true;
			btnSubmitShout.IsEnabled = true;
		}

		void refreshDataWorker_DoWork(object sender, DoWorkEventArgs e)
		{
            List<object> tempComments = new List<object>();

			try
			{
				AnimeSeriesVM animeSeries = (AnimeSeriesVM)e.Argument;
				if (animeSeries == null) return;

                // get comments from Trakt
				List<JMMServerBinary.Contract_Trakt_ShoutUser> rawShouts = JMMServerVM.Instance.clientBinaryHTTP.GetTraktShoutsForAnime(animeSeries.AniDB_ID);
				foreach (JMMServerBinary.Contract_Trakt_ShoutUser contract in rawShouts)
				{
					Trakt_ShoutUserVM traktComment = new Trakt_ShoutUserVM(contract);
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
			
			RefreshShouts();
			
		}

		void TraktShoutsShowControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{

		}

		public void RefreshShouts()
		{
			AnimeSeriesVM animeSeries = (AnimeSeriesVM)this.DataContext;
			if (animeSeries == null) return;

			btnRefresh.IsEnabled = false;
			btnSubmitShout.IsEnabled = false;

			this.Cursor = Cursors.Wait;
			IsLoading = true;
			NumberOfShouts = 0;

			CurrentShouts.Clear();
			refreshDataWorker.RunWorkerAsync(animeSeries);
		}
	}
}

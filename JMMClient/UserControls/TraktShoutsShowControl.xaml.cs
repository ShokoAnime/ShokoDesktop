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
		public ObservableCollection<Trakt_ShoutUserVM> CurrentShouts { get; set; }

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

			CurrentShouts = new ObservableCollection<Trakt_ShoutUserVM>();

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
				JMMServerVM.Instance.clientBinaryHTTP.PostShoutShow(shout.AnimeID, shout.ShoutText, shout.Spoiler, ref msg);
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

			this.Cursor = Cursors.Wait;
			IsLoading = true;

			Trakt_ShoutPost shout = new Trakt_ShoutPost();
			shout.AnimeID = animeSeries.AniDB_ID;
			shout.ShoutText = shoutText;
			shout.Spoiler = chkSpoiler.IsChecked.Value;

			postShoutWorker.RunWorkerAsync(shout);
		}

		void refreshDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			List<Trakt_ShoutUserVM> tempShouts = e.Result as List<Trakt_ShoutUserVM>;
			NumberOfShouts = tempShouts.Count;
			
			foreach (Trakt_ShoutUserVM shout in tempShouts)
				CurrentShouts.Add(shout);

			IsLoading = false;
			this.Cursor = Cursors.Arrow;
			btnRefresh.IsEnabled = true;
			btnSubmitShout.IsEnabled = true;
		}

		void refreshDataWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			List<Trakt_ShoutUserVM> tempShouts = new List<Trakt_ShoutUserVM>();

			try
			{
				AnimeSeriesVM animeSeries = (AnimeSeriesVM)e.Argument;
				if (animeSeries == null) return;

				List<JMMServerBinary.Contract_Trakt_ShoutUser> rawShouts = JMMServerVM.Instance.clientBinaryHTTP.GetTraktShoutsForAnime(animeSeries.AniDB_ID);
				foreach (JMMServerBinary.Contract_Trakt_ShoutUser contract in rawShouts)
				{
					Trakt_ShoutUserVM shout = new Trakt_ShoutUserVM(contract);


					if (!string.IsNullOrEmpty(shout.UserFullImagePath) && !File.Exists(shout.UserFullImagePath))
					{
						// re-download the friends avatar image
						try
						{
							ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.Trakt_ShoutUser, shout, false);
							MainWindow.imageHelper.DownloadImage(req);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.ToString());
						}
					}

					tempShouts.Add(shout);
				}

				
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

			e.Result = tempShouts;
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

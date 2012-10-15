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
//using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using JMMClient.Forms;
using JMMClient.ViewModel;
using System.IO;
using JMMClient.ImageDownload;
using NLog;
using System.Net;
using System.Diagnostics;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for ContinueWatchingTileControl.xaml
	/// </summary>
	public partial class ContinueWatchingTileControl : UserControl
	{

		private static Logger logger = LogManager.GetCurrentClassLogger();
		private BlockingList<object> imagesToDownload = new BlockingList<object>();
		private BackgroundWorker workerImages = new BackgroundWorker();

		public ObservableCollection<AnimeEpisodeVM> UnwatchedEpisodes { get; set; }
		public ICollectionView ViewUnwatchedEpisodes { get; set; }

		public ObservableCollection<RecommendationTile> Recommendations { get; set; }

		public ObservableCollection<Trakt_ShoutUserVM> Shouts { get; set; }
		

		BackgroundWorker episodesWorker = new BackgroundWorker();
		BackgroundWorker recsWorker = new BackgroundWorker();
		BackgroundWorker shoutsWorker = new BackgroundWorker();
		BackgroundWorker postShoutWorker = new BackgroundWorker();

		public static readonly DependencyProperty UnwatchedEpisodeCountProperty = DependencyProperty.Register("UnwatchedEpisodeCount",
			typeof(int), typeof(ContinueWatchingTileControl), new UIPropertyMetadata(0, null));

		public int UnwatchedEpisodeCount
		{
			get { return (int)GetValue(UnwatchedEpisodeCountProperty); }
			set { SetValue(UnwatchedEpisodeCountProperty, value); }
		}

		public static readonly DependencyProperty PosterWidthProperty = DependencyProperty.Register("PosterWidth",
			typeof(double), typeof(ContinueWatchingTileControl), new UIPropertyMetadata((double)180, null));

		public double PosterWidth
		{
			get { return (double)GetValue(PosterWidthProperty); }
			set { SetValue(PosterWidthProperty, value); }
		}

		public ContinueWatchingTileControl()
		{
			InitializeComponent();

			UnwatchedEpisodes = new ObservableCollection<AnimeEpisodeVM>();
			ViewUnwatchedEpisodes = CollectionViewSource.GetDefaultView(UnwatchedEpisodes);

			Recommendations = new ObservableCollection<RecommendationTile>();
			Shouts = new ObservableCollection<Trakt_ShoutUserVM>();

			this.DataContextChanged += new DependencyPropertyChangedEventHandler(ContinueWatchingTileControl_DataContextChanged);

			btnBack.Click += new RoutedEventHandler(btnBack_Click);
			btnPlayNextEp.Click += new RoutedEventHandler(btnPlayNextEp_Click);
			btnPlayAllEps.Click += new RoutedEventHandler(btnPlayAllEps_Click);

			episodesWorker.DoWork += new DoWorkEventHandler(episodesWorker_DoWork);
			episodesWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(episodesWorker_RunWorkerCompleted);

			recsWorker.DoWork += new DoWorkEventHandler(recsWorker_DoWork);
			recsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(recsWorker_RunWorkerCompleted);

			shoutsWorker.DoWork += new DoWorkEventHandler(shoutsWorker_DoWork);
			shoutsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(shoutsWorker_RunWorkerCompleted);

			MainWindow.videoHandler.VideoWatchedEvent += new Utilities.VideoHandler.VideoWatchedEventHandler(videoHandler_VideoWatchedEvent);

			workerImages.DoWork += new DoWorkEventHandler(workerImages_DoWork);
			workerImages.RunWorkerAsync();

			txtShoutNew.GotFocus += new RoutedEventHandler(txtShoutNew_GotFocus);
			txtShoutNew.LostFocus += new RoutedEventHandler(txtShoutNew_LostFocus);
			btnSubmitShout.Click += new RoutedEventHandler(btnSubmitShout_Click);

			postShoutWorker.DoWork += new DoWorkEventHandler(postShoutWorker_DoWork);
			postShoutWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(postShoutWorker_RunWorkerCompleted);

			cRating.OnRatingValueChangedEvent += new RatingControl.RatingValueChangedHandler(cRating_OnRatingValueChangedEvent);
		}

		void cRating_OnRatingValueChangedEvent(RatingValueEventArgs ev)
		{
			AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
			if (ser == null) return;

			try
			{
				decimal rating = (decimal)ev.RatingValue;

				int voteType = 1;
				if (ser.AniDB_Anime.FinishedAiring) voteType = 2;

				JMMServerVM.Instance.VoteAnime(ser.AniDB_ID, rating, voteType);

				// refresh the data
				MainListHelperVM.Instance.UpdateHeirarchy(ser);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void postShoutWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			string msg = e.Result.ToString();
			this.Cursor = Cursors.Arrow;

			MessageBox.Show(msg, "Message", MessageBoxButton.OK, MessageBoxImage.Information);
			txtShoutNew.Text = "";

			AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
			if (ser == null) return;

			shoutsWorker.RunWorkerAsync(ser);
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

			btnSubmitShout.IsEnabled = false;

			this.Cursor = Cursors.Wait;

			Trakt_ShoutPost shout = new Trakt_ShoutPost();
			shout.AnimeID = animeSeries.AniDB_ID;
			shout.ShoutText = shoutText;
			shout.Spoiler = chkSpoiler.IsChecked.Value;

			postShoutWorker.RunWorkerAsync(shout);
		}

		void txtShoutNew_LostFocus(object sender, RoutedEventArgs e)
		{
			txtShoutNew.Height = 30;
			txtShoutNew.Foreground = Brushes.DarkGray;

			if (txtShoutNew.Text.Trim().Length == 0)
				txtShoutNew.Text = "Have Your Say...";
		}

		void txtShoutNew_GotFocus(object sender, RoutedEventArgs e)
		{
			if (txtShoutNew.Text.Equals("Have Your Say...", StringComparison.InvariantCultureIgnoreCase))
				txtShoutNew.Text = "";

			txtShoutNew.Foreground = Brushes.Black;
			txtShoutNew.Height = 150;
		}

		void workerImages_DoWork(object sender, DoWorkEventArgs e)
		{
			ProcessImages();
		}

		private void ProcessImages()
		{
			foreach (object req in imagesToDownload)
			{
				try
				{
					if (req.GetType() == typeof(Trakt_ShoutUserVM))
					{
						Trakt_ShoutUserVM tile = req as Trakt_ShoutUserVM;


						//user
						Uri uriUser = new Uri(tile.UserOnlineImagePath);
						string filenameUser = Path.GetFileName(uriUser.LocalPath);
						string tempNameUser = Path.Combine(Path.GetTempPath(), filenameUser);

						using (WebClient client = new WebClient())
						{
							client.Headers.Add("user-agent", "JMM");


							if (!File.Exists(tempNameUser))
							{
								if (tile.UserOnlineImagePath.Length > 0)
									client.DownloadFile(tile.UserOnlineImagePath, tempNameUser);
							}
							if (File.Exists(tempNameUser)) tile.DelayedUserImage = tempNameUser;

						}

					}


					imagesToDownload.Remove(req);
				}
				catch (Exception ex)
				{
					imagesToDownload.Remove(req);
					logger.ErrorException(ex.ToString(), ex);
				}
			}

		}

		

		void videoHandler_VideoWatchedEvent(Utilities.VideoWatchedEventArgs ev)
		{
			try
			{
				MainWindow mainwdw = (MainWindow)Window.GetWindow(this);

				if (MainWindow.CurrentMainTabIndex == MainWindow.TAB_MAIN_Dashboard && mainwdw.tileContinueWatching.Visibility == System.Windows.Visibility.Visible)
					RefreshData();
			}
			catch { }
		}

		void btnPlayAllEps_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
				if (ser == null) return;

				MainWindow.videoHandler.PlayAllUnwatchedEpisodes(ser.AnimeSeriesID.Value);

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
				AnimeEpisodeVM ep = UnwatchedEpisodes[0];

				if (ep.FilesForEpisode.Count == 1)
					MainWindow.videoHandler.PlayVideo(ep.FilesForEpisode[0]);
				else if (ep.FilesForEpisode.Count > 1)
				{
					if (AppSettings.AutoFileSingleEpisode)
					{
						VideoDetailedVM vid = MainWindow.videoHandler.GetAutoFileForEpisode(ep);
						if (vid != null)
							MainWindow.videoHandler.PlayVideo(vid);
					}
					else
					{
						MainWindow mainwdw = (MainWindow)Window.GetWindow(this);

						PlayVideosForEpisodeForm frm = new PlayVideosForEpisodeForm();
						frm.Owner = mainwdw;
						frm.Init(ep);
						bool? result = frm.ShowDialog();
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
			
			AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
			if (ser == null) return;

			if (!recsWorker.IsBusy)
				recsWorker.RunWorkerAsync(ser);
		}

		void episodesWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				AnimeSeriesVM ser = e.Argument as AnimeSeriesVM;

				List<JMMServerBinary.Contract_AnimeEpisode> rawEps = JMMServerVM.Instance.clientBinaryHTTP.GetAllUnwatchedEpisodes(ser.AnimeSeriesID.Value,
						JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

				int i = 0;
				foreach (JMMServerBinary.Contract_AnimeEpisode raw in rawEps)
				{
					i++;
					AnimeEpisodeVM ep = new AnimeEpisodeVM(raw);
					ep.SetTvDBInfo();
					System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
					{
						UnwatchedEpisodes.Add(ep);
					});

					if (i == 10) break;
				}
			}
			catch (Exception ex)
			{
				logger.ErrorException(ex.ToString(), ex);
			}
		}

		void recsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
			if (ser == null) return;

			if (!shoutsWorker.IsBusy)
				shoutsWorker.RunWorkerAsync(ser);
		}

		void recsWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				AnimeSeriesVM ser = e.Argument as AnimeSeriesVM;

				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					Recommendations.Clear();
				});

				List<JMMServerBinary.Contract_AniDB_Anime_Similar> links = JMMServerVM.Instance.clientBinaryHTTP.GetSimilarAnimeLinks(ser.AniDB_ID,
						JMMServerVM.Instance.CurrentUser.JMMUserID.Value);


				List<AniDB_Anime_SimilarVM> tempList = new List<AniDB_Anime_SimilarVM>();
				foreach (JMMServerBinary.Contract_AniDB_Anime_Similar link in links)
				{
					AniDB_Anime_SimilarVM sim = new AniDB_Anime_SimilarVM();
					sim.Populate(link);
					tempList.Add(sim);
				}

				List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
				sortCriteria.Add(new SortPropOrFieldAndDirection("ApprovalPercentage", true, SortType.eDoubleOrFloat));
				tempList = Sorting.MultiSort<AniDB_Anime_SimilarVM>(tempList, sortCriteria);

				foreach (AniDB_Anime_SimilarVM sim in tempList)
				{
					if (sim.AnimeInfoNotExists)
					{
						string result = JMMServerVM.Instance.clientBinaryHTTP.UpdateAnimeData(sim.SimilarAnimeID);
						if (string.IsNullOrEmpty(result))
						{
							JMMServerBinary.Contract_AniDBAnime animeContract = JMMServerVM.Instance.clientBinaryHTTP.GetAnime(sim.SimilarAnimeID);
							sim.PopulateAnime(animeContract);
						}
					}

					System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
					{
						Recommendations.Add(new RecommendationTile()
						{
							Details = "",
							AnimeName = sim.DisplayName,
							Picture = sim.PosterPath,
							AnimeSeries = sim.AnimeSeries,
							TileSize = "Large",
							Height = 100,
							Source = "AniDB",
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
				logger.ErrorException(ex.ToString(), ex);
			}
		}

		void shoutsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
				
		}

		void shoutsWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			AnimeSeriesVM ser = e.Argument as AnimeSeriesVM;
			List<Trakt_ShoutUserVM> tempShouts = new List<Trakt_ShoutUserVM>();

			try
			{
				System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
				{
					Shouts.Clear();
				});
				List<JMMServerBinary.Contract_Trakt_ShoutUser> rawShouts = JMMServerVM.Instance.clientBinaryHTTP.GetTraktShoutsForAnime(ser.AniDB_ID);
				foreach (JMMServerBinary.Contract_Trakt_ShoutUser contract in rawShouts)
				{
					Trakt_ShoutUserVM shout = new Trakt_ShoutUserVM(contract);

					shout.DelayedUserImage = @"/Images/blankposter.png";
					System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate()
					{
						Shouts.Add(shout);
					});

					imagesToDownload.Add(shout);
				}


			}
			catch (Exception ex)
			{
				logger.ErrorException(ex.ToString(), ex);
			}
		}

		void btnBack_Click(object sender, RoutedEventArgs e)
		{
			//MainWindow mainwdw = (MainWindow)Window.GetWindow(this);
			//mainwdw.ShowDashMetroView(MetroViews.MainMetro);

			DashboardMetroVM.Instance.NavigateBack();
		}

		private void RefreshData()
		{
			AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
			if (ser == null) return;

			ser.RefreshBase();

			UnwatchedEpisodeCount = ser.UnwatchedEpisodeCount;

			UnwatchedEpisodes.Clear();
			Recommendations.Clear();
			Shouts.Clear();

			RefreshUnwatchedEpisodes();
		}

		void ContinueWatchingTileControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
			if (ser == null) return;

			ucExternalLinks.DataContext = ser;

			try
			{
				PosterWidth = 180;
				if (ser.AniDB_Anime.UsePosterOnSeries)
				{
					string imgName = ser.AniDB_Anime.FanartPathThenPosterPath;
					if (File.Exists(imgName))
					{
						BitmapDecoder decoder = BitmapDecoder.Create(new Uri(imgName), BitmapCreateOptions.None, BitmapCacheOption.None);
						BitmapFrame frame = decoder.Frames[0];

						PosterWidth = (double)250 * ((double)frame.PixelWidth / (double)frame.PixelHeight);
					}
				}
			}
			catch (Exception ex)
			{
				logger.ErrorException(ex.ToString(), ex);
			}

			RefreshData();
		}

		private void RefreshUnwatchedEpisodes()
		{
			AnimeSeriesVM ser = this.DataContext as AnimeSeriesVM;
			if (ser == null) return;

			if (!episodesWorker.IsBusy)
				episodesWorker.RunWorkerAsync(ser);
		}

		private void CommandBinding_PlayEpisode(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(AnimeEpisodeVM))
				{
					AnimeEpisodeVM ep = obj as AnimeEpisodeVM;

					if (ep.FilesForEpisode.Count == 1)
						MainWindow.videoHandler.PlayVideo(ep.FilesForEpisode[0]);
					else if (ep.FilesForEpisode.Count > 1)
					{
						if (AppSettings.AutoFileSingleEpisode)
						{
							VideoDetailedVM vid = MainWindow.videoHandler.GetAutoFileForEpisode(ep);
							if (vid != null)
								MainWindow.videoHandler.PlayVideo(vid);
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

			this.Cursor = Cursors.Wait;

			try
			{
				Window parentWindow = Window.GetWindow(this);
				AnimeSeriesVM ser = null;
				bool newStatus = false;

				if (obj.GetType() == typeof(VideoDetailedVM))
				{
					VideoDetailedVM vid = obj as VideoDetailedVM;
					newStatus = !vid.Watched;
					JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnVideo(vid.VideoLocalID, newStatus, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

					MainListHelperVM.Instance.UpdateHeirarchy(vid);

					ser = MainListHelperVM.Instance.GetSeriesForVideo(vid.VideoLocalID);
				}

				if (obj.GetType() == typeof(AnimeEpisodeVM))
				{
					AnimeEpisodeVM ep = obj as AnimeEpisodeVM;
					newStatus = !ep.Watched;

					JMMServerBinary.Contract_ToggleWatchedStatusOnEpisode_Response response = JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnEpisode(ep.AnimeEpisodeID,
						newStatus, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
					if (!string.IsNullOrEmpty(response.ErrorMessage))
					{
						MessageBox.Show(response.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					MainListHelperVM.Instance.UpdateHeirarchy(response.AnimeEpisode);

					ser = MainListHelperVM.Instance.GetSeriesForEpisode(ep);
				}

				RefreshData();
				if (newStatus == true && ser != null)
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
				this.Cursor = Cursors.Arrow;
			}
		}

		private void CommandBinding_VoteUp(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

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
			Window parentWindow = Window.GetWindow(this);

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
			Window parentWindow = Window.GetWindow(this);

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
						DashboardMetroVM.Instance.NavigateForward(MetroViews.ContinueWatching, rec.AnimeSeries);
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

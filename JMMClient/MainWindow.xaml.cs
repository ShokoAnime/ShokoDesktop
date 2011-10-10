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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Resources;
using JMMClient.Properties;
using JMMClient.ViewModel;
using Infralution.Localization.Wpf;
using System.Configuration;
using JMMClient.ImageDownload;
using System.Windows.Threading;
using System.ServiceModel;
using JMMClient.Forms;
using System.IO;
using NLog;
using System.Collections.ObjectModel;
using JMMClient.UserControls;

namespace JMMClient
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private static readonly int TAB_MAIN_Dashboard = 0;
		private static readonly int TAB_MAIN_Collection = 1;
		private static readonly int TAB_MAIN_Server = 2;
		private static readonly int TAB_MAIN_FileManger = 3;
		private static readonly int TAB_MAIN_Settings = 4;
		private static readonly int TAB_MAIN_Pinned = 5;
		private static readonly int TAB_MAIN_Search = 6;

		private static readonly int TAB_FileManger_Unrecognised = 0;
		private static readonly int TAB_FileManger_Ignored = 1;
		private static readonly int TAB_FileManger_ManuallyLinked = 2;
		private static readonly int TAB_FileManger_DuplicateFiles = 3;
		private static readonly int TAB_FileManger_MultipleFiles = 4;

		private static readonly int TAB_Settings_Essential = 0;
		private static readonly int TAB_Settings_AniDB = 1;
		private static readonly int TAB_Settings_TvDB = 2;
		private static readonly int TAB_Settings_WebCache = 3;
		private static readonly int TAB_Settings_Display = 4;

		private int lastFileManagerTab = TAB_FileManger_Unrecognised;

		public static GroupFilterVM groupFilterVM = null;
		public static List<UserCulture> userLanguages = new List<UserCulture>();
		public static ImageDownloader imageHelper = null;
		
		private AnimeGroupVM groupBeforeChanges = null;
		private GroupFilterVM groupFilterBeforeChanges = null;

		BackgroundWorker showChildWrappersWorker = new BackgroundWorker();
		BackgroundWorker refreshGroupsWorker = new BackgroundWorker();
		BackgroundWorker downloadImagesWorker = new BackgroundWorker();
		BackgroundWorker toggleStatusWorker = new BackgroundWorker();
		BackgroundWorker moveSeriesWorker = new BackgroundWorker();

		BackgroundWorker showDashboardWorker = new BackgroundWorker();

		public MainWindow()
		{
			InitializeComponent();

			try
			{

				//listBox1.IsSynchronizedWithCurrentItem = true;
				//treeGroupsSeries.iss
				

				lbGroupsSeries.MouseDoubleClick += new MouseButtonEventHandler(lbGroupsSeries_MouseDoubleClick);
				lbGroupsSeries.SelectionChanged += new SelectionChangedEventHandler(lbGroupsSeries_SelectionChanged);
				this.grdMain.LayoutUpdated += new EventHandler(grdMain_LayoutUpdated);
				this.LayoutUpdated += new EventHandler(MainWindow_LayoutUpdated);



				showChildWrappersWorker.DoWork += new DoWorkEventHandler(showChildWrappersWorker_DoWork);
				showChildWrappersWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(showChildWrappersWorker_RunWorkerCompleted);

				downloadImagesWorker.DoWork += new DoWorkEventHandler(downloadImagesWorker_DoWork);

				refreshGroupsWorker.DoWork += new DoWorkEventHandler(refreshGroupsWorker_DoWork);
				refreshGroupsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(refreshGroupsWorker_RunWorkerCompleted);

				toggleStatusWorker.DoWork += new DoWorkEventHandler(toggleStatusWorker_DoWork);
				toggleStatusWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(toggleStatusWorker_RunWorkerCompleted);

				moveSeriesWorker.DoWork += new DoWorkEventHandler(moveSeriesWorker_DoWork);
				moveSeriesWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(moveSeriesWorker_RunWorkerCompleted);

				txtGroupSearch.TextChanged += new TextChangedEventHandler(txtGroupSearch_TextChanged);

				showDashboardWorker.DoWork += new DoWorkEventHandler(showDashboardWorker_DoWork);
				showDashboardWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(showDashboardWorker_RunWorkerCompleted);

				MainListHelperVM.Instance.ViewGroups.Filter = GroupSearchFilter;
				cboLanguages.SelectionChanged += new SelectionChangedEventHandler(cboLanguages_SelectionChanged);

				if (MainListHelperVM.Instance.SeriesSearchTextBox == null) MainListHelperVM.Instance.SeriesSearchTextBox = seriesSearch.txtSeriesSearch;

				//grdSplitEps.DragCompleted += new System.Windows.Controls.Primitives.DragCompletedEventHandler(grdSplitEps_DragCompleted);


				imageHelper = new ImageDownloader();
				imageHelper.Init();

				InitCulture();

				imageHelper.QueueUpdateEvent += new ImageDownloader.QueueUpdateEventHandler(imageHelper_QueueUpdateEvent);

				cboGroupSort.Items.Clear();
				foreach (string sType in GroupFilterHelper.GetAllSortTypes())
					cboGroupSort.Items.Add(sType);
				cboGroupSort.SelectedIndex = 0;
				btnToolbarSort.Click += new RoutedEventHandler(btnToolbarSort_Click);

				tabControl1.SelectionChanged += new SelectionChangedEventHandler(tabControl1_SelectionChanged);
				tabFileManager.SelectionChanged += new SelectionChangedEventHandler(tabFileManager_SelectionChanged);
				tabSettingsChild.SelectionChanged += new SelectionChangedEventHandler(tabSettingsChild_SelectionChanged);

				this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
				this.StateChanged += new EventHandler(MainWindow_StateChanged);

				this.AddHandler(CloseableTabItem.CloseTabEvent, new RoutedEventHandler(this.CloseTab));
			}
			catch (Exception ex)
			{
				logger.ErrorException(ex.ToString(), ex);
			}
		}


		private void CloseTab(object source, RoutedEventArgs args)
		{
			System.Windows.Controls.TabItem tabItem = args.Source as System.Windows.Controls.TabItem;
			if (tabItem != null)
			{
				System.Windows.Controls.TabControl tabControl = tabItem.Parent as System.Windows.Controls.TabControl;
				if (tabControl != null)
					tabControl.Items.Remove(tabItem);
			}
		}
		

		void MainWindow_StateChanged(object sender, EventArgs e)
		{
			//if (this.WindowState == System.Windows.WindowState.Minimized) this.Hide();

			if (this.WindowState == System.Windows.WindowState.Normal || this.WindowState == System.Windows.WindowState.Maximized)
				AppSettings.DefaultWindowState = this.WindowState;
		}


		void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			this.WindowState = AppSettings.DefaultWindowState;

			// validate settings
			JMMServerVM.Instance.Test();

			// authenticate user
			if (JMMServerVM.Instance.ServerOnline && !JMMServerVM.Instance.AuthenticateUser())
			{
				this.Close();
				return;
			}

			if (JMMServerVM.Instance.ServerOnline)
			{
				tabControl1.SelectedIndex = TAB_MAIN_Dashboard;
				DisplayMainTab(TAB_MAIN_Dashboard);
				DownloadAllImages();
			}
			else
				tabControl1.SelectedIndex = TAB_MAIN_Settings;



		}

		
		

		void MainWindow_LayoutUpdated(object sender, EventArgs e)
		{
			// Why am I doing this?
			// Basically there is weird problem if you try and set the content control's width to the exact
			// ViewportWidth of the parent scroller.
			// On some resolutions, when you maximise the window it will cause UI glitches
			// By setting it slightly less than the max width, these problems go away
			try
			{
				//Debug.Print("Scroller width = {0}", Scroller.ActualWidth);
				//Debug.Print("Scroller ViewportWidth = {0}", Scroller.ViewportWidth);

				double tempWidth = Scroller.ViewportWidth - 8;
				if (tempWidth > 0)
				{
					MainListHelperVM.Instance.MainScrollerWidth = tempWidth;
				}

				tempWidth = tabControl1.ActualWidth - 20;
				//tempWidth = tabControl1.ActualWidth - 300;
				if (tempWidth > 0)
					MainListHelperVM.Instance.FullScrollerWidth = tempWidth;
			}
			catch (Exception ex)
			{
				logger.ErrorException(ex.ToString(), ex);
			}
			
		}

		void tabFileManager_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				if (e.Source is System.Windows.Controls.TabControl)
				{
					System.Windows.Controls.TabControl tab = e.Source as System.Windows.Controls.TabControl;
					this.Cursor = Cursors.Wait;
					if (tab.SelectedIndex == TAB_FileManger_Unrecognised)
					{
						if (unRecVids.UnrecognisedFiles.Count == 0) unRecVids.RefreshUnrecognisedFiles();
						if (unRecVids.AllSeries.Count == 0) unRecVids.RefreshSeries();
						lastFileManagerTab = TAB_FileManger_Unrecognised;
					}

					if (tab.SelectedIndex == TAB_FileManger_Ignored)
					{
						if (ignoredFiles.IgnoredFilesCollection.Count == 0) ignoredFiles.RefreshIgnoredFiles();
						lastFileManagerTab = TAB_FileManger_Ignored;
					}

					if (tab.SelectedIndex == TAB_FileManger_ManuallyLinked)
					{
						if (linkedFiles.ManuallyLinkedFiles.Count == 0) linkedFiles.RefreshLinkedFiles();
						lastFileManagerTab = TAB_FileManger_ManuallyLinked;
					}

					if (tab.SelectedIndex == TAB_FileManger_DuplicateFiles)
					{
						if (duplicateFiles.DuplicateFilesCollection.Count == 0) duplicateFiles.RefreshDuplicateFiles();
						lastFileManagerTab = TAB_FileManger_DuplicateFiles;
					}
					if (tab.SelectedIndex == TAB_FileManger_MultipleFiles)
					{
						if (multipleFiles.CurrentEpisodes.Count == 0) multipleFiles.RefreshMultipleFiles();
						lastFileManagerTab = TAB_FileManger_MultipleFiles;
					}
					this.Cursor = Cursors.Arrow;
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

		}

		void showDashboardWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			DashboardVM.Instance.RefreshData();
		}

		void showDashboardWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			this.Cursor = Cursors.Arrow;
			tabControl1.IsEnabled = true;
		}

		private void DisplayMainTab(int tabIndex)
		{
			try
			{
				if (tabIndex == TAB_MAIN_Dashboard)
				{
					if (DashboardVM.Instance.EpsWatchNext_Recent.Count == 0)
					{
						tabControl1.IsEnabled = false;
						this.Cursor = Cursors.Wait;
						showDashboardWorker.RunWorkerAsync();

					}
				}

				if (tabIndex == TAB_MAIN_Collection)
				{
					if (MainListHelperVM.Instance.AllGroups.Count == 0)
					{
						MainListHelperVM.Instance.RefreshGroupsSeriesData();
					}

					if (MainListHelperVM.Instance.CurrentWrapper == null && lbGroupsSeries.Items.Count == 0)
					{
						MainListHelperVM.Instance.SearchTextBox = txtGroupSearch;
						MainListHelperVM.Instance.CurrentGroupFilter = MainListHelperVM.Instance.AllGroupFilter;
						MainListHelperVM.Instance.ShowChildWrappers(MainListHelperVM.Instance.CurrentWrapper);
					}
				}


				if (tabIndex == TAB_MAIN_FileManger)
				{
					if (unRecVids.UnrecognisedFiles.Count == 0) unRecVids.RefreshUnrecognisedFiles();

				}

				if (tabIndex == TAB_MAIN_Search)
				{
					if (MainListHelperVM.Instance.AllSeries == null || MainListHelperVM.Instance.AllSeries.Count == 0) MainListHelperVM.Instance.RefreshGroupsSeriesData();
				}

				if (tabIndex == TAB_MAIN_Server)
				{
					if (JMMServerVM.Instance.ImportFolders.Count == 0) JMMServerVM.Instance.RefreshImportFolders();
				}

				if (tabIndex == TAB_MAIN_Settings)
				{
					if (JMMServerVM.Instance.ImportFolders.Count == 0) JMMServerVM.Instance.RefreshImportFolders();
					if (JMMServerVM.Instance.SelectedLanguages.Count == 0) JMMServerVM.Instance.RefreshNamingLanguages();
					if (JMMServerVM.Instance.AllUsers.Count == 0) JMMServerVM.Instance.RefreshAllUsers();
					if (JMMServerVM.Instance.AllCategories.Count == 0) JMMServerVM.Instance.RefreshAllCategories();
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
				tabControl1.IsEnabled = true;
			}
		}

		void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				//if (!this.IsLoaded || !JMMServerVM.Instance.UserAuthenticated) return;
				if (!JMMServerVM.Instance.UserAuthenticated) return;


				TabControl tab = e.Source as TabControl;
				if (tab == null) return;

				if (!tab.Name.Equals("tabControl1", StringComparison.InvariantCultureIgnoreCase)) return;

				DisplayMainTab(tabControl1.SelectedIndex);

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
				tabControl1.IsEnabled = true;
			}
		}

		void tabSettingsChild_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				if (e.Source is System.Windows.Controls.TabControl)
				{
					System.Windows.Controls.TabControl tab = e.Source as System.Windows.Controls.TabControl;
					if (tab.SelectedIndex == TAB_Settings_Display)
					{
						if (JMMServerVM.Instance.SelectedLanguages.Count == 0) JMMServerVM.Instance.RefreshNamingLanguages();
					}
					
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnToolbarSort_Click(object sender, RoutedEventArgs e)
		{
			MainListHelperVM.Instance.ViewGroups.SortDescriptions.Clear();
			GroupFilterSorting sortType = GroupFilterHelper.GetEnumForText_Sorting(cboGroupSort.SelectedItem.ToString());
			MainListHelperVM.Instance.ViewGroups.SortDescriptions.Add(GroupFilterHelper.GetSortDescription(sortType, GroupFilterSortDirection.Asc));
		}

		

		void imageHelper_QueueUpdateEvent(QueueUpdateEventArgs ev)
		{
			try
			{
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new System.Windows.Forms.MethodInvoker(delegate()
				{
					tbImageDownloadQueueStatus.Text = ev.queueCount.ToString();
				}));
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}


		void cboLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SetCulture();
		}

		private void InitCulture()
		{
			try
			{
				string currentCulture = AppSettings.Culture;

				cboLanguages.ItemsSource = UserCulture.SupportedLanguages;

				for (int i = 0; i < cboLanguages.Items.Count; i++)
				{
					UserCulture ul = cboLanguages.Items[i] as UserCulture;
					if (ul.Culture.Trim().ToUpper() == currentCulture.Trim().ToUpper())
					{
						cboLanguages.SelectedIndex = i;
						break;
					}

				}
				if (cboLanguages.SelectedIndex < 0)
					cboLanguages.SelectedIndex = 0;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

		}

		private void SetCulture()
		{
			if (cboLanguages.SelectedItem == null) return;
			UserCulture ul = cboLanguages.SelectedItem as UserCulture;

			try
			{
				CultureInfo ci = new CultureInfo(ul.Culture);
				CultureManager.UICulture = ci;

				AppSettings.Culture = ul.Culture;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

		}

		void txtGroupSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			try
			{
				MainListHelperVM.Instance.ViewGroups.Refresh();
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private bool GroupSearchFilter(object obj)
		{
			MainListWrapper grp = obj as MainListWrapper;
			if (grp == null) return true;

			if (obj.GetType() != typeof(AnimeGroupVM) && obj.GetType() != typeof(AnimeSeriesVM))
				return true;

			// first evaluate the group filter
			// if the group doesn't match the group filter we won't continue
			if (obj.GetType() == typeof(AnimeGroupVM))
			{
				AnimeGroupVM grpvm = obj as AnimeGroupVM;
				//if (!GroupSearchFilterHelper.EvaluateGroupFilter(MainListHelperVM.Instance.CurrentGroupFilter, grpvm)) return false;

				return GroupSearchFilterHelper.EvaluateGroupTextSearch(grpvm, txtGroupSearch.Text);
			}

			if (obj.GetType() == typeof(AnimeSeriesVM))
			{
				AnimeSeriesVM ser = obj as AnimeSeriesVM;
				//if (!GroupSearchFilterHelper.EvaluateGroupFilter(MainListHelperVM.Instance.CurrentGroupFilter, ser)) return false;

				return GroupSearchFilterHelper.EvaluateSeriesTextSearch(ser, txtGroupSearch.Text);
			}

			return true;
		}

		void refreshGroupsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			showChildWrappersWorker.RunWorkerAsync(MainListHelperVM.Instance.CurrentWrapper);
		}

		void refreshGroupsWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				MainListHelperVM.Instance.RefreshGroupsSeriesData();
				DownloadAllImages();
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void showChildWrappersWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{
				EnableDisableGroupControls(true);
				if (lbGroupsSeries.Items.Count > 0)
				{
					HighlightMainListItem();
				}
				else
					SetDetailBinding(null);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void showChildWrappersWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				MainListHelperVM.Instance.ShowChildWrappers(e.Argument as MainListWrapper);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void toggleStatusWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			EnableDisableGroupControls(true);
			this.Cursor = Cursors.Arrow;
		}

		
		void toggleStatusWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				object obj = e.Argument;

				if (obj.GetType() == typeof(VideoDetailedVM))
				{
					VideoDetailedVM vid = obj as VideoDetailedVM;
					bool newStatus = !vid.Watched;
					JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnVideo(vid.VideoLocalID, newStatus, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

					//MainListHelperVM.Instance.UpdateHeirarchy(vid, epListMain);
					MainListHelperVM.Instance.UpdateHeirarchy(vid);
				}

				if (obj.GetType() == typeof(AnimeEpisodeVM))
				{
					AnimeEpisodeVM ep = obj as AnimeEpisodeVM;
					bool newStatus = !ep.Watched;
					JMMServerBinary.Contract_ToggleWatchedStatusOnEpisode_Response response = JMMServerVM.Instance.clientBinaryHTTP.ToggleWatchedStatusOnEpisode(ep.AnimeEpisodeID,
						newStatus, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
					if (!string.IsNullOrEmpty(response.ErrorMessage))
					{
						MessageBox.Show(response.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					MainListHelperVM.Instance.UpdateHeirarchy(response.AnimeEpisode);
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void moveSeriesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			EnableDisableGroupControls(true);
			this.Cursor = Cursors.Arrow;

			MainListHelperVM.Instance.ViewGroups.Refresh();
			showChildWrappersWorker.RunWorkerAsync(MainListHelperVM.Instance.CurrentWrapper);
		}

		void moveSeriesWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				object obj = e.Argument;
				if (obj.GetType() != typeof(MoveSeriesDetails)) return;

				MoveSeriesDetails request = obj as MoveSeriesDetails;
				DateTime start = DateTime.Now;

				
				//request.UpdatedSeries.Save();
				JMMServerBinary.Contract_AnimeSeries_SaveResponse response = 
					JMMServerVM.Instance.clientBinaryHTTP.MoveSeries(request.UpdatedSeries.AnimeSeriesID.Value, request.UpdatedSeries.AnimeGroupID,
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				if (!string.IsNullOrEmpty(response.ErrorMessage))
				{
					MessageBox.Show(response.ErrorMessage);
					return;
				}
				else
					request.UpdatedSeries.Populate(response.AnimeSeries);


				// update all the attached groups

				Dictionary<int, JMMServerBinary.Contract_AnimeGroup> grpsDict = new Dictionary<int, JMMServerBinary.Contract_AnimeGroup>();
				List<JMMServerBinary.Contract_AnimeGroup> grps = JMMServerVM.Instance.clientBinaryHTTP.GetAllGroupsAboveGroupInclusive(request.UpdatedSeries.AnimeGroupID,
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				List<JMMServerBinary.Contract_AnimeGroup> grpsOld = JMMServerVM.Instance.clientBinaryHTTP.GetAllGroupsAboveGroupInclusive(request.OldAnimeGroupID,
					JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

				foreach (JMMServerBinary.Contract_AnimeGroup tempGrp in grps)
					grpsDict[tempGrp.AnimeGroupID] = tempGrp;

				foreach (JMMServerBinary.Contract_AnimeGroup tempGrp in grpsOld)
					grpsDict[tempGrp.AnimeGroupID] = tempGrp;
				
				foreach (AnimeGroupVM grp in MainListHelperVM.Instance.AllGroups)
				{
					if (grpsDict.ContainsKey(grp.AnimeGroupID.Value))
					{
						grp.Populate(grpsDict[grp.AnimeGroupID.Value]);
					}

				}
				TimeSpan ts = DateTime.Now - start;
				Console.Write(ts.TotalMilliseconds);


				
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void DownloadAllImages()
		{
			if (!downloadImagesWorker.IsBusy)
				downloadImagesWorker.RunWorkerAsync();
		}

		void downloadImagesWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			// 1. Download posters from AniDB
			List<JMMServerBinary.Contract_AniDBAnime> contracts = JMMServerVM.Instance.clientBinaryHTTP.GetAllAnime();

			int i = 0;
			foreach (JMMServerBinary.Contract_AniDBAnime anime in contracts)
			{
				//Thread.Sleep(5); // don't use too many resources
				imageHelper.DownloadAniDBCover(new AniDB_AnimeVM(anime), false);
				i++;

				//if (i == 80) break;
			}

			// 2. Download posters from TvDB
			List<JMMServerBinary.Contract_TvDB_ImagePoster> posters = JMMServerVM.Instance.clientBinaryHTTP.GetAllTvDBPosters(null);
			foreach (JMMServerBinary.Contract_TvDB_ImagePoster poster in posters)
			{
				//Thread.Sleep(5); // don't use too many resources
				imageHelper.DownloadTvDBPoster(new TvDB_ImagePosterVM(poster), false);
			}

			// 2a. Download posters from MovieDB
			List<JMMServerBinary.Contract_MovieDB_Poster> moviePosters = JMMServerVM.Instance.clientBinaryHTTP.GetAllMovieDBPosters(null);
			foreach (JMMServerBinary.Contract_MovieDB_Poster poster in moviePosters)
			{
				//Thread.Sleep(5); // don't use too many resources
				imageHelper.DownloadMovieDBPoster(new MovieDB_PosterVM(poster), false);
			}

			// 3. Download wide banners from TvDB
			List<JMMServerBinary.Contract_TvDB_ImageWideBanner> banners = JMMServerVM.Instance.clientBinaryHTTP.GetAllTvDBWideBanners(null);
			foreach (JMMServerBinary.Contract_TvDB_ImageWideBanner banner in banners)
			{
				//Thread.Sleep(5); // don't use too many resources
				imageHelper.DownloadTvDBWideBanner(new TvDB_ImageWideBannerVM(banner), false);
			}

			// 4. Download fanart from TvDB
			List<JMMServerBinary.Contract_TvDB_ImageFanart> fanarts = JMMServerVM.Instance.clientBinaryHTTP.GetAllTvDBFanart(null);
			foreach (JMMServerBinary.Contract_TvDB_ImageFanart fanart in fanarts)
			{
				//Thread.Sleep(5); // don't use too many resources
				imageHelper.DownloadTvDBFanart(new TvDB_ImageFanartVM(fanart), false);
			}

			// 4a. Download fanart from MovieDB
			List<JMMServerBinary.Contract_MovieDB_Fanart> movieFanarts = JMMServerVM.Instance.clientBinaryHTTP.GetAllMovieDBFanart(null);
			foreach (JMMServerBinary.Contract_MovieDB_Fanart fanart in movieFanarts)
			{
				//Thread.Sleep(5); // don't use too many resources
				imageHelper.DownloadMovieDBFanart(new MovieDB_FanartVM(fanart), false);
			}

			// 5. Download episode images from TvDB
			List<JMMServerBinary.Contract_TvDB_Episode> eps = JMMServerVM.Instance.clientBinaryHTTP.GetAllTvDBEpisodes(null);
			foreach (JMMServerBinary.Contract_TvDB_Episode episode in eps)
			{
				//Thread.Sleep(5); // don't use too many resources
				imageHelper.DownloadTvDBEpisode(new TvDB_EpisodeVM(episode), false);
			}

			// 6. Download posters from Trakt
			List<JMMServerBinary.Contract_Trakt_ImagePoster> traktPosters = JMMServerVM.Instance.clientBinaryHTTP.GetAllTraktPosters(null);
			foreach (JMMServerBinary.Contract_Trakt_ImagePoster traktposter in traktPosters)
			{
				//Thread.Sleep(5); // don't use too many resources
				if (string.IsNullOrEmpty(traktposter.ImageURL)) continue;
				imageHelper.DownloadTraktPoster(new Trakt_ImagePosterVM(traktposter), false);
			}

			// 7. Download fanart from Trakt
			List<JMMServerBinary.Contract_Trakt_ImageFanart> traktFanarts = JMMServerVM.Instance.clientBinaryHTTP.GetAllTraktFanart(null);
			foreach (JMMServerBinary.Contract_Trakt_ImageFanart traktFanart in traktFanarts)
			{
				//Thread.Sleep(5); // don't use too many resources
				if (string.IsNullOrEmpty(traktFanart.ImageURL)) continue;
				imageHelper.DownloadTraktFanart(new Trakt_ImageFanartVM(traktFanart), false);
			}

			// 8. Download episode images from Trakt
			List<JMMServerBinary.Contract_Trakt_Episode> traktEpisodes = JMMServerVM.Instance.clientBinaryHTTP.GetAllTraktEpisodes(null);
			foreach (JMMServerBinary.Contract_Trakt_Episode traktEp in traktEpisodes)
			{
				//Thread.Sleep(5); // don't use too many resources
				if (string.IsNullOrEmpty(traktEp.EpisodeImage)) continue;

				// special case for trak episodes
				// Trakt will return the fanart image when no episode image exists, but we don't want this
				int pos = traktEp.EpisodeImage.IndexOf(@"episodes/");
				if (pos <= 0) continue;

				imageHelper.DownloadTraktEpisode(new Trakt_EpisodeVM(traktEp), false);
			}
		}

		private void RefreshView()
		{
			if (!JMMServerVM.Instance.ServerOnline) return;

			EnableDisableGroupControls(false);

			try
			{
				this.Cursor = Cursors.Wait;

				// we are look at all the group filters
				if (MainListHelperVM.Instance.CurrentWrapper == null)
				{
					MainListHelperVM.Instance.SearchTextBox = txtGroupSearch;
					MainListHelperVM.Instance.CurrentGroupFilter = MainListHelperVM.Instance.AllGroupFilter;

					//refreshGroupsWorker.RunWorkerAsync(null);

					MainListHelperVM.Instance.RefreshGroupsSeriesData();
					DownloadAllImages();

					MainListHelperVM.Instance.ShowChildWrappers(MainListHelperVM.Instance.CurrentWrapper);
				}

				// we are inside one of the group filters, groups or series
				if (MainListHelperVM.Instance.CurrentWrapper != null)
				{
					// refresh the groups and series data
					refreshGroupsWorker.RunWorkerAsync(null);

					// refresh the episodes
					if (lbGroupsSeries.SelectedItem is AnimeSeriesVM)
					{
						AnimeSeriesVM ser = lbGroupsSeries.SelectedItem as AnimeSeriesVM;
						ser.RefreshEpisodes();
					}
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
				EnableDisableGroupControls(true);
			}
			finally
			{
				this.Cursor = Cursors.Arrow;
				EnableDisableGroupControls(true);
			}
		}

		

		#region Command Bindings

		private void ShowPinnedSeries(AnimeSeriesVM series)
		{
			this.Cursor = Cursors.Wait;

			CloseableTabItem cti = new CloseableTabItem();
			//TabItem cti = new TabItem();
			cti.Header = series.SeriesName;

			AnimeSeries seriesControl = new AnimeSeries();
			seriesControl.DataContext = series;
			cti.Content = seriesControl;

			tabPinned.Items.Add(cti);

			tabControl1.SelectedIndex = TAB_MAIN_Pinned;
			tabPinned.SelectedIndex = tabPinned.Items.Count - 1;

			this.Cursor = Cursors.Arrow;
		}

		private void CommandBinding_CreateSeriesFromAnime(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(AniDB_AnimeVM))
				{
					AniDB_AnimeVM anime = (AniDB_AnimeVM)obj;

					// check if a series already exists
					bool seriesExists = JMMServerVM.Instance.clientBinaryHTTP.GetSeriesExistingForAnime(anime.AnimeID);
					if (seriesExists)
					{
						MessageBox.Show(Properties.Resources.ERROR_SeriesExists, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					NewSeries frmNewSeries = new NewSeries();
					frmNewSeries.Owner = this;
					frmNewSeries.Init(anime, anime.MainTitle);

					bool? result = frmNewSeries.ShowDialog();
					if (result.HasValue && result.Value == true)
					{

					}
				}
				else
				{
					NewSeries frm = new NewSeries();
					frm.Owner = this;
					frm.Init(null, "");
					bool? result = frm.ShowDialog();
					if (result.HasValue && result.Value == true)
					{
						
					}
				}
				
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_ShowPinnedSeries(object sender, ExecutedRoutedEventArgs e)
		{
			//object obj = lbGroupsSeries.SelectedItem;
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(AnimeEpisodeVM))
				{
					AnimeEpisodeVM ep = (AnimeEpisodeVM)obj;
					if (MainListHelperVM.Instance.AllSeriesDictionary.ContainsKey(ep.AnimeSeriesID))
					{
						ShowPinnedSeries(MainListHelperVM.Instance.AllSeriesDictionary[ep.AnimeSeriesID]);
					}
				}

				if (obj.GetType() == typeof(AnimeSeriesVM))
				{
					AnimeSeriesVM ser = (AnimeSeriesVM)obj;
					ShowPinnedSeries(ser);
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_Refresh(object sender, ExecutedRoutedEventArgs e)
		{
			RefreshView();
		}

		private void CommandBinding_Search(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				// move to all groups
				MainListHelperVM.Instance.ShowAllGroups();

				if (e.Parameter is AnimeCategoryVM)
				{
					AnimeCategoryVM obj = e.Parameter as AnimeCategoryVM;
					txtGroupSearch.Text = obj.CategoryName;
				}

				if (e.Parameter is AnimeTagVM)
				{
					AnimeTagVM obj = e.Parameter as AnimeTagVM;
					txtGroupSearch.Text = obj.TagName;
				}

				HighlightMainListItem();
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_Back(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				MainListHelperVM.Instance.MoveBackUpHeirarchy();
				HighlightMainListItem();
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_Edit(object sender, ExecutedRoutedEventArgs e)
		{
			//object obj = lbGroupsSeries.SelectedItem;
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(AnimeGroupVM))
				{
					AnimeGroupVM grp = (AnimeGroupVM)obj;

					if (grp.AnimeGroupID.HasValue)
					{
						groupBeforeChanges = new AnimeGroupVM();
						Cloner.Clone(grp, groupBeforeChanges);
					}

					grp.IsReadOnly = false;
					grp.IsBeingEdited = true;

				}

				if (obj.GetType() == typeof(GroupFilterVM))
				{
					GroupFilterVM gf = (GroupFilterVM)obj;

					if (gf.GroupFilterID.HasValue)
					{
						groupFilterBeforeChanges = new GroupFilterVM();
						groupFilterBeforeChanges.FilterName = gf.FilterName;
						groupFilterBeforeChanges.BaseCondition = gf.BaseCondition;
						groupFilterBeforeChanges.ApplyToSeries = gf.ApplyToSeries;
						groupFilterBeforeChanges.FilterConditions = new ObservableCollection<GroupFilterConditionVM>();
						groupFilterBeforeChanges.SortCriteriaList = new ObservableCollection<GroupFilterSortingCriteria>();

						foreach (GroupFilterConditionVM gfc_cur in gf.FilterConditions)
						{
							GroupFilterConditionVM gfc = new GroupFilterConditionVM();
							gfc.ConditionOperator = gfc_cur.ConditionOperator;
							gfc.ConditionParameter = gfc_cur.ConditionParameter;
							gfc.ConditionType = gfc_cur.ConditionType;
							gfc.GroupFilterConditionID = gfc_cur.GroupFilterConditionID;
							gfc.GroupFilterID = gfc_cur.GroupFilterID;
							groupFilterBeforeChanges.FilterConditions.Add(gfc);
						}

						foreach (GroupFilterSortingCriteria gfcs_cur in gf.SortCriteriaList)
						{
							GroupFilterSortingCriteria gfsc = new GroupFilterSortingCriteria();
							gfsc.GroupFilterID = gfcs_cur.GroupFilterID;
							gfsc.SortDirection = gfcs_cur.SortDirection;
							gfsc.SortType = gfcs_cur.SortType;
							groupFilterBeforeChanges.SortCriteriaList.Add(gfsc);
						}
						//Cloner.Clone(gf, groupFilterBeforeChanges);
					}

					gf.IsLocked = false;
					gf.IsBeingEdited = true;

					groupFilterVM = gf;
					MainListHelperVM.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
					MainListHelperVM.Instance.SetGroupFilterSortingOnForms(gf);
				}

				if (obj.GetType() == typeof(AnimeSeriesVM))
				{

				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

			EnableDisableGroupControls(false);
		}

		private void CommandBinding_Save(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(AnimeGroupVM))
				{
					AnimeGroupVM grp = (AnimeGroupVM)obj;
					bool isnew = !grp.AnimeGroupID.HasValue;
					if (grp.Validate())
					{
						grp.IsReadOnly = true;
						grp.IsBeingEdited = false;
						if (grp.Save() && isnew)
						{
							MainListHelperVM.Instance.AllGroups.Add(grp);
							MainListHelperVM.Instance.ViewGroups.Refresh();
							MainListHelperVM.Instance.LastAnimeGroupID = grp.AnimeGroupID.Value;

							if (!grp.AnimeGroupParentID.HasValue)
							{
								// move to all groups
								// only if it is a top level group
								MainListHelperVM.Instance.ShowAllGroups();
								HighlightMainListItem();
							}
							else
							{
								AnimeGroupVM parentGroup = grp.ParentGroup;
								if (parentGroup != null)
									showChildWrappersWorker.RunWorkerAsync(parentGroup);
							}
						}

					}
					//BindingExpression be = ccDetail.GetBindingExpression(ContentControl.ContentProperty);
					//be.UpdateSource();
				}

				if (obj.GetType() == typeof(GroupFilterVM))
				{
					GroupFilterVM gf = (GroupFilterVM)obj;


					bool isnew = !gf.GroupFilterID.HasValue;
					if (gf.Validate())
					{
						gf.IsLocked = true;
						gf.IsBeingEdited = false;
						if (gf.Save() && isnew)
						{
							MainListHelperVM.Instance.AllGroupFilters.Add(gf);
							MainListHelperVM.Instance.LastGroupFilterID = gf.GroupFilterID.Value;
							showChildWrappersWorker.RunWorkerAsync(null);
						}
						//showChildWrappersWorker.RunWorkerAsync(null);
					}
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

			EnableDisableGroupControls(true);
		}

		private void CommandBinding_ScanFolder(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(ImportFolderVM))
				{
					ImportFolderVM fldr = (ImportFolderVM)obj;

					JMMServerVM.Instance.clientBinaryHTTP.ScanFolder(fldr.ImportFolderID.Value);
					MessageBox.Show("Process is Running", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

		}

		private void CommandBinding_Delete(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			EnableDisableGroupControls(false);

			try
			{
				if (obj.GetType() == typeof(GroupFilterVM))
				{
					GroupFilterVM gf = (GroupFilterVM)obj;

					MessageBoxResult res = MessageBox.Show(string.Format("Are you sure you want to delete the Group Filter: {0}", gf.FilterName),
					"Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
					if (res == MessageBoxResult.Yes)
					{
						// remove from group list
						gf.Delete();





						// lets try and find where we are in the list so we can highlight that item
						// when deleting a a group, we should always have it highlighted
						// in the main list on the left
						int idx = lbGroupsSeries.SelectedIndex;
						if (idx >= 0)
						{
							if (idx > 0)
							{
								// we will move to the item above the item being deleted
								idx = idx - 1;
							}
							// otherwise just move to the first item
							lbGroupsSeries.SelectedIndex = idx;
							lbGroupsSeries.Focus();
							lbGroupsSeries.ScrollIntoView(lbGroupsSeries.Items[idx]);
						}

						// find the group filter
						int pos = -1;
						int i = 0;
						foreach (MainListWrapper wrapper in MainListHelperVM.Instance.CurrentWrapperList)
						{
							if (wrapper is GroupFilterVM)
							{
								GroupFilterVM gfTemp = wrapper as GroupFilterVM;
								if (gfTemp.GroupFilterID.HasValue && gf.GroupFilterID.Value == gfTemp.GroupFilterID.Value)
								{
									pos = i;
									break;
								}
							}
							i++;
						}

						// remove from group filter list
						MainListHelperVM.Instance.AllGroupFilters.Remove(gf);

						// remove from current wrapper list
						if (pos >= 0)
						{
							MainListHelperVM.Instance.CurrentWrapperList.RemoveAt(pos);
							//MainListHelperVM.Instance.ViewGroups.Refresh();
						}

					}
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

			EnableDisableGroupControls(true);
		}

		private void CommandBinding_Cancel(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(AnimeGroupVM))
				{
					AnimeGroupVM grp = (AnimeGroupVM)obj;
					grp.IsReadOnly = true;
					grp.IsBeingEdited = false;

					// copy all editable properties
					if (grp.AnimeGroupID.HasValue) // an existing group
					{
						grp.GroupName = groupBeforeChanges.GroupName;
						grp.IsFave = groupBeforeChanges.IsFave;

						//grp.AnimeGroupParentID = groupBeforeChanges.AnimeGroupParentID;
						grp.Description = groupBeforeChanges.Description;
						grp.SortName = groupBeforeChanges.SortName;

						MainListHelperVM.Instance.ViewGroups.Refresh();
						EnableDisableGroupControls(true);
						MainListHelperVM.Instance.LastAnimeGroupID = grp.AnimeGroupID.Value;
						HighlightMainListItem();
					}
					else
					{
						HighlightMainListItem();
						SetDetailBinding(null);
					}


				}

				if (obj.GetType() == typeof(GroupFilterVM))
				{
					GroupFilterVM gf = (GroupFilterVM)obj;
					gf.IsLocked = true;
					gf.IsBeingEdited = false;

					// copy all editable properties
					if (gf.GroupFilterID.HasValue) // an existing group filter
					{
						gf.FilterName = groupFilterBeforeChanges.FilterName;
						gf.ApplyToSeries = groupFilterBeforeChanges.ApplyToSeries;
						gf.BaseCondition = groupFilterBeforeChanges.BaseCondition;
						gf.FilterConditions.Clear();
						gf.SortCriteriaList.Clear();

						foreach (GroupFilterConditionVM gfc_old in groupFilterBeforeChanges.FilterConditions)
						{
							GroupFilterConditionVM gfc = new GroupFilterConditionVM();
							gfc.ConditionOperator = gfc_old.ConditionOperator;
							gfc.ConditionParameter = gfc_old.ConditionParameter;
							gfc.ConditionType = gfc_old.ConditionType;
							gfc.GroupFilterConditionID = gfc_old.GroupFilterConditionID;
							gfc.GroupFilterID = gfc_old.GroupFilterID;
							gf.FilterConditions.Add(gfc);
						}

						foreach (GroupFilterSortingCriteria gfsc_old in groupFilterBeforeChanges.SortCriteriaList)
						{
							GroupFilterSortingCriteria gfsc = new GroupFilterSortingCriteria();
							gfsc.GroupFilterID = gfsc_old.GroupFilterID;
							gfsc.SortDirection = gfsc_old.SortDirection;
							gfsc.SortType = gfsc_old.SortType;
							gf.SortCriteriaList.Add(gfsc);
						}

						MainListHelperVM.Instance.LastGroupFilterID = gf.GroupFilterID.Value;
					}
					else
					{
						SetDetailBinding(null);
					}
					EnableDisableGroupControls(true);
					HighlightMainListItem();
				}

				if (obj.GetType() == typeof(AnimeSeriesVM))
				{

				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_NewGroupFilter(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				GroupFilterVM gfNew = new GroupFilterVM();
				gfNew.AllowEditing = true;
				gfNew.IsBeingEdited = true;
				gfNew.IsLocked = false;
				gfNew.FilterName = "New Filter";
				gfNew.ApplyToSeries = 0;
				gfNew.BaseCondition = (int)GroupFilterBaseCondition.Include;
				gfNew.FilterConditions = new ObservableCollection<GroupFilterConditionVM>();

				MainListHelperVM.Instance.AllGroupFilters.Add(gfNew);

				groupFilterVM = gfNew;
				MainListHelperVM.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
				MainListHelperVM.Instance.SetGroupFilterSortingOnForms(gfNew);

				SetDetailBinding(gfNew);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_DeleteFilterCondition(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(GroupFilterConditionVM))
				{
					GroupFilterConditionVM gfc = (GroupFilterConditionVM)obj;

					MessageBoxResult res = MessageBox.Show(string.Format("Are you sure you want to delete the Filter Condition: {0}", gfc.NiceDescription),
					"Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
					if (res == MessageBoxResult.Yes)
					{
						// remove from group list
						//gfc.Delete();


						// fund the GroupFilter

						foreach (GroupFilterVM gf in MainListHelperVM.Instance.AllGroupFilters)
						{
							if (!gf.AllowEditing) continue; // all filter
							if (gf.GroupFilterID == gfc.GroupFilterID)
							{
								int pos = -1;
								for (int i = 0; i < gf.FilterConditions.Count; i++)
								{
									if (gfc.ConditionOperator == gf.FilterConditions[i].ConditionOperator &&
										gfc.ConditionParameter == gf.FilterConditions[i].ConditionParameter &&
										gfc.ConditionType == gf.FilterConditions[i].ConditionType)
									{
										pos = i;
										break;
									}
								}
								if (pos >= 0)
									gf.FilterConditions.RemoveAt(pos);

								groupFilterVM = gf;
								MainListHelperVM.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
								MainListHelperVM.Instance.SetGroupFilterSortingOnForms(gf);
							}
						}


					}
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_NewFilterCondition(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				object obj = e.Parameter;
				if (obj == null) return;

				GroupFilterVM gf = (GroupFilterVM)obj;
				GroupFilterConditionVM gfc = new GroupFilterConditionVM();

				GroupFilterConditionForm frm = new GroupFilterConditionForm();
				frm.Owner = this;
				frm.Init(gf, gfc);
				bool? result = frm.ShowDialog();
				if (result.HasValue && result.Value == true)
				{
					gf.FilterConditions.Add(gfc);

					groupFilterVM = gf;
					MainListHelperVM.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
					MainListHelperVM.Instance.SetGroupFilterSortingOnForms(gf);
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_NewFilterSorting(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				object obj = e.Parameter;
				if (obj == null) return;

				GroupFilterVM gf = (GroupFilterVM)obj;
				GroupFilterSortingCriteria gfsc = new GroupFilterSortingCriteria();

				GroupFilterSortingForm frm = new GroupFilterSortingForm();
				frm.Owner = this;
				frm.Init(gf, gfsc);
				bool? result = frm.ShowDialog();
				if (result.HasValue && result.Value == true)
				{
					gf.SortCriteriaList.Add(gfsc);

					groupFilterVM = gf;
					MainListHelperVM.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
					MainListHelperVM.Instance.SetGroupFilterSortingOnForms(gf);
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_MoveUpFilterSort(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(GroupFilterSortingCriteria))
				{
					GroupFilterSortingCriteria gfsc = (GroupFilterSortingCriteria)obj;
					GroupFilterSortMoveUpDown(gfsc, 1);
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_MoveDownFilterSort(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(GroupFilterSortingCriteria))
				{
					GroupFilterSortingCriteria gfsc = (GroupFilterSortingCriteria)obj;
					GroupFilterSortMoveUpDown(gfsc, 2);
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		/// <summary>
		/// Moves group sorting up and down
		/// </summary>
		/// <param name="gfsc"></param>
		/// <param name="direction">1 = up, 2 = down</param>
		private void GroupFilterSortMoveUpDown(GroupFilterSortingCriteria gfsc, int direction)
		{
			// find the sorting condition
			foreach (GroupFilterVM gf in MainListHelperVM.Instance.AllGroupFilters)
			{
				if (!gf.AllowEditing) continue; // all filter
				if (gf.GroupFilterID == gfsc.GroupFilterID)
				{
					int pos = -1;
					for (int i = 0; i < gf.SortCriteriaList.Count; i++)
					{
						if (gfsc.SortType == gf.SortCriteriaList[i].SortType)
						{
							pos = i;
							break;
						}
					}

					if (direction == 1) // up
					{
						if (pos > 0)
						{
							gf.SortCriteriaList.Move(pos, pos - 1);
							groupFilterVM = gf;
							MainListHelperVM.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
							MainListHelperVM.Instance.SetGroupFilterSortingOnForms(gf);
						}
					}
					else
					{
						if (pos + 1 < gf.SortCriteriaList.Count)
						{
							gf.SortCriteriaList.Move(pos, pos + 1);
							groupFilterVM = gf;
							MainListHelperVM.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
							MainListHelperVM.Instance.SetGroupFilterSortingOnForms(gf);
						}
					}
				}
			}
		}

		

		private void CommandBinding_DeleteFilterSort(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(GroupFilterSortingCriteria))
				{
					GroupFilterSortingCriteria gfsc = (GroupFilterSortingCriteria)obj;

					MessageBoxResult res = MessageBox.Show(string.Format("Are you sure you want to delete the sorting?"),
					"Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
					if (res == MessageBoxResult.Yes)
					{
						// find the sorting condition
						foreach (GroupFilterVM gf in MainListHelperVM.Instance.AllGroupFilters)
						{
							if (!gf.AllowEditing) continue; // all filter
							if (gf.GroupFilterID == gfsc.GroupFilterID)
							{
								int pos = -1;
								for (int i = 0; i < gf.SortCriteriaList.Count; i++)
								{
									if (gfsc.SortType == gf.SortCriteriaList[i].SortType)
									{
										pos = i;
										break;
									}
								}
								if (pos >= 0)
									gf.SortCriteriaList.RemoveAt(pos);

								groupFilterVM = gf;
								MainListHelperVM.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
								MainListHelperVM.Instance.SetGroupFilterSortingOnForms(gf);
							}
						}


					}
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_NewGroup(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				AnimeGroupVM grpNew = new AnimeGroupVM();
				grpNew.IsReadOnly = false;
				grpNew.IsBeingEdited = true;
				SetDetailBinding(grpNew);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_DeleteGroup(object sender, ExecutedRoutedEventArgs e)
		{
			EnableDisableGroupControls(false);

			try
			{
				AnimeGroupVM grp = e.Parameter as AnimeGroupVM;
				if (grp == null) return;

				DeleteSeriesGroupForm frm = new DeleteSeriesGroupForm();
				frm.Owner = this;
				bool? result = frm.ShowDialog();

				if (result.HasValue && result.Value == true)
				{
					bool deleteFiles = frm.DeleteFiles;

					this.Cursor = Cursors.Wait;
					JMMServerVM.Instance.clientBinaryHTTP.DeleteAnimeGroup(grp.AnimeGroupID.Value, deleteFiles);

					MainListHelperVM.Instance.RefreshGroupsSeriesData();
					MainListHelperVM.Instance.ShowChildWrappers(MainListHelperVM.Instance.CurrentWrapper);
					SetDetailBinding(null);
					this.Cursor = Cursors.Arrow;
				}

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

			EnableDisableGroupControls(true);
		}

		private void CommandBinding_ViewGroup(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				AnimeGroupVM grp = e.Parameter as AnimeGroupVM;
				if (grp == null) return;

				SetDetailBinding(grp);

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_AddSubGroup(object sender, ExecutedRoutedEventArgs e)
		{
			AnimeGroupVM grp = e.Parameter as AnimeGroupVM;
			if (grp == null) return;

			try
			{
				AnimeGroupVM grpNew = new AnimeGroupVM();
				grpNew.IsReadOnly = false;
				grpNew.IsBeingEdited = true;
				grpNew.AnimeGroupParentID = grp.AnimeGroupID.Value;
				SetDetailBinding(grpNew);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}



		private void CommandBinding_NewSeries(object sender, ExecutedRoutedEventArgs e)
		{

		}

		private void CommandBinding_DeleteSeries(object sender, ExecutedRoutedEventArgs e)
		{
			AnimeSeriesVM ser = e.Parameter as AnimeSeriesVM;
			if (ser == null) return;

			try
			{
				DeleteSeriesGroupForm frm = new DeleteSeriesGroupForm();
				frm.Owner = this;
				bool? result = frm.ShowDialog();

				if (result.HasValue && result.Value == true)
				{
					bool deleteFiles = frm.DeleteFiles;

					this.Cursor = Cursors.Wait;
					JMMServerVM.Instance.clientBinaryHTTP.DeleteAnimeSeries(ser.AnimeSeriesID.Value, deleteFiles);

					MainListHelperVM.Instance.RefreshGroupsSeriesData();
					MainListHelperVM.Instance.ShowChildWrappers(MainListHelperVM.Instance.CurrentWrapper);
					SetDetailBinding(null);
					this.Cursor = Cursors.Arrow;
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

		private void CommandBinding_MoveSeries(object sender, ExecutedRoutedEventArgs e)
		{
			AnimeSeriesVM ser = e.Parameter as AnimeSeriesVM;
			if (ser == null) return;

			try
			{
				MoveSeries frm = new MoveSeries();
				frm.Owner = this;
				frm.Init(ser);
				bool? result = frm.ShowDialog();

				if (result.HasValue && result.Value == true)
				{
					AnimeGroupVM grpSelected = frm.SelectedGroup;
					if (grpSelected == null) return;

					MoveSeriesDetails request = new MoveSeriesDetails();
					request.OldAnimeGroupID = ser.AnimeGroupID;

					ser.AnimeGroupID = grpSelected.AnimeGroupID.Value;
					request.UpdatedSeries = ser;

					this.Cursor = Cursors.Wait;
					EnableDisableGroupControls(false);
					moveSeriesWorker.RunWorkerAsync(request);
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_ClearSearch(object sender, ExecutedRoutedEventArgs e)
		{
			txtGroupSearch.Text = "";
			HighlightMainListItem();
		}

		private void CommandBinding_RunImport(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				JMMServerVM.Instance.RunImport();
				MessageBox.Show("Import is Running", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

		}

		private void CommandBinding_RemoveMissingFiles(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				JMMServerVM.Instance.RemoveMissingFiles();
				MessageBox.Show("Process is Running", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_SyncMyList(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				JMMServerVM.Instance.SyncMyList();
				MessageBox.Show("Process is Running", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_SyncVotes(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				JMMServerVM.Instance.SyncVotes();
				MessageBox.Show("Process is Running", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_RevokeVote(object sender, ExecutedRoutedEventArgs e)
		{
			AnimeSeriesVM ser = e.Parameter as AnimeSeriesVM;
			if (ser == null) return;

			try
			{
				JMMServerVM.Instance.RevokeVote(ser.AniDB_ID);

				// refresh the data
				//ser.RefreshBase();
				//ser.AniDB_Anime.Detail.RefreshBase();

				MainListHelperVM.Instance.UpdateHeirarchy(ser);

				//SetDetailBinding(null);
				//SetDetailBinding(ser);

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}


		private void CommandBinding_ToggleWatchedStatus(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			this.Cursor = Cursors.Wait;
			EnableDisableGroupControls(false);
			toggleStatusWorker.RunWorkerAsync(obj);
		}



		private void CommandBinding_BreadCrumbSelect(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				// switching back to the top view (show all filters)
				if (e.Parameter == null)
				{
					MainListHelperVM.Instance.ShowChildWrappers(null);
				}
				if (e.Parameter is MainListWrapper)
				{
					MainListHelperVM.Instance.ShowChildWrappers(e.Parameter as MainListWrapper);
				}
				HighlightMainListItem();
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_ToggleFave(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			if (obj.GetType() == typeof(AnimeGroupVM))
			{
				AnimeGroupVM grp = (AnimeGroupVM)obj;
				grp.IsFave = grp.IsFave == 1 ? 0 : 1;

				// the user can toggle the fave without going into edit mode
				if (grp.IsReadOnly)
					grp.Save();


				//BindingExpression be = ccDetail.GetBindingExpression(ContentControl.ContentProperty);
				//be.UpdateSource();
			}
		}

		private void CommandBinding_ToggleExpandTags(object sender, ExecutedRoutedEventArgs e)
		{
			UserSettingsVM.Instance.TagsExpanded = !UserSettingsVM.Instance.TagsExpanded;
		}

		private void CommandBinding_ToggleExpandTitles(object sender, ExecutedRoutedEventArgs e)
		{
			UserSettingsVM.Instance.TitlesExpanded = !UserSettingsVM.Instance.TitlesExpanded;
		}


		#region Server Queue Actions

		private void CommandBinding_HasherQueuePause(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				JMMServerVM.Instance.clientBinaryHTTP.SetCommandProcessorHasherPaused(true);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_HasherQueueResume(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				JMMServerVM.Instance.clientBinaryHTTP.SetCommandProcessorHasherPaused(false);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_GeneralQueuePause(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				JMMServerVM.Instance.clientBinaryHTTP.SetCommandProcessorGeneralPaused(true);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_GeneralQueueResume(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				JMMServerVM.Instance.clientBinaryHTTP.SetCommandProcessorGeneralPaused(false);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		#endregion

		#endregion

		
		

		

		public bool GroupFilter_GroupSearch(object obj)
		{
			AnimeGroupVM grpvm = obj as AnimeGroupVM;
			if (grpvm == null) return true;

			return GroupSearchFilterHelper.EvaluateGroupFilter(groupFilterVM, grpvm);
		}

		

		void lbGroupsSeries_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				//epListMain.DataContext = null;

				System.Windows.Controls.ListBox lb = (System.Windows.Controls.ListBox)sender;

				object obj = lb.SelectedItem;
				if (obj == null) return;

				if (obj.GetType() == typeof(AnimeSeriesVM))
				{

					AnimeSeriesVM series = obj as AnimeSeriesVM;
					series.RefreshBase();
					MainListHelperVM.Instance.LastAnimeSeriesID = series.AnimeSeriesID.Value;
					MainListHelperVM.Instance.CurrentSeries = series;
				}

				if (obj.GetType() == typeof(AnimeGroupVM))
				{
					AnimeGroupVM grp = obj as AnimeGroupVM;
					MainListHelperVM.Instance.LastAnimeGroupID = grp.AnimeGroupID.Value;

					if (MainListHelperVM.Instance.LastGroupFilterID != 0 && lbGroupsSeries.SelectedItem != null)
						MainListHelperVM.Instance.LastGroupForGF[MainListHelperVM.Instance.LastGroupFilterID] = lbGroupsSeries.SelectedIndex;
					
				}

				if (obj.GetType() == typeof(GroupFilterVM))
				{
					GroupFilterVM gf = obj as GroupFilterVM;
					MainListHelperVM.Instance.LastGroupFilterID = gf.GroupFilterID.Value;

					groupFilterVM = gf;
					MainListHelperVM.Instance.ViewGroupsForms.Filter = GroupFilter_GroupSearch;
					MainListHelperVM.Instance.SetGroupFilterSortingOnForms(gf);
				}

				//SetDetailBinding(MainListHelperVM.Instance.AllGroups[0]);
				SetDetailBinding(obj);

				if (obj.GetType() == typeof(AnimeSeriesVM))
				{
					AnimeSeriesVM series = obj as AnimeSeriesVM;
					//epListMain.DataContext = series;
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void lbGroupsSeries_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (lbGroupsSeries.SelectedItem == null) return;

			if (lbGroupsSeries.SelectedItem is MainListWrapper)
			{
				//SetDetailBinding(null);
				// this is the last supported drill down
				if (lbGroupsSeries.SelectedItem.GetType() == typeof(AnimeSeriesVM)) return;

				EnableDisableGroupControls(false);
				showChildWrappersWorker.RunWorkerAsync(lbGroupsSeries.SelectedItem);
			}
		}


		private void HighlightMainListItem()
		{
			try
			{
				int wrapperID = 0;
				if (MainListHelperVM.Instance.CurrentWrapper == null)
					wrapperID = MainListHelperVM.Instance.LastGroupFilterID;
				else if (MainListHelperVM.Instance.CurrentWrapper is GroupFilterVM)
					wrapperID = MainListHelperVM.Instance.LastAnimeGroupID;
				else if (MainListHelperVM.Instance.CurrentWrapper is AnimeGroupVM)
					wrapperID = MainListHelperVM.Instance.LastAnimeSeriesID;

				if (wrapperID == 0)
				{
					if (lbGroupsSeries.Items != null && lbGroupsSeries.Items.Count > 0)
					{
						lbGroupsSeries.SelectedIndex = 0;
						lbGroupsSeries.Focus();
					}
				}
				else
				{
					if (MainListHelperVM.Instance.CurrentWrapper is GroupFilterVM)
					{
						// if we are looking at a list of groups
						// move to the next item
						if (MainListHelperVM.Instance.LastGroupFilterID != 0
							&& MainListHelperVM.Instance.LastGroupForGF.ContainsKey(MainListHelperVM.Instance.LastGroupFilterID))
						{
							int lastSelIndex = MainListHelperVM.Instance.LastGroupForGF[MainListHelperVM.Instance.LastGroupFilterID];
							if (lastSelIndex < lbGroupsSeries.Items.Count)
							{
								lbGroupsSeries.SelectedItem = lbGroupsSeries.Items[lastSelIndex];
								lbGroupsSeries.Focus();
								lbGroupsSeries.ScrollIntoView(lbGroupsSeries.Items[lastSelIndex]);
								SetDetailBinding(lbGroupsSeries.SelectedItem);
							}
							else
							{
								// move to the previous item
								if (lastSelIndex - 1 <= lbGroupsSeries.Items.Count)
								{
									if (lastSelIndex > 0)
									{
										lbGroupsSeries.SelectedItem = lbGroupsSeries.Items[lastSelIndex - 1];
										lbGroupsSeries.Focus();
										lbGroupsSeries.ScrollIntoView(lbGroupsSeries.Items[lastSelIndex - 1]);
										SetDetailBinding(lbGroupsSeries.SelectedItem);
									}
								}
							}
						}
						return;
					}
					else
					{
						foreach (var lbItem in lbGroupsSeries.Items)
						{
							if (lbItem is GroupFilterVM)
							{
								GroupFilterVM gf = lbItem as GroupFilterVM;
								if (gf.GroupFilterID == wrapperID)
								{
									lbGroupsSeries.SelectedItem = lbItem;
									lbGroupsSeries.Focus();
									lbGroupsSeries.ScrollIntoView(lbItem);
									SetDetailBinding(gf);
									return;
								}
							}
							/*if (lbItem is AnimeGroupVM)
							{
								AnimeGroupVM ag = lbItem as AnimeGroupVM;
								if (ag.AnimeGroupID == wrapperID)
								{
									lbGroupsSeries.SelectedItem = lbItem;
									lbGroupsSeries.Focus();
									lbGroupsSeries.ScrollIntoView(lbItem);
									SetDetailBinding(ag);
									return;
								}
							}*/
							if (lbItem is AnimeSeriesVM)
							{
								AnimeSeriesVM series = lbItem as AnimeSeriesVM;
								if (series.AnimeSeriesID == wrapperID)
								{
									lbGroupsSeries.SelectedItem = lbItem;
									lbGroupsSeries.Focus();
									lbGroupsSeries.ScrollIntoView(lbItem);
									SetDetailBinding(series);
									return;
								}
							}
							if (lbItem is AnimeEpisodeVM)
							{
								AnimeEpisodeVM ep = lbItem as AnimeEpisodeVM;
								if (ep.AnimeEpisodeID == wrapperID)
								{
									lbGroupsSeries.SelectedItem = lbItem;
									lbGroupsSeries.Focus();
									lbGroupsSeries.ScrollIntoView(lbItem);
									return;
								}
							}
						}
					}
				}
				if (lbGroupsSeries.Items != null && lbGroupsSeries.Items.Count > 0)
				{
					lbGroupsSeries.SelectedIndex = 0;
					lbGroupsSeries.Focus();
					
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}
		

		

		private void EnableDisableGroupControls(bool val)
		{
			lbGroupsSeries.IsEnabled = val;
			txtGroupSearch.IsEnabled = val;
			tbSeriesEpisodes.IsEnabled = val;
			//epListMain.IsEnabled = val;
			//ccDetail.IsEnabled = val;
		}


		private void SetDetailBinding(object objToBind)
		{
			try
			{
				//BindingOperations.ClearBinding(ccDetail, ContentControl.ContentProperty);
				Binding b = new Binding();
				b.Source = objToBind;
				b.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
				ccDetail.SetBinding(ContentControl.ContentProperty, b);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			
		}


		

		void URL_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}

		void grdMain_LayoutUpdated(object sender, EventArgs e)
		{
		}
	}

	

	
}

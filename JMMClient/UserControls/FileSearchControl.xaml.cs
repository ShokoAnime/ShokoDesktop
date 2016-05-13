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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using JMMClient.ViewModel;
using System.IO;
using System.Threading;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for FileSearchControl.xaml
	/// </summary>
	public partial class FileSearchControl : UserControl
	{
		public ICollectionView ViewFiles { get; set; }
		public ObservableCollection<VideoLocalVM> FileResults { get; set; }

		public static readonly DependencyProperty FileCountProperty = DependencyProperty.Register("FileCount",
			typeof(int), typeof(FileSearchControl), new UIPropertyMetadata(0, null));

		public int FileCount
		{
			get { return (int)GetValue(FileCountProperty); }
			set { SetValue(FileCountProperty, value); }
		}

		private readonly string SearchTypeFileName = JMMClient.Properties.Resources.Search_FileName;
		private readonly string SearchTypeHash = JMMClient.Properties.Resources.Search_Hash;
		private readonly string SearchTypeTopOneHundred = JMMClient.Properties.Resources.Search_Last100;

        private readonly string SearchTypeFileSize = JMMClient.Properties.Resources.Search_FileSize;

		BackgroundWorker getDetailsWorker = new BackgroundWorker();
		private int displayingVidID = 0;

		public FileSearchControl()
		{
			InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            FileResults = new ObservableCollection<VideoLocalVM>();
			ViewFiles = CollectionViewSource.GetDefaultView(FileResults);

			btnSearch.Click += new RoutedEventHandler(btnSearch_Click);
			lbVideos.SelectionChanged += new SelectionChangedEventHandler(lbVideos_SelectionChanged);

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            cboSearchType.Items.Clear();
			cboSearchType.Items.Add(JMMClient.Properties.Resources.Search_FileName);
			cboSearchType.Items.Add(JMMClient.Properties.Resources.Search_Hash);
			cboSearchType.Items.Add(JMMClient.Properties.Resources.Search_Last100);
			cboSearchType.SelectedIndex = 0;

			cboSearchType.SelectionChanged += new SelectionChangedEventHandler(cboSearchType_SelectionChanged);

			getDetailsWorker.DoWork += new DoWorkEventHandler(getDetailsWorker_DoWork);
			getDetailsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(getDetailsWorker_RunWorkerCompleted);
		}

		void lbVideos_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// get detailed video, episode and series info
			ccDetail.Content = null;
			ccSeriesDetail.Content = null;
			if (lbVideos.SelectedItems.Count == 0) return;
			if (lbVideos.SelectedItem == null) return;

			VideoLocalVM vid = lbVideos.SelectedItem as VideoLocalVM;
			displayingVidID = vid.VideoLocalID;
			EnableDisableControls(false);

			try
			{
				this.Cursor = Cursors.Wait;

				ccDetail.Content = vid;

				// get the episode(s)
				List<JMMServerBinary.Contract_AnimeEpisode> rawEps = JMMServerVM.Instance.clientBinaryHTTP.GetEpisodesForFile(
					vid.VideoLocalID, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

				if (rawEps.Count > 0)
				{
					AnimeEpisodeVM ep = new AnimeEpisodeVM(rawEps[0]);
					ccSeriesDetail.Content = ep;
				}

				this.Cursor = Cursors.Arrow;

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
				this.Cursor = Cursors.Arrow;
				EnableDisableControls(true);
			}
		}

		void getDetailsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			
		}

		void getDetailsWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			
		}

		void cboSearchType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			FileSearchCriteria searchType = FileSearchCriteria.Name;
			if (cboSearchType.SelectedItem.ToString() == SearchTypeHash) searchType = FileSearchCriteria.ED2KHash;
			if (cboSearchType.SelectedItem.ToString() == SearchTypeTopOneHundred) searchType = FileSearchCriteria.LastOneHundred;

			if (searchType == FileSearchCriteria.LastOneHundred)
				txtFileSearch.Visibility = System.Windows.Visibility.Collapsed;
			else
				txtFileSearch.Visibility = System.Windows.Visibility.Visible;
		}

		

		void btnSearch_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!JMMServerVM.Instance.ServerOnline) return;

				FileSearchCriteria searchType = FileSearchCriteria.Name;
				if (cboSearchType.SelectedItem.ToString() == SearchTypeHash) searchType = FileSearchCriteria.ED2KHash;
				if (cboSearchType.SelectedItem.ToString() == SearchTypeTopOneHundred) searchType = FileSearchCriteria.LastOneHundred;

				if (txtFileSearch.Text.Trim().Length == 0 && searchType != FileSearchCriteria.LastOneHundred)
				{
					MessageBox.Show(Properties.Resources.Seach_Criteria, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
					txtFileSearch.Focus();
					return;
				}

				FileResults.Clear();
				ViewFiles.Refresh();
				FileCount = 0;

				this.Cursor = Cursors.Wait;
				EnableDisableControls(false);
				List<JMMServerBinary.Contract_VideoLocal> rawVids = JMMServerVM.Instance.clientBinaryHTTP.SearchForFiles(
					(int)searchType, txtFileSearch.Text, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

				foreach (JMMServerBinary.Contract_VideoLocal raw in rawVids)
					FileResults.Add(new VideoLocalVM(raw));

				FileCount = rawVids.Count;

				this.Cursor = Cursors.Arrow;

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			finally
			{
				this.Cursor = Cursors.Arrow;
				EnableDisableControls(true);
			}
		}

		private void EnableDisableControls(bool val)
		{
			lbVideos.IsEnabled = val;
			btnSearch.IsEnabled = val;
			ccDetail.IsEnabled = val;
			ccSeriesDetail.IsEnabled = val;
		}

		private void CommandBinding_OpenFolder(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			if (obj.GetType() == typeof(VideoLocalVM))
			{
				VideoLocalVM vid = obj as VideoLocalVM;

				if (File.Exists(vid.FullPath))
				{
					Utils.OpenFolderAndSelectFile(vid.FullPath);
				}
				else
				{
					MessageBox.Show(Properties.Resources.MSG_ERR_FileNotFound, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		

		private void CommandBinding_RehashFile(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				Window parentWindow = Window.GetWindow(this);

				object obj = e.Parameter;
				if (obj == null) return;

				if (obj.GetType() == typeof(VideoLocalVM))
				{
					VideoLocalVM vid = obj as VideoLocalVM;
					EnableDisableControls(false);

					JMMServerVM.Instance.clientBinaryHTTP.RehashFile(vid.VideoLocalID);
				}

				MessageBox.Show(Properties.Resources.MSG_INFO_AddedQueueCmds, "Done", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

			EnableDisableControls(true);
		}

		private void CommandBinding_RescanFile(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				Window parentWindow = Window.GetWindow(this);

				object obj = e.Parameter;
				if (obj == null) return;

				if (obj.GetType() == typeof(VideoLocalVM))
				{
					VideoLocalVM vid = obj as VideoLocalVM;
					EnableDisableControls(false);

					JMMServerVM.Instance.clientBinaryHTTP.RescanFile(vid.VideoLocalID);
				}

				MessageBox.Show(Properties.Resources.MSG_INFO_AddedQueueCmds, Properties.Resources.Done, MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

			EnableDisableControls(true);
		}
	}
}

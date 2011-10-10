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
using JMMClient.ViewModel;
using System.IO;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for IgnoredFiles.xaml
	/// </summary>
	public partial class IgnoredFiles : UserControl
	{
		public ICollectionView ViewFiles { get; set; }
		public ObservableCollection<VideoLocalVM> IgnoredFilesCollection { get; set; }

		public static readonly DependencyProperty OneVideoSelectedProperty = DependencyProperty.Register("OneVideoSelected",
			typeof(bool), typeof(IgnoredFiles), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty MultipleVideosSelectedProperty = DependencyProperty.Register("MultipleVideosSelected",
			typeof(bool), typeof(IgnoredFiles), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty FileCountProperty = DependencyProperty.Register("FileCount",
			typeof(int), typeof(IgnoredFiles), new UIPropertyMetadata(0, null));

		public int FileCount
		{
			get { return (int)GetValue(FileCountProperty); }
			set { SetValue(FileCountProperty, value); }
		}

		public bool OneVideoSelected
		{
			get { return (bool)GetValue(OneVideoSelectedProperty); }
			set { SetValue(OneVideoSelectedProperty, value); }
		}

		public bool MultipleVideosSelected
		{
			get { return (bool)GetValue(MultipleVideosSelectedProperty); }
			set { SetValue(MultipleVideosSelectedProperty, value); }
		}

		public IgnoredFiles()
		{
			InitializeComponent();

			IgnoredFilesCollection = new ObservableCollection<VideoLocalVM>();
			ViewFiles = CollectionViewSource.GetDefaultView(IgnoredFilesCollection);
			ViewFiles.SortDescriptions.Add(new SortDescription("FullPath", ListSortDirection.Ascending));

			btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
	
			lbVideos.SelectionChanged += new SelectionChangedEventHandler(lbVideos_SelectionChanged);
			OneVideoSelected = lbVideos.SelectedItems.Count == 1;
			MultipleVideosSelected = lbVideos.SelectedItems.Count > 1;
		}

		void lbVideos_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				ccDetail.Content = null;
				ccDetailMultiple.Content = null;

				OneVideoSelected = lbVideos.SelectedItems.Count == 1;
				MultipleVideosSelected = lbVideos.SelectedItems.Count > 1;

				// if only one video selected
				if (OneVideoSelected)
				{
					VideoLocalVM vid = lbVideos.SelectedItem as VideoLocalVM;
					ccDetail.Content = vid;
				}

				// if only one video selected
				if (MultipleVideosSelected)
				{
					MultipleVideos mv = new MultipleVideos();
					mv.SelectedCount = lbVideos.SelectedItems.Count;
					mv.VideoLocalIDs = new List<int>();

					foreach (object obj in lbVideos.SelectedItems)
					{
						VideoLocalVM vid = obj as VideoLocalVM;
						mv.VideoLocalIDs.Add(vid.VideoLocalID);
					}

					ccDetailMultiple.Content = mv;
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void CommandBinding_OpenFolder(object sender, ExecutedRoutedEventArgs e)
		{
			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(VideoLocalVM))
				{
					VideoLocalVM vid = obj as VideoLocalVM;

					if (File.Exists(vid.FullPath))
					{
						Utils.OpenFolderAndSelectFile(vid.FullPath);
					}
					else
					{
						MessageBox.Show(Properties.Resources.MSG_ERR_FileNotFound, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void EnableDisableControls(bool val)
		{
			lbVideos.IsEnabled = val;
			btnRefresh.IsEnabled = val;
			ccDetail.IsEnabled = val;
			ccDetailMultiple.IsEnabled = val;
		}

		private void CommandBinding_RestoreFile(object sender, ExecutedRoutedEventArgs e)
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

					string result = JMMServerVM.Instance.clientBinaryHTTP.SetIgnoreStatusOnFile(vid.VideoLocalID, false);
					if (result.Length > 0)
						MessageBox.Show(result, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					else
						RefreshIgnoredFiles();

				}
				if (obj.GetType() == typeof(MultipleVideos))
				{
					MultipleVideos mv = obj as MultipleVideos;
					foreach (int id in mv.VideoLocalIDs)
					{
						string result = JMMServerVM.Instance.clientBinaryHTTP.SetIgnoreStatusOnFile(id, false);
						if (result.Length > 0)
							MessageBox.Show(result, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					}
					RefreshIgnoredFiles();
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

			EnableDisableControls(true);
		}

		void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			RefreshIgnoredFiles();
		}

		public void RefreshIgnoredFiles()
		{
			try
			{
				IgnoredFilesCollection.Clear();

				List<JMMServerBinary.Contract_VideoLocal> vids = JMMServerVM.Instance.clientBinaryHTTP.GetIgnoredFiles(JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				FileCount = vids.Count;

				foreach (JMMServerBinary.Contract_VideoLocal vid in vids)
				{
					IgnoredFilesCollection.Add(new VideoLocalVM(vid));
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}
	}
}

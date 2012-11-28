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

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for ManuallyLinkedFilesControl.xaml
	/// </summary>
	public partial class ManuallyLinkedFilesControl : UserControl
	{
		public ICollectionView ViewFiles { get; set; }
		public ObservableCollection<VideoLocalVM> ManuallyLinkedFiles { get; set; }

		public static readonly DependencyProperty FileCountProperty = DependencyProperty.Register("FileCount",
			typeof(int), typeof(ManuallyLinkedFilesControl), new UIPropertyMetadata(0, null));

		public int FileCount
		{
			get { return (int)GetValue(FileCountProperty); }
			set { SetValue(FileCountProperty, value); }
		}

		public ManuallyLinkedFilesControl()
		{
			InitializeComponent();

			ManuallyLinkedFiles = new ObservableCollection<VideoLocalVM>();
			ViewFiles = CollectionViewSource.GetDefaultView(ManuallyLinkedFiles);
			ViewFiles.SortDescriptions.Add(new SortDescription("FullPath", ListSortDirection.Ascending));
			ViewFiles.Filter = FileSearchFilter;

			btnClearSearch.Click += new RoutedEventHandler(btnClearSearch_Click);
			txtFileSearch.TextChanged += new TextChangedEventHandler(txtFileSearch_TextChanged);
			btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
			btnRescan.Click += new RoutedEventHandler(btnRescan_Click);
		}

		void btnRescan_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!JMMServerVM.Instance.ServerOnline) return;

				this.Cursor = Cursors.Wait;
				JMMServerVM.Instance.clientBinaryHTTP.RescanManuallyLinkedFiles();
				this.Cursor = Cursors.Arrow;

				MessageBox.Show("Files queued for AniDB scan", "Complete", MessageBoxButton.OK, MessageBoxImage.Information);

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

		void txtFileSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			ViewFiles.Refresh();
		}

		void btnClearSearch_Click(object sender, RoutedEventArgs e)
		{
			txtFileSearch.Text = "";
		}

		void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			RefreshLinkedFiles();
		}

		public void RefreshLinkedFiles()
		{
			try
			{
				ManuallyLinkedFiles.Clear();
				if (!JMMServerVM.Instance.ServerOnline) return;

				List<JMMServerBinary.Contract_VideoLocal> vids = JMMServerVM.Instance.clientBinaryHTTP.GetAllManuallyLinkedFiles(JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				FileCount = vids.Count;

				foreach (JMMServerBinary.Contract_VideoLocal vid in vids)
				{
					ManuallyLinkedFiles.Add(new VideoLocalVM(vid));
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private bool FileSearchFilter(object obj)
		{
			VideoLocalVM vid = obj as VideoLocalVM;
			if (vid == null) return true;

			int index = vid.FilePath.IndexOf(txtFileSearch.Text.Trim(), 0, StringComparison.InvariantCultureIgnoreCase);
			if (index > -1) return true;
			return false;
		}
	}
}

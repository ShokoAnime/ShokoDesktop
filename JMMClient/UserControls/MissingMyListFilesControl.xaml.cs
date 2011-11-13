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
using System.Threading;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for MissingMyListFilesControl.xaml
	/// </summary>
	public partial class MissingMyListFilesControl : UserControl
	{
		BackgroundWorker workerFiles = new BackgroundWorker();
		BackgroundWorker workerDeleteFiles = new BackgroundWorker();

		public ICollectionView ViewFiles { get; set; }
		public ObservableCollection<MissingFileVM> MissingFilesCollection { get; set; }

		private List<JMMServerBinary.Contract_MissingFile> contracts = new List<JMMServerBinary.Contract_MissingFile>();

		public static readonly DependencyProperty FileCountProperty = DependencyProperty.Register("FileCount",
			typeof(int), typeof(MissingMyListFilesControl), new UIPropertyMetadata(0, null));

		public int FileCount
		{
			get { return (int)GetValue(FileCountProperty); }
			set { SetValue(FileCountProperty, value); }
		}

		public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading",
			typeof(bool), typeof(MissingMyListFilesControl), new UIPropertyMetadata(false, null));

		public bool IsLoading
		{
			get { return (bool)GetValue(IsLoadingProperty); }
			set 
			{ 
				SetValue(IsLoadingProperty, value);
				IsNotLoading = !IsLoading;
			}
		}

		public static readonly DependencyProperty IsNotLoadingProperty = DependencyProperty.Register("IsNotLoading",
			typeof(bool), typeof(MissingMyListFilesControl), new UIPropertyMetadata(true, null));

		public bool IsNotLoading
		{
			get { return (bool)GetValue(IsNotLoadingProperty); }
			set { SetValue(IsNotLoadingProperty, value); }
		}

		public static readonly DependencyProperty ReadyToRemoveFilesProperty = DependencyProperty.Register("ReadyToRemoveFiles",
			typeof(bool), typeof(MissingMyListFilesControl), new UIPropertyMetadata(false, null));

		public static readonly DependencyProperty StatusMessageProperty = DependencyProperty.Register("StatusMessage",
			typeof(string), typeof(MissingMyListFilesControl), new UIPropertyMetadata("", null));

		public string StatusMessage
		{
			get { return (string)GetValue(StatusMessageProperty); }
			set { SetValue(StatusMessageProperty, value); }
		}

		public bool ReadyToRemoveFiles
		{
			get { return (bool)GetValue(ReadyToRemoveFilesProperty); }
			set { SetValue(ReadyToRemoveFilesProperty, value); }
		}

		public MissingMyListFilesControl()
		{
			InitializeComponent();

			ReadyToRemoveFiles = false;
			IsLoading = false;

			MissingFilesCollection = new ObservableCollection<MissingFileVM>();
			ViewFiles = CollectionViewSource.GetDefaultView(MissingFilesCollection);
			ViewFiles.SortDescriptions.Add(new SortDescription("AnimeTitle", ListSortDirection.Ascending));
			ViewFiles.SortDescriptions.Add(new SortDescription("EpisodeTypeAndNumber", ListSortDirection.Ascending));

			btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
			btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

			workerFiles.DoWork += new DoWorkEventHandler(workerFiles_DoWork);
			workerFiles.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerFiles_RunWorkerCompleted);

			workerDeleteFiles.WorkerReportsProgress = true;
			workerDeleteFiles.DoWork += new DoWorkEventHandler(workerDeleteFiles_DoWork);
			workerDeleteFiles.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerDeleteFiles_RunWorkerCompleted);
			workerDeleteFiles.ProgressChanged += new ProgressChangedEventHandler(workerDeleteFiles_ProgressChanged);
		}

		void workerDeleteFiles_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			StatusMessage = e.UserState.ToString();	
		}

		void workerDeleteFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			btnRefresh.IsEnabled = true;
			btnDelete.IsEnabled = true;

			IsLoading = false;
			ReadyToRemoveFiles = false;
			FileCount = 0;

			StatusMessage = "Complete";
		}

		void workerDeleteFiles_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				List<MissingFileVM> fls = e.Argument as List<MissingFileVM>;

				int i = 0;
				foreach (MissingFileVM mf in fls)
				{
					i++;
					string msg = string.Format("Queueing deletion {0} of {1}", i, fls.Count);
					workerDeleteFiles.ReportProgress(0, msg);
					JMMServerVM.Instance.clientBinaryHTTP.DeleteFileFromMyList(mf.FileID);
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

		}

		void workerFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			contracts = e.Result as List<JMMServerBinary.Contract_MissingFile>;
			foreach (JMMServerBinary.Contract_MissingFile mf in contracts)
				MissingFilesCollection.Add(new MissingFileVM(mf));
			FileCount = contracts.Count;
			ReadyToRemoveFiles = FileCount >= 1;
			btnRefresh.IsEnabled = true;
			IsLoading = false;
			this.Cursor = Cursors.Arrow;
		}

		void workerFiles_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				List<JMMServerBinary.Contract_MissingFile> contractsTemp = JMMServerVM.Instance.clientBinaryHTTP.GetMyListFilesForRemoval(JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				e.Result = contractsTemp;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			IsLoading = true;
			btnRefresh.IsEnabled = false;
			MissingFilesCollection.Clear();
			FileCount = 0;

			StatusMessage = "Loading...";

			workerFiles.RunWorkerAsync();
		}

		void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			

			MessageBoxResult res = MessageBox.Show(string.Format("Are you sure you want to delete all these files from your AniDB list?"),
					"Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (res == MessageBoxResult.Yes)
			{
				IsLoading = true;
				btnRefresh.IsEnabled = false;
				btnDelete.IsEnabled = false;
				
				ReadyToRemoveFiles = false;

				StatusMessage = "Preparing to queue files for removal on server";
				//Thread.Sleep(1500);

				List<MissingFileVM> mfs = new List<MissingFileVM>(MissingFilesCollection);

				workerDeleteFiles.RunWorkerAsync(mfs);

				MissingFilesCollection.Clear();
				FileCount = 0;
			}

		}
	}
}

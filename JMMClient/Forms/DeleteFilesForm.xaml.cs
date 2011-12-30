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
using System.Windows.Shapes;
using JMMClient.ViewModel;
using System.ComponentModel;
using System.Threading;

namespace JMMClient.Forms
{
	/// <summary>
	/// Interaction logic for DeleteFilesForm.xaml
	/// </summary>
	public partial class DeleteFilesForm : Window
	{
		public static readonly DependencyProperty GroupVideoQualityProperty = DependencyProperty.Register("GroupVideoQuality",
			typeof(GroupVideoQualityVM), typeof(DeleteFilesForm), new UIPropertyMetadata(null, null));

		public static readonly DependencyProperty AnimeIDProperty = DependencyProperty.Register("AnimeID",
			typeof(int), typeof(DeleteFilesForm), new UIPropertyMetadata(0, null));

		public static readonly DependencyProperty FileCountProperty = DependencyProperty.Register("FileCount",
			typeof(int), typeof(DeleteFilesForm), new UIPropertyMetadata(0, null));

		public static readonly DependencyProperty DeleteStatusProperty = DependencyProperty.Register("DeleteStatus",
			typeof(string), typeof(DeleteFilesForm), new UIPropertyMetadata("", null));

		public GroupVideoQualityVM GroupVideoQuality
		{
			get { return (GroupVideoQualityVM)GetValue(GroupVideoQualityProperty); }
			set { SetValue(GroupVideoQualityProperty, value); }
		}

		public int AnimeID
		{
			get { return (int)GetValue(AnimeIDProperty); }
			set { SetValue(AnimeIDProperty, value); }
		}

		public int FileCount
		{
			get { return (int)GetValue(FileCountProperty); }
			set { SetValue(FileCountProperty, value); }
		}

		public string DeleteStatus
		{
			get { return (string)GetValue(DeleteStatusProperty); }
			set { SetValue(DeleteStatusProperty, value); }
		}

		private BackgroundWorker deleteFilesWorker = new BackgroundWorker();
		public bool FilesDeleted { get; set; }
		private bool inProgress = false;
		private List<VideoDetailedVM> vids = new List<VideoDetailedVM>();

		public DeleteFilesForm()
		{
			InitializeComponent();

			deleteFilesWorker.WorkerReportsProgress = true;

			deleteFilesWorker.DoWork += new DoWorkEventHandler(deleteFilesWorker_DoWork);
			deleteFilesWorker.ProgressChanged += new ProgressChangedEventHandler(deleteFilesWorker_ProgressChanged);
			deleteFilesWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(deleteFilesWorker_RunWorkerCompleted);

			btnOK.Click += new RoutedEventHandler(btnOK_Click);
			this.Closing += new CancelEventHandler(DeleteFilesForm_Closing);
			FilesDeleted = false;
		}

		void DeleteFilesForm_Closing(object sender, CancelEventArgs e)
		{
			if (inProgress)
			{
				e.Cancel = true;
				MessageBox.Show("Please wait until the process is complete", "Stop", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			this.DialogResult = FilesDeleted;
		}

		void btnOK_Click(object sender, RoutedEventArgs e)
		{
			string msg = string.Format("Are you sure you want to delete these {0} file/s, the physical video files will also be deleted", vids.Count);
			MessageBoxResult res = MessageBox.Show(msg, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);

			if (res == MessageBoxResult.Yes)
			{
				this.Cursor = Cursors.Wait;
				btnOK.Visibility = System.Windows.Visibility.Hidden;
				inProgress = true;
				deleteFilesWorker.RunWorkerAsync(vids);
			}
		}

		void deleteFilesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			this.Cursor = Cursors.Arrow;
			inProgress = false;
		}

		void deleteFilesWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			FilesDeleted = true;
			string msg = e.UserState as string;
			DeleteStatus = msg;
		}

		void deleteFilesWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			List<VideoDetailedVM> vids = e.Argument as List<VideoDetailedVM>;
			if (vids == null) return;

			int i = 0;
			foreach (VideoDetailedVM vid in vids)
			{
				i++;
				string msg = string.Format("Deleting file {0} of {1}", i, vids.Count);
				deleteFilesWorker.ReportProgress(0, msg);
				//Thread.Sleep(500);
				string result = JMMServerVM.Instance.clientBinaryHTTP.DeleteVideoLocalAndFile(vid.VideoLocalID);
				if (result.Length > 0)
				{
					deleteFilesWorker.ReportProgress(0, result);
					return;
				}
			}

			deleteFilesWorker.ReportProgress(100, "Done!");
		}

		public void Init(int animeID, GroupVideoQualityVM gvq)
		{
			this.Cursor = Cursors.Wait;

			GroupVideoQuality = gvq;
			AnimeID = animeID;

			// get the videos
			try
			{
				List<JMMServerBinary.Contract_VideoDetailed> vidContracts = JMMServerVM.Instance.clientBinaryHTTP.GetFilesByGroupAndResolution(AnimeID,
					GroupVideoQuality.GroupName, GroupVideoQuality.Resolution, GroupVideoQuality.VideoSource, GroupVideoQuality.VideoBitDepth, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
				vids = new List<VideoDetailedVM>();
				foreach (JMMServerBinary.Contract_VideoDetailed contract in vidContracts)
				{
					VideoDetailedVM vid = new VideoDetailedVM(contract);
					vids.Add(vid);
				}
				FileCount = vids.Count;
				lbFiles.ItemsSource = vids;
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

		
	}
}

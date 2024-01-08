using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Shoko.Commons.Extensions;
using Shoko.Commons.Utils;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for DeleteFilesForm.xaml
    /// </summary>
    public partial class DeleteFilesForm : Window
    {
        public static readonly DependencyProperty GroupVideoQualityProperty = DependencyProperty.Register("GroupVideoQuality",
            typeof(CL_GroupVideoQuality), typeof(DeleteFilesForm), new UIPropertyMetadata(null, null));

        public static readonly DependencyProperty AnimeIDProperty = DependencyProperty.Register("AnimeID",
            typeof(int), typeof(DeleteFilesForm), new UIPropertyMetadata(0, null));

        public static readonly DependencyProperty FileCountProperty = DependencyProperty.Register("FileCount",
            typeof(int), typeof(DeleteFilesForm), new UIPropertyMetadata(0, null));

        public static readonly DependencyProperty DeleteStatusProperty = DependencyProperty.Register("DeleteStatus",
            typeof(string), typeof(DeleteFilesForm), new UIPropertyMetadata("", null));

        public CL_GroupVideoQuality GroupVideoQuality
        {
            get => (CL_GroupVideoQuality)GetValue(GroupVideoQualityProperty);
            set => SetValue(GroupVideoQualityProperty, value);
        }

        public int AnimeID
        {
            get => (int)GetValue(AnimeIDProperty);
            set => SetValue(AnimeIDProperty, value);
        }

        public int FileCount
        {
            get => (int)GetValue(FileCountProperty);
            set => SetValue(FileCountProperty, value);
        }

        public string DeleteStatus
        {
            get => (string)GetValue(DeleteStatusProperty);
            set => SetValue(DeleteStatusProperty, value);
        }

        public static readonly DependencyProperty GroupFileSummaryProperty = DependencyProperty.Register("GroupFileSummary",
            typeof(VM_GroupFileSummary), typeof(DeleteFilesForm), new UIPropertyMetadata(null, null));

        public VM_GroupFileSummary GroupFileSummary
        {
            get => (VM_GroupFileSummary)GetValue(GroupFileSummaryProperty);
            set => SetValue(GroupFileSummaryProperty, value);
        }

        public static readonly DependencyProperty SummaryTextProperty = DependencyProperty.Register("SummaryText",
            typeof(string), typeof(DeleteFilesForm), new UIPropertyMetadata("", null));

        public string SummaryText
        {
            get => (string)GetValue(SummaryTextProperty);
            set => SetValue(SummaryTextProperty, value);
        }

        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register("GroupName",
            typeof(string), typeof(DeleteFilesForm), new UIPropertyMetadata("", null));

        public string GroupName
        {
            get => (string)GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, value);
        }

        private BackgroundWorker deleteFilesWorker = new BackgroundWorker();
        public bool FilesDeleted { get; set; }
        private bool inProgress;
        private List<VM_VideoDetailed> vids = new List<VM_VideoDetailed>();

        public DeleteFilesForm()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            deleteFilesWorker.WorkerReportsProgress = true;

            deleteFilesWorker.DoWork += deleteFilesWorker_DoWork;
            deleteFilesWorker.ProgressChanged += deleteFilesWorker_ProgressChanged;
            deleteFilesWorker.RunWorkerCompleted += deleteFilesWorker_RunWorkerCompleted;

            btnOK.Click += btnOK_Click;
            Closing += DeleteFilesForm_Closing;
            FilesDeleted = false;
            cb_AutoClose_DeleteFilesForm.IsChecked = AppSettings.AutoClose_DeleteFilesForm;
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void HandleCheck(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.Name == "cb_AutoClose_DeleteFilesForm")
            {
                AppSettings.AutoClose_DeleteFilesForm = true;
            }
        }
        private void HandleUnchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.Name == "cb_AutoClose_DeleteFilesForm")
            {
                AppSettings.AutoClose_DeleteFilesForm = false;
            }
        }

        void DeleteFilesForm_Closing(object sender, CancelEventArgs e)
        {
            if (inProgress)
            {
                e.Cancel = true;
                MessageBox.Show(Commons.Properties.Resources.DeleteFiles_Wait, Commons.Properties.Resources.Stop, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            DialogResult = FilesDeleted;
        }

        void btnOK_Click(object sender, RoutedEventArgs e)
        {
            string msg = string.Format(Commons.Properties.Resources.DeleteFiles_Confirm, vids.Count);
            MessageBoxResult res = MessageBox.Show(msg, Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (res == MessageBoxResult.Yes)
            {
                Cursor = Cursors.Wait;
                btnOK.Visibility = Visibility.Hidden;
                inProgress = true;
                deleteFilesWorker.RunWorkerAsync(vids);
            }
        }

        void deleteFilesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Cursor = Cursors.Arrow;
            inProgress = false;
            if (AppSettings.AutoClose_DeleteFilesForm)
                this.Close();
        }

        void deleteFilesWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            FilesDeleted = true;
            if (e.UserState is string msg)
                DeleteStatus = msg;

            if (e.ProgressPercentage == 100)
            {
                Cursor = Cursors.Arrow;
                if (e.UserState is List<string> list)
                {
                    MessageBox.Show(string.Join("\n", list), Commons.Properties.Resources.Confirm, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        void deleteFilesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (vids == null) return;

            int i = 0;
            List<string> errors = new List<string>();
            List<CL_VideoLocal_Place[]> files = vids.SelectMany(a => a.Places).GroupBy(a => Path.GetDirectoryName(a.FilePath))
                .Select(a => a.ToArray()).ToList();
            int total = files.Sum(a => a.Length);
            foreach (CL_VideoLocal_Place[] vid in files)
            {
                for (int index = 0; index < vid.Length; index++)
                {
                    var n = vid[index];
                    i++;
                    string msg = string.Format(Commons.Properties.Resources.DeleteFiles_Deleting, i, total);
                    deleteFilesWorker.ReportProgress(0, msg);
                    try
                    {
                        string result;
                        if (index < vid.Length - 1)
                            result = VM_ShokoServer.Instance.ShokoServices.DeleteVideoLocalPlaceAndFileSkipFolder(
                                n.VideoLocal_Place_ID);
                        else
                            result = VM_ShokoServer.Instance.ShokoServices.DeleteVideoLocalPlaceAndFile(
                                n.VideoLocal_Place_ID);
                        if (result.Length > 0)
                        {
                            deleteFilesWorker.ReportProgress(0, result);
                            errors.Add("Unable to delete file: " + n.GetFullPath() + " Error: " + result);
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            if (errors.Count > 0) deleteFilesWorker.ReportProgress(100, errors);
            else deleteFilesWorker.ReportProgress(100, Commons.Properties.Resources.Done);
        }

        public void Init(int animeID, CL_GroupVideoQuality gvq)
        {
            Cursor = Cursors.Wait;

            GroupVideoQuality = gvq;
            AnimeID = animeID;

            // get the videos
            try
            {

                vids = VM_ShokoServer.Instance.ShokoServices.GetFilesByGroupAndResolution(AnimeID,
                    string.IsNullOrEmpty(GroupVideoQuality.GroupName) ? "null" : GroupVideoQuality.GroupName, GroupVideoQuality.Resolution, GroupVideoQuality.VideoSource, GroupVideoQuality.VideoBitDepth, VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_VideoDetailed>();
                FileCount = vids.Count;
                lbFiles.ItemsSource = vids;
                GroupName = GroupVideoQuality.GroupName;
                SummaryText = $"{GroupVideoQuality.VideoSource} - {GroupVideoQuality.Resolution}";
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }

        }

        public void Init(int animeID, VM_GroupFileSummary gfs)
        {
            Cursor = Cursors.Wait;

            GroupFileSummary = gfs;
            AnimeID = animeID;

            // get the videos
            try
            {
                vids = VM_ShokoServer.Instance.ShokoServices.GetFilesByGroup(AnimeID,
                    GroupFileSummary.GroupName, VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_VideoDetailed>();

                FileCount = vids.Count;
                lbFiles.ItemsSource = vids;

                GroupName = GroupFileSummary.GroupName;
                SummaryText = "";
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }

        }

        public void Init(List<VM_VideoDetailed> videos)
        {
            Cursor = Cursors.Wait;

            // get the videos
            try
            {
                vids = videos;
                FileCount = vids.Count;
                lbFiles.ItemsSource = vids;

                SummaryText = "";
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }

        }
    }
}

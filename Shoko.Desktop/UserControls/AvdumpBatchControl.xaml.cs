using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for AvdumpBatchControl.xaml
    /// </summary>
    public partial class AvdumpBatchControl : UserControl
    {
        private BackgroundWorker workerAvdump = new BackgroundWorker();
        private bool stopDump = false;

        public static readonly DependencyProperty AnyVideosSelectedProperty = DependencyProperty.Register("AnyVideosSelected",
            typeof(bool), typeof(AvdumpBatchControl), new UIPropertyMetadata(false, null));

        public static readonly DependencyProperty OneVideoSelectedProperty = DependencyProperty.Register("OneVideoSelected",
            typeof(bool), typeof(AvdumpBatchControl), new UIPropertyMetadata(false, null));

        public static readonly DependencyProperty MultipleVideosSelectedProperty = DependencyProperty.Register("MultipleVideosSelected",
            typeof(bool), typeof(AvdumpBatchControl), new UIPropertyMetadata(false, null));

        public bool AnyVideosSelected
        {
            get { return (bool)GetValue(AnyVideosSelectedProperty); }
            set { SetValue(AnyVideosSelectedProperty, value); }
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

        public static readonly DependencyProperty IsDumpRunningProperty = DependencyProperty.Register("IsDumpRunning",
            typeof(bool), typeof(AvdumpBatchControl), new UIPropertyMetadata(false, null));

        public bool IsDumpRunning
        {
            get { return (bool)GetValue(IsDumpRunningProperty); }
            set { SetValue(IsDumpRunningProperty, value); }
        }

        public AvdumpBatchControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            lbVideos.SelectionChanged += new SelectionChangedEventHandler(lbVideos_SelectionChanged);
            btnClearList.Click += new RoutedEventHandler(btnClearList_Click);
            btnDumpFiles.Click += new RoutedEventHandler(btnDumpFiles_Click);
            btnDumpStop.Click += new RoutedEventHandler(btnDumpStop_Click);

            workerAvdump.DoWork += new DoWorkEventHandler(workerAvdump_DoWork);
            workerAvdump.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerAvdump_RunWorkerCompleted);
        }

        void btnDumpStop_Click(object sender, RoutedEventArgs e)
        {
            stopDump = true;
        }

        void btnDumpFiles_Click(object sender, RoutedEventArgs e)
        {
            if (workerAvdump.IsBusy)
            {
                Utils.ShowErrorMessage(Shoko.Commons.Properties.Resources.AVDump_WorkerRunning);
                return;
            }

            List<VM_AVDump> filesToDump = new List<VM_AVDump>();

            foreach (VM_AVDump dump in VM_MainListHelper.Instance.AVDumpFiles)
            {
                if (dump.HasBeenDumped) continue;

                filesToDump.Add(dump);
            }


            if (filesToDump.Count > 0)
            {
                btnClearList.IsEnabled = false;
                btnDumpFiles.IsEnabled = false;
                lbVideos.IsEnabled = false;
                IsDumpRunning = true;
                workerAvdump.RunWorkerAsync(filesToDump);
            }

        }

        void workerAvdump_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsDumpRunning = false;
            btnClearList.IsEnabled = true;
            btnDumpFiles.IsEnabled = true;
            lbVideos.IsEnabled = true;
            stopDump = false;
        }

        void workerAvdump_DoWork(object sender, DoWorkEventArgs e)
        {
            List<VM_AVDump> filesToDump = e.Argument as List<VM_AVDump>;
            if (filesToDump == null) return;

            try
            {
                foreach (VM_AVDump dump in filesToDump)
                {
                    if (stopDump) return;

                    // get the record from main VM
                    VM_AVDump tempDump = null;
                    foreach (VM_AVDump dm in VM_MainListHelper.Instance.AVDumpFiles)
                    {
                        if (dm.FullPath == dump.FullPath)
                        {
                            tempDump = dm;
                            break;
                        }
                    }

                    if (tempDump == null) continue;

                    tempDump.IsBeingDumped = true;
                    tempDump.DumpStatus = Shoko.Commons.Properties.Resources.AVDump_Processing;

                    tempDump.AVDumpFullResult = VM_ShokoServer.Instance.ShokoServices.AVDumpFile(tempDump.VideoLocal.VideoLocalID);
                    tempDump.ED2KDump = Utils.GetED2KDump(tempDump.AVDumpFullResult);
                    tempDump.IsBeingDumped = false;
                    if (string.IsNullOrEmpty(tempDump.ED2KDump))
                    {
                        tempDump.DumpStatus = Shoko.Commons.Properties.Resources.AVDump_Error;
                        tempDump.HasBeenDumped = false;
                    }
                    else
                    {
                        tempDump.DumpStatus = Shoko.Commons.Properties.Resources.AVDump_Complete;
                        tempDump.HasBeenDumped = true;
                    }
                }
            }
            catch (Exception)
            {
                //Utils.ShowErrorMessage(ex);
            }
        }

        void btnClearList_Click(object sender, RoutedEventArgs e)
        {
            VM_MainListHelper.Instance.AVDumpFiles.Clear();
        }

        void lbVideos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ccDetail.Content = null;

                AnyVideosSelected = lbVideos.SelectedItems.Count > 0;
                OneVideoSelected = lbVideos.SelectedItems.Count == 1;
                MultipleVideosSelected = lbVideos.SelectedItems.Count > 1;

                // if only one video selected
                if (OneVideoSelected)
                {
                    VM_AVDump vid = lbVideos.SelectedItem as VM_AVDump;
                    ccDetail.Content = vid;
                }

                // if only one video selected
                if (MultipleVideosSelected)
                {
                    MultipleAvdumps mv = new MultipleAvdumps();
                    mv.SelectedCount = lbVideos.SelectedItems.Count;
                    mv.AVDumps = new List<VM_AVDump>();

                    foreach (object obj in lbVideos.SelectedItems)
                    {
                        VM_AVDump vid = obj as VM_AVDump;
                        mv.AVDumps.Add(vid);
                    }

                    ccDetailMultiple.Content = mv;
                }


                //SetConfirmDetails();
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

            if (obj.GetType() == typeof(VM_AVDump))
            {
                VM_AVDump vid = obj as VM_AVDump;

                if (File.Exists(vid.FullPath))
                {
                    Utils.OpenFolderAndSelectFile(vid.FullPath);
                }
                else
                {
                    MessageBox.Show(Shoko.Commons.Properties.Resources.MSG_ERR_FileNotFound, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CommandBinding_RemoveFile(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            if (obj.GetType() == typeof(VM_AVDump))
            {
                VM_AVDump vid = obj as VM_AVDump;

                int dumpToRemove = -1;
                for (int i = 0; i < VM_MainListHelper.Instance.AVDumpFiles.Count; i++)
                {
                    if (VM_MainListHelper.Instance.AVDumpFiles[i].FullPath == vid.FullPath)
                    {
                        dumpToRemove = i;
                        break;
                    }
                }

                if (dumpToRemove >= 0)
                    VM_MainListHelper.Instance.AVDumpFiles.RemoveAt(dumpToRemove);
            }
        }
    }
}

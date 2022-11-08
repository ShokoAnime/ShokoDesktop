using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Commons.Utils;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for AnimeFolderSummaryControl.xaml
    /// </summary>
    public partial class AnimeFolderSummaryControl : UserControl
    {
        BackgroundWorker dataWorker = new BackgroundWorker();

        public static readonly DependencyProperty TotalFileCountProperty = DependencyProperty.Register("TotalFileCount",
            typeof(int), typeof(AnimeFolderSummaryControl), new UIPropertyMetadata(0, null));

        public int TotalFileCount
        {
            get { return (int)GetValue(TotalFileCountProperty); }
            set { SetValue(TotalFileCountProperty, value); }
        }

        public static readonly DependencyProperty TotalFileSizeProperty = DependencyProperty.Register("TotalFileSize",
            typeof(string), typeof(AnimeFolderSummaryControl), new UIPropertyMetadata("", null));

        public string TotalFileSize
        {
            get { return (string)GetValue(TotalFileSizeProperty); }
            set { SetValue(TotalFileSizeProperty, value); }
        }

        public static readonly DependencyProperty IsDataLoadingProperty = DependencyProperty.Register("IsDataLoading",
            typeof(bool), typeof(AnimeFolderSummaryControl), new UIPropertyMetadata(true, null));

        public bool IsDataLoading
        {
            get { return (bool)GetValue(IsDataLoadingProperty); }
            set { SetValue(IsDataLoadingProperty, value); }
        }

        public static readonly DependencyProperty IsDataFinishedLoadingProperty = DependencyProperty.Register("IsDataFinishedLoading",
            typeof(bool), typeof(AnimeFolderSummaryControl), new UIPropertyMetadata(false, null));

        public bool IsDataFinishedLoading
        {
            get { return (bool)GetValue(IsDataFinishedLoadingProperty); }
            set
            {
                SetValue(IsDataFinishedLoadingProperty, value);
            }
        }

        private VM_AnimeSeries_User thisSeries = null;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ObservableCollectionEx<AnimeFolderSummary> AnimeFolderSummaryRecords { get; set; }
        public ICollectionView ViewFolderSummary { get; set; }

        public AnimeFolderSummaryControl()
        {
            InitializeComponent();

            AnimeFolderSummaryRecords = new ObservableCollectionEx<AnimeFolderSummary>();
            ViewFolderSummary = CollectionViewSource.GetDefaultView(AnimeFolderSummaryRecords);

            //btnChooseFolder.Click += BtnChooseFolder_Click;

            DataContextChanged += new DependencyPropertyChangedEventHandler(AnimeFolderSummaryControl_DataContextChanged);
        }

        /*private void BtnChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (thisSeries!= null && !string.IsNullOrEmpty(thisSeries.DefaultFolder) && Directory.Exists(thisSeries.DefaultFolder))
                dialog.SelectedPath = thisSeries.DefaultFolder;

            if (thisSeries != null && dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                thisSeries.DefaultFolder = dialog.SelectedPath;
                thisSeries.Save();
                RefreshRecords();
            }
        }*/

        void AnimeFolderSummaryControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (DataContext == null)
                {
                    AnimeFolderSummaryRecords.Clear();
                    return;
                }

                RefreshRecords();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void RefreshRecords()
        {
            try
            {
                IsDataLoading = true;
                IsDataFinishedLoading = false;

                AnimeFolderSummaryRecords.Clear();

                TotalFileCount = 0;
                double fileSize = 0;


                VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                if (anime == null) return;

               VM_AnimeSeries_User serie = (VM_AnimeSeries_User)VM_ShokoServer.Instance.ShokoServices.GetSeriesForAnime(anime.AnimeID,
                        VM_ShokoServer.Instance.CurrentUser.JMMUserID);

                if (serie == null) return;
                thisSeries = serie;

                Dictionary<string, AnimeFolderSummary> folders = new Dictionary<string, AnimeFolderSummary>();

                foreach (VM_VideoLocal vid in anime.AllVideoLocals)
                {
                    TotalFileCount++;
                    fileSize += (double)vid.FileSize;
                    foreach (CL_VideoLocal_Place vplace in vid.Places)
                    {
                        var videoLocation = vplace.GetLocalFileSystemFullPath();
                        if (string.IsNullOrWhiteSpace(videoLocation))
                        {
                            var filename = vplace.GetFullPath();
                            videoLocation = filename.Substring(0, filename.Length - (Path.GetFileName(filename).Length + 1));

                        }
                        else
                        {
                            videoLocation = Path.GetDirectoryName(videoLocation);
                        }
                        if (!folders.ContainsKey(videoLocation))
                        {
                            AnimeFolderSummary fs = new AnimeFolderSummary();
                            fs.FolderName = videoLocation;
                            fs.FileCount = 0;
                            fs.TotalFileSize = 0;
                            folders[videoLocation] = fs;
                        }

                        folders[videoLocation].FileCount = folders[videoLocation].FileCount + 1;
                        folders[videoLocation].TotalFileSize = folders[videoLocation].TotalFileSize + vid.FileSize;
                    }
                }

                foreach (AnimeFolderSummary afs in folders.Values)
                {
                    afs.IsDefaultFolder = false;

                    if (!string.IsNullOrEmpty(thisSeries.DefaultFolder))
                    {
                        if (thisSeries.DefaultFolder.Equals(afs.FolderName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            afs.IsDefaultFolder = true;
                        }
                    }
                }
                AnimeFolderSummaryRecords.ReplaceRange(folders.Values);
                TotalFileSize = Formatting.FormatFileSize(fileSize);
                IsDataLoading = false;
                IsDataFinishedLoading = true;
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
                if (obj.GetType() == typeof(AnimeFolderSummary))
                {
                    AnimeFolderSummary afs = obj as AnimeFolderSummary;
                    Utils.OpenFolder(afs.FolderName);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }

    public class AnimeFolderSummary : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                var args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }

        public string FolderName { get; set; }

        public int FileCount { get; set; }

        public double TotalFileSize { get; set; }

        public string TotalFileSizeFormatted => Formatting.FormatFileSize(TotalFileSize);

        private bool isDefaultFolder = false;
        public bool IsDefaultFolder
        {
            get { return isDefaultFolder; }
            set
            {
                isDefaultFolder = value;
                IsNotDefaultFolder = !isDefaultFolder;
                NotifyPropertyChanged("IsDefaultFolder");
            }
        }

        private bool isNotDefaultFolder = true;
        public bool IsNotDefaultFolder
        {
            get { return isNotDefaultFolder; }
            set
            {
                isNotDefaultFolder = value;
                NotifyPropertyChanged("IsNotDefaultFolder");
            }
        }

    }
}

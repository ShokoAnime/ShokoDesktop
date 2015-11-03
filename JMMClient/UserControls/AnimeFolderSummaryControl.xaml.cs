using JMMClient.ViewModel;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace JMMClient.UserControls
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

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ObservableCollection<AnimeFolderSummary> AnimeFolderSummaryRecords { get; set; }
        public ICollectionView ViewFolderSummary { get; set; }

        public AnimeFolderSummaryControl()
        {
            InitializeComponent();

            AnimeFolderSummaryRecords = new ObservableCollection<AnimeFolderSummary>();
            ViewFolderSummary = CollectionViewSource.GetDefaultView(AnimeFolderSummaryRecords);

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(AnimeFolderSummaryControl_DataContextChanged);
        }

        void AnimeFolderSummaryControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (this.DataContext == null)
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


                AniDB_AnimeVM anime = this.DataContext as AniDB_AnimeVM;
                if (anime == null) return;

                Dictionary<string, AnimeFolderSummary> folders = new Dictionary<string, AnimeFolderSummary>();

                foreach (VideoLocalVM vid in anime.AllVideoLocals)
                {
                    TotalFileCount++;
                    fileSize += (double)vid.FileSize;
                    if (!folders.ContainsKey(vid.FileDirectory))
                    {
                        AnimeFolderSummary fs = new AnimeFolderSummary();
                        fs.FolderName = vid.FileDirectory;
                        fs.FileCount = 0;
                        fs.TotalFileSize = 0;
                        folders[vid.FileDirectory] = fs;
                    }

                    folders[vid.FileDirectory].FileCount = folders[vid.FileDirectory].FileCount + 1;
                    folders[vid.FileDirectory].TotalFileSize = folders[vid.FileDirectory].TotalFileSize + vid.FileSize;
                }

                foreach (AnimeFolderSummary afs in folders.Values)
                    AnimeFolderSummaryRecords.Add(afs);

                //ViewFolderSummary.Refresh();

                TotalFileSize = Utils.FormatFileSize(fileSize);


                IsDataLoading = false;
                IsDataFinishedLoading = true;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }

    public class AnimeFolderSummary
    {
        public string FolderName { get; set; }

        public int FileCount { get; set; }

        public double TotalFileSize { get; set; }

        public string TotalFileSizeFormatted
        {
            get
            {
                return Utils.FormatFileSize(TotalFileSize);
            }
        }

    }
}

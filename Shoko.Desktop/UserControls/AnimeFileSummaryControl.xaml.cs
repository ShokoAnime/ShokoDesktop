using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Commons.Utils;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for AnimeFileSummaryControl.xaml
    /// </summary>
    public partial class AnimeFileSummaryControl : UserControl
    {
        BackgroundWorker dataWorker = new BackgroundWorker();

        public static readonly DependencyProperty TotalFileCountProperty = DependencyProperty.Register("TotalFileCount",
            typeof(int), typeof(AnimeFileSummaryControl), new UIPropertyMetadata(0, null));

        public int TotalFileCount
        {
            get { return (int)GetValue(TotalFileCountProperty); }
            set { SetValue(TotalFileCountProperty, value); }
        }

        public static readonly DependencyProperty TotalFileSizeProperty = DependencyProperty.Register("TotalFileSize",
            typeof(string), typeof(AnimeFileSummaryControl), new UIPropertyMetadata("", null));

        public string TotalFileSize
        {
            get { return (string)GetValue(TotalFileSizeProperty); }
            set { SetValue(TotalFileSizeProperty, value); }
        }

        public static readonly DependencyProperty IsDataLoadingProperty = DependencyProperty.Register("IsDataLoading",
            typeof(bool), typeof(AnimeFileSummaryControl), new UIPropertyMetadata(true, null));

        public bool IsDataLoading
        {
            get { return (bool)GetValue(IsDataLoadingProperty); }
            set { SetValue(IsDataLoadingProperty, value); }
        }

        public static readonly DependencyProperty IsDataFinishedLoadingProperty = DependencyProperty.Register("IsDataFinishedLoading",
            typeof(bool), typeof(AnimeFileSummaryControl), new UIPropertyMetadata(false, null));

        public bool IsDataFinishedLoading
        {
            get { return (bool)GetValue(IsDataFinishedLoadingProperty); }
            set
            {
                SetValue(IsDataFinishedLoadingProperty, value);
                if (!value)
                {
                    DisplayGroupSummary = false;
                    DisplayGroupQualityDetails = false;
                }
                else
                {
                    DisplayGroupSummary = IsGroupSummary;
                    DisplayGroupQualityDetails = IsGroupQualityDetails;
                }
            }
        }

        public static readonly DependencyProperty IsGroupSummaryProperty = DependencyProperty.Register("IsGroupSummary",
            typeof(bool), typeof(AnimeFileSummaryControl), new UIPropertyMetadata(false, null));

        public bool IsGroupSummary
        {
            get { return (bool)GetValue(IsGroupSummaryProperty); }
            set { SetValue(IsGroupSummaryProperty, value); }
        }

        public static readonly DependencyProperty IsGroupQualityDetailsProperty = DependencyProperty.Register("IsGroupQualityDetails",
            typeof(bool), typeof(AnimeFileSummaryControl), new UIPropertyMetadata(true, null));

        public bool IsGroupQualityDetails
        {
            get { return (bool)GetValue(IsGroupQualityDetailsProperty); }
            set { SetValue(IsGroupQualityDetailsProperty, value); }
        }

        public static readonly DependencyProperty DisplayGroupSummaryProperty = DependencyProperty.Register("DisplayGroupSummary",
            typeof(bool), typeof(AnimeFileSummaryControl), new UIPropertyMetadata(false, null));

        public bool DisplayGroupSummary
        {
            get { return (bool)GetValue(DisplayGroupSummaryProperty); }
            set { SetValue(DisplayGroupSummaryProperty, value); }
        }

        public static readonly DependencyProperty DisplayGroupQualityDetailsProperty = DependencyProperty.Register("DisplayGroupQualityDetails",
            typeof(bool), typeof(AnimeFileSummaryControl), new UIPropertyMetadata(true, null));

        public bool DisplayGroupQualityDetails
        {
            get { return (bool)GetValue(DisplayGroupQualityDetailsProperty); }
            set { SetValue(DisplayGroupQualityDetailsProperty, value); }
        }


        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ObservableCollectionEx<CL_GroupVideoQuality> VideoQualityRecords { get; set; }
        public ICollectionView ViewSummary { get; set; }

        public ObservableCollectionEx<VM_GroupFileSummary> GroupSummaryRecords { get; set; }
        public ICollectionView ViewGroupSummary { get; set; }

        public AnimeFileSummaryControl()
        {
            InitializeComponent();

            VideoQualityRecords = new ObservableCollectionEx<CL_GroupVideoQuality>();
            ViewSummary = CollectionViewSource.GetDefaultView(VideoQualityRecords);

            GroupSummaryRecords = new ObservableCollectionEx<VM_GroupFileSummary>();
            ViewGroupSummary = CollectionViewSource.GetDefaultView(GroupSummaryRecords);

            ViewGroupSummary.SortDescriptions.Add(new SortDescription("GroupName", ListSortDirection.Ascending));

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            cboSortGroupQual.Items.Clear();
            cboSortGroupQual.Items.Add(Shoko.Commons.Properties.Resources.Anime_QualityRanking);
            cboSortGroupQual.Items.Add(Shoko.Commons.Properties.Resources.Anime_ReleaseGroup);
            cboSortGroupQual.SelectionChanged += new SelectionChangedEventHandler(cboSortGroupQual_SelectionChanged);

            cboFileSummaryType.Items.Clear();
            cboFileSummaryType.Items.Add(Shoko.Commons.Properties.Resources.Anime_GroupDetails);
            cboFileSummaryType.Items.Add(Shoko.Commons.Properties.Resources.Anime_GroupSummary);
            cboFileSummaryType.SelectedIndex = 0;
            cboFileSummaryType.SelectionChanged += new SelectionChangedEventHandler(cboFileSummaryType_SelectionChanged);

            DataContextChanged += new DependencyPropertyChangedEventHandler(AnimeFileSummaryControl_DataContextChanged);
            //dataWorker.DoWork += new DoWorkEventHandler(dataWorker_DoWork);
            //dataWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(dataWorker_RunWorkerCompleted);
        }

        void cboFileSummaryType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboFileSummaryType.SelectedIndex == 0)
            {
                IsGroupQualityDetails = true;
                IsGroupSummary = false;
                AppSettings.FileSummaryTypeDefault = 0;
            }

            if (cboFileSummaryType.SelectedIndex == 1)
            {
                IsGroupQualityDetails = false;
                IsGroupSummary = true;
                AppSettings.FileSummaryTypeDefault = 1;
            }

            RefreshRecords();
        }

        void cboSortGroupQual_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewSummary.SortDescriptions.Clear();

            if (cboSortGroupQual.SelectedIndex == 0)
            {
                ViewSummary.SortDescriptions.Add(new SortDescription("Ranking", ListSortDirection.Descending));
                ViewSummary.SortDescriptions.Add(new SortDescription("FileCountNormal", ListSortDirection.Descending));
                AppSettings.FileSummaryQualSortDefault = 0;
            }

            if (cboSortGroupQual.SelectedIndex == 1)
            {
                ViewSummary.SortDescriptions.Add(new SortDescription("GroupName", ListSortDirection.Ascending));
                ViewSummary.SortDescriptions.Add(new SortDescription("Ranking", ListSortDirection.Descending));
                AppSettings.FileSummaryQualSortDefault = 1;
            }
            ViewSummary.Refresh();
        }

        void AnimeFileSummaryControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (DataContext == null)
                {
                    VideoQualityRecords.Clear();
                    GroupSummaryRecords.Clear();
                    return;
                }

                bool changedCombo = false;

                if (cboSortGroupQual.SelectedIndex != AppSettings.FileSummaryQualSortDefault && cboSortGroupQual.SelectedIndex >= 0) changedCombo = true;
                if (cboFileSummaryType.SelectedIndex != AppSettings.FileSummaryTypeDefault && cboFileSummaryType.SelectedIndex >= 0) changedCombo = true;

                cboSortGroupQual.SelectedIndex = AppSettings.FileSummaryQualSortDefault;
                cboFileSummaryType.SelectedIndex = AppSettings.FileSummaryTypeDefault;

                if (!changedCombo)
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

               
                

                TotalFileCount = 0;
                double fileSize = 0;

                VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                if (anime == null) return;

                if (IsGroupQualityDetails)
                {
                    List<CL_GroupVideoQuality> summ = VM_ShokoServer.Instance.ShokoServices.GetGroupVideoQualitySummary(anime.AnimeID).CastList<CL_GroupVideoQuality>();
                    foreach (CL_GroupVideoQuality vidQual in summ)
                    {
                        TotalFileCount += vidQual.FileCountNormal + vidQual.FileCountSpecials;
                        fileSize += vidQual.TotalFileSize;
                    }
                    VideoQualityRecords.ReplaceRange(summ);
                }
                else
                    VideoQualityRecords.Clear();
                if (IsGroupSummary)
                {
                    List<VM_GroupFileSummary> summ = VM_ShokoServer.Instance.ShokoServices.GetGroupFileSummary(anime.AnimeID).CastList<VM_GroupFileSummary>();
                    foreach (VM_GroupFileSummary vidQual in summ)
                    {
                        TotalFileCount += vidQual.FileCountNormal + vidQual.FileCountSpecials;
                        fileSize += vidQual.TotalFileSize;
                    }
                    GroupSummaryRecords.ReplaceRange(summ);
                }
                else
                    GroupSummaryRecords.Clear();

                TotalFileSize = Formatting.FormatFileSize(fileSize);


                IsDataLoading = false;
                IsDataFinishedLoading = true;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_DeleteAllFiles(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                VM_AniDB_Anime anime = DataContext as VM_AniDB_Anime;
                if (anime == null) return;

                Window wdw = Window.GetWindow(this);
                if (obj.GetType() == typeof(CL_GroupVideoQuality))
                {
                    CL_GroupVideoQuality gvq = (CL_GroupVideoQuality)obj;

                    Cursor = Cursors.Wait;
                    DeleteFilesForm frm = new DeleteFilesForm();
                    frm.Owner = wdw;
                    frm.Init(anime.AnimeID, gvq);
                    bool? result = frm.ShowDialog();
                    if (result.Value)
                    {
                        // refresh
                        RefreshRecords();
                    }

                    Cursor = Cursors.Arrow;
                }

                if (obj.GetType() == typeof(VM_GroupFileSummary))
                {
                    VM_GroupFileSummary gfs = (VM_GroupFileSummary)obj;

                    Cursor = Cursors.Wait;
                    DeleteFilesForm frm = new DeleteFilesForm();
                    frm.Owner = wdw;
                    frm.Init(anime.AnimeID, gfs);
                    bool? result = frm.ShowDialog();
                    if (result.Value)
                    {
                        // refresh
                        RefreshRecords();
                    }

                    Cursor = Cursors.Arrow;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }
}

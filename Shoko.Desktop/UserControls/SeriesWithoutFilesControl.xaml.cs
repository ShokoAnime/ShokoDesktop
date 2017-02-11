using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for SeriesWithoutFilesControl.xaml
    /// </summary>
    public partial class SeriesWithoutFilesControl : UserControl
    {
        BackgroundWorker workerFiles = new BackgroundWorker();
        public ICollectionView ViewSeries { get; set; }
        public ObservableCollection<VM_AnimeSeries_User> MissingSeriesCollection { get; set; }

        public static readonly DependencyProperty SeriesCountProperty = DependencyProperty.Register("SeriesCount",
            typeof(int), typeof(SeriesWithoutFilesControl), new UIPropertyMetadata(0, null));

        public int SeriesCount
        {
            get { return (int)GetValue(SeriesCountProperty); }
            set { SetValue(SeriesCountProperty, value); }
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading",
            typeof(bool), typeof(SeriesWithoutFilesControl), new UIPropertyMetadata(false, null));

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
            typeof(bool), typeof(SeriesWithoutFilesControl), new UIPropertyMetadata(true, null));

        public bool IsNotLoading
        {
            get { return (bool)GetValue(IsNotLoadingProperty); }
            set { SetValue(IsNotLoadingProperty, value); }
        }

        public SeriesWithoutFilesControl()
        {
            InitializeComponent();

            IsLoading = false;

            MissingSeriesCollection = new ObservableCollection<VM_AnimeSeries_User>();
            ViewSeries = CollectionViewSource.GetDefaultView(MissingSeriesCollection);
            ViewSeries.SortDescriptions.Add(new SortDescription("SeriesName", ListSortDirection.Ascending));

            btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

            workerFiles.DoWork += new DoWorkEventHandler(workerFiles_DoWork);
            workerFiles.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerFiles_RunWorkerCompleted);
        }

        void workerFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<VM_AnimeSeries_User> contracts = e.Result as List<VM_AnimeSeries_User>;
            foreach (VM_AnimeSeries_User mf in contracts)
                MissingSeriesCollection.Add(mf);
            SeriesCount = contracts.Count;
            btnRefresh.IsEnabled = true;
            IsLoading = false;
            Cursor = Cursors.Arrow;
        }

        void workerFiles_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                List<VM_AnimeSeries_User> contractsTemp = VM_ShokoServer.Instance.ShokoServices.GetSeriesWithoutAnyFiles(
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeSeries_User>();
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
            MissingSeriesCollection.Clear();
            SeriesCount = 0;

            workerFiles.RunWorkerAsync();
        }

        private void CommandBinding_DeleteSeries(object sender, ExecutedRoutedEventArgs e)
        {
            VM_AnimeSeries_User ser = e.Parameter as VM_AnimeSeries_User;
            if (ser == null) return;

            Window parentWindow = Window.GetWindow(this);

            try
            {
                DeleteSeriesGroupForm frm = new DeleteSeriesGroupForm();
                frm.Owner = parentWindow;
                bool? result = frm.ShowDialog();

                if (result.HasValue && result.Value == true)
                {
                    //bool deleteFiles = frm.DeleteFiles;

                    Cursor = Cursors.Wait;
                    VM_ShokoServer.Instance.ShokoServices.DeleteAnimeSeries(ser.AnimeSeriesID, frm.DeleteFiles, frm.DeleteGroups);

                    VM_MainListHelper.Instance.RefreshGroupsSeriesData();
                    VM_MainListHelper.Instance.ShowChildWrappers(VM_MainListHelper.Instance.CurrentWrapper);

                    MissingSeriesCollection.Remove(ser);
                    ViewSeries.Refresh();

                    Cursor = Cursors.Arrow;
                }
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

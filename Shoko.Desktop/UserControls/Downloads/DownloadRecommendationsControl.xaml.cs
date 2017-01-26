using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Enums;

namespace Shoko.Desktop.UserControls.Downloads
{
    /// <summary>
    /// Interaction logic for DownloadRecommendationsControl.xaml
    /// </summary>
    public partial class DownloadRecommendationsControl : UserControl
    {
        public ICollectionView ViewRecs { get; set; }
        public ObservableCollection<VM_Recommendation> Recs { get; set; }

        BackgroundWorker getMissingDataWorker = new BackgroundWorker();

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading",
            typeof(bool), typeof(DownloadRecommendationsControl), new UIPropertyMetadata(false, null));

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
            typeof(bool), typeof(DownloadRecommendationsControl), new UIPropertyMetadata(true, null));

        public bool IsNotLoading
        {
            get { return (bool)GetValue(IsNotLoadingProperty); }
            set { SetValue(IsNotLoadingProperty, value); }
        }


        public static readonly DependencyProperty StatusMessageProperty = DependencyProperty.Register("StatusMessage",
            typeof(string), typeof(DownloadRecommendationsControl), new UIPropertyMetadata("", null));

        public string StatusMessage
        {
            get { return (string)GetValue(StatusMessageProperty); }
            set { SetValue(StatusMessageProperty, value); }
        }


        public DownloadRecommendationsControl()
        {
            InitializeComponent();

            IsLoading = false;

            Recs = new ObservableCollection<VM_Recommendation>();
            ViewRecs = CollectionViewSource.GetDefaultView(Recs);

            btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            btnGetRecDownloadMissingInfo.Click += new RoutedEventHandler(btnGetRecDownloadMissingInfo_Click);

            getMissingDataWorker.DoWork += new DoWorkEventHandler(getMissingDataWorker_DoWork);
            getMissingDataWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(getMissingDataWorker_RunWorkerCompleted);
        }

        private void CommandBinding_IgnoreAnimeDownload(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_Recommendation))
                {
                    VM_Recommendation rec = obj as VM_Recommendation;
                    if (rec == null) return;

                    VM_ShokoServer.Instance.ShokoServices.IgnoreAnime(rec.RecommendedAnimeID, (int)RecommendationType.Download,
                        VM_ShokoServer.Instance.CurrentUser.JMMUserID);

                    RefreshData();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnGetRecDownloadMissingInfo_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            IsLoading = true;
            IsEnabled = false;
            parentWindow.Cursor = Cursors.Wait;
            getMissingDataWorker.RunWorkerAsync();
        }

        void getMissingDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            RefreshData();

            parentWindow.Cursor = Cursors.Arrow;
            IsEnabled = true;
            IsLoading = false;
        }

        void getMissingDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                foreach (VM_Recommendation rec in Recs)
                {
                    if (!rec.Recommended_AnimeInfoExists)
                    {
                        VM_ShokoServer.Instance.ShokoServices.UpdateAnimeData(rec.RecommendedAnimeID);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void RefreshData()
        {
            try
            {
                Window parentWindow = Window.GetWindow(this);
                parentWindow.Cursor = Cursors.Wait;

                Recs.Clear();

                List<VM_Recommendation> contracts =
                    VM_ShokoServer.Instance.ShokoServices.GetRecommendations(VM_UserSettings.Instance.DownloadsRecItems, VM_ShokoServer.Instance.CurrentUser.JMMUserID,
                    (int)RecommendationType.Download).CastList<VM_Recommendation>();

                foreach (VM_Recommendation contract in contracts)
                {
                    Recs.Add(contract);
                }

                ViewRecs.Refresh();

                parentWindow.Cursor = Cursors.Arrow;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
            }
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }
    }
}

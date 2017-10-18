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

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for IgnoredAnimeControl.xaml
    /// </summary>
    public partial class IgnoredAnimeControl : UserControl
    {
        BackgroundWorker workerAnime = new BackgroundWorker();
        public ICollectionView ViewAnime { get; set; }
        public ObservableCollection<VM_IgnoreAnime> MissingAnimeCollection { get; set; }

        public static readonly DependencyProperty AnimeCountProperty = DependencyProperty.Register("AnimeCount",
            typeof(int), typeof(IgnoredAnimeControl), new UIPropertyMetadata(0, null));

        public int AnimeCount
        {
            get => (int) GetValue(AnimeCountProperty);
            set => SetValue(AnimeCountProperty, value);
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading",
            typeof(bool), typeof(IgnoredAnimeControl), new UIPropertyMetadata(false, null));

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set
            {
                SetValue(IsLoadingProperty, value);
                IsNotLoading = !IsLoading;
            }
        }

        public static readonly DependencyProperty IsNotLoadingProperty = DependencyProperty.Register("IsNotLoading",
            typeof(bool), typeof(IgnoredAnimeControl), new UIPropertyMetadata(true, null));

        public bool IsNotLoading
        {
            get => (bool) GetValue(IsNotLoadingProperty);
            set => SetValue(IsNotLoadingProperty, value);
        }

        public IgnoredAnimeControl()
        {
            InitializeComponent();

            IsLoading = false;

            MissingAnimeCollection = new ObservableCollection<VM_IgnoreAnime>();
            ViewAnime = CollectionViewSource.GetDefaultView(MissingAnimeCollection);
            ViewAnime.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));

            btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

            workerAnime.DoWork += new DoWorkEventHandler(workerFiles_DoWork);
            workerAnime.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerFiles_RunWorkerCompleted);
        }

        void workerFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<VM_IgnoreAnime> contracts = e.Result as List<VM_IgnoreAnime>;
            foreach (VM_IgnoreAnime mf in contracts)
                MissingAnimeCollection.Add(mf);
            AnimeCount = contracts.Count;
            btnRefresh.IsEnabled = true;
            IsLoading = false;
            Cursor = Cursors.Arrow;
        }

        void workerFiles_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                List<VM_IgnoreAnime> contractsTemp = VM_ShokoServer.Instance.ShokoServices.GetIgnoredAnime(
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_IgnoreAnime>();
                e.Result = contractsTemp;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void RefreshData()
        {
            IsLoading = true;
            btnRefresh.IsEnabled = false;
            MissingAnimeCollection.Clear();
            AnimeCount = 0;

            workerAnime.RunWorkerAsync();
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void CommandBinding_DeleteIgnoredAnime(object sender, ExecutedRoutedEventArgs e)
        {
            VM_IgnoreAnime ign = e.Parameter as VM_IgnoreAnime;
            if (ign == null) return;

            Window parentWindow = Window.GetWindow(this);

            try
            {
                if (MessageBox.Show(Shoko.Commons.Properties.Resources.IgnoredAnime_DeleteMessage, Shoko.Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Cursor = Cursors.Wait;
                    VM_ShokoServer.Instance.ShokoServices.RemoveIgnoreAnime(ign.IgnoreAnimeID);
                    Cursor = Cursors.Arrow;

                    RefreshData();
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

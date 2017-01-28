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
using Shoko.Models.Client;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for ManuallyLinkedFilesControl.xaml
    /// </summary>
    public partial class ManuallyLinkedFilesControl : UserControl
    {
        public ICollectionView ViewFiles { get; set; }
        public ObservableCollection<VM_VideoLocal> ManuallyLinkedFiles { get; set; }

        public static readonly DependencyProperty FileCountProperty = DependencyProperty.Register("FileCount",
            typeof(int), typeof(ManuallyLinkedFilesControl), new UIPropertyMetadata(0, null));

        public int FileCount
        {
            get { return (int)GetValue(FileCountProperty); }
            set { SetValue(FileCountProperty, value); }
        }

        public ManuallyLinkedFilesControl()
        {
            InitializeComponent();

            ManuallyLinkedFiles = new ObservableCollection<VM_VideoLocal>();
            ViewFiles = CollectionViewSource.GetDefaultView(ManuallyLinkedFiles);
            ViewFiles.SortDescriptions.Add(new SortDescription("FullPath", ListSortDirection.Ascending));
            ViewFiles.Filter = FileSearchFilter;

            btnClearSearch.Click += new RoutedEventHandler(btnClearSearch_Click);
            txtFileSearch.TextChanged += new TextChangedEventHandler(txtFileSearch_TextChanged);
            btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            btnRescan.Click += new RoutedEventHandler(btnRescan_Click);
        }

        void btnRescan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!VM_ShokoServer.Instance.ServerOnline) return;

                Cursor = Cursors.Wait;
                VM_ShokoServer.Instance.ShokoServices.RescanManuallyLinkedFiles();
                Cursor = Cursors.Arrow;

                MessageBox.Show(Shoko.Commons.Properties.Resources.Unrecognized_AniDBScan, Shoko.Commons.Properties.Resources.Complete, MessageBoxButton.OK, MessageBoxImage.Information);

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

        void txtFileSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewFiles.Refresh();
        }

        void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtFileSearch.Text = "";
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshLinkedFiles();
        }

        public void RefreshLinkedFiles()
        {
            try
            {
                ManuallyLinkedFiles.Clear();
                if (!VM_ShokoServer.Instance.ServerOnline) return;

                List<VM_VideoLocal> vids = VM_ShokoServer.Instance.ShokoServices.GetAllManuallyLinkedFiles(VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_VideoLocal>();
                FileCount = vids.Count;

                foreach (VM_VideoLocal vid in vids)
                {
                    ManuallyLinkedFiles.Add(vid);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private bool FileSearchFilter(object obj)
        {
            VM_VideoLocal vid = obj as VM_VideoLocal;
            if (vid == null) return true;
            foreach (CL_VideoLocal_Place n in vid.Places)
            {
                int index = n.FilePath.IndexOf(txtFileSearch.Text.Trim(), 0,
                    StringComparison.InvariantCultureIgnoreCase);
                if (index > -1) return true;
            }
            return false;
        }
    }
}

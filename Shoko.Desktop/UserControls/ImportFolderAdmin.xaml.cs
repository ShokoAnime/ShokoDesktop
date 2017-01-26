using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for ImportFolderAdmin.xaml
    /// </summary>
    public partial class ImportFolderAdmin : UserControl
    {
        public ImportFolderAdmin()
        {
            InitializeComponent();

            btnAddImportFolder.Click += new RoutedEventHandler(btnAddImportFolder_Click);
            btnDeleteImportFolder.Click += new RoutedEventHandler(btnDeleteImportFolder_Click);
            lbImportFolders.MouseDoubleClick += new MouseButtonEventHandler(lbImportFolders_MouseDoubleClick);
            btnRefreshImportFolders.Click += new RoutedEventHandler(btnRefreshImportFolders_Click);
        }

        void btnRefreshImportFolders_Click(object sender, RoutedEventArgs e)
        {
            VM_ShokoServer.Instance.RefreshCloudAccounts();
            VM_ShokoServer.Instance.RefreshImportFolders();
        }

        void lbImportFolders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object obj = lbImportFolders.SelectedItem;
            if (obj == null) return;

            VM_ImportFolder ns = (VM_ImportFolder)obj;
            ImportFolder frm = new ImportFolder();
            frm.Owner = GetTopParent();
            frm.Init(ns);
            bool? result = frm.ShowDialog();
        }

        void btnDeleteImportFolder_Click(object sender, RoutedEventArgs e)
        {
            object obj = lbImportFolders.SelectedItem;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_ImportFolder))
                {
                    VM_ImportFolder ns = (VM_ImportFolder)obj;

                    MessageBoxResult res = MessageBox.Show(string.Format(Properties.Resources.ImportFolder_Delete, ns.ImportFolderLocation), Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        ns.Delete();
                        //VM_ShokoServer.Instance.RefreshImportFolders();
                        MessageBox.Show(Properties.Resources.ShokoServer_ProcessRunning, Properties.Resources.ShokoServer_Running, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnAddImportFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ImportFolder frm = new ImportFolder();
                frm.Owner = GetTopParent();
                frm.Init(new VM_ImportFolder());
                bool? result = frm.ShowDialog();

                VM_ShokoServer.Instance.RefreshImportFolders();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private Window GetTopParent()
        {
            DependencyObject dpParent = Parent;
            do
            {
                dpParent = LogicalTreeHelper.GetParent(dpParent);
            }
            while (dpParent.GetType().BaseType != typeof(Window));

            return dpParent as Window;
        }
    }
}

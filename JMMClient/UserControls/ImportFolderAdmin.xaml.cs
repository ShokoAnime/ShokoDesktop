using JMMClient.Forms;
using JMMClient.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JMMClient.UserControls
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
            JMMServerVM.Instance.RefreshCloudAccounts();
            JMMServerVM.Instance.RefreshImportFolders();
        }

        void lbImportFolders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object obj = lbImportFolders.SelectedItem;
            if (obj == null) return;

            ImportFolderVM ns = (ImportFolderVM)obj;
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
                if (obj.GetType() == typeof(ImportFolderVM))
                {
                    ImportFolderVM ns = (ImportFolderVM)obj;

                    MessageBoxResult res = MessageBox.Show(string.Format("Are you sure you want to delete the Import Folder: {0}\nAny files in this folder will also be removed from the database", ns.ImportFolderLocation), "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        ns.Delete();
                        //JMMServerVM.Instance.RefreshImportFolders();
                        MessageBox.Show("Process is running on the server, and may take a while to complete", "Running", MessageBoxButton.OK, MessageBoxImage.Information);
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
                frm.Init(new ImportFolderVM());
                bool? result = frm.ShowDialog();

                JMMServerVM.Instance.RefreshImportFolders();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private Window GetTopParent()
        {
            DependencyObject dpParent = this.Parent;
            do
            {
                dpParent = LogicalTreeHelper.GetParent(dpParent);
            }
            while (dpParent.GetType().BaseType != typeof(Window));

            return dpParent as Window;
        }
    }
}

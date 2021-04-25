using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for ImportFolder.xaml
    /// </summary>
    public partial class ImportFolder : Window
    {
        private VM_ImportFolder importFldr = null;

        public ImportFolder()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);
            chkDropSource.Checked += ChkDropSource_Checked;
            chkDropDestination.Checked += ChkDropDestination_Checked;
            btnCancel.Click += btnCancel_Click;
            btnSave.Click += btnSave_Click;
            btnChooseFolder.Click += BtnChooseFolder_Click;
            btnProvChooseFolder.Click += BtnProvChooseFolder_Click;
            comboProvider.SelectionChanged += ComboProvider_SelectionChanged;
        }

        private bool EventLock;

        private void ComboProvider_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comboProvider.SelectedItem == null) return;
            GridLocalMapping.Visibility = Visibility.Visible;
            chkIsWatched.IsEnabled = true;
        }

        private void BtnProvChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            if (comboProvider.SelectedItem == null)
                return;
            FolderBrowser fld = new FolderBrowser();
            fld.Init(txtImportFolderLocation.Text);
            fld.Owner = this;
            bool? result = fld.ShowDialog();
            if (result.HasValue && result.Value)
                txtImportFolderLocation.Text = fld.SelectedPath;
        }

        private void BtnChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            //needed check, 
            if (CommonFileDialog.IsPlatformSupported)
            {
                var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;

                if (!string.IsNullOrEmpty(txtLocalPath.Text) &&
                    Directory.Exists(txtLocalPath.Text))
                    dialog.InitialDirectory = txtLocalPath.Text;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    txtLocalPath.Text = dialog.FileName;
                }
            }
            else
            {
                System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

                if (!string.IsNullOrEmpty(txtLocalPath.Text) &&
                    Directory.Exists(txtLocalPath.Text))
                    dialog.SelectedPath = txtLocalPath.Text;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtLocalPath.Text = dialog.SelectedPath;
                }
            }
        }

        private void ChkDropDestination_Checked(object sender, RoutedEventArgs e)
        {
            if (EventLock || !chkDropDestination.IsChecked.HasValue || !chkDropDestination.IsChecked.Value) return;
            if (chkDropSource.IsChecked.HasValue && chkDropSource.IsChecked.Value)
            {
                EventLock = true;
                chkDropSource.IsChecked = false;
                EventLock = false;
            }
        }

        private void ChkDropSource_Checked(object sender, RoutedEventArgs e)
        {
            if (EventLock || !chkDropSource.IsChecked.HasValue || !chkDropSource.IsChecked.Value) return;
            if (chkDropDestination.IsChecked.HasValue && chkDropDestination.IsChecked.Value)
            {
                EventLock = true;
                chkDropDestination.IsChecked = false;
                EventLock = false;
            }
        }

        void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // An import folder cannot be both the drop source and the drop destination
                if (chkDropDestination.IsChecked.HasValue && chkDropSource.IsChecked.HasValue && chkDropDestination.IsChecked.Value && chkDropSource.IsChecked.Value)
                {
                    MessageBox.Show(Shoko.Commons.Properties.Resources.MSG_ERR_DropSourceDestCheck, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // The import folder location cannot be blank. Enter a valid path on OMM Server
                if (string.IsNullOrEmpty(txtImportFolderLocation.Text))
                {
                    MessageBox.Show(Shoko.Commons.Properties.Resources.MSG_ERR_ImportFolderLocationCheck, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtImportFolderLocation.Focus();
                    return;
                }

                // default the local path to the server path
                if (string.IsNullOrEmpty(txtLocalPath.Text))
                    importFldr.LocalPathTemp = txtImportFolderLocation.Text.Trim();
                else
                    importFldr.LocalPathTemp = txtLocalPath.Text.Trim();
                importFldr.ImportFolderName = "NA";
                importFldr.ImportFolderLocation = txtImportFolderLocation.Text.Trim();
                importFldr.IsDropDestination = chkDropDestination.IsChecked ?? false ? 1 : 0;
                importFldr.IsDropSource = chkDropSource.IsChecked ?? false ? 1 : 0;
                importFldr.IsWatched = chkIsWatched.IsChecked ?? false ? 1 : 0;
                if (!importFldr.Save()) return;

                VM_ShokoServer.Instance.RefreshImportFolders();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            DialogResult = true;
            Close();
        }

        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public void Init(VM_ImportFolder ifldr)
        {
            try
            {
                // prevent overwriting of UI values
                if (ifldr != null)
                {
                    importFldr = ifldr;

                    txtImportFolderLocation.Text = importFldr.ImportFolderLocation;
                    txtLocalPath.Text = importFldr.LocalPath;
                    chkDropDestination.IsChecked = importFldr.IsDropDestination == 1;
                    chkDropSource.IsChecked = importFldr.IsDropSource == 1;
                    chkIsWatched.IsChecked = importFldr.IsWatched == 1;
                }
                else
                {
                    importFldr = new VM_ImportFolder();
                }
                txtImportFolderLocation.Focus();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }
}
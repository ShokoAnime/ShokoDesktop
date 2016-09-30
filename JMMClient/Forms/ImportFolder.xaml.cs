using JMMClient.ViewModel;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace JMMClient.Forms
{
    /// <summary>
    /// Interaction logic for ImportFolder.xaml
    /// </summary>
    public partial class ImportFolder : Window
    {
        private ImportFolderVM importFldr = null;

        public ImportFolder()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);
            chkDropSource.Checked += ChkDropSource_Checked;
            chkDropDestination.Checked += ChkDropDestination_Checked;
            chkIsWatched.Checked += ChkIsWatched_Checked;
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnSave.Click += new RoutedEventHandler(btnSave_Click);
            btnChooseFolder.Click += BtnChooseFolder_Click;
            btnProvChooseFolder.Click += BtnProvChooseFolder_Click;
            comboProvider.SelectionChanged += ComboProvider_SelectionChanged;
        }

        private void ComboProvider_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comboProvider.SelectedItem != null)
            {
                CloudAccountVM account = (CloudAccountVM)comboProvider.SelectedItem;
                GridLocalMapping.Visibility = (account.CloudID ?? 0) == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void BtnProvChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            if (comboProvider.SelectedItem == null)
                return;
            CloudAccountVM cl = (CloudAccountVM)comboProvider.SelectedItem;
            FolderBrowser fld=new FolderBrowser();
            fld.Init(cl, txtImportFolderLocation.Text);
            fld.Owner = this;
            bool? result = fld.ShowDialog();
            if (result.HasValue && result.Value)
                txtImportFolderLocation.Text = fld.SelectedPath;
        }

        private void BtnChooseFolder_Click(object sender, RoutedEventArgs e)
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

        private void ChkIsWatched_Checked(object sender, RoutedEventArgs e)
        {
            if (!checkchange && importFldr.CloudID.HasValue && chkDropDestination.IsChecked.HasValue &&
                chkDropDestination.IsChecked.Value)
            {
                checkchange = true;
                chkIsWatched.IsChecked = false;
                checkchange = false;
            }
        }

        private void ChkDropDestination_Checked(object sender, RoutedEventArgs e)
        {
            if (!checkchange && chkDropDestination.IsChecked.HasValue && chkDropDestination.IsChecked.Value)
            {
                if (chkDropSource.IsChecked.HasValue && chkDropSource.IsChecked.Value)
                {
                    checkchange = true;
                    chkDropSource.IsChecked = false;
                    checkchange = false;
                }
                if (importFldr.CloudID.HasValue && chkIsWatched.IsChecked.HasValue && chkIsWatched.IsChecked.Value)
                {
                    checkchange = true;
                    chkIsWatched.IsChecked = false;
                    checkchange = false;
                }
            }
        }

        private bool checkchange = false;

        private void ChkDropSource_Checked(object sender, RoutedEventArgs e)
        {
            if (!checkchange && chkDropSource.IsChecked.HasValue && chkDropSource.IsChecked.Value)
            {
                if (chkDropDestination.IsChecked.HasValue && chkDropDestination.IsChecked.Value)
                {
                    checkchange = true;
                    chkDropDestination.IsChecked = false;
                    checkchange = false;
                }
            }
        }

        void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // An import folder cannot be both the drop source and the drop destination
                if (chkDropDestination.IsChecked.HasValue && chkDropSource.IsChecked.HasValue && chkDropDestination.IsChecked.Value && chkDropSource.IsChecked.Value)
                {
                    MessageBox.Show(Properties.Resources.MSG_ERR_DropSourceDestCheck, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // The import folder location cannot be blank. Enter a valid path on OMM Server
                if (string.IsNullOrEmpty(txtImportFolderLocation.Text))
                {
                    MessageBox.Show(Properties.Resources.MSG_ERR_ImportFolderLocationCheck, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtImportFolderLocation.Focus();
                    return;
                }

                // default the local path to the server path
                if (string.IsNullOrEmpty(txtLocalPath.Text))
                    importFldr.LocalPathTemp = txtImportFolderLocation.Text.Trim();
                else
                    importFldr.LocalPathTemp = txtLocalPath.Text.Trim();
                CloudAccountVM cl = (CloudAccountVM)comboProvider.SelectedItem;
                importFldr.CloudID = cl.CloudID;
                importFldr.ImportFolderName = "NA";
                importFldr.ImportFolderLocation = txtImportFolderLocation.Text.Trim();
                importFldr.IsDropDestination = chkDropDestination.IsChecked.Value ? 1 : 0;
                importFldr.IsDropSource = chkDropSource.IsChecked.Value ? 1 : 0;
                importFldr.IsWatched = chkIsWatched.IsChecked.Value ? 1 : 0;
                importFldr.Save();

                JMMServerVM.Instance.RefreshImportFolders();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            this.DialogResult = true;
            this.Close();
        }

        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        public void Init(ImportFolderVM ifldr)
        {
            try
            {
                importFldr = ifldr;

                txtImportFolderLocation.Text = importFldr.ImportFolderLocation;
                txtLocalPath.Text = importFldr.LocalPath;
                chkDropDestination.IsChecked = importFldr.IsDropDestination == 1;
                chkDropSource.IsChecked = importFldr.IsDropSource == 1;
                chkIsWatched.IsChecked = importFldr.IsWatched == 1;
                if ((ifldr.CloudID ?? 0)==0)
                    comboProvider.SelectedIndex = 0;
                else
                    comboProvider.SelectedItem = JMMServerVM.Instance.FolderProviders.FirstOrDefault(a => a.CloudID == ifldr.CloudID.Value);
                txtImportFolderLocation.Focus();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }
}
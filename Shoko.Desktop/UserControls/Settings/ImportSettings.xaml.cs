using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pri.LongPath;
using Shoko.Desktop.ViewModel;

namespace Shoko.Desktop.UserControls.Settings
{
    /// <summary>
    /// Interaction logic for ImportSettings.xaml
    /// </summary>
    public partial class ImportSettings : UserControl
    {
        public ImportSettings()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            btnSave.Click += new RoutedEventHandler(btnSave_Click);

            chkImportSettings_ImportOnStart.Click += new RoutedEventHandler(settingChanged);
            chkImportSettings_ScanDropOnStart.Click += new RoutedEventHandler(settingChanged);
            chkImportSettings_RenameOnImport.Click += new RoutedEventHandler(settingChanged);
            chkImportSettings_MoveOnImport.Click += new RoutedEventHandler(settingChanged);
            chkImportSettings_UseEpisodeStatus.Click += new RoutedEventHandler(settingChanged);

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            btnChooseImagesFolder.Click += new RoutedEventHandler(btnChooseImagesFolder_Click);
            btnSetDefaultFolder.Click += BtnSetDefaultFolder_Click;
            btnSetJMMServer.Click += BtnSetJMMServer_Click;

        }

        private void BtnSetJMMServer_Click(object sender, RoutedEventArgs e)
        {
            string path = AppSettings.JMMServerImagePath;
          if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
          {
            if(path != null)
              MessageBox.Show(string.Format(Shoko.Commons.Properties.Resources.Settings_SetShokoServerFolderNonExistent, path));
            return;
          }
          AppSettings.ImagesPath = path;
        }

        private void BtnSetDefaultFolder_Click(object sender, RoutedEventArgs e)
        {
            string path = AppSettings.DefaultImagePath;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            AppSettings.ImagesPath = path;
        }

        void btnChooseImagesFolder_Click(object sender, RoutedEventArgs e)
        {
            //needed check, 
            if (CommonFileDialog.IsPlatformSupported)
            {
                var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                //CommonFileDialogResult result = dialog.ShowDialog();
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    AppSettings.ImagesPath = dialog.FileName;
                }
            }
            else
            {
                System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    AppSettings.ImagesPath = dialog.SelectedPath;
                }
            }
        }



        void btnSave_Click(object sender, RoutedEventArgs e)
        {
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void settingChanged(object sender, RoutedEventArgs e)
        {
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }
    }
}

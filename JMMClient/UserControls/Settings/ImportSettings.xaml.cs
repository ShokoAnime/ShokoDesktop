using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace JMMClient.UserControls
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
            chkImportSettings_UseEpisodeStatus.Click += new RoutedEventHandler(settingChanged);
            chkImportSettings_HashCRC32.Click += new RoutedEventHandler(settingChanged);
            chkImportSettings_HashMD5.Click += new RoutedEventHandler(settingChanged);
            chkImportSettings_SHA1.Click += new RoutedEventHandler(settingChanged);

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            cboImagesPath.Items.Clear();
            cboImagesPath.Items.Add(JMMClient.Properties.Resources.ImportSettings_Default);
            cboImagesPath.Items.Add(JMMClient.Properties.Resources.ImportSettings_Custom);
            cboImagesPath.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(cboImagesPath_SelectionChanged);
            btnChooseImagesFolder.Click += new RoutedEventHandler(btnChooseImagesFolder_Click);

            if (AppSettings.BaseImagesPathIsDefault)
                cboImagesPath.SelectedIndex = 0;
            else
                cboImagesPath.SelectedIndex = 1;

        }

        void btnChooseImagesFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AppSettings.BaseImagesPath = dialog.SelectedPath;
            }
        }

        void cboImagesPath_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cboImagesPath.SelectedIndex == 0)
            {
                AppSettings.BaseImagesPathIsDefault = true;
                btnChooseImagesFolder.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                AppSettings.BaseImagesPathIsDefault = false;
                btnChooseImagesFolder.Visibility = System.Windows.Visibility.Visible;
            }

        }

        void btnSave_Click(object sender, RoutedEventArgs e)
        {
            JMMServerVM.Instance.SaveServerSettingsAsync();
        }

        void settingChanged(object sender, RoutedEventArgs e)
        {
            JMMServerVM.Instance.SaveServerSettingsAsync();
        }
    }
}

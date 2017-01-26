using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Desktop.Downloads;
using Shoko.Desktop.ViewModel;

namespace Shoko.Desktop.UserControls.Downloads
{
    /// <summary>
    /// Interaction logic for DownloadSettingsBakaBT.xaml
    /// </summary>
    public partial class DownloadSettingsBakaBT : UserControl
    {
        public DownloadSettingsBakaBT()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            btnTest.Click += new RoutedEventHandler(btnTest_Click);
            btnResetCookie.Click += new RoutedEventHandler(btnResetCookie_Click);
        }

        void btnResetCookie_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.BakaBTCookieHeader = "";
        }

        void btnTest_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.BakaBTCookieHeader = "";

            if (string.IsNullOrEmpty(txtUsername.Text))
            {
                MessageBox.Show(Properties.Resources.Downloads_BakaBTDetails, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                txtUsername.Focus();
                return;
            }

            VM_UserSettings.Instance.BakaBTUsername = txtUsername.Text.Trim();

            Window parentWindow = Window.GetWindow(this);
            parentWindow.Cursor = Cursors.Wait;
            IsEnabled = false;

            TorrentsBakaBT bakaBT = new TorrentsBakaBT();

            VM_UserSettings.Instance.BakaBTCookieHeader = bakaBT.Login(VM_UserSettings.Instance.BakaBTUsername, VM_UserSettings.Instance.BakaBTPassword);


            parentWindow.Cursor = Cursors.Arrow;
            IsEnabled = true;

            if (!string.IsNullOrEmpty(VM_UserSettings.Instance.BakaBTCookieHeader))
                MessageBox.Show(Properties.Resources.Downloads_Connected, Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
            else
            {
                MessageBox.Show(Properties.Resources.Downloads_Failed, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                txtUsername.Focus();
                return;
            }
        }
    }
}

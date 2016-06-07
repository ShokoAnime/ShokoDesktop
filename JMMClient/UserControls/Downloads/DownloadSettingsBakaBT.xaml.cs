using JMMClient.Downloads;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JMMClient.UserControls
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
            UserSettingsVM.Instance.BakaBTCookieHeader = "";
        }

        void btnTest_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.BakaBTCookieHeader = "";

            if (string.IsNullOrEmpty(txtUsername.Text))
            {
                MessageBox.Show(Properties.Resources.Downloads_BakaBTDetails, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                txtUsername.Focus();
                return;
            }

            UserSettingsVM.Instance.BakaBTUsername = txtUsername.Text.Trim();

            Window parentWindow = Window.GetWindow(this);
            parentWindow.Cursor = Cursors.Wait;
            this.IsEnabled = false;

            TorrentsBakaBT bakaBT = new TorrentsBakaBT();

            UserSettingsVM.Instance.BakaBTCookieHeader = bakaBT.Login(UserSettingsVM.Instance.BakaBTUsername, UserSettingsVM.Instance.BakaBTPassword);


            parentWindow.Cursor = Cursors.Arrow;
            this.IsEnabled = true;

            if (!string.IsNullOrEmpty(UserSettingsVM.Instance.BakaBTCookieHeader))
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

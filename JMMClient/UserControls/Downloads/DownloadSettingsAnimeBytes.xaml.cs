using JMMClient.Downloads;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for DownloadSettingsAnimeBytes.xaml
    /// </summary>
    public partial class DownloadSettingsAnimeBytes : UserControl
    {
        public DownloadSettingsAnimeBytes()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            btnTest.Click += new RoutedEventHandler(btnTest_Click);
            btnResetCookie.Click += new RoutedEventHandler(btnResetCookie_Click);
        }

        void btnResetCookie_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.AnimeBytesCookieHeader = "";
        }

        void btnTest_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.AnimeBytesCookieHeader = "";

            if (string.IsNullOrEmpty(txtUsername.Text))
            {
                MessageBox.Show(Properties.Resources.Downloads_AnimeBytesDetails, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                txtUsername.Focus();
                return;
            }

            UserSettingsVM.Instance.AnimeBytesUsername = txtUsername.Text.Trim();

            Window parentWindow = Window.GetWindow(this);
            parentWindow.Cursor = Cursors.Wait;
            this.IsEnabled = false;

            TorrentsAnimeBytes AnimeBytes = new TorrentsAnimeBytes();

            UserSettingsVM.Instance.AnimeBytesCookieHeader = AnimeBytes.Login(UserSettingsVM.Instance.AnimeBytesUsername, UserSettingsVM.Instance.AnimeBytesPassword);

            parentWindow.Cursor = Cursors.Arrow;
            this.IsEnabled = true;

            if (!string.IsNullOrEmpty(UserSettingsVM.Instance.AnimeBytesCookieHeader))
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

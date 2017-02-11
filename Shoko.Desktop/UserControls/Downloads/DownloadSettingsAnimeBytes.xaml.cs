using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Commons.Downloads;
using Shoko.Desktop.ViewModel;

namespace Shoko.Desktop.UserControls.Downloads
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
            VM_UserSettings.Instance.AnimeBytesCookieHeader = "";
        }

        void btnTest_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.AnimeBytesCookieHeader = "";

            if (string.IsNullOrEmpty(txtUsername.Text))
            {
                MessageBox.Show(Shoko.Commons.Properties.Resources.Downloads_AnimeBytesDetails, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                txtUsername.Focus();
                return;
            }

            VM_UserSettings.Instance.AnimeBytesUsername = txtUsername.Text.Trim();

            Window parentWindow = Window.GetWindow(this);
            parentWindow.Cursor = Cursors.Wait;
            IsEnabled = false;

            TorrentsAnimeBytes AnimeBytes = new TorrentsAnimeBytes();

            VM_UserSettings.Instance.AnimeBytesCookieHeader = AnimeBytes.Login(VM_UserSettings.Instance.AnimeBytesUsername, VM_UserSettings.Instance.AnimeBytesPassword);

            parentWindow.Cursor = Cursors.Arrow;
            IsEnabled = true;

            if (!string.IsNullOrEmpty(VM_UserSettings.Instance.AnimeBytesCookieHeader))
                MessageBox.Show(Shoko.Commons.Properties.Resources.Downloads_Connected, Shoko.Commons.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
            else
            {
                MessageBox.Show(Shoko.Commons.Properties.Resources.Downloads_Failed, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                txtUsername.Focus();
                return;
            }
        }
    }
}

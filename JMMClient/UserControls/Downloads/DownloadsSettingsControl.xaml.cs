using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for DownloadsSettingsControl.xaml
    /// </summary>
    public partial class DownloadsSettingsControl : UserControl
    {
        public DownloadsSettingsControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            btnTest.Click += new RoutedEventHandler(btnTest_Click);

            udRefreshInterval.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udRefreshInterval_ValueChanged);
        }

        void udRefreshInterval_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UserSettingsVM.Instance.UTorrentRefreshInterval = udRefreshInterval.Value.Value;
        }

        void btnTest_Click(object sender, RoutedEventArgs e)
        {
            if (!UTorrentHelperVM.Instance.AreCredentialsValid())
            {
                MessageBox.Show(Properties.Resources.Downloads_uTorrentDetails, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                txtServer.Focus();
                return;
            }

            Window parentWindow = Window.GetWindow(this);
            parentWindow.Cursor = Cursors.Wait;
            this.IsEnabled = false;

            bool success = UTorrentHelperVM.Instance.TestConnection();

            parentWindow.Cursor = Cursors.Arrow;
            this.IsEnabled = true;

            if (success)
                MessageBox.Show(Properties.Resources.Downloads_Connected, Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
            else
            {
                MessageBox.Show(Properties.Resources.Downloads_Failed, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                txtServer.Focus();
                return;
            }
        }
    }
}

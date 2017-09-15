using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;

namespace Shoko.Desktop.UserControls.Settings
{
    /// <summary>
    /// Interaction logic for JMMServerSettings.xaml
    /// </summary>
    public partial class JMMServerSettings : UserControl
    {
        public JMMServerSettings()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            txtServer.Text = AppSettings.JMMServer_Address;
            txtPort.Text = AppSettings.JMMServer_Port;
            //txtFilePort.Text = AppSettings.JMMServer_FilePort;
            txtProxy.Text = AppSettings.ProxyAddress;
            btnAutoStartLocalJMMServer.IsChecked = AppSettings.AutoStartLocalJMMServer;


            btnTest.Click += new RoutedEventHandler(btnTest_Click);
        }

        void btnTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AppSettings.JMMServer_Address = txtServer.Text.Trim();
                AppSettings.JMMServer_Port = txtPort.Text.Trim();

                string proxyAddress = txtProxy.Text.Trim();
                if (!string.IsNullOrEmpty(proxyAddress))
                {
                    if (!Uri.IsWellFormedUriString(proxyAddress, UriKind.Absolute))
                    {
                        MessageBox.Show("The proxy address is not a valid URI", Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    AppSettings.ProxyAddress = proxyAddress;
                }
                //AppSettings.JMMServer_FilePort = txtFilePort.Text.Trim();

                if (VM_ShokoServer.Instance.SetupClient())
                {
                    // authenticate user
                    if (!VM_ShokoServer.Instance.AuthenticateUser())
                    {
                        Window mainWindow = Window.GetWindow(this);
                        mainWindow?.Close();
                        return;
                    }

                    VM_MainListHelper.Instance.ClearData();
                    VM_MainListHelper.Instance.RefreshGroupsSeriesData();
                    //MessageBox.Show(Shoko.Commons.Properties.Resources.Success, "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex.Message, ex);
            }
        }

        private void btnAutoStartLocalJMMServer_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.AutoStartLocalJMMServer = (bool)btnAutoStartLocalJMMServer.IsChecked;
        }
    }
}

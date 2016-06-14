using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace JMMClient.UserControls
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
            btnAutoStartLocalJMMServer.IsChecked = AppSettings.AutoStartLocalJMMServer;


            btnTest.Click += new RoutedEventHandler(btnTest_Click);
        }

        void btnTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AppSettings.JMMServer_Address = txtServer.Text.Trim();
                AppSettings.JMMServer_Port = txtPort.Text.Trim();
                //AppSettings.JMMServer_FilePort = txtFilePort.Text.Trim();

                if (JMMServerVM.Instance.SetupBinaryClient())
                {
                    // authenticate user
                    if (!JMMServerVM.Instance.AuthenticateUser())
                    {
                        Window maniWindow = Window.GetWindow(this);
                        maniWindow.Close();
                        return;
                    }

                    //MessageBox.Show(Properties.Resources.Success, "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAutoStartLocalJMMServer_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.AutoStartLocalJMMServer = (bool)btnAutoStartLocalJMMServer.IsChecked;
        }
    }
}

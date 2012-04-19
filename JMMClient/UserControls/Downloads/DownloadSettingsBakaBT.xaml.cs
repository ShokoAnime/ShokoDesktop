using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JMMClient.Downloads;

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
				MessageBox.Show("Please enter BakaBT login details", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
				MessageBox.Show("Connected sucessfully", "Sucess", MessageBoxButton.OK, MessageBoxImage.Information);
			else
			{
				MessageBox.Show("Failed! See log for more details if needed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				txtUsername.Focus();
				return;
			}
		}
	}
}

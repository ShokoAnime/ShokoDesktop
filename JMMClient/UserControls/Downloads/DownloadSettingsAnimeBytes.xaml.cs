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
	/// Interaction logic for DownloadSettingsAnimeBytes.xaml
	/// </summary>
	public partial class DownloadSettingsAnimeBytes : UserControl
	{
		public DownloadSettingsAnimeBytes()
		{
			InitializeComponent();

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
				MessageBox.Show("Please enter Anime Byt.es login details", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

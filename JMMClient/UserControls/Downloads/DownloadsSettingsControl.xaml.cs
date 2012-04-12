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
				MessageBox.Show("Please enter all uTorrent details", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
				MessageBox.Show("Connected sucessfully", "Sucess", MessageBoxButton.OK, MessageBoxImage.Information);
			else
			{
				MessageBox.Show("Failed! See log for more details if needed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				txtServer.Focus();
				return;
			}
		}
	}
}

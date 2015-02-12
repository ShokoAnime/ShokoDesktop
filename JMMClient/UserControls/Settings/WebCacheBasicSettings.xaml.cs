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
	/// Interaction logic for WebCacheBasicSettings.xaml
	/// </summary>
	public partial class WebCacheBasicSettings : UserControl
	{
		public WebCacheBasicSettings()
		{
			InitializeComponent();
			chkWebCache_Anonymous.Click += new RoutedEventHandler(settingChanged);
			chkWebCache_FileEpisodes_Get.Click += new RoutedEventHandler(settingChanged);
			chkWebCache_FileEpisodes_Send.Click += new RoutedEventHandler(settingChanged);
			chkWebCache_TvDBAssociations_Get.Click += new RoutedEventHandler(settingChanged);
			chkWebCache_TvDBAssociations_Send.Click += new RoutedEventHandler(settingChanged);
			chkWebCache_MALAssociations_Get.Click += new RoutedEventHandler(settingChanged);
			chkWebCache_MALAssociations_Send.Click += new RoutedEventHandler(settingChanged);
            chkWebCache_TraktAssociations_Get.Click += new RoutedEventHandler(settingChanged);
            chkWebCache_TraktAssociations_Send.Click += new RoutedEventHandler(settingChanged);
            chkWebCache_UserInfo.Click += new RoutedEventHandler(settingChanged);
			btnSave.Click += new RoutedEventHandler(btnSave_Click);
		}

		void btnSave_Click(object sender, RoutedEventArgs e)
		{
			// test to see if this server is valid
			string uri = string.Format("http://{0}/GetPing.aspx", JMMServerVM.Instance.WebCache_Address);
			string xml = Utils.DownloadWebPage(uri);

			if (!xml.Trim().Contains("PONG"))
			{
				Utils.ShowErrorMessage("Server is not valid!");
				return;
			}

			JMMServerVM.Instance.SaveServerSettingsAsync();
			MessageBox.Show("Success", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		void settingChanged(object sender, RoutedEventArgs e)
		{
			JMMServerVM.Instance.SaveServerSettingsAsync();
		}
	}
}

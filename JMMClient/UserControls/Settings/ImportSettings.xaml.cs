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
	/// Interaction logic for ImportSettings.xaml
	/// </summary>
	public partial class ImportSettings : UserControl
	{
		public ImportSettings()
		{
			InitializeComponent();

			btnSave.Click += new RoutedEventHandler(btnSave_Click);

			chkImportSettings_HashCRC32.Click += new RoutedEventHandler(settingChanged);
			chkImportSettings_HashMD5.Click += new RoutedEventHandler(settingChanged);
			chkImportSettings_ImportOnStart.Click += new RoutedEventHandler(settingChanged);
			chkImportSettings_SHA1.Click += new RoutedEventHandler(settingChanged);
			chkImportSettings_WatchFiles.Click += new RoutedEventHandler(settingChanged);
			chkImportSettings_UseEpisodeStatus.Click += new RoutedEventHandler(settingChanged);
		}

		void btnSave_Click(object sender, RoutedEventArgs e)
		{
			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void settingChanged(object sender, RoutedEventArgs e)
		{
			JMMServerVM.Instance.SaveServerSettingsAsync();
		}
	}
}

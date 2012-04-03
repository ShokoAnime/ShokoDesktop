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
	/// Interaction logic for AniDBDownloadSettings.xaml
	/// </summary>
	public partial class AniDBDownloadSettings : UserControl
	{
		public AniDBDownloadSettings()
		{
			InitializeComponent();

			chkDownloadGroups.Click += new RoutedEventHandler(settingChanged);
			chkDownloadReviews.Click += new RoutedEventHandler(settingChanged);
		}

		void settingChanged(object sender, RoutedEventArgs e)
		{
			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

	}
}

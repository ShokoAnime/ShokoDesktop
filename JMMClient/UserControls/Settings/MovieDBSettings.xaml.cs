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
	/// Interaction logic for MovieDBSettings.xaml
	/// </summary>
	public partial class MovieDBSettings : UserControl
	{
		public MovieDBSettings()
		{
			InitializeComponent();

			chkMovieDB_FanartAutoDownload.Click += new RoutedEventHandler(settingChanged);
			chkMovieDB_PosterAutoDownload.Click += new RoutedEventHandler(settingChanged);
			udMaxFanarts.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udMaxFanarts_ValueChanged);
		}

		void udMaxFanarts_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			JMMServerVM.Instance.MovieDB_AutoFanartAmount = udMaxFanarts.Value.Value;
			JMMServerVM.Instance.SaveServerSettingsAsync();
		}


		void settingChanged(object sender, RoutedEventArgs e)
		{
			JMMServerVM.Instance.SaveServerSettingsAsync();
		}
	}
}

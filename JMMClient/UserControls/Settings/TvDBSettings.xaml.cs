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
using Microsoft.Windows.Controls;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for TvDBSettings.xaml
	/// </summary>
	public partial class TvDBSettings : UserControl
	{
		public TvDBSettings()
		{
			InitializeComponent();

			chkTvDB_FanartAutoDownload.Click += new RoutedEventHandler(settingChanged);
			chkTvDB_PosterAutoDownload.Click += new RoutedEventHandler(settingChanged);
			chkTvDB_WideBannerAutoDownload.Click += new RoutedEventHandler(settingChanged);
			udMaxFanarts.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udMaxFanarts_ValueChanged);

			cboUpdateFrequency.Items.Clear();
			cboUpdateFrequency.Items.Add(Properties.Resources.UpdateFrequency_Daily);
			cboUpdateFrequency.Items.Add(Properties.Resources.UpdateFrequency_12Hours);
			cboUpdateFrequency.Items.Add(Properties.Resources.UpdateFrequency_6Hours);
			cboUpdateFrequency.Items.Add(Properties.Resources.UpdateFrequency_Never);

			switch (JMMServerVM.Instance.TvDB_UpdateFrequency)
			{
				case ScheduledUpdateFrequency.Daily: cboUpdateFrequency.SelectedIndex = 0; break;
				case ScheduledUpdateFrequency.HoursTwelve: cboUpdateFrequency.SelectedIndex = 1; break;
				case ScheduledUpdateFrequency.HoursSix: cboUpdateFrequency.SelectedIndex = 2; break;
				case ScheduledUpdateFrequency.Never: cboUpdateFrequency.SelectedIndex = 3; break;
			}

			cboUpdateFrequency.SelectionChanged += new SelectionChangedEventHandler(cboUpdateFrequency_SelectionChanged);
		}

		void cboUpdateFrequency_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (cboUpdateFrequency.SelectedIndex)
			{
				case 0: JMMServerVM.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.Daily; break;
				case 1: JMMServerVM.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve; break;
				case 2: JMMServerVM.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.HoursSix; break;
				case 3: JMMServerVM.Instance.TvDB_UpdateFrequency = ScheduledUpdateFrequency.Never; break;
			}

			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void udMaxFanarts_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			JMMServerVM.Instance.TvDB_AutoFanartAmount = udMaxFanarts.Value.Value;
			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void settingChanged(object sender, RoutedEventArgs e)
		{
			JMMServerVM.Instance.SaveServerSettingsAsync();
		}
	}
}

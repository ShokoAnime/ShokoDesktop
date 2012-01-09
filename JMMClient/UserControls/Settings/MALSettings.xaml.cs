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
	/// Interaction logic for MALSettings.xaml
	/// </summary>
	public partial class MALSettings : UserControl
	{
		public MALSettings()
		{
			InitializeComponent();

			cboUpdateFrequency.Items.Clear();
			cboUpdateFrequency.Items.Add(Properties.Resources.UpdateFrequency_Daily);
			cboUpdateFrequency.Items.Add(Properties.Resources.UpdateFrequency_12Hours);
			cboUpdateFrequency.Items.Add(Properties.Resources.UpdateFrequency_6Hours);
			cboUpdateFrequency.Items.Add(Properties.Resources.UpdateFrequency_Never);

			switch (JMMServerVM.Instance.MAL_UpdateFrequency)
			{
				case ScheduledUpdateFrequency.Daily: cboUpdateFrequency.SelectedIndex = 0; break;
				case ScheduledUpdateFrequency.HoursTwelve: cboUpdateFrequency.SelectedIndex = 1; break;
				case ScheduledUpdateFrequency.HoursSix: cboUpdateFrequency.SelectedIndex = 2; break;
				case ScheduledUpdateFrequency.Never: cboUpdateFrequency.SelectedIndex = 3; break;
			}

			cboUpdateFrequency.SelectionChanged += new SelectionChangedEventHandler(cboUpdateFrequency_SelectionChanged);

			btnTest.Click += new RoutedEventHandler(btnTest_Click);		
		}

		void cboUpdateFrequency_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (cboUpdateFrequency.SelectedIndex)
			{
				case 0: JMMServerVM.Instance.MAL_UpdateFrequency = ScheduledUpdateFrequency.Daily; break;
				case 1: JMMServerVM.Instance.MAL_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve; break;
				case 2: JMMServerVM.Instance.MAL_UpdateFrequency = ScheduledUpdateFrequency.HoursSix; break;
				case 3: JMMServerVM.Instance.MAL_UpdateFrequency = ScheduledUpdateFrequency.Never; break;
			}

			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void btnTest_Click(object sender, RoutedEventArgs e)
		{
			JMMServerVM.Instance.TestMALLogin();
		}
	}
}

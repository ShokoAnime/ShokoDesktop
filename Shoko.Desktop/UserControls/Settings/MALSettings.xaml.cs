using System.Windows;
using System.Windows.Controls;
using Shoko.Desktop.ViewModel;
using Shoko.Models.Enums;

namespace Shoko.Desktop.UserControls.Settings
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
            cboUpdateFrequency.Items.Add(Shoko.Commons.Properties.Resources.UpdateFrequency_Daily);
            cboUpdateFrequency.Items.Add(Shoko.Commons.Properties.Resources.UpdateFrequency_12Hours);
            cboUpdateFrequency.Items.Add(Shoko.Commons.Properties.Resources.UpdateFrequency_6Hours);
            cboUpdateFrequency.Items.Add(Shoko.Commons.Properties.Resources.UpdateFrequency_Never);

            switch (VM_ShokoServer.Instance.MAL_UpdateFrequency)
            {
                case ScheduledUpdateFrequency.Daily: cboUpdateFrequency.SelectedIndex = 0; break;
                case ScheduledUpdateFrequency.HoursTwelve: cboUpdateFrequency.SelectedIndex = 1; break;
                case ScheduledUpdateFrequency.HoursSix: cboUpdateFrequency.SelectedIndex = 2; break;
                case ScheduledUpdateFrequency.Never: cboUpdateFrequency.SelectedIndex = 3; break;
            }

            cboUpdateFrequency.SelectionChanged += new SelectionChangedEventHandler(cboUpdateFrequency_SelectionChanged);

            btnTest.Click += new RoutedEventHandler(btnTest_Click);
            chkNeverDecrease.Click += new RoutedEventHandler(chkNeverDecrease_Click);
        }

        void chkNeverDecrease_Click(object sender, RoutedEventArgs e)
        {
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void cboUpdateFrequency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cboUpdateFrequency.SelectedIndex)
            {
                case 0: VM_ShokoServer.Instance.MAL_UpdateFrequency = ScheduledUpdateFrequency.Daily; break;
                case 1: VM_ShokoServer.Instance.MAL_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve; break;
                case 2: VM_ShokoServer.Instance.MAL_UpdateFrequency = ScheduledUpdateFrequency.HoursSix; break;
                case 3: VM_ShokoServer.Instance.MAL_UpdateFrequency = ScheduledUpdateFrequency.Never; break;
            }

            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void btnTest_Click(object sender, RoutedEventArgs e)
        {
            VM_ShokoServer.Instance.TestMALLogin();
        }
    }
}

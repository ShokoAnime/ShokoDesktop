using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Models.Enums;

namespace Shoko.Desktop.UserControls.Settings
{
    /// <summary>
    /// Interaction logic for TraktSettings.xaml
    /// </summary>
    public partial class TraktSettings : UserControl
    {
        public TraktSettings()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            Utils.PopulateScheduledComboBox(cboUpdateFrequency, VM_ShokoServer.Instance.Trakt_UpdateFrequency);
            cboUpdateFrequency.SelectionChanged += new SelectionChangedEventHandler(cboUpdateFrequency_SelectionChanged);

            EvaluateVisibility();

            btnTest.Click += new RoutedEventHandler(btnTest_Click);

            chkTrakt_EpisodeAutoDownload.Click += new RoutedEventHandler(settingChanged);
            chkTrakt_Enabled.Click += chkTrakt_Enabled_Click;
        }

        void chkTrakt_Enabled_Click(object sender, RoutedEventArgs e)
        {
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
            EvaluateVisibility();
        }

        void settingChanged(object sender, RoutedEventArgs e)
        {
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void cboUpdateFrequency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cboUpdateFrequency.SelectedIndex)
            {
                case 0: VM_ShokoServer.Instance.Trakt_UpdateFrequency = ScheduledUpdateFrequency.HoursSix; break;
                case 1: VM_ShokoServer.Instance.Trakt_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve; break;
                case 2: VM_ShokoServer.Instance.Trakt_UpdateFrequency = ScheduledUpdateFrequency.Daily; break;
                case 3: VM_ShokoServer.Instance.Trakt_UpdateFrequency = ScheduledUpdateFrequency.WeekOne; break;
                case 4: VM_ShokoServer.Instance.Trakt_UpdateFrequency = ScheduledUpdateFrequency.MonthOne; break;
                case 5: VM_ShokoServer.Instance.Trakt_UpdateFrequency = ScheduledUpdateFrequency.Never; break;
            }

            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void btnTest_Click(object sender, RoutedEventArgs e)
        {
            VM_ShokoServer.Instance.AuthorizeTraktPIN(txtTraktPIN.Text.Trim());
            VM_ShokoServer.Instance.GetServerSettings();
            EvaluateVisibility();
        }

        private void EvaluateVisibility()
        {
            Visibility vis = Visibility.Collapsed;
            if (VM_ShokoServer.Instance.Trakt_IsEnabled) vis = Visibility.Visible;

            spPINLabel.Visibility = vis;
            spPINData.Visibility = vis;
            spPINLink.Visibility = vis;

            btnTest.Visibility = vis;
            spValidity.Visibility = vis;

            spUpdatesLabel.Visibility = vis;
            spUpdatesData.Visibility = vis;

            spEpisodeLabel.Visibility = vis;
            spEpisodeData.Visibility = vis;

            
            if (!string.IsNullOrEmpty(VM_ShokoServer.Instance.Trakt_AuthToken))
            {
                long validUntil = 0;
                long.TryParse(VM_ShokoServer.Instance.Trakt_TokenExpirationDate, out validUntil);
                if (validUntil > 0)
                {
                    DateTime? validDate = Utils.GetUTCDate(validUntil);
                    if (validDate.HasValue && DateTime.Now < validDate.Value)
                    {
                        tbValidity.Text = string.Format(Shoko.Commons.Properties.Resources.Trakt_TokenValid, validDate.ToString());
                    }
                    else
                    {
                        tbValidity.Text = Shoko.Commons.Properties.Resources.Trakt_TokenExpired;
                    }
                }
            }
            else
                tbValidity.Text = Shoko.Commons.Properties.Resources.Trakt_ShokoNotAuth;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            EvaluateVisibility();
        }
    }
}

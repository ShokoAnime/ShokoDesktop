using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace JMMClient.UserControls
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

            Utils.PopulateScheduledComboBox(cboUpdateFrequency, JMMServerVM.Instance.Trakt_UpdateFrequency);
            cboUpdateFrequency.SelectionChanged += new SelectionChangedEventHandler(cboUpdateFrequency_SelectionChanged);

            EvaluateVisibility();

            btnTest.Click += new RoutedEventHandler(btnTest_Click);

            chkTrakt_EpisodeAutoDownload.Click += new RoutedEventHandler(settingChanged);
            chkTrakt_FanartAutoDownload.Click += new RoutedEventHandler(settingChanged);
            chkTrakt_PostersAutoDownload.Click += new RoutedEventHandler(settingChanged);
            chkTrakt_Enabled.Click += chkTrakt_Enabled_Click;
        }

        void chkTrakt_Enabled_Click(object sender, RoutedEventArgs e)
        {
            JMMServerVM.Instance.SaveServerSettingsAsync();
            EvaluateVisibility();
        }

        void settingChanged(object sender, RoutedEventArgs e)
        {
            JMMServerVM.Instance.SaveServerSettingsAsync();
        }

        void cboUpdateFrequency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cboUpdateFrequency.SelectedIndex)
            {
                case 0: JMMServerVM.Instance.Trakt_UpdateFrequency = ScheduledUpdateFrequency.HoursSix; break;
                case 1: JMMServerVM.Instance.Trakt_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve; break;
                case 2: JMMServerVM.Instance.Trakt_UpdateFrequency = ScheduledUpdateFrequency.Daily; break;
                case 3: JMMServerVM.Instance.Trakt_UpdateFrequency = ScheduledUpdateFrequency.WeekOne; break;
                case 4: JMMServerVM.Instance.Trakt_UpdateFrequency = ScheduledUpdateFrequency.MonthOne; break;
                case 5: JMMServerVM.Instance.Trakt_UpdateFrequency = ScheduledUpdateFrequency.Never; break;
            }

            JMMServerVM.Instance.SaveServerSettingsAsync();
        }

        void btnTest_Click(object sender, RoutedEventArgs e)
        {
            JMMServerVM.Instance.AuthorizeTraktPIN(txtTraktPIN.Text.Trim());
            JMMServerVM.Instance.GetServerSettings();
            EvaluateVisibility();
        }

        private void EvaluateVisibility()
        {
            System.Windows.Visibility vis = System.Windows.Visibility.Collapsed;
            if (JMMServerVM.Instance.Trakt_IsEnabled) vis = System.Windows.Visibility.Visible;

            spPINLabel.Visibility = vis;
            spPINData.Visibility = vis;
            spPINLink.Visibility = vis;

            btnTest.Visibility = vis;
            spValidity.Visibility = vis;

            spUpdatesLabel.Visibility = vis;
            spUpdatesData.Visibility = vis;

            spFanartLabel.Visibility = vis;
            spFanartData.Visibility = vis;

            spPostersLabel.Visibility = vis;
            spPostersData.Visibility = vis;

            spEpisodeLabel.Visibility = vis;
            spEpisodeData.Visibility = vis;


            bool validToken = false;
            if (!string.IsNullOrEmpty(JMMServerVM.Instance.Trakt_AuthToken))
            {
                long validUntil = 0;
                long.TryParse(JMMServerVM.Instance.Trakt_TokenExpirationDate, out validUntil);
                if (validUntil > 0)
                {
                    DateTime? validDate = Utils.GetUTCDate(validUntil);
                    if (validDate.HasValue && DateTime.Now < validDate.Value)
                    {
                        tbValidity.Text = string.Format(Properties.Resources.Trakt_TokenValid, validDate.ToString());
                        validToken = true;
                    }
                    else
                    {
                        tbValidity.Text = Properties.Resources.Trakt_TokenExpired;
                    }
                }
            }
            else
                tbValidity.Text = Properties.Resources.Trakt_JMMNotAuth;

            /*
            if (validToken)
                tbValidity.Visibility = System.Windows.Visibility.Visible;
            else
                tbValidity.Visibility = System.Windows.Visibility.Collapsed;*/
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            EvaluateVisibility();
        }
    }
}

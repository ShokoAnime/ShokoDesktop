using NLog;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using JMMClient.VideoPlayers;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for VideoPlayerSettingsControl.xaml
    /// </summary>
    public partial class VideoPlayerSettingsControl : UserControl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public VideoPlayerSettingsControl()
        {
            InitializeComponent();

            chkAutoSetWatched.IsChecked = UserSettingsVM.Instance.VideoAutoSetWatched;

            chkAutoSetWatched.Click += new RoutedEventHandler(chkAutoSetWatched_Click);

            chkMpcWebUiIntegration.IsChecked = UserSettingsVM.Instance.MPCWebUiIntegration;

            chkMpcWebUiIntegration.Click += new RoutedEventHandler(chkMpcWebUiIntegration_Click);

            cboDefaultPlayer.Items.Clear();
            cboDefaultPlayer.Items.Add("Internal MPV");
            cboDefaultPlayer.Items.Add("MPC");
            cboDefaultPlayer.Items.Add("VLC");
            cboDefaultPlayer.Items.Add("Zoom Player");
            cboDefaultPlayer.Items.Add("------");
            cboDefaultPlayer.Items.Add("External MPV");
            cboDefaultPlayer.Items.Add("Pot Player");
            cboDefaultPlayer.Items.Add("Windows Default");
            switch (AppSettings.DefaultPlayer_GroupList)
            {
                case (int)VideoPlayer.MPV:
                    cboDefaultPlayer.SelectedIndex = 0;
                    break;
                case (int)VideoPlayer.MPC:
                    cboDefaultPlayer.SelectedIndex = 1;
                    break;
                case (int)VideoPlayer.VLC:
                    cboDefaultPlayer.SelectedIndex = 2;
                    break;
                case (int)VideoPlayer.ZoomPlayer:
                    cboDefaultPlayer.SelectedIndex = 3;
                    break;
                case 4:
                    // Not handled, placeholder
                    return;
                case (int)VideoPlayer.ExternalMPV:
                    cboDefaultPlayer.SelectedIndex = 5;
                    break;
                case (int)VideoPlayer.PotPlayer:
                    cboDefaultPlayer.SelectedIndex = 6;
                    break;
                case (int)VideoPlayer.WindowsDefault:
                    cboDefaultPlayer.SelectedIndex = 7;
                    break;
                default:
                    cboDefaultPlayer.SelectedIndex = 0;
                    break;

            }

            cboDefaultPlayer.SelectionChanged += new SelectionChangedEventHandler(cboDefaultPlayer_SelectionChanged);
            MainWindow.videoHandler.Init();
            RefreshConfigured();

        }

        public Visibility DefaultConfigured
        {
            get
            {
                // If windows default player always mark as configured
                if (cboDefaultPlayer.SelectedIndex == 7)
                    return Visibility.Visible;

                bool val = MainWindow.videoHandler.IsActive((VideoPlayer) cboDefaultPlayer.SelectedIndex);
                return val ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public string ActivePlayer => MainWindow.videoHandler.DefaultPlayer !=null ? MainWindow.videoHandler.DefaultPlayer.Player.ToString() : "";

        public Visibility DefaultNotConfigured => DefaultConfigured == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;


        void cboDefaultPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cboDefaultPlayer.SelectedIndex)
            {
                case 0: UserSettingsVM.Instance.DefaultPlayer_GroupList = (int)VideoPlayer.MPV; break;
                case 1: UserSettingsVM.Instance.DefaultPlayer_GroupList = (int)VideoPlayer.MPC; break;
                case 2: UserSettingsVM.Instance.DefaultPlayer_GroupList = (int)VideoPlayer.VLC; break;
                case 3: UserSettingsVM.Instance.DefaultPlayer_GroupList = (int)VideoPlayer.ZoomPlayer; break;
                case 4:
                    cboDefaultPlayer.SelectedIndex = cboDefaultPlayer.SelectedIndex + 1;
                    return;
                case 5: UserSettingsVM.Instance.DefaultPlayer_GroupList = (int)VideoPlayer.ExternalMPV; break;
                case 6: UserSettingsVM.Instance.DefaultPlayer_GroupList = (int)VideoPlayer.PotPlayer;
                    break;
                case 7: UserSettingsVM.Instance.DefaultPlayer_GroupList = (int)VideoPlayer.WindowsDefault;
                    break;
            }
            RefreshConfigured();
        }

        private void RefreshConfigured()
        {
            TextDefaultConfigured.Visibility = DefaultConfigured;
            TextDefaultConfigured.Text = JMMClient.Properties.Resources.VideoPlayer_Configured + " (" + ActivePlayer + ")";
            TextDefaultNotConfigured.Visibility = DefaultNotConfigured;
        }

        void chkAutoSetWatched_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.VideoAutoSetWatched = chkAutoSetWatched.IsChecked.Value;
            MainWindow.videoHandler.Init();
        }      

        void chkMpcWebUiIntegration_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.MPCWebUiIntegration = chkMpcWebUiIntegration.IsChecked.Value;
            MainWindow.videoHandler.Init();
        }

        void chkVLCWebUiIntegration_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.videoHandler.Init();
        }      

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshConfigured();
        }
    }
}

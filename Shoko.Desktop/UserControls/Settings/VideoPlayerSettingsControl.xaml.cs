﻿using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using NLog;
using Shoko.Desktop.Enums;
using Shoko.Desktop.ViewModel;
using UserControl = System.Windows.Controls.UserControl;

namespace Shoko.Desktop.UserControls.Settings
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

            chkAutoSetWatched.IsChecked = VM_UserSettings.Instance.VideoAutoSetWatched;

            chkAutoSetWatched.Click += new RoutedEventHandler(chkAutoSetWatched_Click);

            chkMpcWebUiIntegration.IsChecked = VM_UserSettings.Instance.MPCWebUiIntegration;

            chkMpcWebUiIntegration.Click += new RoutedEventHandler(chkMpcWebUiIntegration_Click);

            cboDefaultPlayer.Items.Clear();
            cboDefaultPlayer.Items.Add("MPC");
            cboDefaultPlayer.Items.Add("VLC");
            cboDefaultPlayer.Items.Add("Zoom Player");
            cboDefaultPlayer.Items.Add("------");
            cboDefaultPlayer.Items.Add("External MPV");
            cboDefaultPlayer.Items.Add("Pot Player");
            cboDefaultPlayer.Items.Add("Windows Default");
            switch (AppSettings.DefaultPlayer_GroupList)
            {
                case (int)VideoPlayer.MPC:
                    cboDefaultPlayer.SelectedIndex = 0;
                    break;
                case (int)VideoPlayer.VLC:
                    cboDefaultPlayer.SelectedIndex = 1;
                    break;
                case (int)VideoPlayer.ZoomPlayer:
                    cboDefaultPlayer.SelectedIndex = 2;
                    break;
                case 3:
                    // Not handled, placeholder
                    return;
                case (int)VideoPlayer.ExternalMPV:
                    cboDefaultPlayer.SelectedIndex = 4;
                    break;
                case (int)VideoPlayer.PotPlayer:
                    cboDefaultPlayer.SelectedIndex = 5;
                    break;
                case (int)VideoPlayer.WindowsDefault:
                    cboDefaultPlayer.SelectedIndex = 6;
                    break;
                default:
                    cboDefaultPlayer.SelectedIndex = 0;
                    break;

            }

            cboDefaultPlayer.SelectionChanged += new SelectionChangedEventHandler(cboDefaultPlayer_SelectionChanged);
            
            cmbPreferredMPC.Items.Clear();
            cmbPreferredMPC.Items.Add("MPC");
            cmbPreferredMPC.Items.Add("MPC-HC");
            cmbPreferredMPC.Items.Add("MPC-BE");

            switch (AppSettings.PreferredMPC)
            {
                case "MPC":
                    cmbPreferredMPC.SelectedIndex = 0;
                    break;
                case "MPC-HC":
                    cmbPreferredMPC.SelectedIndex = 1;
                    break;
                case "MPC-BE":
                    cmbPreferredMPC.SelectedIndex = 2;
                    break;
            }
            cmbPreferredMPC.SelectionChanged += CmbPreferredMpcOnSelectionChanged;
            MainWindow.videoHandler.Init();
            RefreshConfigured();

        }

        private void CmbPreferredMpcOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppSettings.PreferredMPC = cmbPreferredMPC.Text;
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
                case 0: VM_UserSettings.Instance.DefaultPlayer_GroupList = (int)VideoPlayer.MPC; break;
                case 1: VM_UserSettings.Instance.DefaultPlayer_GroupList = (int)VideoPlayer.VLC; break;
                case 2: VM_UserSettings.Instance.DefaultPlayer_GroupList = (int)VideoPlayer.ZoomPlayer; break;
                case 3:
                    cboDefaultPlayer.SelectedIndex = cboDefaultPlayer.SelectedIndex + 1;
                    return;
                case 4: VM_UserSettings.Instance.DefaultPlayer_GroupList = (int)VideoPlayer.ExternalMPV; break;
                case 5: VM_UserSettings.Instance.DefaultPlayer_GroupList = (int)VideoPlayer.PotPlayer;
                    break;
                case 6: VM_UserSettings.Instance.DefaultPlayer_GroupList = (int)VideoPlayer.WindowsDefault;
                    break;
            }
            RefreshConfigured();
        }

        private void RefreshConfigured()
        {
            TextDefaultConfigured.Visibility = DefaultConfigured;
            TextDefaultConfigured.Text = Commons.Properties.Resources.VideoPlayer_Configured + " (" + ActivePlayer + ")";
            TextDefaultNotConfigured.Visibility = DefaultNotConfigured;
            
        }

        void chkAutoSetWatched_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.VideoAutoSetWatched = chkAutoSetWatched.IsChecked.Value;
            MainWindow.videoHandler.Init();
        }

        void chkMpcWebUiIntegration_Click(object sender, RoutedEventArgs e)
        {
            VM_UserSettings.Instance.MPCWebUiIntegration = chkMpcWebUiIntegration.IsChecked.Value;
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

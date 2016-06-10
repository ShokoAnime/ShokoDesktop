using NLog;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

            btnChooseMPCLocation.Click += new RoutedEventHandler(btnChooseMPCLocation_Click);
            btnTestMPCLocation.Click += new RoutedEventHandler(btnTestMPCLocation_Click);
            btnClearMPCLocation.Click += new RoutedEventHandler(btnClearMPCLocation_Click);

            btnChoosePotLocation.Click += new RoutedEventHandler(btnChoosePotLocation_Click);
            btnTestPotLocation.Click += new RoutedEventHandler(btnTestPotLocation_Click);
            btnClearPotLocation.Click += new RoutedEventHandler(btnClearPotLocation_Click);

            btnChooseVLCLocation.Click += new RoutedEventHandler(btnChooseVLCLocation_Click);
            btnTestVLCLocation.Click += new RoutedEventHandler(btnTestVLCLocation_Click);
            btnClearVLCLocation.Click += new RoutedEventHandler(btnClearVLCLocation_Click);


            chkAutoSetWatched.IsChecked = UserSettingsVM.Instance.VideoAutoSetWatched;

            chkAutoSetWatched.Click += new RoutedEventHandler(chkAutoSetWatched_Click);

            chkMpcIniIntegration.IsChecked = UserSettingsVM.Instance.MPCIniIntegration;
            chkMpcWebUiIntegration.IsChecked = UserSettingsVM.Instance.MPCWebUiIntegration;

            chkMpcIniIntegration.Click += new RoutedEventHandler(chkMpcIniIntegration_Click);
            chkMpcWebUiIntegration.Click += new RoutedEventHandler(chkMpcWebUiIntegration_Click);

            cboDefaultPlayer.Items.Clear();
            cboDefaultPlayer.Items.Add("Windows Default");
            cboDefaultPlayer.Items.Add("MPC");
            cboDefaultPlayer.Items.Add("PotPlayer");
            cboDefaultPlayer.Items.Add("VLC");
            switch (AppSettings.DefaultPlayer_GroupList)
            {
                case (int)DefaultVideoPlayer.MPC:
                    cboDefaultPlayer.SelectedIndex = 1;
                    break;

                case (int)DefaultVideoPlayer.PotPlayer:
                    cboDefaultPlayer.SelectedIndex = 2;
                    break;

                case (int)DefaultVideoPlayer.VLC:
                    cboDefaultPlayer.SelectedIndex = 3;
                    break;

                case (int)DefaultVideoPlayer.WindowsDefault:
                    cboDefaultPlayer.SelectedIndex = 0;
                    break;

                default:
                    cboDefaultPlayer.SelectedIndex = 0;
                    break;
            }

            cboDefaultPlayer.SelectionChanged += new SelectionChangedEventHandler(cboDefaultPlayer_SelectionChanged);
        }

        void cboDefaultPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cboDefaultPlayer.SelectedIndex)
            {
                case 0: UserSettingsVM.Instance.DefaultPlayer_GroupList = (int)DefaultVideoPlayer.WindowsDefault; break;
                case 1: UserSettingsVM.Instance.DefaultPlayer_GroupList = (int)DefaultVideoPlayer.MPC; break;
                case 2: UserSettingsVM.Instance.DefaultPlayer_GroupList = (int)DefaultVideoPlayer.PotPlayer; break;
                case 3: UserSettingsVM.Instance.DefaultPlayer_GroupList = (int)DefaultVideoPlayer.VLC; break;
                default: UserSettingsVM.Instance.DisplayStyle_GroupList = (int)DefaultVideoPlayer.WindowsDefault; break;
            }
        }

        void chkAutoSetWatched_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.VideoAutoSetWatched = chkAutoSetWatched.IsChecked.Value;
            MainWindow.videoHandler.Init();
        }

        void chkMpcIniIntegration_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.MPCWebUiIntegration = false;
            chkMpcWebUiIntegration.IsChecked = false;
            UserSettingsVM.Instance.MPCIniIntegration = chkMpcIniIntegration.IsChecked.Value;
            MainWindow.videoHandler.Init();
        }

        void chkMpcWebUiIntegration_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.MPCIniIntegration = false;
            chkMpcIniIntegration.IsChecked = false;
            UserSettingsVM.Instance.MPCWebUiIntegration = chkMpcWebUiIntegration.IsChecked.Value;
            MainWindow.videoHandler.Init();
        }

        void btnChooseMPCLocation_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                UserSettingsVM.Instance.MPCFolder = dialog.SelectedPath;
                MainWindow.videoHandler.Init();
            }
        }

        void btnTestMPCLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(UserSettingsVM.Instance.MPCFolder))
                {
                    MessageBox.Show(Properties.Resources.VideoPlayer_MPCNotSelected, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!Directory.Exists(UserSettingsVM.Instance.MPCFolder))
                {
                    MessageBox.Show(Properties.Resources.VideoPlayer_MPCNotFound, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // look for an ini file
                string[] iniFiles = Directory.GetFiles(UserSettingsVM.Instance.MPCFolder, "*.ini");
                if (iniFiles.Length == 0)
                {
                    MessageBox.Show(Properties.Resources.VideoPlayer_MPCINIMissing, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string[] lines = File.ReadAllLines(iniFiles[0]);

                bool foundFileHistory = false;
                string lastHistoryLine = "";
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("File Name 0=", StringComparison.InvariantCultureIgnoreCase))
                    {
                        foundFileHistory = true;
                        lastHistoryLine = lines[i];
                        break;
                    }
                }

                if (!foundFileHistory)
                    MessageBox.Show(Properties.Resources.VideoPlayer_INIFound, Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show(Properties.Resources.VideoPlayer_INIFoundHistory + Environment.NewLine + lastHistoryLine, Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnClearMPCLocation_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.MPCFolder = string.Empty;
        }


        void btnChoosePotLocation_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                UserSettingsVM.Instance.PotPlayerFolder = dialog.SelectedPath;
                MainWindow.videoHandler.Init();
            }
        }

        void btnTestPotLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(UserSettingsVM.Instance.PotPlayerFolder))
                {
                    MessageBox.Show(Properties.Resources.VideoPlayer_PotPlayerNotSelected, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!Directory.Exists(UserSettingsVM.Instance.PotPlayerFolder))
                {
                    MessageBox.Show(Properties.Resources.VideoPlayer_PotPlayerNotFound, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // look for an ini file
                string[] iniFiles = Directory.GetFiles(UserSettingsVM.Instance.PotPlayerFolder, "*.ini");
                if (iniFiles.Length == 0)
                {
                    MessageBox.Show(Properties.Resources.VideoPlayer_PotPlayerINIMissing, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string[] lines = File.ReadAllLines(iniFiles[0]);

                bool foundFileHistory = false;
                bool foundSectionStart = false;
                bool foundSectionEnd = false;

                string lastHistoryLine = "";
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].ToLower().Contains("[rememberfiles]"))
                        foundSectionStart = true;

                    if (foundSectionStart && lines[i].Trim().ToLower().StartsWith("[") && !lines[i].ToLower().Contains("[rememberfiles]"))
                        foundSectionEnd = true;

                    if (foundSectionStart && !foundSectionEnd)
                    {
                        if (lines[i].StartsWith("0=", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foundFileHistory = true;
                            lastHistoryLine = lines[i];
                            break;
                        }
                    }
                }

                if (!foundFileHistory)
                    MessageBox.Show(Properties.Resources.VideoPlayer_INIFound, Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show(Properties.Resources.VideoPlayer_INIFoundHistory + Environment.NewLine + lastHistoryLine, Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnClearPotLocation_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.PotPlayerFolder = string.Empty;
        }

        void btnChooseVLCLocation_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                UserSettingsVM.Instance.VLCFolder = dialog.SelectedPath;
                MainWindow.videoHandler.Init();
            }
        }

        void btnTestVLCLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(UserSettingsVM.Instance.VLCFolder))
                {
                    MessageBox.Show(Properties.Resources.VideoPlayer_VLCNotSelected, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!Directory.Exists(UserSettingsVM.Instance.VLCFolder))
                {
                    MessageBox.Show(Properties.Resources.VideoPlayer_VLCNotFound, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // look for an ini file
                string[] iniFiles = Directory.GetFiles(UserSettingsVM.Instance.VLCFolder, "*.ini");
                if (iniFiles.Length == 0)
                {
                    MessageBox.Show(Properties.Resources.VideoPlayer_VLCINIMissing, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string[] lines = File.ReadAllLines(iniFiles[0]);

                string lastFile = null;

                bool foundSectionStart = false;

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    if (line.Equals("[RecentsMRL]", StringComparison.InvariantCultureIgnoreCase))
                        foundSectionStart = true;

                    if (foundSectionStart
                        && line.Trim().ToLower().StartsWith("list="))
                    {
                        var last = line.Remove(0, 5)
                            .Split(',').Last();

                        Uri tmp = null;
                        if (Uri.TryCreate(last, UriKind.Absolute, out tmp))
                            lastFile = tmp.LocalPath;
                        break;
                    }
                }

                if (String.IsNullOrEmpty(lastFile))
                    MessageBox.Show(Properties.Resources.VideoPlayer_INIFound, Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show(Properties.Resources.VideoPlayer_INIFoundHistory + Environment.NewLine + lastFile, Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnClearVLCLocation_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsVM.Instance.VLCFolder = string.Empty;
        }

    }
}

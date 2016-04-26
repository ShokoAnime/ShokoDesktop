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
    /// Interaction logic for DownloadsTorrentBlackHole.xaml
    /// </summary>
    public partial class DownloadsTorrentBlackHole : UserControl
    {
        public DownloadsTorrentBlackHole()
        {
            InitializeComponent();

            btnBlackHoleFolder.Click += new RoutedEventHandler(btnBlackHoleFolder_Click);
        }

        void btnBlackHoleFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                UserSettingsVM.Instance.TorrentBlackholeFolder = dialog.SelectedPath;
                
            }
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using Shoko.Desktop.ViewModel;

namespace Shoko.Desktop.UserControls.Downloads
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
                VM_UserSettings.Instance.TorrentBlackholeFolder = dialog.SelectedPath;

            }
        }
    }
}

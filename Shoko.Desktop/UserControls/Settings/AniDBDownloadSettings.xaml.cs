using System.Windows;
using System.Windows.Controls;
using Shoko.Desktop.ViewModel;

namespace Shoko.Desktop.UserControls.Settings
{
    /// <summary>
    /// Interaction logic for AniDBDownloadSettings.xaml
    /// </summary>
    public partial class AniDBDownloadSettings : UserControl
    {
        public AniDBDownloadSettings()
        {
            InitializeComponent();

            chkDownloadGroups.Click += new RoutedEventHandler(settingChanged);
            chkDownloadReviews.Click += new RoutedEventHandler(settingChanged);
            chkDownloadCharacters.Click += new RoutedEventHandler(settingChanged);
            chkDownloadCreators.Click += new RoutedEventHandler(settingChanged);
        }

        void settingChanged(object sender, RoutedEventArgs e)
        {
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

    }
}

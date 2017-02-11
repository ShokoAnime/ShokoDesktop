using System.Windows;
using System.Windows.Controls;
using Shoko.Commons.Utils;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;

namespace Shoko.Desktop.UserControls.Settings
{
    /// <summary>
    /// Interaction logic for WebCacheBasicSettings.xaml
    /// </summary>
    public partial class WebCacheBasicSettings : UserControl
    {
        public WebCacheBasicSettings()
        {
            InitializeComponent();
            chkWebCache_Anonymous.Click += new RoutedEventHandler(settingChanged);
            chkWebCache_FileEpisodes_Get.Click += new RoutedEventHandler(settingChanged);
            chkWebCache_FileEpisodes_Send.Click += new RoutedEventHandler(settingChanged);
            chkWebCache_TvDBAssociations_Get.Click += new RoutedEventHandler(settingChanged);
            chkWebCache_TvDBAssociations_Send.Click += new RoutedEventHandler(settingChanged);
            chkWebCache_MALAssociations_Get.Click += new RoutedEventHandler(settingChanged);
            chkWebCache_MALAssociations_Send.Click += new RoutedEventHandler(settingChanged);
            chkWebCache_TraktAssociations_Get.Click += new RoutedEventHandler(settingChanged);
            chkWebCache_TraktAssociations_Send.Click += new RoutedEventHandler(settingChanged);
            chkWebCache_UserInfo.Click += new RoutedEventHandler(settingChanged);
            btnSave.Click += new RoutedEventHandler(btnSave_Click);
        }

        void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // test to see if this server is valid
            string uri = string.Format("http://{0}/GetPing.aspx", VM_ShokoServer.Instance.WebCache_Address);
            string xml = Misc.DownloadWebPage(uri);

            if (!xml.Trim().Contains("PONG"))
            {
                Utils.ShowErrorMessage("Server is not valid!");
                return;
            }

            VM_ShokoServer.Instance.SaveServerSettingsAsync();
            MessageBox.Show(Shoko.Commons.Properties.Resources.Success, Shoko.Commons.Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        void settingChanged(object sender, RoutedEventArgs e)
        {
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }
    }
}

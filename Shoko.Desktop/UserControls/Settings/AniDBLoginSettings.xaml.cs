using System.Windows;
using System.Windows.Controls;
using Shoko.Desktop.ViewModel;

namespace Shoko.Desktop.UserControls.Settings
{
    /// <summary>
    /// Interaction logic for AniDBLoginSettings.xaml
    /// </summary>
    public partial class AniDBLoginSettings : UserControl
    {

        public AniDBLoginSettings()
        {
            InitializeComponent();

            btnTest.Click += new RoutedEventHandler(btnTest_Click);
        }

        void btnTest_Click(object sender, RoutedEventArgs e)
        {
            //string test = txtPassword.Password;
            VM_ShokoServer.Instance.TestAniDBLogin();
        }
    }
}

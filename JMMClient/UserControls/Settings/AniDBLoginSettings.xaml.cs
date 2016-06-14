using System.Windows;
using System.Windows.Controls;

namespace JMMClient.UserControls
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
            JMMServerVM.Instance.TestAniDBLogin();
        }
    }
}

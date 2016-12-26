using System.Windows;

namespace JMMClient.Forms
{
    /// <summary>
    /// Interaction logic for UpdateForm.xaml
    /// </summary>
    public partial class UpdateForm : Window
    {
        public UpdateForm()
        {
            InitializeComponent();
            tbUpdateAvailable.Visibility = IsNewVersionAvailable() ? Visibility.Visible : Visibility.Hidden;
        }

        public bool IsNewVersionAvailable()
        {
            if (JMMServerVM.Instance.ApplicationVersion == JMMServerVM.Instance.ApplicationVersionLatest)
                return false;

            return true;
        }
    }
}

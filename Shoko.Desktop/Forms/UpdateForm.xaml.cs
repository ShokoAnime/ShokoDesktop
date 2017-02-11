using System.Windows;
using Shoko.Desktop.ViewModel;

namespace Shoko.Desktop.Forms
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
            if (VM_ShokoServer.Instance.ApplicationVersion == VM_ShokoServer.Instance.ApplicationVersionLatest)
                return false;

            return true;
        }
    }
}

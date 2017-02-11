using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for ChangePasswordForm.xaml
    /// </summary>
    public partial class ChangePasswordForm : Window
    {
        public VM_JMMUser JMMUser = null;
        public string newPassword = "";
        public ChangePasswordForm()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            btnSave.Click += new RoutedEventHandler(btnSave_Click);
        }

        void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!txtPassword.Password.Equals(txtPasswordConfirm.Password))
            {
                MessageBox.Show(Shoko.Commons.Properties.Resources.User_PasswordMatch, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                txtPassword.Focus();
                return;
            }
            Window parentWindow = GetWindow(this);

            try
            {
                parentWindow.Cursor = Cursors.Wait;
                VM_ShokoServer.Instance.ShokoServices.ChangePassword(JMMUser.JMMUserID, txtPassword.Password);
                newPassword = txtPassword.Password;
                parentWindow.Cursor = Cursors.Arrow;
                Close();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void Init(VM_JMMUser jmmUser)
        {
            JMMUser = jmmUser;
            txtUsername.Text = JMMUser.Username;
        }
    }
}

using JMMClient.ViewModel;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace JMMClient.Forms
{
    /// <summary>
    /// Interaction logic for ChangePasswordForm.xaml
    /// </summary>
    public partial class ChangePasswordForm : Window
    {
        public JMMUserVM JMMUser = null;
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
                MessageBox.Show(Properties.Resources.User_PasswordMatch, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                txtPassword.Focus();
                return;
            }
            Window parentWindow = Window.GetWindow(this);

            try
            {
                parentWindow.Cursor = Cursors.Wait;
                JMMServerVM.Instance.clientBinaryHTTP.ChangePassword(JMMUser.JMMUserID.Value, txtPassword.Password);
                newPassword = txtPassword.Password;
                parentWindow.Cursor = Cursors.Arrow;
                this.Close();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void Init(JMMUserVM jmmUser)
        {
            JMMUser = jmmUser;
            txtUsername.Text = JMMUser.Username;
        }
    }
}

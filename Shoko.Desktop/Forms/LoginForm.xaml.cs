using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Server;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for LoginForm.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        public VM_JMMUser ThisUser { get; set; }

        public LoginForm()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            btnLogin.Click += new RoutedEventHandler(btnLogin_Click);

            ThisUser = null;

            cboUsers.Items.Clear();

            List<JMMUser> users = VM_ShokoServer.Instance.ShokoServices.GetAllUsers();
            foreach (JMMUser user in users)
                cboUsers.Items.Add(user);

            if (cboUsers.Items.Count > 0)
                cboUsers.SelectedIndex = 0;

            txtPassword.PasswordChanged += new RoutedEventHandler(txtPassword_PasswordChanged);
            Loaded += new RoutedEventHandler(LoginForm_Loaded);
        }

        void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "";
        }

        void LoginForm_Loaded(object sender, RoutedEventArgs e)
        {
            txtPassword.Focus();
        }

        void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            ThisUser = null;

            VM_JMMUser user = cboUsers.SelectedItem as VM_JMMUser;
            if (user != null)
            {
                JMMUser retUser = VM_ShokoServer.Instance.ShokoServices.AuthenticateUser(user.Username, txtPassword.Password.Trim());
                if (retUser != null)
                {
                    ThisUser = user;
                    DialogResult = true;
                    VM_ShokoServer.Instance.RefreshImportFolders();
                    Close();
                }
                else
                {
                    txtPassword.Focus();
                    txtStatus.Text = Shoko.Commons.Properties.Resources.Login_IncorrectPassword;
                }
            }
        }
    }
}

using JMMClient.ViewModel;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace JMMClient.Forms
{
    /// <summary>
    /// Interaction logic for LoginForm.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        public JMMUserVM ThisUser { get; set; }

        public LoginForm()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            btnLogin.Click += new RoutedEventHandler(btnLogin_Click);

            ThisUser = null;

            cboUsers.Items.Clear();

            List<JMMServerBinary.Contract_JMMUser> users = JMMServerVM.Instance.clientBinaryHTTP.GetAllUsers();
            foreach (JMMServerBinary.Contract_JMMUser user in users)
                cboUsers.Items.Add(new JMMUserVM(user));

            if (cboUsers.Items.Count > 0)
                cboUsers.SelectedIndex = 0;

            txtPassword.PasswordChanged += new RoutedEventHandler(txtPassword_PasswordChanged);
            this.Loaded += new RoutedEventHandler(LoginForm_Loaded);
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

            JMMUserVM user = cboUsers.SelectedItem as JMMUserVM;
            if (user != null)
            {
                JMMServerBinary.Contract_JMMUser retUser = JMMServerVM.Instance.clientBinaryHTTP.AuthenticateUser(user.Username, txtPassword.Password.Trim());
                if (retUser != null)
                {
                    ThisUser = user;
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    txtPassword.Focus();
                    txtStatus.Text = Properties.Resources.Login_IncorrectPassword;
                }
            }
        }
    }
}

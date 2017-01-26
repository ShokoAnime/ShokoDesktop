using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for UserAdminControl.xaml
    /// </summary>
    public partial class UserAdminControl : UserControl
    {
        public static readonly DependencyProperty IsUserSelectedProperty = DependencyProperty.Register("IsUserSelected",
            typeof(bool), typeof(UserAdminControl), new UIPropertyMetadata(false, null));

        public bool IsUserSelected
        {
            get { return (bool)GetValue(IsUserSelectedProperty); }
            set { SetValue(IsUserSelectedProperty, value); }
        }

        public static readonly DependencyProperty SelectedUserProperty = DependencyProperty.Register("SelectedUser",
            typeof(VM_JMMUser), typeof(UserAdminControl), new UIPropertyMetadata(null, null));

        public VM_JMMUser SelectedUser
        {
            get { return (VM_JMMUser)GetValue(SelectedUserProperty); }
            set { SetValue(SelectedUserProperty, value); }
        }

        public string selectedUserPassword = "";
        public string SelectedUserPassword
        {
            get { return (selectedUserPassword); }
            set { selectedUserPassword = value; }
        }

        public UserAdminControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            IsUserSelected = false;
            SelectedUser = null;

            Loaded += new RoutedEventHandler(UserAdminControl_Loaded);
            lbUsers.SelectionChanged += new SelectionChangedEventHandler(lbUsers_SelectionChanged);

            lbTags.MouseDoubleClick += new MouseButtonEventHandler(lbTags_MouseDoubleClick);

            btnNewUser.Click += new RoutedEventHandler(btnNewUser_Click);
            btnSave.Click += new RoutedEventHandler(btnSave_Click);
            btnChangePassword.Click += new RoutedEventHandler(btnChangePassword_Click);
        }

        void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window parentWindow = Window.GetWindow(this);

                ChangePasswordForm frm = new ChangePasswordForm();
                frm.Owner = parentWindow;
                frm.Init(SelectedUser);
                frm.ShowDialog();
                SelectedUserPassword = frm.newPassword;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text.Trim()))
            {
                MessageBox.Show(Properties.Resources.User_EnterUsername, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                txtUsername.Focus();
                return;
            }
            string[] plexusers = txtPlexUsers.Text.Split(',');
            foreach (string n in plexusers)
            {
                foreach (VM_JMMUser us in lbUsers.Items)
                {
                    if (us != lbUsers.SelectedItem)
                    {
                        if (us.PlexUsers!=null && us.GetPlexUsers().Count>0)
                        {
                            foreach (string m in us.GetPlexUsers())
                            {
                                if (n.Trim().ToLower() == m.Trim().ToLower())
                                {
                                    MessageBox.Show(string.Format(Properties.Resources.User_PlexAssigned, n.Trim(), us.Username), Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                                    txtPlexUsers.Focus();
                                    return;
                                }
                            }
                        }
                    }
                }

            }
            //SelectedUser = lbUsers.SelectedItem as JMMUserVM;

            SelectedUser.Username = txtUsername.Text.Trim();
            SelectedUser.HideCategories = string.Join(",",new HashSet<string>(txtTags.Text.Split(',').Select(a=>a.Trim()).Where(a=>!string.IsNullOrEmpty(a))));
            SelectedUser.IsAdmin = chkIsAdmin.IsChecked.Value ? 1 : 0;
            SelectedUser.IsAniDBUser = chkIsAniDB.IsChecked.Value ? 1 : 0;
            SelectedUser.IsTraktUser = chkIsTrakt.IsChecked.Value ? 1 : 0;
            SelectedUser.CanEditServerSettings = chkEditSettings.IsChecked.Value ? 1 : 0;
            SelectedUser.PlexUsers = string.Join(",",new HashSet<string>(txtPlexUsers.Text.Split(',').Select(a => a.Trim()).Where(a => !string.IsNullOrEmpty(a))));
            SelectedUser.Password = SelectedUserPassword;

            try
            {
                Cursor = Cursors.Wait;
                string ret = VM_ShokoServer.Instance.ShokoServices.SaveUser(SelectedUser);
                Cursor = Cursors.Arrow;
                if (ret.Length > 0)
                {
                    MessageBox.Show(ret, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtUsername.Focus();
                }

                VM_ShokoServer.Instance.RefreshAllUsers();

                if (lbUsers.Items.Count > 0)
                {
                    lbUsers.SelectedIndex = 0;
                    lbUsers.Focus();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_DeleteUser(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_JMMUser))
                {
                    VM_JMMUser user = (VM_JMMUser)obj;

                    MessageBoxResult res = MessageBox.Show(string.Format(Properties.Resources.User_Delete, user.Username),
                    "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        if (user.JMMUserID!=0)
                        {
                            if (user.JMMUserID == VM_ShokoServer.Instance.CurrentUser.JMMUserID)
                            {
                                MessageBox.Show(Properties.Resources.User_DeleteError, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }

                        Cursor = Cursors.Wait;
                        string ret = VM_ShokoServer.Instance.ShokoServices.DeleteUser(user.JMMUserID);
                        Cursor = Cursors.Arrow;
                        if (ret.Length > 0)
                            MessageBox.Show(ret, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);

                        VM_ShokoServer.Instance.RefreshAllUsers();

                        if (lbUsers.Items.Count > 0)
                        {
                            lbUsers.SelectedIndex = 0;
                            lbUsers.Focus();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnNewUser_Click(object sender, RoutedEventArgs e)
        {
            SelectedUser = new VM_JMMUser();
            DisplayUser();
        }

        public void Init()
        {

        }

        void lbTags_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object obj = lbTags.SelectedItem;
            if (obj == null) return;

            string tagName = obj.ToString().Trim();
            string currentList = txtTags.Text.Trim();

            // add to the selected list
            string[] tags = currentList.Split(',');
            foreach (string cat in tags)
                if (cat.Trim().Equals(tagName, StringComparison.InvariantCultureIgnoreCase)) return;

            if (currentList.Length > 0) currentList += ", ";
            currentList += tagName;

            txtTags.Text = currentList;
        }

        void lbUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbUsers.Items.Count == 0)
            {
                IsUserSelected = false;
                SelectedUser = null;
                return;
            }

            SelectedUser = lbUsers.SelectedItem as VM_JMMUser;
            if (SelectedUser == null)
                IsUserSelected = false;
            else
                IsUserSelected = true;

            DisplayUser();
        }

        private void DisplayUser()
        {
            btnChangePassword.Visibility = Visibility.Hidden;
            txtUsername.Text = "";
            txtTags.Text = "";
            chkIsAdmin.IsChecked = false;
            chkIsAniDB.IsChecked = false;
            chkIsTrakt.IsChecked = false;
            chkEditSettings.IsChecked = false;
            txtPlexUsers.Text = "";
            txtUsername.Focus();

            if (SelectedUser?.JMMUserID == null) return;

            btnChangePassword.Visibility = Visibility.Visible;
            txtUsername.Text = SelectedUser.Username;
            SelectedUserPassword = SelectedUser.Password;
            txtTags.Text = string.Join(", ",SelectedUser.GetHideCategories());
            chkIsAdmin.IsChecked = SelectedUser.IsAdminUser;
            chkIsAniDB.IsChecked = SelectedUser.IsAniDBUserBool;
            chkIsTrakt.IsChecked = SelectedUser.IsTraktUserBool;
            chkEditSettings.IsChecked = SelectedUser.CanEditSettings;
            txtPlexUsers.Text = string.Join(", ",SelectedUser.GetPlexUsers());
        }

        void UserAdminControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (lbUsers.Items.Count > 0)
            {
                lbUsers.SelectedIndex = 0;
                lbUsers.Focus();
            }
        }
    }
}

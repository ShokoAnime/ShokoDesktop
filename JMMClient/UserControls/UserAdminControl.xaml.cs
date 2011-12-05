using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JMMClient.ViewModel;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace JMMClient.UserControls
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
			typeof(JMMUserVM), typeof(UserAdminControl), new UIPropertyMetadata(null, null));

		public JMMUserVM SelectedUser
		{
			get { return (JMMUserVM)GetValue(SelectedUserProperty); }
			set { SetValue(SelectedUserProperty, value); }
		}

		public UserAdminControl()
		{
			InitializeComponent();

			IsUserSelected = false;
			SelectedUser = null;

			this.Loaded += new RoutedEventHandler(UserAdminControl_Loaded);
			lbUsers.SelectionChanged += new SelectionChangedEventHandler(lbUsers_SelectionChanged);

			lbCategories.MouseDoubleClick += new MouseButtonEventHandler(lbCategories_MouseDoubleClick);

			btnNewUser.Click += new RoutedEventHandler(btnNewUser_Click);
			btnSave.Click += new RoutedEventHandler(btnSave_Click);
		}

		void btnSave_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(txtUsername.Text.Trim()))
			{
				MessageBox.Show("Please enter a username", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				txtUsername.Focus();
			}

			SelectedUser.Username = txtUsername.Text.Trim();
			SelectedUser.Password = txtPassword.Text.Trim();
			SelectedUser.HideCategories = txtCategories.Text.Trim();
			SelectedUser.IsAdmin = chkIsAdmin.IsChecked.Value ? 1 : 0;
			SelectedUser.IsAniDBUser = chkIsAniDB.IsChecked.Value ? 1 : 0;
			SelectedUser.IsTraktUser = chkIsTrakt.IsChecked.Value ? 1 : 0;
			SelectedUser.CanEditServerSettings = chkEditSettings.IsChecked.Value ? 1 : 0;
			

			try
			{
				this.Cursor = Cursors.Wait;
				string ret = JMMServerVM.Instance.clientBinaryHTTP.SaveUser(SelectedUser.ToContract());
				this.Cursor = Cursors.Arrow;
				if (ret.Length > 0)
				{
					MessageBox.Show(ret, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					txtUsername.Focus();
				}

				JMMServerVM.Instance.RefreshAllUsers();

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
				if (obj.GetType() == typeof(JMMUserVM))
				{
					JMMUserVM user = (JMMUserVM)obj;

					MessageBoxResult res = MessageBox.Show(string.Format("Are you sure you want to delete the User: {0}", user.Username),
					"Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
					if (res == MessageBoxResult.Yes)
					{
						if (user.JMMUserID.HasValue)
						{
							if (user.JMMUserID.Value == JMMServerVM.Instance.CurrentUser.JMMUserID.Value)
							{
								MessageBox.Show("Cannot delete currently logged in user", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
								return;
							}
						}

						this.Cursor = Cursors.Wait;
						string ret = JMMServerVM.Instance.clientBinaryHTTP.DeleteUser(user.JMMUserID.Value);
						this.Cursor = Cursors.Arrow;
						if (ret.Length > 0)
							MessageBox.Show(ret, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

						JMMServerVM.Instance.RefreshAllUsers();

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
			SelectedUser = new JMMUserVM();
			DisplayUser();
		}

		public void Init()
		{
			
		}

		void lbCategories_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			object obj = lbCategories.SelectedItem;
			if (obj == null) return;

			string catName = obj.ToString().Trim();
			string currentList = txtCategories.Text.Trim();

			// add to the selected list
			string[] cats = currentList.Split(',');
			foreach (string cat in cats)
				if (cat.Trim().Equals(catName, StringComparison.InvariantCultureIgnoreCase)) return;

			if (currentList.Length > 0) currentList += ",";
			currentList += catName;

			txtCategories.Text = currentList;
		}

		void lbUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (lbUsers.Items.Count == 0)
			{
				IsUserSelected = false;
				SelectedUser = null;
				return;
			}

			SelectedUser = lbUsers.SelectedItem as JMMUserVM;
			if (SelectedUser == null)
				IsUserSelected = false;
			else
				IsUserSelected = true;

			DisplayUser();
		}

		private void DisplayUser()
		{
			txtUsername.Text = "";
			txtPassword.Text = "";
			txtCategories.Text = "";
			chkIsAdmin.IsChecked = false;
			chkIsAniDB.IsChecked = false;
			chkIsTrakt.IsChecked = false;
			chkEditSettings.IsChecked = false;

			txtUsername.Focus();

			if (SelectedUser == null || !SelectedUser.JMMUserID.HasValue) return;

			txtUsername.Text = SelectedUser.Username;
			txtPassword.Text = SelectedUser.Password;
			txtCategories.Text = SelectedUser.HideCategories;
			chkIsAdmin.IsChecked = SelectedUser.IsAdminUser;
			chkIsAniDB.IsChecked = SelectedUser.IsAniDBUserBool;
			chkIsTrakt.IsChecked = SelectedUser.IsTraktUserBool;
			chkEditSettings.IsChecked = SelectedUser.CanEditSettings;
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

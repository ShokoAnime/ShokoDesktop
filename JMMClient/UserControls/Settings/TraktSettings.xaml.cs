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

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for TraktSettings.xaml
	/// </summary>
	public partial class TraktSettings : UserControl
	{
		public TraktSettings()
		{
			InitializeComponent();

			cboUpdateFrequency.Items.Clear();
			cboUpdateFrequency.Items.Add(Properties.Resources.UpdateFrequency_Daily);
			cboUpdateFrequency.Items.Add(Properties.Resources.UpdateFrequency_12Hours);
			cboUpdateFrequency.Items.Add(Properties.Resources.UpdateFrequency_6Hours);
			cboUpdateFrequency.Items.Add(Properties.Resources.UpdateFrequency_Never);

			switch (JMMServerVM.Instance.Trakt_UpdateFrequency)
			{
				case ScheduledUpdateFrequency.Daily: cboUpdateFrequency.SelectedIndex = 0; break;
				case ScheduledUpdateFrequency.HoursTwelve: cboUpdateFrequency.SelectedIndex = 1; break;
				case ScheduledUpdateFrequency.HoursSix: cboUpdateFrequency.SelectedIndex = 2; break;
				case ScheduledUpdateFrequency.Never: cboUpdateFrequency.SelectedIndex = 3; break;
			}

			cboUpdateFrequency.SelectionChanged += new SelectionChangedEventHandler(cboUpdateFrequency_SelectionChanged);



			cboSyncFrequency.Items.Clear();
			cboSyncFrequency.Items.Add(Properties.Resources.UpdateFrequency_Daily);
			cboSyncFrequency.Items.Add(Properties.Resources.UpdateFrequency_12Hours);
			cboSyncFrequency.Items.Add(Properties.Resources.UpdateFrequency_6Hours);
			cboSyncFrequency.Items.Add(Properties.Resources.UpdateFrequency_Never);

			switch (JMMServerVM.Instance.Trakt_SyncFrequency)
			{
				case ScheduledUpdateFrequency.Daily: cboSyncFrequency.SelectedIndex = 0; break;
				case ScheduledUpdateFrequency.HoursTwelve: cboSyncFrequency.SelectedIndex = 1; break;
				case ScheduledUpdateFrequency.HoursSix: cboSyncFrequency.SelectedIndex = 2; break;
				case ScheduledUpdateFrequency.Never: cboSyncFrequency.SelectedIndex = 3; break;
			}

			cboSyncFrequency.SelectionChanged += new SelectionChangedEventHandler(cboSyncFrequency_SelectionChanged);


			btnTest.Click += new RoutedEventHandler(btnTest_Click);
			txtUsername.TextChanged += new TextChangedEventHandler(txtUsername_TextChanged);

			EvaulateVisibility();

			btnJoinTrakt.Click += new RoutedEventHandler(btnJoinTrakt_Click);
		}

		void btnJoinTrakt_Click(object sender, RoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			try
			{
				parentWindow.Cursor = Cursors.Wait;
				string retMessage = "";
				bool success = JMMServerVM.Instance.clientBinaryHTTP.CreateTraktAccount(txtUsernameSignup.Text.Trim(), txtPasswordSignup.Password.Trim(), 
					txtEmail.Text.Trim(), ref retMessage);
				parentWindow.Cursor = Cursors.Arrow;

				if (success)
				{
					MessageBox.Show(retMessage, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
					JMMServerVM.Instance.GetServerSettings();
					DashboardVM.Instance.RefreshTraktFriends(true, true);
				}
				else
					MessageBox.Show(retMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		private void EvaulateVisibility()
		{
			if (string.IsNullOrEmpty(txtUsername.Text))
				bdrSignup.Visibility = System.Windows.Visibility.Visible;
			else
				bdrSignup.Visibility = System.Windows.Visibility.Collapsed;
		}

		void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
		{
			EvaulateVisibility();
		}

		void cboSyncFrequency_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (cboSyncFrequency.SelectedIndex)
			{
				case 0: JMMServerVM.Instance.Trakt_SyncFrequency = ScheduledUpdateFrequency.Daily; break;
				case 1: JMMServerVM.Instance.Trakt_SyncFrequency = ScheduledUpdateFrequency.HoursTwelve; break;
				case 2: JMMServerVM.Instance.Trakt_SyncFrequency = ScheduledUpdateFrequency.HoursSix; break;
				case 3: JMMServerVM.Instance.Trakt_SyncFrequency = ScheduledUpdateFrequency.Never; break;
			}

			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void cboUpdateFrequency_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (cboUpdateFrequency.SelectedIndex)
			{
				case 0: JMMServerVM.Instance.Trakt_UpdateFrequency = ScheduledUpdateFrequency.Daily; break;
				case 1: JMMServerVM.Instance.Trakt_UpdateFrequency = ScheduledUpdateFrequency.HoursTwelve; break;
				case 2: JMMServerVM.Instance.Trakt_UpdateFrequency = ScheduledUpdateFrequency.HoursSix; break;
				case 3: JMMServerVM.Instance.Trakt_UpdateFrequency = ScheduledUpdateFrequency.Never; break;
			}

			JMMServerVM.Instance.SaveServerSettingsAsync();
		}

		void btnTest_Click(object sender, RoutedEventArgs e)
		{
			JMMServerVM.Instance.TestTraktLogin();
		}
	}
}

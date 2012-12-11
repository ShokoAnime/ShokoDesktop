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
using System.IO;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for MPCSettingsControl.xaml
	/// </summary>
	public partial class MPCSettingsControl : UserControl
	{
		public MPCSettingsControl()
		{
			InitializeComponent();

			btnChooseMPCLocation.Click += new RoutedEventHandler(btnChooseMPCLocation_Click);
			btnTestMPCLocation.Click += new RoutedEventHandler(btnTestMPCLocation_Click);

			btnChoosePotLocation.Click += new RoutedEventHandler(btnChoosePotLocation_Click);
			btnTestPotLocation.Click += new RoutedEventHandler(btnTestPotLocation_Click);

			chkAutoSetWatched.IsChecked = UserSettingsVM.Instance.VideoAutoSetWatched;

			chkAutoSetWatched.Click += new RoutedEventHandler(chkAutoSetWatched_Click);
		}

		void btnTestPotLocation_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (string.IsNullOrEmpty(UserSettingsVM.Instance.PotPlayerFolder))
				{
					MessageBox.Show("Pot Player Folder not selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				if (!Directory.Exists(UserSettingsVM.Instance.PotPlayerFolder))
				{
					MessageBox.Show("Pot Player Folder does not exist", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				// look for an ini file
				string[] iniFiles = Directory.GetFiles(UserSettingsVM.Instance.PotPlayerFolder, "*.ini");
				if (iniFiles.Length == 0)
				{
					MessageBox.Show("No ini files found in the Pot Player Folder", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				string[] lines = File.ReadAllLines(iniFiles[0]);

				bool foundFileHistory = false;
				bool foundSectionStart = false;
				bool foundSectionEnd = false;

				string lastHistoryLine = "";
				for (int i = 0; i < lines.Length; i++)
				{
					if (lines[i].ToLower().Contains("[rememberfiles]"))
						foundSectionStart = true;

					if (foundSectionStart && lines[i].Trim().ToLower().StartsWith("[") && !lines[i].ToLower().Contains("[rememberfiles]"))
						foundSectionEnd = true;

					if (foundSectionStart && !foundSectionEnd)
					{
						if (lines[i].StartsWith("0=", StringComparison.InvariantCultureIgnoreCase))
						{
							foundFileHistory = true;
							lastHistoryLine = lines[i];
							break;
						}
					}
				}

				if (!foundFileHistory)
					MessageBox.Show("INI file found, but no history found for previous watched files", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
				else
					MessageBox.Show("INI file found. Sample of recently watched file..." + Environment.NewLine + lastHistoryLine, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnChoosePotLocation_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				UserSettingsVM.Instance.PotPlayerFolder = dialog.SelectedPath;
				MainWindow.videoHandler.Init();
			}
		}

		void chkAutoSetWatched_Click(object sender, RoutedEventArgs e)
		{
			UserSettingsVM.Instance.VideoAutoSetWatched = chkAutoSetWatched.IsChecked.Value;
			MainWindow.videoHandler.Init();
		}

		void btnTestMPCLocation_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (string.IsNullOrEmpty(UserSettingsVM.Instance.MPCFolder))
				{
					MessageBox.Show("MPC Folder not selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				if (!Directory.Exists(UserSettingsVM.Instance.MPCFolder))
				{
					MessageBox.Show("MPC Folder does not exist", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				// look for an ini file
				string[] iniFiles = Directory.GetFiles(UserSettingsVM.Instance.MPCFolder, "*.ini");
				if (iniFiles.Length == 0)
				{
					MessageBox.Show("No ini files found in the MPC Folder", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				string[] lines = File.ReadAllLines(iniFiles[0]);

				bool foundFileHistory = false;
				string lastHistoryLine = "";
				for (int i = 0; i < lines.Length; i++)
				{
					if (lines[i].StartsWith("File Name 0=", StringComparison.InvariantCultureIgnoreCase))
					{
						foundFileHistory = true;
						lastHistoryLine = lines[i];
						break;
					}
				}

				if (!foundFileHistory)
					MessageBox.Show("INI file found, but no history found for previous watched files", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
				else
					MessageBox.Show("INI file found. Sample of recently watched file..." + Environment.NewLine + lastHistoryLine, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnChooseMPCLocation_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				UserSettingsVM.Instance.MPCFolder = dialog.SelectedPath;
				MainWindow.videoHandler.Init();
			}
		}
	}
}

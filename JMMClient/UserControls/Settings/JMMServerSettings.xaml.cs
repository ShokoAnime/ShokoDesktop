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
	/// Interaction logic for JMMServerSettings.xaml
	/// </summary>
	public partial class JMMServerSettings : UserControl
	{
		public JMMServerSettings()
		{
			InitializeComponent();

			txtServer.Text = AppSettings.JMMServer_Address;
			txtPort.Text = AppSettings.JMMServer_Port;
			txtFilePort.Text = AppSettings.JMMServer_FilePort;
			

			btnTest.Click += new RoutedEventHandler(btnTest_Click);
		}

		void btnTest_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				AppSettings.JMMServer_Address = txtServer.Text.Trim();
				AppSettings.JMMServer_Port = txtPort.Text.Trim();
				AppSettings.JMMServer_FilePort = txtFilePort.Text.Trim();

				if (JMMServerVM.Instance.SetupBinaryClient())
				{
					// authenticate user
					if (!JMMServerVM.Instance.AuthenticateUser())
					{
						Window maniWindow = Window.GetWindow(this);
						maniWindow.Close();
						return;
					}

					//MessageBox.Show(Properties.Resources.Success, "", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}

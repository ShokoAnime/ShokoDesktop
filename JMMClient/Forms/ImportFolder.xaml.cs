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
using System.Windows.Shapes;
using JMMClient.ViewModel;
using System.Threading;
using System.Globalization;

namespace JMMClient.Forms
{
	/// <summary>
	/// Interaction logic for ImportFolder.xaml
	/// </summary>
	public partial class ImportFolder : Window
	{
		private ImportFolderVM importFldr = null;

		public ImportFolder()
		{
			InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnSave.Click += new RoutedEventHandler(btnSave_Click);
        }

        void btnSave_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// An import folder cannot be both the drop source and the drop destination
				if (chkDropDestination.IsChecked.HasValue && chkDropSource.IsChecked.HasValue && chkDropDestination.IsChecked.Value && chkDropSource.IsChecked.Value)
				{
					MessageBox.Show(Properties.Resources.MSG_ERR_DropSourceDestCheck, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				// The import folder location cannot be blank. Enter a valid path on OMM Server
				if (string.IsNullOrEmpty(txtImportFolderLocation.Text))
				{
					MessageBox.Show(Properties.Resources.MSG_ERR_ImportFolderLocationCheck, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
					txtImportFolderLocation.Focus();
					return;
				}

				// default the local path to the server path
				if (string.IsNullOrEmpty(txtLocalPath.Text))
					importFldr.LocalPathTemp = txtImportFolderLocation.Text.Trim();
				else
					importFldr.LocalPathTemp = txtLocalPath.Text.Trim();

				importFldr.ImportFolderName = "NA";
				importFldr.ImportFolderLocation = txtImportFolderLocation.Text.Trim();
				importFldr.IsDropDestination = chkDropDestination.IsChecked.Value ? 1 : 0;
				importFldr.IsDropSource = chkDropSource.IsChecked.Value ? 1 : 0;
				importFldr.IsWatched = chkIsWatched.IsChecked.Value ? 1 : 0;
				importFldr.Save();

				JMMServerVM.Instance.RefreshImportFolders();
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}

			this.DialogResult = true;
			this.Close();
		}

		void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			this.Close();
		}

		public void Init(ImportFolderVM ifldr)
		{
			try
			{
				importFldr = ifldr;

				txtImportFolderLocation.Text = importFldr.ImportFolderLocation;
				txtLocalPath.Text = importFldr.LocalPath;
				chkDropDestination.IsChecked = importFldr.IsDropDestination == 1;
				chkDropSource.IsChecked = importFldr.IsDropSource == 1;
				chkIsWatched.IsChecked = importFldr.IsWatched == 1;

				txtImportFolderLocation.Focus();
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}
	}
}

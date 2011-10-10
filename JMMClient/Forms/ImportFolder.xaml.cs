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
					MessageBox.Show(Properties.Resources.MSG_ERR_DropSourceDestCheck, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				// The import folder location cannot be blank. Enter a valid path on OMM Server
				if (string.IsNullOrEmpty(txtImportFolderLocation.Text))
				{
					MessageBox.Show(Properties.Resources.MSG_ERR_ImportFolderLocationCheck, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					txtImportFolderLocation.Focus();
					return;
				}

				// default the local path to the server path
				if (string.IsNullOrEmpty(txtLocalPath.Text))
					importFldr.LocalPathTemp = txtImportFolderLocation.Text.Trim();
				else
					importFldr.LocalPathTemp = txtLocalPath.Text.Trim();


				if (string.IsNullOrEmpty(txtImportFolderName.Text))
					importFldr.ImportFolderName = "NA";
				else
					importFldr.ImportFolderName = txtImportFolderName.Text.Trim();

				importFldr.ImportFolderLocation = txtImportFolderLocation.Text.Trim();
				importFldr.IsDropDestination = chkDropDestination.IsChecked.Value ? 1 : 0;
				importFldr.IsDropSource = chkDropSource.IsChecked.Value ? 1 : 0;
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
				txtImportFolderName.Text = importFldr.ImportFolderName;
				txtLocalPath.Text = importFldr.LocalPath;
				chkDropDestination.IsChecked = importFldr.IsDropDestination == 1;
				chkDropSource.IsChecked = importFldr.IsDropSource == 1;

				txtImportFolderName.Focus();
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;

namespace JMMClient.ViewModel
{
	public class ImportFolderVM
	{
		public int? ImportFolderID { get; set; }
		public string ImportFolderName { get; set; }
		public string ImportFolderLocation { get; set; }
		public int IsDropSource { get; set; }
		public int IsDropDestination { get; set; }
		public string LocalPathTemp { get; set; }

		public string LocalPath
		{
			get
			{
				if (ImportFolderID.HasValue)
				{
					if (AppSettings.ImportFolderMappings.ContainsKey(ImportFolderID.Value))
						return AppSettings.ImportFolderMappings[ImportFolderID.Value];
					else
						return ImportFolderLocation;
				}
				else
					return LocalPathTemp;

			}
		}

		public bool LocalPathIsValid
		{
			get
			{
				try
				{
					if (string.IsNullOrEmpty(LocalPath)) return false;

					return Directory.Exists(LocalPath);
				}
				catch { }

				return false;
			}
		}

		public bool LocalPathIsInvalid
		{
			get
			{
				return !LocalPathIsValid;
			}
		}

		public bool FolderIsDropSource
		{
			get
			{
				return IsDropSource == 1;
			}
		}

		public bool FolderIsDropDestination
		{
			get
			{
				return IsDropDestination == 1;
			}
		}

		public ImportFolderVM()
		{
		}

		

		public ImportFolderVM(JMMServerBinary.Contract_ImportFolder contract)
		{
			// read only members
			this.ImportFolderID = contract.ImportFolderID;
			this.ImportFolderName = contract.ImportFolderName;
			this.ImportFolderLocation = contract.ImportFolderLocation;
			this.IsDropSource = contract.IsDropSource;
			this.IsDropDestination = contract.IsDropDestination;
		}

		public JMMServerBinary.Contract_ImportFolder ToContract()
		{
			JMMServerBinary.Contract_ImportFolder contract = new JMMServerBinary.Contract_ImportFolder();
			contract.ImportFolderID = this.ImportFolderID;
			contract.ImportFolderName = this.ImportFolderName;
			contract.ImportFolderLocation = this.ImportFolderLocation;
			contract.IsDropSource = this.IsDropSource;
			contract.IsDropDestination = this.IsDropDestination;

			return contract;
		}

		public bool Save()
		{
			try
			{
				JMMServerBinary.Contract_ImportFolder_SaveResponse response = JMMServerVM.Instance.clientBinaryHTTP.SaveImportFolder(this.ToContract());
				if (!string.IsNullOrEmpty(response.ErrorMessage))
				{
					MessageBox.Show(response.ErrorMessage);
					return false;
				}

				AppSettings.SetImportFolderMapping(response.ImportFolder.ImportFolderID.Value, LocalPathTemp);

				return true;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			return false;
		}

		public void Delete()
		{
			try
			{

				string result = JMMServerVM.Instance.clientBinaryHTTP.DeleteImportFolder(ImportFolderID.Value);
				if (!string.IsNullOrEmpty(result))
					MessageBox.Show(result);
				else
					AppSettings.RemoveImportFolderMapping(ImportFolderID.Value);
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}
	}
}

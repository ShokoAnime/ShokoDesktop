using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Shoko.Commons;
using Shoko.Desktop.Utilities;
using Shoko.Models.Client;
using Shoko.Models.Server;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_ImportFolder : ImportFolder
    {
        public string LocalPathTemp { get; set; }

        public string LocalPath => CloudID.HasValue ? string.Empty : (ImportFolderID != 0 ? FolderMappings.Instance.GetMapping(ImportFolderID) : LocalPathTemp);

        public bool LocalPathIsValid => FolderMappings.Instance.IsValid(ImportFolderID);

        public bool IsCloud => CloudID.HasValue;

        public bool FolderIsWatched => IsWatched == 1;

        public bool FolderIsDropSource => IsDropSource == 1;

        public bool FolderIsDropDestination => IsDropDestination == 1;



        public BitmapImage Icon
        {
            get
            {
                VM_CloudAccount v = VM_ShokoServer.Instance.FolderProviders.FirstOrDefault(a => a.CloudID == (CloudID ?? 0));
                if (v != null)
                    return v.Bitmap;
                return new BitmapImage();
            }
        }


        public bool Save()
        {
            try
            {
                CL_Response<ImportFolder> response = VM_ShokoServer.Instance.ShokoServices.SaveImportFolder(this);
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    MessageBox.Show(response.ErrorMessage);
                    return false;
                }
                FolderMappings.Instance.MapFolder(response.Result.ImportFolderID, LocalPathTemp);

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

                string result = VM_ShokoServer.Instance.ShokoServices.DeleteImportFolder(ImportFolderID);
                if (!string.IsNullOrEmpty(result))
                    MessageBox.Show(result);
                else
                    FolderMappings.Instance.UnMapFolder(ImportFolderID);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }
}

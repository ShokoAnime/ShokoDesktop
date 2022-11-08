using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.WPFHelpers;
using Shoko.Models.Client;
using Shoko.Models.Server;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_ImportFolder : ImportFolder
    {
        [JsonIgnore, XmlIgnore]
        public string LocalPathTemp { get; set; }

        [JsonIgnore, XmlIgnore]
        public string LocalPath => ImportFolderID != 0 ? FolderMappings.Instance.GetMapping(ImportFolderID) : LocalPathTemp;

        [JsonIgnore, XmlIgnore]
        public bool LocalPathIsValid => FolderMappings.Instance.IsValid(this);

        [JsonIgnore, XmlIgnore]
        public bool FolderIsWatched => IsWatched == 1;

        [JsonIgnore, XmlIgnore]
        public bool FolderIsDropSource => IsDropSource == 1;

        [JsonIgnore, XmlIgnore]
        public bool FolderIsDropDestination => IsDropDestination == 1;


        [JsonIgnore, XmlIgnore]
        public BitmapImage Icon
        {
            get
            {
                return new BitmapImage(new Uri(@"/ShokoDesktop;component/Images/16_folder.png", UriKind.Relative));
            }
        }


        public bool Save()
        {
            try
            {
                CL_Response<ImportFolder> response = VM_ShokoServer.Instance.ShokoServices.SaveImportFolder(this.ToContract());
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

        public ImportFolder ToContract()
        {
            if (ImportFolderID == 0) ImportFolderID = -1;
            var result = new ImportFolder
            {
                ImportFolderID = ImportFolderID,
                ImportFolderLocation = ImportFolderLocation,
                ImportFolderName = ImportFolderName,
                ImportFolderType = ImportFolderType,
                IsDropDestination = IsDropDestination,
                IsDropSource = IsDropSource,
                IsWatched = IsWatched
            };
            return result;
        }
    }
}

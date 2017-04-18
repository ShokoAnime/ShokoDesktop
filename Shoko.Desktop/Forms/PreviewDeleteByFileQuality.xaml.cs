using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System;
using Shoko.Desktop.ViewModel;
using Shoko.Models.Client;
using Shoko.Commons.Extensions;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for PreviewDeleteByFileQuality.xaml
    /// </summary>
    public partial class PreviewDeleteByFileQuality : Window
    {
        public List<string> FilesToDelete = new List<string>();
        private int UserID;

        public PreviewDeleteByFileQuality()
        {
            InitializeComponent();
        }

        public PreviewDeleteByFileQuality(List<CL_VideoLocal> files)
        {
            InitializeComponent();
            FilesToDelete = files.Select((a) =>
            {
                var place = a.Places.FirstOrDefault();
                if (place == null) return null;
                string path;
                try
                {
                    path = Path.Combine(place.ImportFolder.ImportFolderLocation, place.FilePath);
                } catch (Exception e)
                {
                    path = place.ImportFolder.ImportFolderLocation + " - " + place.FilePath;
                }
                return path;
            }).Where(a => a != null).OrderByNatural(a => a).ToList();
            lblInfo.Content = String.Format(Shoko.Commons.Properties.Resources.DeleteFiles_Confirm, files.Count);
            lstPreviewResults.ItemsSource = FilesToDelete;
        }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            if (VM_ShokoServer.Instance.ServerOnline) VM_ShokoServer.Instance.ShokoServices.DeleteMultipleFilesWithPreferences(VM_ShokoServer.Instance.CurrentUser.JMMUserID);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using JMMClient.JMMServerBinary;

namespace JMMClient.ViewModel
{
    public class CloudAccountVM
    {
        public string Provider { get; set; }
        public string Name { get; set; }
        public BitmapImage Icon { get; set; }
        public int? CloudID { get; set; }

        public CloudAccountVM(Contract_CloudAccount cc)
        {
            Icon=Application.Current.Dispatcher.Invoke(() =>
            {
                if (cc?.Icon == null)
                    return null;

                MemoryStream ms = new MemoryStream(cc.Icon);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage icon = new BitmapImage();
                icon.BeginInit();
                icon.StreamSource = ms;
                icon.EndInit();
                return icon;
            });
            Name = cc.Name;
            Provider = cc.Provider;
            CloudID = cc.CloudID;
        }
    }
}

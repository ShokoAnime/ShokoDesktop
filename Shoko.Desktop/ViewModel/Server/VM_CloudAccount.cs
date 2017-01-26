using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Shoko.Models.Client;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_CloudAccount : CL_CloudAccount
    {


        private BitmapImage _bitmap;

        public BitmapImage Bitmap
        {
            get
            {
                return _bitmap ?? (_bitmap = Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Icon == null)
                        return null;

                    MemoryStream ms = new MemoryStream(Icon);
                    ms.Seek(0, SeekOrigin.Begin);
                    BitmapImage icon = new BitmapImage();
                    icon.BeginInit();
                    icon.StreamSource = ms;
                    icon.EndInit();
                    return icon;
                }));
            }
        }
    }
}

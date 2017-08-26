using System;
using System.IO;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Utils;
using Shoko.Models.Client;
using Shoko.Models.Enums;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_CloudAccount : CL_CloudAccount
    {


        private BitmapImage _bitmap;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public BitmapImage Bitmap
        {
            get
            {
                return _bitmap ?? (_bitmap = Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Icon == null || Misc.GetImageFormat(Icon) == ImageFormatEnum.unknown)
                    {
                        return new BitmapImage(new Uri("/Images/LoadingError.png"));
                    }

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

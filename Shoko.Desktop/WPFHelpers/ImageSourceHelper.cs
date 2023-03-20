using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DevExpress.Xpf.Core.MvvmSample.Helpers;
using Shoko.Commons.Utils;
using Shoko.Models.Enums;

namespace Shoko.Desktop.WPFHelpers
{
    public static class ImageSourceHelper
    {
        public static readonly Uri UriLoadingError = new Uri("/Images/LoadingError.png", UriKind.Relative);

        public static ImageSource GetImageSource(Uri uri, Dispatcher dispatcher)
        {
            if (uri == null) return null;
            BitmapImage bi = null;
            BackgroundHelper.DoWithDispatcher(dispatcher, () =>
            {
                try
                {
                    bi = new BitmapImage(uri);
                }
                catch
                {
                    bi = new BitmapImage(UriLoadingError);
                }
            });
            return bi;
        }

        public static ImageSource GetImageSource(Stream stream, Dispatcher dispatcher)
        {
            if (stream == null) return null;
            BitmapImage bi = null;

            BackgroundHelper.DoWithDispatcher(dispatcher, () =>
            {
                try
                {
                    bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = stream;
                    bi.EndInit();
                }
                catch
                {
                    bi = new BitmapImage(UriLoadingError);
                }

            });
            return bi;
        }

        public static ImageSource GetImageSource(byte[] data, Dispatcher dispatcher)
        {
            // check that it is an image
            if (data != null && Misc.GetImageFormat(data) != null)
                return GetImageSource(new MemoryStream(data), dispatcher);
            var bi = new BitmapImage(UriLoadingError);
            return bi;
        }
    }
}

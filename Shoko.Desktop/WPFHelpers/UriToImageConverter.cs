using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using NLog;

namespace Shoko.Desktop.WPFHelpers
{
    public class UriToImageConverter : IValueConverter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is BitmapImage)
                return value;

            value = !string.IsNullOrEmpty(value as string) ? new Uri((string) value, UriKind.RelativeOrAbsolute) : null;

            BitmapImage bi;
            try
            {
                if (value == null)
                {
                    bi = new BitmapImage(new Uri("/Images/LoadingError.png", UriKind.Relative));
                    logger.Error($"Unable to load image - A null or empty path was used");
                    return bi;
                }
                bi = new BitmapImage((Uri)value);
            }
            catch
            {
                try
                {
                    bi = new BitmapImage(new Uri("/Images/LoadingError.png", UriKind.Relative));
                    logger.Error($"Unable to load {value} - It is not a valid image.");
                }
                catch
                {
                    bi = new BitmapImage(new Uri("/Images/blankposter.png"));
                    logger.Error("Unable to load LoadingError.png - It is not a valid image. Loading the fallback...fallback image");
                }
            }
            return bi;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}

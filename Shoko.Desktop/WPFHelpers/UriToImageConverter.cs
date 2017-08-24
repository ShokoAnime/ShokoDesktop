using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Shoko.Desktop.WPFHelpers
{
    public class UriToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string)
            {
                value = new Uri((string)value, UriKind.RelativeOrAbsolute);
            }

            BitmapImage bi;
            try
            {
                bi = new BitmapImage((Uri)value);
            }
            catch (Exception e)
            {
                bi = new BitmapImage(new Uri("/Images/LoadingError.png", UriKind.Relative));
            }
            return bi;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}

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
                value = new Uri((string)value);
            }

            if (!(value is Uri)) return null;

            BitmapImage bi = new BitmapImage();
            try
            {
                bi.BeginInit();
                bi.DecodePixelWidth = 200;
                bi.UriSource = (Uri)value;
                bi.EndInit();
            }
            catch (Exception e)
            {
                bi.BeginInit();
                bi.DecodePixelWidth = 200;
                bi.UriSource = new Uri("/Images/LoadingError.png");
                bi.EndInit();
            }
            return bi;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}

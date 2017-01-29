using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Shoko.Desktop.WPFHelpers
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class InverseBooleanToVisibilityConverter : IValueConverter
    {
        // Methods
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            if (value is bool)
            {
                flag = (bool)value;
            }
            else if (value is bool?)
            {
                bool? nullable = (bool?)value;
                flag = nullable.HasValue ? nullable.Value : false;
            }
            return (flag ? Visibility.Collapsed : Visibility.Visible);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => ((value is Visibility) && (((Visibility)value) == Visibility.Collapsed));
    }
    

}

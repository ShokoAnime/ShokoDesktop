using System;
using System.Globalization;
using System.Windows.Data;

namespace Shoko.Desktop.WPFHelpers
{
    public class StringFormatConverter : IValueConverter
    {
        public IValueConverter InnerConverter { get; set; }
        public object InnerConverterParameter { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (InnerConverter != null)
                value = InnerConverter.Convert(value, typeof(object), InnerConverterParameter, culture);
            return value == null ? string.Empty : string.Format((string)parameter, value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace JMMClient.Helpers
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

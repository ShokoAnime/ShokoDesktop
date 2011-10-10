using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace JMMClient
{
	public class UriToImageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
			{
				return null;
			}

			if (value is string)
			{
				value = new Uri((string)value);
			}

			if (value is Uri)
			{
				BitmapImage bi = new BitmapImage();
				bi.BeginInit();
				bi.DecodePixelWidth = 200;
				//bi.DecodePixelHeight = 60;                
				bi.UriSource = (Uri)value;
				bi.EndInit();
				return bi;
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}
}

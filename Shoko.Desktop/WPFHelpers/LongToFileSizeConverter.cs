using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using NLog;
using Shoko.Commons.Utils;

namespace Shoko.Desktop.WPFHelpers
{
    public class LongToFileSizeConverter : IValueConverter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            long filesize;
            if (value is int intsize) filesize = (long) intsize;
            else if (value is long size) filesize = size;
            else if (value is string stringSize) filesize = long.Parse(stringSize);
            else filesize = 0;
            
            // now we have long for sure, or you really gave it the wrong value
            string formatted = FormatFileSize(filesize);
            return formatted;
        }

        private static string FormatFileSize(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
                return "0" + " " + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = System.Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 2);
            return num.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " " + suf[place];
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}

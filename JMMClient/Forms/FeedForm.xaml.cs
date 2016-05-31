using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JMMClient.Forms
{
    /// <summary>
    /// Interaction logic for AboutForm.xaml
    /// </summary>
    public partial class FeedForm : Window
    {
        public FeedForm()
        {
            InitializeComponent();
        }

        private void lnkGoToArt_Click(object sender, RoutedEventArgs e)
        {
            String url = (sender as Hyperlink).Tag as String;

            if (String.IsNullOrWhiteSpace(url)) return;

            System.Diagnostics.Process.Start(url);
        }
    }

    public class HTMLEscapedCharactersConverter : IValueConverter
    {
        private static readonly char[] MapChars = { '\x091', '\x092', '\x093', '\x094' };

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var htmlText = value as string;
            if (!string.IsNullOrEmpty(htmlText))
            {
                htmlText = System.Net.WebUtility.HtmlDecode(htmlText);
                if (htmlText.IndexOfAny(MapChars) > 0)
                {
                    var decodedText = new StringBuilder(htmlText.Length);
                    foreach (var ch in htmlText)
                        switch (ch)
                        {
                            // Windows Code page 1252: http://en.wikipedia.org/wiki/Windows-1252 
                            case '\x091':
                                decodedText.Append('\x2018');
                                break;

                            case '\x092':
                                decodedText.Append('\x2019');
                                break;

                            case '\x093':
                                decodedText.Append('\x201C');
                                break;

                            case '\x094':
                                decodedText.Append('\x201D');
                                break;

                            default:
                                decodedText.Append(ch);
                                break;
                        }
                    return decodedText.ToString();
                }
            }

            return htmlText ?? String.Empty;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            if (s != null)
            {
                s = WebUtility.HtmlEncode(s);
            }

            return s ?? String.Empty;
        }
    }
}
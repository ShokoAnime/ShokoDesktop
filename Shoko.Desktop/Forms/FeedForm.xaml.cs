using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shoko.Desktop.Utilities;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for AboutForm.xaml
    /// </summary>
    public partial class FeedForm : Window
    {
        private readonly List<newsEntry> _parsedNews = new List<newsEntry>();

        public class newsEntry
        {
            public newsEntry(JToken title, JToken contentTxt, JToken url, JToken pubDate)
            {
                this.Title = title.ToString();
                this.ContentTXT = contentTxt.ToString();
                this.URL = url.ToString();
            }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("content_txt")]
            public string ContentTXT { get; set; }

            [JsonProperty("url")]
            public string URL { get; set; }

            [JsonProperty("pubDate")]
            public string pubDate { get; set; }
        }

        public List<newsEntry> ParsedNews => _parsedNews;

        public FeedForm()
        {

            InitializeComponent();

            var w = new WebClient();
            var obj = JObject.Parse(w.DownloadString("https://shokoanime.com/jsonfeed/index.json"));

            foreach (var news in obj["items"])
            {
                ParsedNews.Add(new newsEntry(news["title"], news["content_text"], news["url"], news["date_published"]));
            }

            DataContext = this;
            lstItems.ItemsSource = ParsedNews;
        }

        private void lnkGoToArt_Click(object sender, RoutedEventArgs e)
        {
            String url = (sender as Hyperlink).Tag as String;

            if (String.IsNullOrWhiteSpace(url)) return;

            Utils.OpenUrl(url);
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
                htmlText = WebUtility.HtmlDecode(htmlText);
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

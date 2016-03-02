using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Globalization;
using System.Collections.Specialized;
using System.Configuration;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for SeriesSearchControl.xaml
    /// </summary>
    public partial class SeriesSearchControl : UserControl
    {


        public SeriesSearchControl()
        {
            InitializeComponent();

            //AllSeries = new ObservableCollection<AnimeSeriesVM>();

            btnClearSearch.Click += new RoutedEventHandler(btnClearSearch_Click);
            txtSeriesSearch.TextChanged += new TextChangedEventHandler(txtSeriesSearch_TextChanged);

            this.GotFocus += new RoutedEventHandler(SeriesSearchControl_GotFocus);

            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            string cult = appSettings["Culture"];
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cult);

            cboSearchType.Items.Add(JMMClient.Properties.Resources.Search_TitleOnly);
            cboSearchType.Items.Add(JMMClient.Properties.Resources.Search_Everything);
            cboSearchType.SelectedIndex = 0;
            cboSearchType.SelectionChanged += new SelectionChangedEventHandler(cboSearchType_SelectionChanged);
        }

        void cboSearchType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cboSearchType.SelectedIndex)
            {
                case 0: MainListHelperVM.Instance.SerSearchType = SeriesSearchType.TitleOnly; break;
                case 1: MainListHelperVM.Instance.SerSearchType = SeriesSearchType.Everything; break;
                default: MainListHelperVM.Instance.SerSearchType = SeriesSearchType.TitleOnly; break;
            }
            MainListHelperVM.Instance.SearchResultCount = 0;
            MainListHelperVM.Instance.ViewSeriesSearch.Refresh();
        }

        void SeriesSearchControl_GotFocus(object sender, RoutedEventArgs e)
        {
            //txtSeriesSearch.Focus();
        }

        void txtSeriesSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSeriesSearch.Text = "";
        }
    }
}

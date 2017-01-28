using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Shoko.Models.Enums;
using Shoko.Desktop.ViewModel;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for SeriesSearchControl.xaml
    /// </summary>
    public partial class SeriesSearchControl : UserControl
    {


        public SeriesSearchControl()
        {
            InitializeComponent();

            //AllSeries = new ObservableCollection<VM_AnimeSeries_User>();

            btnClearSearch.Click += new RoutedEventHandler(btnClearSearch_Click);
            txtSeriesSearch.TextChanged += new TextChangedEventHandler(txtSeriesSearch_TextChanged);

            GotFocus += new RoutedEventHandler(SeriesSearchControl_GotFocus);

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);


            cboSearchType.Items.Add(Shoko.Commons.Properties.Resources.Search_TitleOnly);
            cboSearchType.Items.Add(Shoko.Commons.Properties.Resources.Search_Everything);
            cboSearchType.SelectedIndex = 0;
            cboSearchType.SelectionChanged += new SelectionChangedEventHandler(cboSearchType_SelectionChanged);
        }

        void cboSearchType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cboSearchType.SelectedIndex)
            {
                case 0: VM_MainListHelper.Instance.SerSearchType = SeriesSearchType.TitleOnly; break;
                case 1: VM_MainListHelper.Instance.SerSearchType = SeriesSearchType.Everything; break;
                default: VM_MainListHelper.Instance.SerSearchType = SeriesSearchType.TitleOnly; break;
            }
            VM_MainListHelper.Instance.SearchResultCount = 0;
            VM_MainListHelper.Instance.ViewSeriesSearch.Refresh();
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

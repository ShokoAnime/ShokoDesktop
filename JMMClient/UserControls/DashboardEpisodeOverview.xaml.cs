using System.Windows;
using System.Windows.Controls;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for DashboardEpisodeOverview.xaml
    /// </summary>
    public partial class DashboardEpisodeOverview : UserControl
    {
        public DashboardEpisodeOverview()
        {
            InitializeComponent();
        }

        private void ImageBorder_Loaded(object sender, RoutedEventArgs e)
        {
            Border border = (Border)sender;
            Thickness borderThickness = border.BorderThickness;

            if (border.ActualWidth <= borderThickness.Left + borderThickness.Right ||
                border.ActualHeight <= borderThickness.Top + borderThickness.Bottom)
            { // When the border contains no content, hide the border so that an empty border isn't rendered
                border.Visibility = Visibility.Hidden;
            }
        }
    }
}
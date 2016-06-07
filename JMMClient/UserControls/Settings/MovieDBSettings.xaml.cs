using System.Windows;
using System.Windows.Controls;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for MovieDBSettings.xaml
    /// </summary>
    public partial class MovieDBSettings : UserControl
    {
        public MovieDBSettings()
        {
            InitializeComponent();

            chkMovieDB_FanartAutoDownload.Click += new RoutedEventHandler(settingChanged);
            chkMovieDB_PosterAutoDownload.Click += new RoutedEventHandler(settingChanged);
            udMaxFanarts.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udMaxFanarts_ValueChanged);
            udMaxPosters.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udMaxPosters_ValueChanged);
        }

        void udMaxPosters_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            JMMServerVM.Instance.MovieDB_AutoPostersAmount = udMaxPosters.Value.Value;
            JMMServerVM.Instance.SaveServerSettingsAsync();
        }

        void udMaxFanarts_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            JMMServerVM.Instance.MovieDB_AutoFanartAmount = udMaxFanarts.Value.Value;
            JMMServerVM.Instance.SaveServerSettingsAsync();
        }


        void settingChanged(object sender, RoutedEventArgs e)
        {
            JMMServerVM.Instance.SaveServerSettingsAsync();
        }
    }
}

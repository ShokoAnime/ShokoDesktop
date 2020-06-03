using System;
using System.Windows;
using System.Windows.Controls;
using Shoko.Desktop.ViewModel;

namespace Shoko.Desktop.UserControls.Settings
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
            udMaxFanarts.ValueChanged += new EventHandler<DependencyPropertyChangedEventArgs>(udMaxFanarts_ValueChanged);
            udMaxPosters.ValueChanged += new EventHandler<DependencyPropertyChangedEventArgs>(udMaxPosters_ValueChanged);
        }

        void udMaxPosters_ValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            VM_ShokoServer.Instance.MovieDB_AutoPostersAmount = udMaxPosters.Value;
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        void udMaxFanarts_ValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            VM_ShokoServer.Instance.MovieDB_AutoFanartAmount = udMaxFanarts.Value;
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }


        void settingChanged(object sender, RoutedEventArgs e)
        {
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }
    }
}

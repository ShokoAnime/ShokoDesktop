using System;
using System.Windows;
using System.Windows.Controls;
using Shoko.Desktop.ViewModel;

namespace Shoko.Desktop.UserControls.Settings
{
    /// <summary>
    /// Interaction logic for DisplayImageSizes.xaml
    /// </summary>
    public partial class DisplayImageSizesSettings : UserControl
    {


        public DisplayImageSizesSettings()
        {
            InitializeComponent();

            udImageSize_GroupList.ValueChanged += new EventHandler<DependencyPropertyChangedEventArgs>(udImageSize_GroupList_ValueChanged);
            udImageSize_SeriesPoster.ValueChanged += new EventHandler<DependencyPropertyChangedEventArgs>(udImageSize_SeriesPoster_ValueChanged);
            udImageSize_EpisodeImage.ValueChanged += new EventHandler<DependencyPropertyChangedEventArgs>(udImageSize_EpisodeImage_ValueChanged);
        }

        void udImageSize_EpisodeImage_ValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            VM_UserSettings.Instance.DisplayWidth_EpisodeImage = udImageSize_EpisodeImage.Value;
        }

        void udImageSize_SeriesPoster_ValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            VM_UserSettings.Instance.DisplayHeight_SeriesInfo = udImageSize_SeriesPoster.Value;
        }

        void udImageSize_GroupList_ValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            VM_UserSettings.Instance.DisplayHeight_GroupList = udImageSize_GroupList.Value;
        }
    }
}

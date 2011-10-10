using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for DisplayImageSizes.xaml
	/// </summary>
	public partial class DisplayImageSizesSettings : UserControl
	{
		

		public DisplayImageSizesSettings()
		{
			InitializeComponent();

			udImageSize_GroupList.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udImageSize_GroupList_ValueChanged);
			udImageSize_SeriesPoster.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udImageSize_SeriesPoster_ValueChanged);
			udImageSize_EpisodeImage.ValueChanged += new RoutedPropertyChangedEventHandler<object>(udImageSize_EpisodeImage_ValueChanged);
		}

		void udImageSize_EpisodeImage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			UserSettingsVM.Instance.DisplayWidth_EpisodeImage = udImageSize_EpisodeImage.Value.Value;
		}

		void udImageSize_SeriesPoster_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			UserSettingsVM.Instance.DisplayHeight_SeriesInfo = udImageSize_SeriesPoster.Value.Value;
		}

		void udImageSize_GroupList_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			UserSettingsVM.Instance.DisplayHeight_GroupList = udImageSize_GroupList.Value.Value;
		}
	}
}

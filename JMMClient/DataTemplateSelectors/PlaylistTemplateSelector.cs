using JMMClient.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace JMMClient
{
    public class PlaylistTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                if (item.GetType() == typeof(PlaylistVM))
                {
                    return element.FindResource("Playlist_PlaylistTemplate") as DataTemplate;
                }

                if (item.GetType() == typeof(PlaylistItemVM))
                {
                    PlaylistItemVM pli = item as PlaylistItemVM;


                    if (pli.ItemType == PlaylistItemType.AnimeSeries) return element.FindResource("Playlist_AnimeSeriesTemplate") as DataTemplate;
                    if (pli.ItemType == PlaylistItemType.Episode) return element.FindResource("Playlist_AnimeEpisodeTemplate") as DataTemplate;
                }

                /*if (item.GetType() == typeof(AnimeSeriesVM))
				{
					return element.FindResource("Playlist_AnimeSeriesTemplate") as DataTemplate;
				}
				
				if (item.GetType() == typeof(AnimeEpisodeVM))
				{
					return element.FindResource("Playlist_AnimeEpisodeTemplate") as DataTemplate;
				}*/
            }

            return null;
        }
    }
}

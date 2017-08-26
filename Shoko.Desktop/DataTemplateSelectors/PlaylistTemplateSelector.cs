using System.Windows;
using System.Windows.Controls;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Enums;

namespace Shoko.Desktop.DataTemplateSelectors
{
    public class PlaylistTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                if (item.GetType() == typeof(VM_Playlist))
                {
                    return element.FindResource("Playlist_PlaylistTemplate") as DataTemplate;
                }

                if (item.GetType() == typeof(VM_PlaylistItem))
                {
                    VM_PlaylistItem pli = item as VM_PlaylistItem;


                    if (pli.ItemType == PlaylistItemType.AnimeSeries) return element.FindResource("Playlist_AnimeSeriesTemplate") as DataTemplate;
                    if (pli.ItemType == PlaylistItemType.Episode) return element.FindResource("Playlist_AnimeEpisodeTemplate") as DataTemplate;
                }
            }

            return null;
        }
    }
}

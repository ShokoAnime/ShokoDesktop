using System.Windows;
using System.Windows.Controls;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.DataTemplateSelectors
{
    public class WatchingEpisodeTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                if (item.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    VM_AnimeEpisode_User ep = item as VM_AnimeEpisode_User;

                    if (ep.EpisodeOrder == 0)
                        return element.FindResource("AnimeEpisodePrevious") as DataTemplate;

                    if (ep.EpisodeOrder == 1)
                        return element.FindResource("AnimeEpisodeCurrent") as DataTemplate;

                    return element.FindResource("AnimeEpisodeOther") as DataTemplate;
                }


            }

            return null;
        }
    }
}

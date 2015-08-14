using JMMClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace JMMClient
{
    public class WatchingEpisodeTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                if (item.GetType() == typeof(AnimeEpisodeDisplayVM))
                {
                    AnimeEpisodeDisplayVM ep = item as AnimeEpisodeDisplayVM;

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

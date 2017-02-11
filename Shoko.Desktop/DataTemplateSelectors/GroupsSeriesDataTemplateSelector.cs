using System.Windows;
using System.Windows.Controls;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.DataTemplateSelectors
{
    public class GroupsSeriesDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                if (item.GetType() == typeof(VM_GroupFilter))
                {
                    return element.FindResource("groupFilterDetailTemplate") as DataTemplate;
                }
                if (item.GetType() == typeof(VM_AnimeGroup_User))
                {
                    return element.FindResource("groupDetailTemplate") as DataTemplate;
                }
                if (item.GetType() == typeof(VM_AnimeSeries_User))
                {
                    return element.FindResource("seriesDetailTemplate") as DataTemplate;
                    //return element.FindResource("seriesDetailHuluTemplate") as DataTemplate;
                }
                if (item.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    return element.FindResource("episodeDetailTemplate") as DataTemplate;
                }
                if (item.GetType() == typeof(VM_VideoDetailed))
                {
                    return element.FindResource("videoDetailTemplate") as DataTemplate;
                }
            }

            return null;
        }
    }
}

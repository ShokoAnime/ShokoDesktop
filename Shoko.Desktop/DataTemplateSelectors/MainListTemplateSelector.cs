using System.Windows;
using System.Windows.Controls;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.DataTemplateSelectors
{
    public class MainListTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                if (item.GetType() == typeof(VM_GroupFilter))
                {
                    return element.FindResource("MainList_GroupFilterTemplate") as DataTemplate;
                }
                if (item.GetType() == typeof(VM_AnimeGroup_User))
                {
                    switch (AppSettings.DisplayStyle_GroupList)
                    {
                        case 1: return element.FindResource("MainList_AnimeGroupTemplate") as DataTemplate;
                        case 2: return element.FindResource("MainList_AnimeGroupSimpleTemplate") as DataTemplate;
                        default: return element.FindResource("MainList_AnimeGroupTemplate") as DataTemplate;
                    }
                }
                if (item.GetType() == typeof(VM_AnimeSeries_User))
                {
                    return element.FindResource("MainList_AnimeSeriesTemplate") as DataTemplate;
                }
                if (item.GetType() == typeof(VM_AnimeEpisodeType))
                {
                    return element.FindResource("MainList_AnimeEpisodeTypeTemplate") as DataTemplate;
                }
                if (item.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    return element.FindResource("MainList_AnimeEpisodeTemplate") as DataTemplate;
                }
                if (item.GetType() == typeof(VM_VideoDetailed))
                {
                    return element.FindResource("MainList_VideoDetailedTemplate") as DataTemplate;
                }
            }

            return null;
        }
    }


}

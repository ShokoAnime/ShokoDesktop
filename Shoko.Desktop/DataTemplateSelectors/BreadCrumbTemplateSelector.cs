using System.Windows;
using System.Windows.Controls;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.DataTemplateSelectors
{
    public class BreadCrumbTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null)
            {
                if (item == null)
                {
                    return element.FindResource("BreadCrumb_TopViewTemplate") as DataTemplate;
                }
                if (item.GetType() == typeof(VM_GroupFilter))
                {
                    return element.FindResource("BreadCrumb_GroupFilterTemplate") as DataTemplate;
                }
                if (item.GetType() == typeof(VM_AnimeGroup_User))
                {
                    return element.FindResource("BreadCrumb_GroupTemplate") as DataTemplate;
                }
                if (item.GetType() == typeof(VM_AnimeSeries_User))
                {
                    return element.FindResource("BreadCrumb_SeriesTemplate") as DataTemplate;
                }
                if (item.GetType() == typeof(VM_AnimeEpisodeType))
                {
                    return element.FindResource("BreadCrumb_EpisodeTypeTemplate") as DataTemplate;
                }
                if (item.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    return element.FindResource("BreadCrumb_EpisodeTemplate") as DataTemplate;
                }
            }

            return null;
        }
    }
}

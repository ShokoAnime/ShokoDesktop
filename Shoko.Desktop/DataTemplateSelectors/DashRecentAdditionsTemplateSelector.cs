using System.Windows;
using System.Windows.Controls;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.DataTemplateSelectors
{
    public class DashRecentAdditionsTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                if (item.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    return element.FindResource("RecentAdditionsEpisode_Detailed") as DataTemplate; // re-use this template
                }
                if (item.GetType() == typeof(VM_AnimeSeries_User))
                {
                    return element.FindResource("RecentAdditionsSeries_Detailed") as DataTemplate;
                }

            }

            return null;
        }
    }
}

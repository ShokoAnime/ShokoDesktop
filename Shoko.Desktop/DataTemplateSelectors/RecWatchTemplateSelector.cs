using System.Windows;
using System.Windows.Controls;
using Shoko.Desktop.UserControls;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.DataTemplateSelectors
{
    public class RecWatchTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                if (item.GetType() == typeof(VM_Recommendation))
                {
                    return element.FindResource("RecWatch_Detailed") as DataTemplate;
                }
                if (item.GetType() == typeof(SyncVotesDummy))
                {
                    return element.FindResource("MylistVotesDownloadTemplate") as DataTemplate;
                }
            }

            return null;
        }
    }
}

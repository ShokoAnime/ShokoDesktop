using System.Windows;
using System.Windows.Controls;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.DataTemplateSelectors
{
    public class PeopleSayingTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                if (item.GetType() == typeof(VM_Trakt_CommentUser))
                    return element.FindResource("TraktCommentTemplate") as DataTemplate;

                if (item.GetType() == typeof(VM_AniDB_Recommendation))
                    return element.FindResource("AniDBCommentTemplate") as DataTemplate;

            }

            return null;
        }
    }
}

using JMMClient.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace JMMClient
{
    public class PeopleSayingTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                if (item.GetType() == typeof(Trakt_CommentUserVM))
                    return element.FindResource("TraktCommentTemplate") as DataTemplate;

                if (item.GetType() == typeof(AniDB_RecommendationVM))
                    return element.FindResource("AniDBCommentTemplate") as DataTemplate;

            }

            return null;
        }
    }
}

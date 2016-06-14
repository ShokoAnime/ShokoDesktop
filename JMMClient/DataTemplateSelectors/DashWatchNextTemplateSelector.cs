using System.Windows;
using System.Windows.Controls;

namespace JMMClient
{
    public class DashWatchNextTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                switch (AppSettings.Dash_WatchNext_Style)
                {
                    case DashWatchNextStyle.Simple:
                        return element.FindResource("WatchNextTemplate") as DataTemplate;
                    case DashWatchNextStyle.Detailed:
                        return element.FindResource("WatchNextTemplate_Detailed") as DataTemplate;
                    default:
                        return element.FindResource("WatchNextTemplate") as DataTemplate;
                }
            }

            return null;
        }
    }
}

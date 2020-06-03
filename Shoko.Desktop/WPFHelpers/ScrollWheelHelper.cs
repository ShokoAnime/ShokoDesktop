using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace Shoko.Desktop.WPFHelpers
{
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property |
        AttributeTargets.Delegate | AttributeTargets.Field | AttributeTargets.Event |
        AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.GenericParameter)]
    public sealed class CanBeNullAttribute : Attribute { }

    public static class ScrollWheelHelper
    {
        public static readonly DependencyProperty WheelScrollsHorizontallyProperty
            = DependencyProperty.RegisterAttached("WheelScrollsHorizontally",
                typeof(bool),
                typeof(ScrollWheelHelper),
                new PropertyMetadata(false, UseHorizontalScrollingChangedCallback));

        private static void UseHorizontalScrollingChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as UIElement;

            if (element == null)
                throw new Exception("Attached property must be used with UIElement.");

            if ((bool)e.NewValue)
                element.PreviewMouseWheel += OnPreviewMouseWheel;
            else
                element.PreviewMouseWheel -= OnPreviewMouseWheel;
        }

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs args)
        {
            var scrollViewer = ((UIElement)sender).FindDescendant<ScrollViewer>();

            if (scrollViewer == null)
                return;

            if (args.Delta < 0)
                for (int i = 0; i < 3; i++) scrollViewer.LineRight();
            else
                for (int i = 0; i < 3; i++) scrollViewer.LineLeft();

            args.Handled = true;
        }

        public static void SetWheelScrollsHorizontally(ScrollViewer element, bool value) => element.SetValue(WheelScrollsHorizontallyProperty, value);
        public static bool GetWheelScrollsHorizontally(ScrollViewer element) => (bool)element.GetValue(WheelScrollsHorizontallyProperty);

        [CanBeNull]
        private static T FindDescendant<T>([CanBeNull] this DependencyObject d) where T : DependencyObject
        {
            if (d == null)
                return null;

            if (d is T a) return a;

            var childCount = VisualTreeHelper.GetChildrenCount(d);

            for (var i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(d, i);

                var result = child as T ?? FindDescendant<T>(child);

                if (result != null)
                    return result;
            }

            return null;
        }
    }
}

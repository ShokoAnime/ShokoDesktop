using System.Windows;
using System.Windows.Controls;

namespace JMMClient
{
    public class CustomToolTip : ToolTip
    {
        public static readonly DependencyProperty HasRoundCornersProperty =
           DependencyProperty.Register("HasRoundCorners", typeof(bool), typeof(CustomToolTip), new FrameworkPropertyMetadata(true));

        static CustomToolTip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomToolTip), new FrameworkPropertyMetadata(typeof(CustomToolTip)));
        }

        public bool HasRoundCorners
        {
            get { return (bool)GetValue(HasRoundCornersProperty); }
            set { SetValue(HasRoundCornersProperty, value); }
        }
    }
}
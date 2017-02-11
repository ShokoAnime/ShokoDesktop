using System.Windows;
using System.Windows.Controls;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for DuplicateFileDetailControl.xaml
    /// </summary>
    public partial class DuplicateFileDetailControl : UserControl
    {
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded",
            typeof(bool), typeof(DuplicateFileDetailControl), new UIPropertyMetadata(false, isExpandedCallback));

        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register("IsCollapsed",
            typeof(bool), typeof(DuplicateFileDetailControl), new UIPropertyMetadata(true, isCollapsedCallback));

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }

        private static void isExpandedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //EpisodeDetail input = (EpisodeDetail)d;
            //input.tbTest.Text = e.NewValue as string;
        }

        private static void isCollapsedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //EpisodeDetail input = (EpisodeDetail)d;
            //input.tbTest.Text = e.NewValue as string;
        }

        public DuplicateFileDetailControl()
        {
            InitializeComponent();

            btnToggleExpander.Click += new RoutedEventHandler(btnToggleExpander_Click);
        }

        void btnToggleExpander_Click(object sender, RoutedEventArgs e)
        {
            IsExpanded = !IsExpanded;
            IsCollapsed = !IsCollapsed;
        }
    }
}

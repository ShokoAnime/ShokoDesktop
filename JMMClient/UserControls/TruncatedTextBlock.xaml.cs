using System;
using System.Windows;
using System.Windows.Controls;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for TruncatedTextBlock.xaml
    /// </summary>
    public partial class TruncatedTextBlock : UserControl
    {
        public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register("DisplayText",
            typeof(string), typeof(TruncatedTextBlock), new UIPropertyMetadata("", displayTextChangedCallback));

        public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register("MaxLength",
            typeof(int), typeof(TruncatedTextBlock), new UIPropertyMetadata(-1, maxLengthChangedCallback));

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded",
            typeof(bool), typeof(TruncatedTextBlock), new UIPropertyMetadata(false, isExpandedCallback));

        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register("IsCollapsed",
            typeof(bool), typeof(TruncatedTextBlock), new UIPropertyMetadata(true, isCollapsedCallback));

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
            TruncatedTextBlock input = (TruncatedTextBlock)d;
            //input.tbTest.Text = e.NewValue as string;
        }

        private static void isCollapsedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TruncatedTextBlock input = (TruncatedTextBlock)d;
            //input.tbTest.Text = e.NewValue as string;
        }

        private static string displayText = "";
        private static int maxLength = -1;

        public string DisplayText
        {
            get { return (string)GetValue(DisplayTextProperty); }
            set { SetValue(DisplayTextProperty, value); }
        }

        private static void displayTextChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TruncatedTextBlock input = (TruncatedTextBlock)d;
            displayText = e.NewValue as string;
            SetValues(displayText, maxLength, input);
        }

        public int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }

        private static void maxLengthChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TruncatedTextBlock input = (TruncatedTextBlock)d;
            maxLength = int.Parse(e.NewValue.ToString());
            SetValues(displayText, maxLength, input);
        }

        private static void SetValues(string text, int length, TruncatedTextBlock input)
        {
            Console.Write("");

            string newText = text;
            if (newText.Length > length && length > 0)
            {
                newText = newText.Substring(0, length) + " ...";
            }

            input.txtTText.Text = newText;
            input.txtTTextFull.Text = text;
        }

        public TruncatedTextBlock()
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

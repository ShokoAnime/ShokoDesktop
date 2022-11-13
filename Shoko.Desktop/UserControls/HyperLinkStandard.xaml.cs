using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NLog;
using Shoko.Desktop.Utilities;

namespace Shoko.Desktop.UserControls
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    /// Interaction logic for HyperLinkStandard.xaml
    /// </summary>
    public partial class HyperLinkStandard
    {
        public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register("DisplayText",
            typeof(string), typeof(HyperLinkStandard), new UIPropertyMetadata("", displayTextChangedCallback));

        public static readonly DependencyProperty URLProperty = DependencyProperty.Register("URL",
            typeof(string), typeof(HyperLinkStandard), new UIPropertyMetadata("", urlChangedCallback));

        public static readonly DependencyProperty ForegroundOverrideProperty = DependencyProperty.Register("ForegroundOverride",
            typeof(string), typeof(HyperLinkStandard), new UIPropertyMetadata("", foregroundOverrideChangedCallback));

        public string ForegroundOverride
        {
            get => (string)GetValue(ForegroundOverrideProperty);
            set => SetValue(ForegroundOverrideProperty, value);
        }

        public string DisplayText
        {
            get => (string)GetValue(DisplayTextProperty);
            set => SetValue(DisplayTextProperty, value);
        }

        private static void displayTextChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HyperLinkStandard input = (HyperLinkStandard)d;
            input.txtLink.Text = e.NewValue as string;
        }

        private static void foregroundOverrideChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bc = new BrushConverter();
            HyperLinkStandard input = (HyperLinkStandard)d;
            input.txtLink.Foreground = (Brush)bc.ConvertFrom(e.NewValue as string);
            input.Foreground = (Brush)bc.ConvertFrom(e.NewValue as string);
        }

        public string URL
        {
            get => (string)GetValue(URLProperty);
            set => SetValue(URLProperty, value);
        }

        private static void urlChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //HyperLinkStandard input = (HyperLinkStandard)d;

        }

        public HyperLinkStandard()
        {
            InitializeComponent();

            hlURL.Click += hlURL_Click;
        }

        void hlURL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Uri uri = new Uri(URL);

                Utils.OpenUrl(uri.AbsoluteUri);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($"Unable to open hyperlink: {ex}");
            }

            e.Handled = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for HyperLinkStandard.xaml
	/// </summary>
	public partial class HyperLinkStandard : UserControl
	{
		public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register("DisplayText",
			typeof(string), typeof(HyperLinkStandard), new UIPropertyMetadata("", displayTextChangedCallback));

		public static readonly DependencyProperty URLProperty = DependencyProperty.Register("URL",
			typeof(string), typeof(HyperLinkStandard), new UIPropertyMetadata("", urlChangedCallback));

		public static readonly DependencyProperty ForegroundOverrideProperty = DependencyProperty.Register("ForegroundOverride",
			typeof(string), typeof(HyperLinkStandard), new UIPropertyMetadata("", foregroundOverrideChangedCallback));

		public string ForegroundOverride
		{
			get { return (string)GetValue(ForegroundOverrideProperty); }
			set { SetValue(ForegroundOverrideProperty, value); }
		}

		public string DisplayText
		{
			get { return (string)GetValue(DisplayTextProperty); }
			set { SetValue(DisplayTextProperty, value); }
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
			get { return (string)GetValue(URLProperty); }
			set { SetValue(URLProperty, value); }
		}

		private static void urlChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			//HyperLinkStandard input = (HyperLinkStandard)d;
			
		}

		public HyperLinkStandard()
		{
			InitializeComponent();

			hlURL.Click += new RoutedEventHandler(hlURL_Click);
		}

		void hlURL_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Uri uri = new Uri(URL);
				Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
			}
			catch { }

			e.Handled = true;
		}
	}
}

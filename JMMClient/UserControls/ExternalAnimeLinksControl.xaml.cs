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

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for ExternalAnimeLinksControl.xaml
	/// </summary>
	public partial class ExternalAnimeLinksControl : UserControl
	{
		public static readonly DependencyProperty LinkTypeProperty = DependencyProperty.Register("LinkType",
			typeof(int), typeof(ExternalAnimeLinksControl), new UIPropertyMetadata(1, linkTypeChangedCallback));

		public int LinkType
		{
			get { return (int)GetValue(LinkTypeProperty); }
			set 
			{ 
				SetValue(LinkTypeProperty, value);
				IsSiteLink = LinkType == 1;
				IsDiscussionLink = LinkType == 2;
			}
		}

		public static readonly DependencyProperty IsDiscussionLinkProperty = DependencyProperty.Register("IsDiscussionLink",
			typeof(bool), typeof(ExternalAnimeLinksControl), new UIPropertyMetadata(false, null));

		public bool IsDiscussionLink
		{
			get { return (bool)GetValue(IsDiscussionLinkProperty); }
			set { SetValue(IsDiscussionLinkProperty, value); }
		}

		public static readonly DependencyProperty IsSiteLinkProperty = DependencyProperty.Register("IsSiteLink",
			typeof(bool), typeof(ExternalAnimeLinksControl), new UIPropertyMetadata(false, null));

		public bool IsSiteLink
		{
			get { return (bool)GetValue(IsSiteLinkProperty); }
			set { SetValue(IsSiteLinkProperty, value); }
		}

		public ExternalAnimeLinksControl()
		{
			InitializeComponent();
		}

		private static void linkTypeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var bc = new BrushConverter();
			ExternalAnimeLinksControl input = (ExternalAnimeLinksControl)d;
			//input.txtLink.Foreground = (Brush)bc.ConvertFrom(e.NewValue as string);
			//input.Foreground = (Brush)bc.ConvertFrom(e.NewValue as string);

			if (e == null || e.NewValue == null) return;
			int linkType = int.Parse(e.NewValue.ToString());

			input.IsSiteLink = linkType == 1;
			input.IsDiscussionLink = linkType == 2;

		}
	}
}

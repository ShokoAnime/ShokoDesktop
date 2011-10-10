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

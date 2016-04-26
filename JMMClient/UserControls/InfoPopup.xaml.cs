using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
	/// Interaction logic for InfoPopup.xaml
	/// </summary>
	public partial class InfoPopup : UserControl
	{
		#region HeaderText dependency property

		/// <summary>
		/// An optional title to be displayed. May be omitted.
		/// </summary>
		public static readonly DependencyProperty HeaderTextProperty =
			DependencyProperty.Register("HeaderText",
										typeof(string),
										typeof(InfoPopup),
										new FrameworkPropertyMetadata("", HeaderTextPropertyChanged));

		/// <summary>
		/// A property wrapper for the <see cref="HeaderTextProperty"/>
		/// dependency property:<br/>
		/// An optional title to be displayed. May be omitted.
		/// </summary>
		public string HeaderText
		{
			get { return (string)GetValue(HeaderTextProperty); }
			set { SetValue(HeaderTextProperty, value); }
		}


		/// <summary>
		/// A static callback listener which is being invoked if the
		/// <see cref="HeaderTextProperty"/> dependency property has
		/// been changed. Invokes the <see cref="OnHeaderTextPropertyChanged"/>
		/// instance method of the changed instance.
		/// </summary>
		/// <param name="d">The currently processed owner of the property.</param>
		/// <param name="e">Provides information about the updated property.</param>
		private static void HeaderTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			InfoPopup owner = (InfoPopup)d;
			owner.OnHeaderTextPropertyChanged(e);
		}


		/// <summary>
		/// Handles changes of the <see cref="HeaderTextProperty"/> dependency property. As
		/// WPF internally uses the dependency property system and bypasses the
		/// <see cref="HeaderText"/> property wrapper, updates of the property's value
		/// should be handled here.
		/// </summary
		/// <param name="e">Provides information about the updated property.</param>
		private void OnHeaderTextPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			string newValue = (string)e.NewValue;
			txtTitle.Visibility = String.IsNullOrEmpty(newValue)
									  ? Visibility.Collapsed
									  : Visibility.Visible;
		}

		#endregion

		#region BodyText dependency property

		/// <summary>
		/// The information to be displayed through the popup.
		/// </summary>
		public static readonly DependencyProperty BodyTextProperty =
			DependencyProperty.Register("BodyText",
										typeof(string),
										typeof(InfoPopup),
										new FrameworkPropertyMetadata(""));

		/// <summary>
		/// A property wrapper for the <see cref="BodyTextProperty"/>
		/// dependency property:<br/>
		/// The information to be displayed through the popup.
		/// </summary>
		public string BodyText
		{
			get { return (string)GetValue(BodyTextProperty); }
			set { SetValue(BodyTextProperty, value); }
		}

		#endregion

		public InfoPopup()
		{
			InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);
        }
	}
}

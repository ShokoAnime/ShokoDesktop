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
using System.Windows.Shapes;

namespace JMMClient.Forms
{
	/// <summary>
	/// Interaction logic for DialogText.xaml
	/// </summary>
	public partial class DialogText : Window
	{
		public string EnteredText { get; set; }

		public DialogText()
		{
			InitializeComponent();

			btnOK.Click += new RoutedEventHandler(btnOK_Click);
			this.Loaded += new RoutedEventHandler(DialogText_Loaded);
		}

		void DialogText_Loaded(object sender, RoutedEventArgs e)
		{
			txtData.Focus();
		}

		void btnOK_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
			EnteredText = txtData.Text.Trim();
			this.Close();
		}

		public void Init(string prompt, string defaultText)
		{
			txtPrompt.Text = prompt;
			EnteredText = defaultText;
			txtData.Text = EnteredText;
		}
	}
}

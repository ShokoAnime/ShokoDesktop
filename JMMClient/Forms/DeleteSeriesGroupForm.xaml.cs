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
	/// Interaction logic for DeleteSeriesGroupForm.xaml
	/// </summary>
	public partial class DeleteSeriesGroupForm : Window
	{
		public bool DeleteFiles { get; set; }
		public bool DeleteGroups { get; set; }

		public DeleteSeriesGroupForm()
		{
			InitializeComponent();

			DeleteFiles = false;
			DeleteGroups = true;
			btnOK.Click += new RoutedEventHandler(btnOK_Click);
		}

		void btnOK_Click(object sender, RoutedEventArgs e)
		{
			DeleteFiles = chkDeleteFiles.IsChecked.Value;
			DeleteGroups = chkDeleteGroups.IsChecked.Value; 

			this.DialogResult = true;
			this.Close();
		}
	}
}

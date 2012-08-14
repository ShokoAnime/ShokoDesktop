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
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for UpdateAniDBDataControl.xaml
	/// </summary>
	public partial class UpdateAniDBDataControl : UserControl
	{
		public UpdateAniDBDataControl()
		{
			InitializeComponent();

			btnQueueCommands.Click += new RoutedEventHandler(btnQueueCommands_Click);
			btnEstimate.Click += new RoutedEventHandler(btnEstimate_Click);
		}

		private void Test()
		{


			XDocument xmlDoc = XDocument.Load(@"C:\Projects\SVN\JMM\1344911476-1630-10345\mylist.xml");

			var my_anime_list = from file in xmlDoc.Descendants("file")
							select new
							{
								FID = file.Element("FID").Value,
								State = file.Element("State").Value,
								VersionName = file.Element("VersionName").Value,
							};

			foreach (var file in my_anime_list)
			{
				Debug.WriteLine(file.FID);
			}

		}
		void btnEstimate_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Window wdw = Window.GetWindow(this);
				wdw.Cursor = Cursors.Wait;

				int filesQueued = JMMServerVM.Instance.clientBinaryHTTP.UpdateAniDBFileData(chkMissingInfo.IsChecked.Value, chkOutofDate.IsChecked.Value, true);

				MessageBox.Show(string.Format("{0} Files will be queued for processing", filesQueued), "Done", MessageBoxButton.OK, MessageBoxImage.Information);

				wdw.Cursor = Cursors.Arrow;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

		void btnQueueCommands_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Window wdw = Window.GetWindow(this);
				wdw.Cursor = Cursors.Wait;

				int filesQueued = JMMServerVM.Instance.clientBinaryHTTP.UpdateAniDBFileData(chkMissingInfo.IsChecked.Value, chkOutofDate.IsChecked.Value, false);

				MessageBox.Show(string.Format("{0} Files queued for processing", filesQueued), "Done", MessageBoxButton.OK, MessageBoxImage.Information);

				wdw.Cursor = Cursors.Arrow;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}
	}
}

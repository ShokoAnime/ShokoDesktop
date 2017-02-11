using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for UpdateAniDBDataControl.xaml
    /// </summary>
    public partial class UpdateAniDBDataControl : UserControl
    {
        public UpdateAniDBDataControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

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

                int filesQueued = VM_ShokoServer.Instance.ShokoServices.UpdateAniDBFileData(chkMissingInfo.IsChecked.Value, chkOutofDate.IsChecked.Value, true);

                MessageBox.Show(string.Format(Shoko.Commons.Properties.Resources.UpdateAniDB_QueueCount, filesQueued), Shoko.Commons.Properties.Resources.UpdateAniDB_UpdateAniDB, MessageBoxButton.OK, MessageBoxImage.Information);

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

                int filesQueued = VM_ShokoServer.Instance.ShokoServices.UpdateAniDBFileData(chkMissingInfo.IsChecked.Value, chkOutofDate.IsChecked.Value, false);

                MessageBox.Show(string.Format(Shoko.Commons.Properties.Resources.UpdateAniDB_QueueActual, filesQueued), Shoko.Commons.Properties.Resources.UpdateAniDB_UpdateAniDB, MessageBoxButton.OK, MessageBoxImage.Information);

                wdw.Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Shoko.Desktop.ViewModel;

namespace Shoko.Desktop.UserControls.Settings
{
    /// <summary>
    /// Interaction logic for PlexSettings.xaml
    /// </summary>
    public partial class PlexSettings : UserControl
    {
        public PlexSettings()
        {
            InitializeComponent();

            btnAdd.Click += BtnAddOnClick;
            btnRemove.Click += BtnRemoveOnClick;
            txtPlexHost.LostKeyboardFocus += (sender, args) =>
            {
                VM_ShokoServer.Instance.Plex_ServerHost = txtPlexHost.Text;
                VM_ShokoServer.Instance.SaveServerSettingsAsync();
            };
        }
        private void BtnRemoveOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            VM_ShokoServer.Instance.Plex_Sections.Remove((int)lstPlexIDs.SelectedItem);
            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }

        private void BtnAddOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            int item;
            string currentText = txtNewItem.Text;
            txtNewItem.Text = "";
            if (!int.TryParse(currentText, out item))
            {
                return;
            }

            if (VM_ShokoServer.Instance.Plex_Sections.Contains(item))
            {
                return;
            }

            VM_ShokoServer.Instance.Plex_Sections.Add(item);

            VM_ShokoServer.Instance.SaveServerSettingsAsync();
        }
    }
}

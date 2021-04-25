using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Shoko.Desktop.ViewModel;
using Shoko.Models.Client;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for DialogText.xaml
    /// </summary>
    public partial class AskDeleteFile : Window
    {

        public List<CL_VideoLocal_Place> Selected { get; private set; }
        private List<CL_VideoLocal_Place> _places;
        public bool DeleteFiles { get; set; }

        public AskDeleteFile(string title, string message, List<CL_VideoLocal_Place> places)
        {
            InitializeComponent();
            _places = places;
            btnConfirm.Click += BtnConfirm_Click;
            btnCancel.Click += BtnCancel_Click;
            Title = title;
            txtDesc.Text = message;
            InitImportFolders();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (DeleteFiles)
            {
                DialogResult = false;
                Selected = new List<CL_VideoLocal_Place>();
                Close();
                return;
            }
            DialogResult = true;
            Selected = _places;
            Close();
        }

        public void InitImportFolders()
        {
            WrapPanel.Children.Clear();
            StackPanel st = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 10, 0)
            };
            Image im = new Image
            {
                Height = 24,
                Width = 24,
                Source = new BitmapImage(new Uri(@"/ShokoDesktop;component/Images/16_folder.png", UriKind.Relative)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0)
            };
            TextBlock tx = new TextBlock
            {
                Margin = new Thickness(0, 0, 5, 0),
                Text = "Local File",
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.DemiBold
            };
            CheckBox chk = new CheckBox { VerticalAlignment = VerticalAlignment.Center, IsChecked = true };
            
            chk.Checked += (a, b) =>
            {
                DeleteFiles = true;
            };

            chk.Unchecked += (a, b) =>
            {
                DeleteFiles = false;
            };

            st.Children.Add(im);
            st.Children.Add(tx);
            st.Children.Add(chk);
            WrapPanel.Children.Add(st);
        }
    }
}




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
        Dictionary<string, bool> chks = new Dictionary<string, bool>();
        private Dictionary<int, Tuple<string, BitmapImage>> dict = new Dictionary<int, Tuple<string, BitmapImage>>();
        private List<CL_VideoLocal_Place> _places;

        public AskDeleteFile(string title, string message, List<CL_VideoLocal_Place> places)
        {
            InitializeComponent();
            _places = places;
            btnConfirm.Click += BtnConfirm_Click;
            btnCancel.Click += BtnCancel_Click;
            if (VM_ShokoServer.Instance.FolderProviders.Count == 0)
                VM_ShokoServer.Instance.RefreshCloudAccounts();
            dict = VM_ShokoServer.Instance.FolderProviders.ToDictionary(a => (int)a.CloudID,
                a => new Tuple<string, BitmapImage>(a.Provider, a.Bitmap));
            chks = new Dictionary<string, bool>();

            Dictionary<string, BitmapImage> types = new Dictionary<string, BitmapImage>();
            foreach (CL_VideoLocal_Place vv in places)
            {
                Tuple<string, BitmapImage> tup = dict[vv.ImportFolder.CloudID ?? 0];
                if (!types.ContainsKey(tup.Item1))
                {
                    chks[tup.Item1] = true;
                    types.Add(tup.Item1, tup.Item2);
                }
            }
            Title = title;
            txtDesc.Text = message;
            InitImportFolders(types);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            List<CL_VideoLocal_Place> vplaces=new List<CL_VideoLocal_Place>();
            foreach (string s in chks.Keys)
            {
                if (chks[s])
                {
                    foreach (CL_VideoLocal_Place v in _places)
                    {
                        if (dict[v.ImportFolder.CloudID ?? 0].Item1 == s)
                        {
                            if (!vplaces.Contains(v))
                                vplaces.Add(v);
                        }
                    }
                }
            }
            DialogResult = vplaces.Count > 0;
            Selected = vplaces;
            Close();
        }

        public void InitImportFolders(Dictionary<string, BitmapImage> types)
        {
            WrapPanel.Children.Clear();
            foreach (string s in types.Keys)
            {
                StackPanel st = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                Image im = new Image
                {
                    Height = 24,
                    Width = 24,
                    Source = types[s],
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 5, 0)
                };
                TextBlock tx = new TextBlock
                {
                    Margin = new Thickness(0, 0, 5, 0),
                    Text = s,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.DemiBold
                };
                CheckBox chk = new CheckBox { VerticalAlignment = VerticalAlignment.Center, IsChecked = true };
                
                chk.Checked += (a, b) =>
                {
                    chks[s] = true;
                };

                chk.Unchecked += (a, b) =>
                {
                    chks[s] = false;
                };

                st.Children.Add(im);
                st.Children.Add(tx);
                st.Children.Add(chk);
                WrapPanel.Children.Add(st);
            }
        }
    }
}




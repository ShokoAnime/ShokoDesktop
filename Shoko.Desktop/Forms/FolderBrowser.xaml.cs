using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for CloudFolderBrowser.xaml
    /// </summary>
    public partial class FolderBrowser : Window
    {
        private object obj = new object();

        public string SelectedPath { get; set; } = string.Empty;
        private VM_CloudAccount account;
        
        public FolderBrowser()
        {
            InitializeComponent();
            TrView.SelectedItemChanged += TrView_SelectedItemChanged;
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;
        }



        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private List<string> GetFromDirectory(string path)
        {

            return VM_ShokoServer.Instance.ShokoServices.DirectoriesFromImportFolderPath(account.CloudID, path);
        }

        public void Init(VM_CloudAccount cl, string initialpath)
        {
            account = cl;
            PopulateMainDir(initialpath);
        }

        private void RecursiveAddFromDirectory(ItemCollection coll, string path, string[] parts, int pos)
        {
            List<string> s = GetFromDirectory(path);
            if (s == null)
                return;
            foreach (string k in s)
            {
                int idx = k.LastIndexOf("\\");
                string n = (idx >= 0) ? k.Substring(idx + 1) : k;
                if (path.EndsWith(":"))
                    path += "\\";
                string combined = Path.Combine(path, n);
                TreeViewItem item = GenerateFromDirectory(n,combined);
                if (parts.Length > pos)
                {
                    if (n.Equals(parts[pos],StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (pos<parts.Length-1)
                            RecursiveAddFromDirectory(item.Items, combined, parts, pos + 1);
                        item.IsSelected = true;
                    }
                }
                coll.Add(item);
            }
        }

        public void PopulateMainDir(string initialpath)
        {
            Cursor = Cursors.Wait;
            initialpath = initialpath.Replace("/", "\\");
            while (initialpath.StartsWith("\\"))
                initialpath = initialpath.Substring(1);
            string[] pars = initialpath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            RecursiveAddFromDirectory(TrView.Items, account.CloudID == 0 ? "" : account.Name, pars, (account.CloudID)==0 ? 0 : 1);
            Cursor = Cursors.Arrow;
        }


       

        private TreeViewItem GenerateFromDirectory(string d, string full)
        {
            TreeViewItem item = new TreeViewItem();
            item.Header = d;
            item.Tag = full;
            item.FontWeight = FontWeights.Normal;
            item.Items.Add(obj);
            item.Expanded += Item_Expanded;
            return item;
        }
        private void Item_Expanded(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count>0 && item.Items[0]==obj)
            {
                item.Items.Clear();
                try
                {

                    string path = (string) item.Tag;
                    List<string> ss = GetFromDirectory(path);
                    if (ss == null)
                        return;
                    foreach (string k in ss)
                    {
                        int idx = k.LastIndexOf("\\");
                        string n = (idx >= 0) ? k.Substring(idx + 1) : k;
                        if (path.EndsWith(":"))
                            path += "\\";
                        
                        item.Items.Add(GenerateFromDirectory(n, Path.Combine(path,n)));
                    }
                }
                catch (Exception) { }
            }
            Cursor = Cursors.Arrow;
        }
        private void TrView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView tree = (TreeView)sender;
            TreeViewItem item = tree.SelectedItem as TreeViewItem;
            if (item != null)
            {
                SelectedPath = (string) item.Tag;
            }
        }

    }
}

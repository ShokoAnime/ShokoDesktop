using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JMMClient.ViewModel;

namespace JMMClient.Forms
{
    /// <summary>
    /// Interaction logic for CloudFolderBrowser.xaml
    /// </summary>
    public partial class FolderBrowser : Window
    {
        private object obj = new object();

        public string SelectedPath { get; set; } = string.Empty;
        private CloudAccountVM account;
        
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
            return JMMServerVM.Instance.clientBinaryHTTP.DirectoriesFromImportFolderPath(account.CloudID??0, path);
        }

        public void Init(CloudAccountVM cl, string initialpath)
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

                TreeViewItem item = GenerateFromDirectory(n,Path.Combine(path,n));
                if (parts.Length > pos)
                {
                    if (n.Equals(parts[pos],StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (pos<parts.Length-1)
                            RecursiveAddFromDirectory(item.Items, Path.Combine(path,n), parts, pos + 1);
                        item.IsSelected = true;
                    }
                }
                coll.Add(item);
            }
        }

        public void PopulateMainDir(string initialpath)
        {
            this.Cursor = Cursors.Wait;
            initialpath = initialpath.Replace("/", "\\");
            while (initialpath.StartsWith("\\"))
                initialpath = initialpath.Substring(1);
            string[] pars = initialpath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            RecursiveAddFromDirectory(TrView.Items, account.Name, pars, (account.CloudID ?? 0)==0 ? 0 : 1);
            this.Cursor = Cursors.Arrow;
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
            this.Cursor = Cursors.Wait;
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

                        item.Items.Add(GenerateFromDirectory(n, Path.Combine(path,n)));
                    }
                }
                catch (Exception) { }
            }
            this.Cursor = Cursors.Arrow;
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

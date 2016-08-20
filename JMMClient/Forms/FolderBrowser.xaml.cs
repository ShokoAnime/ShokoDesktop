using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JMMClient.Forms
{
    /// <summary>
    /// Interaction logic for CloudFolderBrowser.xaml
    /// </summary>
    public partial class FolderBrowser : Window
    {
        private object obj = new object();

        public string SelectedPath { get; set; } = string.Empty;
        private int accountid;

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
            return JMMServerVM.Instance.clientBinaryHTTP.DirectoriesFromImportFolderPath(accountid, path);
        }

        public void Init(int accountcloudid, string initialpath)
        {
            accountid = accountcloudid;
            PopulateMainDir(initialpath);
        }

        private void RecursiveAddFromDirectory(ItemCollection coll, string path, string[] parts, int pos)
        {
            List<string> s = GetFromDirectory(path);
            if (s == null)
                return;
            foreach (string n in s)
            {
                TreeViewItem item = GenerateFromDirectory(n,Path.Combine(path,n));
                if (parts.Length > pos)
                {
                    if (n.Equals(parts[0],StringComparison.InvariantCultureIgnoreCase))
                    {
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
            RecursiveAddFromDirectory(TrView.Items, string.Empty, pars, 0);
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
                    foreach (string s in ss)
                    {
                        item.Items.Add(GenerateFromDirectory(s, Path.Combine(path, s)));
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

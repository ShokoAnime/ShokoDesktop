using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for DuplicateFilesControl.xaml
    /// </summary>
    public partial class DuplicateFilesControl : UserControl
    {
        public ICollectionView ViewFiles { get; set; }
        public ObservableCollection<VM_DuplicateFile> DuplicateFilesCollection { get; set; }

        public static readonly DependencyProperty FileCountProperty = DependencyProperty.Register("FileCount",
            typeof(int), typeof(DuplicateFilesControl), new UIPropertyMetadata(0, null));

        public int FileCount
        {
            get { return (int)GetValue(FileCountProperty); }
            set { SetValue(FileCountProperty, value); }
        }

        private int lastSelIndex = 0;

        public DuplicateFilesControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            DuplicateFilesCollection = new ObservableCollection<VM_DuplicateFile>();
            ViewFiles = CollectionViewSource.GetDefaultView(DuplicateFilesCollection);
            ViewFiles.SortDescriptions.Add(new SortDescription("LocalFileName1", ListSortDirection.Ascending));

            btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            lbDuplicateFiles.SelectionChanged += new SelectionChangedEventHandler(lbDuplicateFiles_SelectionChanged);

            btnReevaluate.Click += new RoutedEventHandler(btnReevaluate_Click);
        }

        void btnReevaluate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;
                VM_ShokoServer.Instance.ShokoServices.ReevaluateDuplicateFiles();
                RefreshDuplicateFiles();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        void lbDuplicateFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbDuplicateFiles.Items.Count > 0)
                lastSelIndex = lbDuplicateFiles.SelectedIndex;
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshDuplicateFiles();
        }

        public void RefreshDuplicateFiles()
        {
            try
            {
                Cursor = Cursors.Wait;
                DuplicateFilesCollection.Clear();

                List<VM_DuplicateFile> dfs = VM_ShokoServer.Instance.ShokoServices.GetAllDuplicateFiles().CastList<VM_DuplicateFile>();
                FileCount = dfs.Count;

                foreach (VM_DuplicateFile df in dfs)
                {
                    DuplicateFilesCollection.Add(df);
                }

                // move to the next item
                if (lastSelIndex <= lbDuplicateFiles.Items.Count)
                {
                    lbDuplicateFiles.SelectedIndex = lastSelIndex;
                    lbDuplicateFiles.Focus();
                    lbDuplicateFiles.ScrollIntoView(lbDuplicateFiles.SelectedItem);
                }
                else
                {
                    // move to the previous item
                    if (lastSelIndex - 1 <= lbDuplicateFiles.Items.Count)
                    {
                        lbDuplicateFiles.SelectedIndex = lastSelIndex - 1;
                        lbDuplicateFiles.Focus();
                        lbDuplicateFiles.ScrollIntoView(lbDuplicateFiles.SelectedItem);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void CommandBinding_OpenFolder1(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_DuplicateFile))
                {
                    VM_DuplicateFile df = obj as VM_DuplicateFile;
                    if (File.Exists(df.GetLocalFilePath1()))
                        Utils.OpenFolderAndSelectFile(df.GetLocalFilePath1());
                    else
                        MessageBox.Show(Shoko.Commons.Properties.Resources.MSG_ERR_FileNotFound, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_OpenFolder2(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_DuplicateFile))
                {
                    VM_DuplicateFile df = obj as VM_DuplicateFile;
                    if (File.Exists(df.GetLocalFilePath2()))
                        Utils.OpenFolderAndSelectFile(df.GetLocalFilePath2());
                    else
                        MessageBox.Show(Shoko.Commons.Properties.Resources.MSG_ERR_FileNotFound, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_PlayVideo1(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_DuplicateFile))
                {
                    VM_DuplicateFile df = obj as VM_DuplicateFile;
                    Utils.OpenUrl(df.GetLocalFilePath1());
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void CommandBinding_PlayVideo2(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_DuplicateFile))
                {
                    VM_DuplicateFile df = obj as VM_DuplicateFile;
                    Utils.OpenUrl(df.GetLocalFilePath2());
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void CommandBinding_DeleteFile1(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_DuplicateFile))
                {
                    VM_DuplicateFile df = obj as VM_DuplicateFile;

                    MessageBoxResult res = MessageBox.Show(string.Format(Shoko.Commons.Properties.Resources.DuplicateFiles_ConfirmDelete),
                        Shoko.Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (res == MessageBoxResult.Yes)
                    {
                        Cursor = Cursors.Wait;
                        string result = VM_ShokoServer.Instance.ShokoServices.DeleteDuplicateFile(df.File1VideoLocalPlaceID);
                        if (result.Length > 0)
                            MessageBox.Show(result, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                        {
                            DuplicateFilesCollection.Remove(df);
                            FileCount = DuplicateFilesCollection.Count;
                            //RefreshDuplicateFiles();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void CommandBinding_DeleteFile2(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_DuplicateFile))
                {
                    VM_DuplicateFile df = obj as VM_DuplicateFile;

                    MessageBoxResult res = MessageBox.Show(string.Format(Shoko.Commons.Properties.Resources.DuplicateFiles_ConfirmDelete),
                        Shoko.Commons.Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (res == MessageBoxResult.Yes)
                    {
                        Cursor = Cursors.Wait;
                        string result = VM_ShokoServer.Instance.ShokoServices.DeleteDuplicateFile(df.File2VideoLocalPlaceID);
                        if (result.Length > 0)
                            MessageBox.Show(result, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                        {
                            DuplicateFilesCollection.Remove(df);
                            FileCount = DuplicateFilesCollection.Count;
                            //RefreshDuplicateFiles();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }
    }
}

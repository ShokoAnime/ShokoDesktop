using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Enums;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for IgnoredFiles.xaml
    /// </summary>
    public partial class IgnoredFiles : UserControl
    {
        public ICollectionView ViewFiles { get; set; }
        public ObservableCollection<VM_VideoLocal> IgnoredFilesCollection { get; set; }

        public static readonly DependencyProperty OneVideoSelectedProperty = DependencyProperty.Register("OneVideoSelected",
            typeof(bool), typeof(IgnoredFiles), new UIPropertyMetadata(false, null));

        public static readonly DependencyProperty MultipleVideosSelectedProperty = DependencyProperty.Register("MultipleVideosSelected",
            typeof(bool), typeof(IgnoredFiles), new UIPropertyMetadata(false, null));

        public static readonly DependencyProperty FileCountProperty = DependencyProperty.Register("FileCount",
            typeof(int), typeof(IgnoredFiles), new UIPropertyMetadata(0, null));

        public int FileCount
        {
            get { return (int)GetValue(FileCountProperty); }
            set { SetValue(FileCountProperty, value); }
        }

        public bool OneVideoSelected
        {
            get { return (bool)GetValue(OneVideoSelectedProperty); }
            set { SetValue(OneVideoSelectedProperty, value); }
        }

        public bool MultipleVideosSelected
        {
            get { return (bool)GetValue(MultipleVideosSelectedProperty); }
            set { SetValue(MultipleVideosSelectedProperty, value); }
        }

        public IgnoredFiles()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            IgnoredFilesCollection = new ObservableCollection<VM_VideoLocal>();
            ViewFiles = CollectionViewSource.GetDefaultView(IgnoredFilesCollection);
            ViewFiles.SortDescriptions.Add(new SortDescription("FileName", ListSortDirection.Ascending));

            btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

            lbVideos.SelectionChanged += new SelectionChangedEventHandler(lbVideos_SelectionChanged);
            OneVideoSelected = lbVideos.SelectedItems.Count == 1;
            MultipleVideosSelected = lbVideos.SelectedItems.Count > 1;
        }

        void lbVideos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ccDetail.Content = null;
                ccDetailMultiple.Content = null;

                OneVideoSelected = lbVideos.SelectedItems.Count == 1;
                MultipleVideosSelected = lbVideos.SelectedItems.Count > 1;

                // if only one video selected
                if (OneVideoSelected)
                {
                    VM_VideoLocal vid = lbVideos.SelectedItem as VM_VideoLocal;
                    ccDetail.Content = vid;
                }

                // if only one video selected
                if (MultipleVideosSelected)
                {
                    MultipleVideos mv = new MultipleVideos();
                    mv.SelectedCount = lbVideos.SelectedItems.Count;
                    mv.VideoLocalIDs = new List<int>();

                    foreach (object obj in lbVideos.SelectedItems)
                    {
                        VM_VideoLocal vid = obj as VM_VideoLocal;
                        mv.VideoLocalIDs.Add(vid.VideoLocalID);
                    }

                    ccDetailMultiple.Content = mv;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_PlayVideo(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_VideoLocal))
                {
                    VM_VideoLocal vid = obj as VM_VideoLocal;
                    bool force = true;
                    if (MainWindow.videoHandler.DefaultPlayer.Player.ToString() !=
                        Enum.GetName(typeof(VideoPlayer), VideoPlayer.WindowsDefault))
                    {
                        if (vid.ResumePosition > 0)
                        {
                            AskResumeVideo ask = new AskResumeVideo(vid.ResumePosition);
                            ask.Owner = Window.GetWindow(this);
                            if (ask.ShowDialog() == true)
                                force = false;
                        }
                    }
                    MainWindow.videoHandler.PlayVideo(vid, force);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void CommandBinding_OpenFolder(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_VideoLocal))
                {
                    VM_VideoLocal vid = obj as VM_VideoLocal;

                    if (File.Exists(Commons.Extensions.Models.GetLocalFileSystemFullPath(vid)))
                    {
                        Utils.OpenFolderAndSelectFile(Commons.Extensions.Models.GetLocalFileSystemFullPath(vid));
                    }
                    else
                    {
                        MessageBox.Show(Properties.Resources.MSG_ERR_FileNotFound, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void EnableDisableControls(bool val)
        {
            lbVideos.IsEnabled = val;
            btnRefresh.IsEnabled = val;
            ccDetail.IsEnabled = val;
            ccDetailMultiple.IsEnabled = val;
        }

        private void CommandBinding_RestoreFile(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Window parentWindow = Window.GetWindow(this);

                object obj = e.Parameter;
                if (obj == null) return;

                if (obj.GetType() == typeof(VM_VideoLocal))
                {
                    VM_VideoLocal vid = obj as VM_VideoLocal;
                    EnableDisableControls(false);

                    string result = VM_ShokoServer.Instance.ShokoServices.SetIgnoreStatusOnFile(vid.VideoLocalID, false);
                    if (result.Length > 0)
                        MessageBox.Show(result, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        RefreshIgnoredFiles();

                }
                if (obj.GetType() == typeof(MultipleVideos))
                {
                    MultipleVideos mv = obj as MultipleVideos;
                    foreach (int id in mv.VideoLocalIDs)
                    {
                        string result = VM_ShokoServer.Instance.ShokoServices.SetIgnoreStatusOnFile(id, false);
                        if (result.Length > 0)
                            MessageBox.Show(result, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    RefreshIgnoredFiles();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            EnableDisableControls(true);
        }

        private void CommandBinding_DeleteFile(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Window parentWindow = Window.GetWindow(this);

                object obj = e.Parameter;
                if (obj == null) return;

                if (obj.GetType() == typeof(VM_VideoLocal))
                {
                    VM_VideoLocal vid = obj as VM_VideoLocal;


                    AskDeleteFile dlg = new AskDeleteFile(string.Format(Properties.Resources.DeleteFile_Title, vid.FileName), Properties.Resources.Unrecognized_ConfirmDelete + "\r\n\r\n" + Properties.Resources.DeleteFile_Confirm, vid.Places);
                    dlg.Owner = Window.GetWindow(this);
                    bool? res = dlg.ShowDialog();
                    if (res.HasValue && res.Value)
                    {
                        EnableDisableControls(false);
                        string tresult = string.Empty;
                        Cursor = Cursors.Wait;
                        foreach (CL_VideoLocal_Place lv in dlg.Selected)
                        {
                            string result = VM_ShokoServer.Instance.ShokoServices.DeleteVideoLocalPlaceAndFile(lv.VideoLocal_Place_ID);
                            if (result.Length > 0)
                                tresult += result + "\r\n";
                        }
                        if (!string.IsNullOrEmpty(tresult))
                            MessageBox.Show(tresult, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        RefreshIgnoredFiles();
                    }


                }

                if (obj.GetType() == typeof(MultipleVideos))
                {
                    MultipleVideos mv = obj as MultipleVideos;
                    AskDeleteFile dlg = new AskDeleteFile(Properties.Resources.DeleteFile_Multiple, Properties.Resources.Unrecognized_DeleteSelected + "\r\n\r\n" + Properties.Resources.DeleteFile_Confirm, mv.VideoLocals.SelectMany(a=>a.Places).ToList());
                    dlg.Owner = Window.GetWindow(this);
                    bool? res = dlg.ShowDialog();
                    if (res.HasValue && res.Value)
                    {
                        EnableDisableControls(false);
                        string tresult = string.Empty;
                        Cursor = Cursors.Wait;
                        foreach (CL_VideoLocal_Place lv in dlg.Selected)
                        {
                            string result = VM_ShokoServer.Instance.ShokoServices.DeleteVideoLocalPlaceAndFile(lv.VideoLocal_Place_ID);
                            if (result.Length > 0)
                                tresult += result + "\r\n";
                        }
                        if (!string.IsNullOrEmpty(tresult))
                            MessageBox.Show(tresult, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        RefreshIgnoredFiles();
                    }
                }


            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            EnableDisableControls(true);
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshIgnoredFiles();
        }

        public void RefreshIgnoredFiles()
        {
            try
            {
                IgnoredFilesCollection.Clear();

                List<VM_VideoLocal> vids = VM_ShokoServer.Instance.ShokoServices.GetIgnoredFiles(VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_VideoLocal>();
                FileCount = vids.Count;

                foreach (VM_VideoLocal vid in vids)
                {
                    IgnoredFilesCollection.Add(vid);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }
}

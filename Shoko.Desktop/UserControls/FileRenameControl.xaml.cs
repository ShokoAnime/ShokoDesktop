using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models;
using Shoko.Models.Client;
using Shoko.Models.Enums;
using Shoko.Models.Server;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for FileRenameControl.xaml
    /// </summary>
    public partial class FileRenameControl : UserControl
    {
        public ICollectionView ViewFiles { get; set; }
        public ObservableCollection<VM_VideoLocal_Renamed> FileResults { get; set; }

        public ICollectionView ViewTags { get; set; }
        public ObservableCollection<RenameTag> RenameTags { get; set; }

        public ICollectionView ViewTests { get; set; }
        public ObservableCollection<RenameTest> RenameTests { get; set; }

        //public ICollectionView ViewScripts { get; set; }
        public ObservableCollection<VM_RenameScript> RenameScripts { get; set; }
        
        public ObservableCollection<Controller> ScriptProcessors { get; set; }

        public static readonly DependencyProperty FileCountProperty = DependencyProperty.Register("FileCount",
            typeof(int), typeof(FileRenameControl), new UIPropertyMetadata(0, null));

        public int FileCount
        {
            get { return (int)GetValue(FileCountProperty); }
            set
            {
                SetValue(FileCountProperty, value);
                FileCountStatus = string.Format(Shoko.Commons.Properties.Resources.Rename_Files, value);
            }
        }

        public static readonly DependencyProperty FileCountStatusProperty = DependencyProperty.Register("FileCountStatus",
            typeof(string), typeof(FileRenameControl), new UIPropertyMetadata("", null));

        public string FileCountStatus
        {
            get { return (string)GetValue(FileCountStatusProperty); }
            set { SetValue(FileCountStatusProperty, value); }
        }

        public static readonly DependencyProperty LoadTypeIsRandomProperty = DependencyProperty.Register("LoadTypeIsRandom",
            typeof(bool), typeof(FileRenameControl), new UIPropertyMetadata(true, null));

        public bool LoadTypeIsRandom
        {
            get { return (bool)GetValue(LoadTypeIsRandomProperty); }
            set { SetValue(LoadTypeIsRandomProperty, value); }
        }

        public static readonly DependencyProperty LoadTypeIsSeriesProperty = DependencyProperty.Register("LoadTypeIsSeries",
            typeof(bool), typeof(FileRenameControl), new UIPropertyMetadata(false, null));

        public bool LoadTypeIsSeries
        {
            get { return (bool)GetValue(LoadTypeIsSeriesProperty); }
            set { SetValue(LoadTypeIsSeriesProperty, value); }
        }

        public static readonly DependencyProperty LoadTypeIsAllProperty = DependencyProperty.Register("LoadTypeIsAll",
            typeof(bool), typeof(FileRenameControl), new UIPropertyMetadata(false, null));

        public bool LoadTypeIsAll
        {
            get { return (bool)GetValue(LoadTypeIsAllProperty); }
            set { SetValue(LoadTypeIsAllProperty, value); }
        }

        public static readonly DependencyProperty LoadTypeIsLastProperty = DependencyProperty.Register("LoadTypeIsLast",
            typeof(bool), typeof(FileRenameControl), new UIPropertyMetadata(false, null));

        public bool LoadTypeIsLast
        {
            get { return (bool)GetValue(LoadTypeIsLastProperty); }
            set { SetValue(LoadTypeIsLastProperty, value); }
        }

        public static readonly DependencyProperty LoadTypeHasNumberProperty = DependencyProperty.Register("LoadTypeHasNumber",
            typeof(bool), typeof(FileRenameControl), new UIPropertyMetadata(false, null));

        public bool LoadTypeHasNumber
        {
            get { return (bool)GetValue(LoadTypeHasNumberProperty); }
            set { SetValue(LoadTypeHasNumberProperty, value); }
        }

        public static readonly DependencyProperty WorkerRunningProperty = DependencyProperty.Register("WorkerRunning",
            typeof(bool), typeof(FileRenameControl), new UIPropertyMetadata(false, null));

        public bool WorkerRunning
        {
            get { return (bool)GetValue(WorkerRunningProperty); }
            set { SetValue(WorkerRunningProperty, value); }
        }

        public static readonly DependencyProperty WorkerNotRunningProperty = DependencyProperty.Register("WorkerNotRunning",
            typeof(bool), typeof(FileRenameControl), new UIPropertyMetadata(true, null));

        public bool WorkerNotRunning
        {
            get { return (bool)GetValue(WorkerNotRunningProperty); }
            set { SetValue(WorkerNotRunningProperty, value); }
        }

        public static readonly DependencyProperty WorkerStatusProperty = DependencyProperty.Register("WorkerStatus",
            typeof(string), typeof(FileRenameControl), new UIPropertyMetadata("", null));

        public string WorkerStatus
        {
            get { return (string)GetValue(WorkerStatusProperty); }
            set { SetValue(WorkerStatusProperty, value); }
        }

        private readonly string LoadTypeRandom = Shoko.Commons.Properties.Resources.Rename_Random;
        private readonly string LoadTypeSeries = Shoko.Commons.Properties.Resources.Rename_Series;
        private readonly string LoadTypeAll = Shoko.Commons.Properties.Resources.Rename_All;
        private readonly string LoadTypeLast = Shoko.Commons.Properties.Resources.Rename_Last;

        private readonly string FilterTypeAll = Shoko.Commons.Properties.Resources.Random_All;
        private readonly string FilterTypeFailed = Shoko.Commons.Properties.Resources.Rename_Failed;
        private readonly string FilterTypePassed = Shoko.Commons.Properties.Resources.Rename_Passed;

        private int? defaultScriptID = null;

        BackgroundWorker previewWorker = new BackgroundWorker();
        BackgroundWorker renameWorker = new BackgroundWorker();
        private bool stopWorker = false;

        public FileRenameControl()
        {
            InitializeComponent();

            FileResults = new ObservableCollection<VM_VideoLocal_Renamed>();
            ViewFiles = CollectionViewSource.GetDefaultView(FileResults);

            RenameTags = new ObservableCollection<RenameTag>();
            foreach (RenameTag tag in RenameHelper.GetAllTags())
                RenameTags.Add(tag);
            ViewTags = CollectionViewSource.GetDefaultView(RenameTags);
            cboTagType.SelectedIndex = 0;

            RenameTests = new ObservableCollection<RenameTest>();
            foreach (RenameTest test in RenameHelper.GetAllTests())
                RenameTests.Add(test);
            ViewTests = CollectionViewSource.GetDefaultView(RenameTests);
            cboTestType.SelectedIndex = 0;

            RenameScripts = new ObservableCollection<VM_RenameScript>();
            ScriptProcessors = new ObservableCollection<Controller>();


            btnLoadFiles.Click += new RoutedEventHandler(btnLoadFiles_Click);
            btnPreviewFiles.Click += new RoutedEventHandler(btnPreviewFiles_Click);
            btnRenameFiles.Click += new RoutedEventHandler(btnRenameFiles_Click);
            btnPreviewStop.Click += new RoutedEventHandler(btnPreviewStop_Click);
            btnClearList.Click += new RoutedEventHandler(btnClearList_Click);
            btnAddTag.Click += new RoutedEventHandler(btnAddTag_Click);
            btnAddTest.Click += new RoutedEventHandler(btnAddTest_Click);

            btnNewScript.Click += new RoutedEventHandler(btnNewScript_Click);
            btnSaveScript.Click += new RoutedEventHandler(btnSaveScript_Click);
            btnDeleteScript.Click += new RoutedEventHandler(btnDeleteScript_Click);

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            cboLoadType.Items.Clear();
            cboLoadType.Items.Add(LoadTypeRandom);
            cboLoadType.Items.Add(LoadTypeSeries);
            cboLoadType.Items.Add(LoadTypeLast);
            cboLoadType.Items.Add(LoadTypeAll);
            cboLoadType.SelectedIndex = 0;

            cboLoadType.SelectionChanged += new SelectionChangedEventHandler(cboLoadType_SelectionChanged);

            cboFilterType.Items.Clear();
            cboFilterType.Items.Add(Shoko.Commons.Properties.Resources.Random_All);
            cboFilterType.Items.Add(Shoko.Commons.Properties.Resources.Rename_Failed);
            cboFilterType.Items.Add(Shoko.Commons.Properties.Resources.Rename_Passed);
            cboFilterType.SelectionChanged += new SelectionChangedEventHandler(cboFilterType_SelectionChanged);
            cboFilterType.SelectedIndex = 0;

            cboScript.SelectionChanged += new SelectionChangedEventHandler(cboScript_SelectionChanged);

            previewWorker.DoWork += new DoWorkEventHandler(previewWorker_DoWork);
            previewWorker.WorkerSupportsCancellation = true;
            previewWorker.WorkerReportsProgress = true;
            previewWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(previewWorker_RunWorkerCompleted);
            previewWorker.ProgressChanged += new ProgressChangedEventHandler(previewWorker_ProgressChanged);


            renameWorker.DoWork += new DoWorkEventHandler(renameWorker_DoWork);
            renameWorker.WorkerSupportsCancellation = true;
            renameWorker.WorkerReportsProgress = true;
            renameWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(renameWorker_RunWorkerCompleted);
            renameWorker.ProgressChanged += new ProgressChangedEventHandler(renameWorker_ProgressChanged);

            txtRenameScript.LostFocus += new RoutedEventHandler(txtRenameScript_LostFocus);

            ViewTags.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            ViewTests.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            //ViewScripts.SortDescriptions.Add(new SortDescription("ScriptName", ListSortDirection.Ascending));
        }



        void btnDeleteScript_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboScript.Items.Count == 0) return;
                if (cboScript.SelectedItem == null) return;
                VM_RenameScript script = cboScript.SelectedItem as VM_RenameScript;

                string msg = string.Format(Shoko.Commons.Properties.Resources.Rename_DeleteScript, script.ScriptName);
                MessageBoxResult res = MessageBox.Show(msg, Shoko.Commons.Properties.Resources.Confirm,
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (res == MessageBoxResult.Yes)
                {
                    Cursor = Cursors.Wait;

                    string errorMsg = VM_ShokoServer.Instance.ShokoServices.DeleteRenameScript(script.RenameScriptID);
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        Utils.ShowErrorMessage(errorMsg);
                    }
                    else
                        defaultScriptID = null;

                    // refresh data
                    RefreshScripts();
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            Cursor = Cursors.Arrow;
        }

        void btnSaveScript_Click(object sender, RoutedEventArgs e)
        {
            SaveScript();
        }

        private bool SaveScript()
        {
            try
            {
                if (cboScript.Items.Count == 0) return false;
                if (cboScript.SelectedItem == null) return false;

                VM_RenameScript script = cboScript.SelectedItem as VM_RenameScript;
                script.IsEnabledOnImport = chkIsUsedForImports.IsChecked.Value ? 1 : 0;
                script.Script = txtRenameScript.Text;

                Controller controller = cboController.SelectedItem as Controller;
                script.RenamerType = controller.RenamerType;

                if (script.Save())
                {
                    defaultScriptID = script.RenameScriptID;

                    // refresh data
                    RefreshScripts();
                    return true;
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            return false;
        }

        void btnNewScript_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogText dlg = new DialogText();
                dlg.Init(Shoko.Commons.Properties.Resources.Rename_EnterScriptName, "");
                dlg.Owner = Window.GetWindow(this);
                bool? res = dlg.ShowDialog();
                if (res.HasValue && res.Value)
                {
                    if (string.IsNullOrEmpty(dlg.EnteredText))
                    {
                        Utils.ShowErrorMessage(Shoko.Commons.Properties.Resources.Rename_BlankScript);
                        return;
                    }

                    RenameScript script = new RenameScript();
                    script.IsEnabledOnImport = 0;
                    script.Script = "";
                    script.ScriptName = dlg.EnteredText;
                    script.RenamerType = "Legacy";
                    CL_Response<RenameScript> resp = VM_ShokoServer.Instance.ShokoServices.SaveRenameScript(script);

                    if (!string.IsNullOrEmpty(resp.ErrorMessage))
                    {
                        Utils.ShowErrorMessage(resp.ErrorMessage);
                        return;
                    }
                    defaultScriptID = resp.Result.RenameScriptID;
                    // refresh data
                    RefreshScripts();
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void cboScript_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboScript.Items.Count == 0) return;
            if (cboScript.SelectedItem == null) return;

            VM_RenameScript script = cboScript.SelectedItem as VM_RenameScript;
            txtRenameScript.Text = script.Script;
            chkIsUsedForImports.IsChecked = script.IsEnabledOnImportBool;

            int idxi = 0;
            foreach (var controller in ScriptProcessors)
            {
                if (controller.RenamerType == (cboScript.SelectedValue as VM_RenameScript)?.RenamerType)
                    cboController.SelectedIndex = idxi;
                idxi++;
            }
        }

        public void RefreshScripts()
        {
            try
            {
                RenameScripts.Clear();

                List<VM_RenameScript> scripts = VM_ShokoServer.Instance.ShokoServices.GetAllRenameScripts()
                    .Cast<VM_RenameScript>()
                    .OrderBy(a => a.ScriptName)
                    .ToList();

                if (scripts.Count > 0)
                {
                    int idx = 0;
                    if (defaultScriptID.HasValue)
                    {
                        foreach (VM_RenameScript scr in scripts)
                        {
                            if (scr.RenameScriptID == defaultScriptID) break;
                            idx++;
                        }
                    }

                    foreach (VM_RenameScript scr in scripts)
                        RenameScripts.Add(scr);

                    cboScript.SelectedIndex = idx;
                }
                else
                    defaultScriptID = null;

                ScriptProcessors.Clear();
                var scriptControllers = VM_ShokoServer.Instance.ShokoServices.GetScriptTypes();
                int idxi = 0;
                foreach (var controller in scriptControllers)
                {
                    ScriptProcessors.Add(new Controller { RenamerType = controller.Key, Description = controller.Value });
                    if (controller.Key == (cboScript.SelectedValue as VM_RenameScript)?.RenamerType)
                        cboController.SelectedIndex = idxi;
                    idxi++;
                }
                

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void cboFilterType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewFiles.Filter = FileFilter;
        }

        private bool FileFilter(object obj)
        {
            VM_VideoLocal_Renamed vid = obj as VM_VideoLocal_Renamed;
            if (vid == null) return false;

            string filterType = cboFilterType.SelectedItem.ToString();

            if (filterType.Equals(FilterTypeAll)) return true;

            if (filterType.Equals(FilterTypeFailed) && !vid.Success) return true;
            if (filterType.Equals(FilterTypePassed) && vid.Success) return true;

            return false;
        }

        void txtRenameScript_LostFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        void btnAddTag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboTagType.SelectedItem == null) return;

                RenameTag tag = cboTagType.SelectedItem as RenameTag;
                txtRenameScript.Text = txtRenameScript.Text.Insert(txtRenameScript.CaretIndex, tag.Tag);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage($"Unable to add tag: {ex.Message}", ex);
            }
        }

        void btnAddTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (cboTestType.SelectedItem == null) return;

                RenameTest test = cboTestType.SelectedItem as RenameTest;
                txtRenameScript.Text = txtRenameScript.Text.Insert(txtRenameScript.CaretIndex, test.Test);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage($"Unable to add test: {ex.Message}", ex);
            }
        }

        void btnClearList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileResults.Clear();
                ViewFiles.Refresh();
                FileCount = 0;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnPreviewStop_Click(object sender, RoutedEventArgs e)
        {
            stopWorker = true;
        }

        void renameWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ViewFiles.Refresh();
            WorkerStatusContainer status = e.UserState as WorkerStatusContainer;
            WorkerStatus = string.Format(Shoko.Commons.Properties.Resources.Rename_Changed, status.CurrentFile,
                status.TotalFileCount);
        }

        void renameWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ViewFiles.Refresh();
            WorkerRunning = false;
            WorkerNotRunning = true;
            stopWorker = false;
            EnableDisableControls(true);
        }

        void renameWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerJob job = e.Argument as WorkerJob;
            BackgroundWorker worker = sender as BackgroundWorker;

            foreach (VM_VideoLocal_Renamed ren in job.FileResults)
            {
                if (stopWorker) return;
                ren.NewFileName = "";
            }

            int curFile = 0;
            int delay = 0;
            foreach (VM_VideoLocal_Renamed ren in job.FileResults)
            {
                if (stopWorker) return;

                curFile++;
                delay++;


                VM_VideoLocal_Renamed raw =
                    (VM_VideoLocal_Renamed) VM_ShokoServer.Instance.ShokoServices.RenameAndMoveFile(ren.VideoLocalID,
                        job.RenameScript, job.Move);

                ren.NewFileName = raw.NewFileName;
                ren.Success = raw.Success;

                // do this so we don't lock the UI
                if (delay == 20)
                {
                    renameWorker.ReportProgress(0, new WorkerStatusContainer(job.FileResults.Count, curFile));
                    delay = 0;
                }
            }
        }

        void btnRenameFiles_Click(object sender, RoutedEventArgs e)
        {
            string msg = string.Format(Shoko.Commons.Properties.Resources.Rename_Confirm);
            MessageBoxResult res = MessageBox.Show(msg, Shoko.Commons.Properties.Resources.Confirm,
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (res != MessageBoxResult.Yes) return;

            EnableDisableControls(false);

            if (!SaveScript()) return;

            WorkerRunning = true;
            WorkerNotRunning = false;
            stopWorker = false;

            WorkerJob job = new WorkerJob();
            job.RenameScript = (cboScript.SelectedItem as VM_RenameScript)?.ScriptName;
            job.FileResults = FileResults;
            job.Move = chkMoveIfNeeded.IsChecked ?? false;

            if (job.RenameScript == null)
            {
                MessageBox.Show("The Selected Item is NULL", Shoko.Commons.Properties.Resources.Error,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            renameWorker.RunWorkerAsync(job);
        }

        void previewWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ViewFiles.Refresh();
            WorkerStatusContainer status = e.UserState as WorkerStatusContainer;
            WorkerStatus = string.Format(Shoko.Commons.Properties.Resources.Rename_Changed, status.CurrentFile, status.TotalFileCount);
        }

        void previewWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ViewFiles.Refresh();
            WorkerRunning = false;
            WorkerNotRunning = true;
            stopWorker = false;
            EnableDisableControls(true);
            //HideShowControls(System.Windows.Visibility.Visible);
        }

        void previewWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerJob job = e.Argument as WorkerJob;
            BackgroundWorker worker = sender as BackgroundWorker;

            foreach (VM_VideoLocal_Renamed ren in job.FileResults)
            {
                if (stopWorker) return;
                ren.NewFileName = "";
            }

            int curFile = 0;
            int delay = 0;
            foreach (VM_VideoLocal_Renamed ren in job.FileResults)
            {
                if (stopWorker) return;

                curFile++;
                delay++;

                VM_VideoLocal_Renamed raw = (VM_VideoLocal_Renamed)VM_ShokoServer.Instance.ShokoServices.RenameFilePreview(
                    ren.VideoLocalID);

                ren.NewFileName = raw.NewFileName;
                ren.Success = raw.Success;

                // do this so we don't lock the UI
                if (delay == 20)
                {
                    previewWorker.ReportProgress(0, new WorkerStatusContainer(job.FileResults.Count, curFile));
                    delay = 0;
                }
            }
        }

        void btnPreviewFiles_Click(object sender, RoutedEventArgs e)
        {
            EnableDisableControls(false);
            //HideShowControls(System.Windows.Visibility.Hidden);
            WorkerRunning = true;
            WorkerNotRunning = false;
            stopWorker = false;

            VM_RenameScript script = new VM_RenameScript();
            script.IsEnabledOnImport = 0;
            script.Script = txtRenameScript.Text;
            script.ScriptName = Constants.Renamer.TempFileName;

            Controller controller = cboController.SelectedItem as Controller;
            script.RenamerType = controller?.RenamerType; //prevent null reference  

            if (!script.Save()) return;

            WorkerJob job = new WorkerJob();
            job.RenameScript = "NULL";
            job.FileResults = FileResults;

            previewWorker.RunWorkerAsync(job);

        }

        void cboLoadType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadTypeIsRandom = (cboLoadType.SelectedItem.ToString() == LoadTypeRandom);
            LoadTypeIsSeries = (cboLoadType.SelectedItem.ToString() == LoadTypeSeries);
            LoadTypeIsAll = (cboLoadType.SelectedItem.ToString() == LoadTypeAll);
            LoadTypeIsLast = (cboLoadType.SelectedItem.ToString() == LoadTypeLast);

            LoadTypeHasNumber = LoadTypeIsLast || LoadTypeIsRandom;
        }

        void btnLoadFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!VM_ShokoServer.Instance.ServerOnline) return;

                ViewFiles.Refresh();

                Cursor = Cursors.Wait;
                EnableDisableControls(false);
                List<VM_VideoLocal> rawVids=new List<VM_VideoLocal>();

                if (LoadTypeIsRandom)
                {
                    rawVids = VM_ShokoServer.Instance.ShokoServices
                        .RandomFileRenamePreview(udRandomFiles.Value,
                            VM_ShokoServer.Instance.CurrentUser.JMMUserID)
                        .CastList<VM_VideoLocal>();
                }
                else if (LoadTypeIsAll)
                {
                    rawVids = VM_ShokoServer.Instance.ShokoServices
                        .RandomFileRenamePreview(int.MaxValue, VM_ShokoServer.Instance.CurrentUser.JMMUserID)
                        .CastList<VM_VideoLocal>();
                }
                else if (LoadTypeIsSeries)
                {
                    Window wdw = Window.GetWindow(this);
                    SelectGroupSeriesForm frm = new SelectGroupSeriesForm();
                    frm.Owner = wdw;
                    frm.Init();

                    bool? result = frm.ShowDialog();
                    if (result.HasValue && result.Value == true)
                    {
                        if (frm.SelectedObject.GetType() == typeof(VM_AnimeGroup_User))
                        {
                            VM_AnimeGroup_User grp = frm.SelectedObject as VM_AnimeGroup_User;
                            foreach (VM_AnimeSeries_User ser in grp.AllAnimeSeries)
                            {
                                rawVids.AddRange(VM_ShokoServer.Instance.ShokoServices
                                    .GetVideoLocalsForAnime(ser.AniDB_ID, VM_ShokoServer.Instance.CurrentUser.JMMUserID)
                                    .Cast<VM_VideoLocal>());
                            }
                        }
                        if (frm.SelectedObject.GetType() == typeof(VM_AnimeSeries_User))
                        {
                            VM_AnimeSeries_User ser = frm.SelectedObject as VM_AnimeSeries_User;
                            rawVids.AddRange(VM_ShokoServer.Instance.ShokoServices
                                .GetVideoLocalsForAnime(ser.AniDB_ID, VM_ShokoServer.Instance.CurrentUser.JMMUserID)
                                .Cast<VM_VideoLocal>());
                        }
                    }
                }
                else if (LoadTypeIsLast)
                {
                    int number = udRandomFiles.Value;
                    rawVids = VM_ShokoServer.Instance.ShokoServices
                        .SearchForFiles((int) FileSearchCriteria.LastOneHundred, number.ToString(),
                            VM_ShokoServer.Instance.CurrentUser.JMMUserID)
                        .OrderByDescending(a => a.DateTimeCreated)
                        .CastList<VM_VideoLocal>();
                }

                foreach (VM_VideoLocal vid in rawVids)
                {
                    VM_VideoLocal_Renamed ren = new VM_VideoLocal_Renamed();
                    ren.VideoLocalID = vid.VideoLocalID;
                    ren.VideoLocal = vid;
                    ren.Success = false;
                    FileResults.Add(ren);
                }

                FileCount = FileResults.Count;

                Cursor = Cursors.Arrow;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
                EnableDisableControls(true);
            }
        }

        private void EnableDisableControls(bool val)
        {
            btnLoadFiles.IsEnabled = val;
            btnPreviewFiles.IsEnabled = val;
            cboLoadType.IsEnabled = val;
            btnRenameFiles.IsEnabled = val;
        }

        private void HideShowControls(Visibility val)
        {
            btnLoadFiles.Visibility = val;
            btnPreviewFiles.Visibility = val;
            cboLoadType.Visibility = val;
        }
    }

    public class WorkerJob
    {
        public string RenameScript { get; set; }
        public int MaxFiles { get; set; }
        public ObservableCollection<VM_VideoLocal_Renamed> FileResults { get; set; }
        public bool Move { get; set; }
    }

    public class WorkerStatusContainer
    {
        public int TotalFileCount { get; set; }
        public int CurrentFile { get; set; }

        public WorkerStatusContainer()
        {
        }

        public WorkerStatusContainer(int totalFileCount, int currentFile)
        {
            TotalFileCount = totalFileCount;
            CurrentFile = currentFile;
        }
    }

    public class Controller
    {
        public string RenamerType { get; set; }
        public string Description { get; set; }
    }
}

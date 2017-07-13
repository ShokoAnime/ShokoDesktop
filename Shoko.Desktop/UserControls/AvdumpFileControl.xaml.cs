using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Shoko.Commons.Extensions;
using Shoko.Commons.Utils;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for AvdumpFileControl.xaml
    /// </summary>
    public partial class AvdumpFileControl : UserControl
    {
        private readonly BackgroundWorker workerAvdump = new BackgroundWorker();
        private List<CancellationTokenSource> runningTasks = new List<CancellationTokenSource>();

        public VM_AniDB_Anime SelectedAnime { get; set; }

        public ICollectionView ViewAnime { get; set; }
        public ObservableCollection<VM_AniDB_Anime> AllAnime { get; set; }

        public static readonly DependencyProperty IsAnimeNotPopulatedProperty = DependencyProperty.Register("IsAnimeNotPopulated",
            typeof(bool), typeof(AvdumpFileControl), new UIPropertyMetadata(true, null));

        public bool IsAnimeNotPopulated
        {
            get => (bool)GetValue(IsAnimeNotPopulatedProperty);
            set => SetValue(IsAnimeNotPopulatedProperty, value);
        }

        public static readonly DependencyProperty IsAnimePopulatedProperty = DependencyProperty.Register("IsAnimePopulated",
            typeof(bool), typeof(AvdumpFileControl), new UIPropertyMetadata(false, null));


        public bool IsAnimePopulated
        {
            get => (bool)GetValue(IsAnimePopulatedProperty);
            set => SetValue(IsAnimePopulatedProperty, value);
        }

        public static readonly DependencyProperty AnimeURLProperty = DependencyProperty.Register("AnimeURL",
            typeof(string), typeof(AvdumpFileControl), new UIPropertyMetadata("", null));


        public string AnimeURL
        {
            get => (string)GetValue(AnimeURLProperty);
            set => SetValue(AnimeURLProperty, value);
        }

        public static readonly DependencyProperty AvdumpDetailsNotValidProperty = DependencyProperty.Register("AvdumpDetailsNotValid",
            typeof(bool), typeof(AvdumpFileControl), new UIPropertyMetadata(false, null));


        public bool AvdumpDetailsNotValid
        {
            get => (bool)GetValue(AvdumpDetailsNotValidProperty);
            set => SetValue(AvdumpDetailsNotValidProperty, value);
        }

        public static readonly DependencyProperty ValidED2KDumpProperty = DependencyProperty.Register("ValidED2KDump",
            typeof(bool), typeof(AvdumpFileControl), new UIPropertyMetadata(false, null));


        public bool ValidED2KDump
        {
            get => (bool)GetValue(ValidED2KDumpProperty);
            set => SetValue(ValidED2KDumpProperty, value);
        }

        public static readonly DependencyProperty DumpSingleProperty = DependencyProperty.Register("DumpSingle",
            typeof(bool), typeof(AvdumpFileControl), new UIPropertyMetadata(false, null));


        public bool DumpSingle
        {
            get => (bool)GetValue(DumpSingleProperty);
            set => SetValue(DumpSingleProperty, value);
        }

        public static readonly DependencyProperty DumpMultipleProperty = DependencyProperty.Register("DumpMultiple",
            typeof(bool), typeof(AvdumpFileControl), new UIPropertyMetadata(false, null));


        public bool DumpMultiple
        {
            get => (bool)GetValue(DumpMultipleProperty);
            set => SetValue(DumpMultipleProperty, value);
        }

        public static readonly DependencyProperty AvDumpTextProperty = DependencyProperty.Register("AvDumpText",
            typeof(string), typeof(AvdumpFileControl), new UIPropertyMetadata("", null));


        public string AvDumpText
        {
            get => (string)GetValue(AvDumpTextProperty);
            set => SetValue(AvDumpTextProperty, value);
        }

        public string SelectedCount
        {
            get
            {
                var dumpList = DataContext as MultipleAvdumps;
                if (dumpList != null) return dumpList.SelectedCount.ToString();
                if (DataContext is VM_AVDump) return string.Intern("1");
                return string.Intern("0");
            }
        }

        public AvdumpFileControl()
        {
            InitializeComponent();

            SetSelectedAnime(null);

            AllAnime = new ObservableCollection<VM_AniDB_Anime>();

            btnClearAnimeSearch.Click += btnClearAnimeSearch_Click;
            txtAnimeSearch.TextChanged += txtAnimeSearch_TextChanged;
            lbAnime.SelectionChanged += lbAnime_SelectionChanged;
            hlURL.Click += hlURL_Click;
            btnClipboard.Click += btnClipboard_Click;

            workerAvdump.DoWork += workerAvdump_DoWork;
            workerAvdump.RunWorkerCompleted += workerAvdump_RunWorkerCompleted;

            DataContextChanged += AvdumpFileControl_DataContextChanged;
            AvdumpDetailsNotValid = string.IsNullOrEmpty(VM_ShokoServer.Instance.AniDB_AVDumpClientPort) || string.IsNullOrEmpty(VM_ShokoServer.Instance.AniDB_AVDumpKey);

            try
            {
                ViewAnime = CollectionViewSource.GetDefaultView(AllAnime);
                ViewAnime.Filter = AnimeSearchFilter;

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void AvdumpFileControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                DumpSingle = false;
                DumpMultiple = false;

                if (DataContext == null) return;
                

                if (DataContext.GetType() == typeof(VM_AVDump))
                {
                    VM_AVDump dump = DataContext as VM_AVDump;
                    if (dump != null)
                    {
                        AllAnime.Clear();
                        SearchAnime(dump);

                        if (string.IsNullOrEmpty(dump.AVDumpFullResult))
                        {
                            string ed2kDump = "Pre-calculated ED2K Dump string" + Environment.NewLine;
                            ed2kDump += "---------------------------" + Environment.NewLine;
                            ed2kDump += "This does not mean the data has been uploaded to AniDB yet" + Environment.NewLine;
                            ed2kDump += "---------------------------" + Environment.NewLine;
                            ed2kDump += $@"ed2k://|file|{dump.FileName}|{dump.FileSize}|{dump.VideoLocal.Hash}|/" + Environment.NewLine;

                            dump.AVDumpFullResult = ed2kDump;
                        }

                        dump.ED2KDump = Utils.GetED2KDump(dump.AVDumpFullResult);
                        SetED2KDump(dump.ED2KDump);
                    }
                    DumpSingle = true;
                }
                else if (DataContext.GetType() == typeof(MultipleAvdumps))
                {
                    MultipleAvdumps dumpList = DataContext as MultipleAvdumps;
                    AllAnime.Clear();

                    string massAvDump = "";
                    if (dumpList != null && dumpList.AVDumps.Count >= 1)
                    {
                        SearchAnime(dumpList.AVDumps[0]);

                        foreach (VM_AVDump dump in dumpList.AVDumps)
                        {
                            if (string.IsNullOrEmpty(dump.AVDumpFullResult))
                            {
                                string ed2kDump = "Pre-calculated ED2K Dump string" + Environment.NewLine;
                                ed2kDump += "---------------------------" + Environment.NewLine;
                                ed2kDump += "This does not mean the data has been uploaded to AniDB yet" + Environment.NewLine;
                                ed2kDump += "---------------------------" + Environment.NewLine;
                                ed2kDump += $@"ed2k://|file|{dump.FileName}|{dump.FileSize}|{dump.VideoLocal.Hash}|/" + Environment.NewLine;

                                dump.AVDumpFullResult = ed2kDump;
                            }
                            dump.ED2KDump = Utils.GetED2KDump(dump.AVDumpFullResult);
                            massAvDump += dump.ED2KDump + Environment.NewLine;
                        }
                    }
                    SetED2KDump(massAvDump);
                    DumpMultiple = true;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnClipboard_Click(object sender, RoutedEventArgs e)
        {
            SetED2KDump(AvDumpText);
        }

        private void SetED2KDump(string result)
        {
            ValidED2KDump = false;
            Clipboard.Clear();
            Clipboard.SetDataObject("");
            AvDumpText = "";

            if (string.IsNullOrEmpty(result)) return;

            Clipboard.SetDataObject(result);
            ValidED2KDump = true;
            AvDumpText = result;
        }

        void btnAvdumpFile_Click(object sender, RoutedEventArgs e)
        {
            btnClipboard.IsEnabled = false;
            ValidED2KDump = false;
            txtOutput.Text = "Processing...";
            workerAvdump.RunWorkerAsync(DataContext as VM_VideoLocal);
        }

        public async void SearchAnime(object argument)
        {
            if (runningTasks.Count > 0)
            {
                foreach (CancellationTokenSource runningTask in runningTasks)
                    runningTask.Cancel();
            }
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            runningTasks.Add(tokenSource);

            Progress<int> progress = new Progress<int>(ReportProgress);

            var series = await Task.Run(() => SearchAnime(token, argument, progress), token);
            AllAnime.Clear();
            series.ForEach(a => AllAnime.Add(a));
            if (AllAnime.Count >= 1)
                lbAnime.SelectedIndex = 0;
        }

        private List<VM_AniDB_Anime> SearchAnime(CancellationToken token, object argument, IProgress<int> progress)
        {
            progress.Report(0);
            List<VM_AniDB_Anime> tempAnime = new List<VM_AniDB_Anime>();
            SearchAnime(token, argument, tempAnime);
            progress.Report(100);
            return tempAnime;
        }

        private void SearchAnime(CancellationToken token, object argument, List<VM_AniDB_Anime> tempAnime)
        {
            VM_AVDump avdump = argument as VM_AVDump ?? (argument as MultipleAvdumps)?.AVDumps[0];

            if (avdump == null) return;

            foreach (VM_AniDB_Anime anime in VM_ShokoServer.Instance.ShokoServices
                .SearchAnimeWithFilename(VM_ShokoServer.Instance.CurrentUser.JMMUserID,
                    avdump.VideoLocal.ClosestAnimeMatchString).CastList<VM_AniDB_Anime>())
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                tempAnime.Add(anime);
            }

            if (tempAnime.Count > 0) return;
            if (token.IsCancellationRequested)
            {
                return;
            }

            foreach (VM_AniDB_Anime anime in VM_ShokoServer.Instance.ShokoServices.GetAllAnime()
                .CastList<VM_AniDB_Anime>())
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                tempAnime.Add(anime);
            }
        }

        public void ReportProgress(int progress)
        {
            if (progress == 0)
            {
                txtAnimeSearch.Text = string.Intern("Loading...");
                txtAnimeSearch.IsEnabled = false;
                txtAnimeSearch.IsReadOnly = true;
                txtAnimeSearch.Focusable = false;

                lbAnime.IsEnabled = false;

                btnClearAnimeSearch.IsEnabled = false;
            }
            else
            {
                txtAnimeSearch.IsReadOnly = false;
                txtAnimeSearch.Focusable = true;
                txtAnimeSearch.Text = string.Empty;
                txtAnimeSearch.IsEnabled = true;

                lbAnime.IsEnabled = true;

                btnClearAnimeSearch.IsEnabled = true;
            }
        }

        void workerAvdump_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            btnClipboard.IsEnabled = true;

            string result = e.Result.ToString();
            txtOutput.Text = result;

            SetED2KDump(Utils.GetED2KDump(result));
        }

        void workerAvdump_DoWork(object sender, DoWorkEventArgs e)
        {
            VM_VideoLocal vid = e.Argument as VM_VideoLocal;

            //Create process
            Process pProcess = new Process();

            //strCommand is path and file name of command to run
            string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(appPath, "AVDump2CL.exe");

            if (!File.Exists(filePath))
            {
                e.Result = "Could not find AvDump2 CLI: " + filePath;
                return;
            }
            if (string.IsNullOrEmpty(vid.GetLocalFileSystemFullPath()))
            {
                e.Result = "Unable to map video file : " + vid.FileName;
                return;
            }
            if (!File.Exists(vid.GetLocalFileSystemFullPath()))
            {
                e.Result = "Could not find Video File: " + vid.GetLocalFileSystemFullPath();
                return;
            }

            pProcess.StartInfo.FileName = filePath;

            //strCommandParameters are parameters to pass to program
            string fileName = (char)34 + vid.GetLocalFileSystemFullPath() + (char)34;

            pProcess.StartInfo.Arguments =
                $@" --Auth={VM_ShokoServer.Instance.AniDB_Username}:{VM_ShokoServer.Instance.AniDB_AVDumpKey} --LPort={VM_ShokoServer.Instance.AniDB_AVDumpClientPort} --PrintEd2kLink -t {fileName}";

            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();
            string strOutput = pProcess.StandardOutput.ReadToEnd();

            //Wait for process to finish
            pProcess.WaitForExit();

            e.Result = strOutput;
        }

        void hlURL_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri(string.Format(Constants.URLS.AniDB_Series_NewRelease, SelectedAnime.AnimeID));
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
        }

        void btnClearAnimeSearch_Click(object sender, RoutedEventArgs e)
        {
            txtAnimeSearch.Text = "";
        }

        void lbAnime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VM_AniDB_Anime anime = lbAnime.SelectedItem as VM_AniDB_Anime;
            if (anime == null) return;

            SetSelectedAnime(anime);
        }

        private void SetSelectedAnime(VM_AniDB_Anime anime)
        {
            if (anime != null)
            {
                IsAnimeNotPopulated = false;
                IsAnimePopulated = true;
                SelectedAnime = anime;
            }
            else
            {
                IsAnimeNotPopulated = true;
                IsAnimePopulated = false;
                SelectedAnime = null;
            }
        }

        void txtAnimeSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewAnime.Refresh();
        }

        private bool AnimeSearchFilter(object obj)
        {
            VM_AniDB_Anime anime = obj as VM_AniDB_Anime;
            if (anime == null) return true;

            return GroupSearchFilterHelper.EvaluateAnimeTextSearch(anime, txtAnimeSearch.Text);
        }
    }
}

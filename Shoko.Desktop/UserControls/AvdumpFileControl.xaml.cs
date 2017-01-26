using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for AvdumpFileControl.xaml
    /// </summary>
    public partial class AvdumpFileControl : UserControl
    {
        BackgroundWorker workerAvdump = new BackgroundWorker();

        public VM_AniDB_Anime SelectedAnime { get; set; }

        public ICollectionView ViewAnime { get; set; }
        public ObservableCollection<VM_AniDB_Anime> AllAnime { get; set; }

        public static readonly DependencyProperty IsAnimeNotPopulatedProperty = DependencyProperty.Register("IsAnimeNotPopulated",
            typeof(bool), typeof(AvdumpFileControl), new UIPropertyMetadata(true, null));

        public bool IsAnimeNotPopulated
        {
            get { return (bool)GetValue(IsAnimeNotPopulatedProperty); }
            set { SetValue(IsAnimeNotPopulatedProperty, value); }
        }

        public static readonly DependencyProperty IsAnimePopulatedProperty = DependencyProperty.Register("IsAnimePopulated",
            typeof(bool), typeof(AvdumpFileControl), new UIPropertyMetadata(false, null));


        public bool IsAnimePopulated
        {
            get { return (bool)GetValue(IsAnimePopulatedProperty); }
            set { SetValue(IsAnimePopulatedProperty, value); }
        }

        public static readonly DependencyProperty AnimeURLProperty = DependencyProperty.Register("AnimeURL",
            typeof(string), typeof(AvdumpFileControl), new UIPropertyMetadata("", null));


        public string AnimeURL
        {
            get { return (string)GetValue(AnimeURLProperty); }
            set { SetValue(AnimeURLProperty, value); }
        }

        public static readonly DependencyProperty AvdumpDetailsNotValidProperty = DependencyProperty.Register("AvdumpDetailsNotValid",
            typeof(bool), typeof(AvdumpFileControl), new UIPropertyMetadata(false, null));


        public bool AvdumpDetailsNotValid
        {
            get { return (bool)GetValue(AvdumpDetailsNotValidProperty); }
            set { SetValue(AvdumpDetailsNotValidProperty, value); }
        }

        public static readonly DependencyProperty ValidED2KDumpProperty = DependencyProperty.Register("ValidED2KDump",
            typeof(bool), typeof(AvdumpFileControl), new UIPropertyMetadata(false, null));


        public bool ValidED2KDump
        {
            get { return (bool)GetValue(ValidED2KDumpProperty); }
            set { SetValue(ValidED2KDumpProperty, value); }
        }

        public static readonly DependencyProperty DumpSingleProperty = DependencyProperty.Register("DumpSingle",
            typeof(bool), typeof(AvdumpFileControl), new UIPropertyMetadata(false, null));


        public bool DumpSingle
        {
            get { return (bool)GetValue(DumpSingleProperty); }
            set { SetValue(DumpSingleProperty, value); }
        }

        public static readonly DependencyProperty DumpMultipleProperty = DependencyProperty.Register("DumpMultiple",
            typeof(bool), typeof(AvdumpFileControl), new UIPropertyMetadata(false, null));


        public bool DumpMultiple
        {
            get { return (bool)GetValue(DumpMultipleProperty); }
            set { SetValue(DumpMultipleProperty, value); }
        }

        public static readonly DependencyProperty AvDumpTextProperty = DependencyProperty.Register("AvDumpText",
            typeof(string), typeof(AvdumpFileControl), new UIPropertyMetadata("", null));


        public string AvDumpText
        {
            get { return (string)GetValue(AvDumpTextProperty); }
            set { SetValue(AvDumpTextProperty, value); }
        }

        public AvdumpFileControl()
        {
            InitializeComponent();

            SetSelectedAnime(null);

            AllAnime = new ObservableCollection<VM_AniDB_Anime>();

            btnClearAnimeSearch.Click += new RoutedEventHandler(btnClearAnimeSearch_Click);
            txtAnimeSearch.TextChanged += new TextChangedEventHandler(txtAnimeSearch_TextChanged);
            lbAnime.SelectionChanged += new SelectionChangedEventHandler(lbAnime_SelectionChanged);
            hlURL.Click += new RoutedEventHandler(hlURL_Click);
            btnClipboard.Click += new RoutedEventHandler(btnClipboard_Click);

            workerAvdump.DoWork += new DoWorkEventHandler(workerAvdump_DoWork);
            workerAvdump.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerAvdump_RunWorkerCompleted);

            DataContextChanged += new DependencyPropertyChangedEventHandler(AvdumpFileControl_DataContextChanged);
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
                        foreach (VM_AniDB_Anime anime in VM_AniDB_Anime.BestLevenshteinDistanceMatches(dump.VideoLocal.ClosestAnimeMatchString, 10))
                        {
                            AllAnime.Add(anime);
                        }
                        if (AllAnime.Count > 0)
                            lbAnime.SelectedIndex = 0;

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

                if (DataContext.GetType() == typeof(MultipleAvdumps))
                {
                    MultipleAvdumps dumpList = DataContext as MultipleAvdumps;
                    AllAnime.Clear();

                    foreach (VM_AniDB_Anime anime in VM_AniDB_Anime.BestLevenshteinDistanceMatches(dumpList.AVDumps[0].VideoLocal.ClosestAnimeMatchString, 10))
                        AllAnime.Add(anime);

                    if (AllAnime.Count > 0)
                        lbAnime.SelectedIndex = 0;

                    string massAvDump = "";
                    if (dumpList != null)
                    {

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
            string appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
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
            Uri uri = new Uri(string.Format(Models.Constants.URLS.AniDB_Series_NewRelease, SelectedAnime.AnimeID));
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

using JMMClient.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for AvdumpFileControl.xaml
    /// </summary>
    public partial class AvdumpFileControl : UserControl
    {
        BackgroundWorker workerAvdump = new BackgroundWorker();

        public AniDB_AnimeVM SelectedAnime { get; set; }

        public ICollectionView ViewAnime { get; set; }
        public ObservableCollection<AniDB_AnimeVM> AllAnime { get; set; }

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

            AllAnime = new ObservableCollection<AniDB_AnimeVM>();

            btnClearAnimeSearch.Click += new RoutedEventHandler(btnClearAnimeSearch_Click);
            txtAnimeSearch.TextChanged += new TextChangedEventHandler(txtAnimeSearch_TextChanged);
            lbAnime.SelectionChanged += new SelectionChangedEventHandler(lbAnime_SelectionChanged);
            hlURL.Click += new RoutedEventHandler(hlURL_Click);
            btnClipboard.Click += new RoutedEventHandler(btnClipboard_Click);

            workerAvdump.DoWork += new DoWorkEventHandler(workerAvdump_DoWork);
            workerAvdump.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerAvdump_RunWorkerCompleted);

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(AvdumpFileControl_DataContextChanged);

            AvdumpDetailsNotValid = string.IsNullOrEmpty(JMMServerVM.Instance.AniDB_AVDumpClientPort) || string.IsNullOrEmpty(JMMServerVM.Instance.AniDB_AVDumpKey);

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

                if (this.DataContext == null) return;

                if (this.DataContext.GetType() == typeof(AVDumpVM))
                {
                    AVDumpVM dump = this.DataContext as AVDumpVM;
                    if (dump != null)
                    {
                        AllAnime.Clear();
                        foreach (AniDB_AnimeVM anime in AniDB_AnimeVM.BestLevenshteinDistanceMatches(dump.VideoLocal.ClosestAnimeMatchString, 10))
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
                            ed2kDump += string.Format(@"ed2k://|file|{0}|{1}|{2}|/", dump.FileName, dump.FileSize, dump.VideoLocal.Hash) + Environment.NewLine;

                            dump.AVDumpFullResult = ed2kDump;
                        }

                        dump.ED2KDump = Utils.GetED2KDump(dump.AVDumpFullResult);
                        SetED2KDump(dump.ED2KDump);
                    }
                    DumpSingle = true;
                }

                if (this.DataContext.GetType() == typeof(MultipleAvdumps))
                {
                    MultipleAvdumps dumpList = this.DataContext as MultipleAvdumps;
                    AllAnime.Clear();

                    foreach (AniDB_AnimeVM anime in AniDB_AnimeVM.BestLevenshteinDistanceMatches(dumpList.AVDumps[0].VideoLocal.ClosestAnimeMatchString, 10))
                        AllAnime.Add(anime);

                    if (AllAnime.Count > 0)
                        lbAnime.SelectedIndex = 0;

                    string massAvDump = "";
                    if (dumpList != null)
                    {

                        foreach (AVDumpVM dump in dumpList.AVDumps)
                        {
                            if (string.IsNullOrEmpty(dump.AVDumpFullResult))
                            {
                                string ed2kDump = "Pre-calculated ED2K Dump string" + Environment.NewLine;
                                ed2kDump += "---------------------------" + Environment.NewLine;
                                ed2kDump += "This does not mean the data has been uploaded to AniDB yet" + Environment.NewLine;
                                ed2kDump += "---------------------------" + Environment.NewLine;
                                ed2kDump += string.Format(@"ed2k://|file|{0}|{1}|{2}|/", dump.FileName, dump.FileSize, dump.VideoLocal.Hash) + Environment.NewLine;

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
            workerAvdump.RunWorkerAsync(this.DataContext as VideoLocalVM);
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
            VideoLocalVM vid = e.Argument as VideoLocalVM;

            //Create process
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();

            //strCommand is path and file name of command to run
            string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string filePath = System.IO.Path.Combine(appPath, "AVDump2CL.exe");

            if (!File.Exists(filePath))
            {
                e.Result = "Could not find AvDump2 CLI: " + filePath;
                return;
            }
            if (string.IsNullOrEmpty(vid?.LocalFileSystemFullPath))
            {
                e.Result = "Unable to map video file : " + vid.FileName;
                return;
            }
            if (!File.Exists(vid.LocalFileSystemFullPath))
            {
                e.Result = "Could not find Video File: " + vid.LocalFileSystemFullPath;
                return;
            }

            pProcess.StartInfo.FileName = filePath;

            //strCommandParameters are parameters to pass to program
            string fileName = (char)34 + vid.LocalFileSystemFullPath + (char)34;

            pProcess.StartInfo.Arguments =
                string.Format(@" --Auth={0}:{1} --LPort={2} --PrintEd2kLink -t {3}", JMMServerVM.Instance.AniDB_Username, JMMServerVM.Instance.AniDB_AVDumpKey,
                JMMServerVM.Instance.AniDB_AVDumpClientPort, fileName);

            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
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
            AniDB_AnimeVM anime = lbAnime.SelectedItem as AniDB_AnimeVM;
            if (anime == null) return;

            SetSelectedAnime(anime);
        }

        private void SetSelectedAnime(AniDB_AnimeVM anime)
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
            AniDB_AnimeVM anime = obj as AniDB_AnimeVM;
            if (anime == null) return true;

            return GroupSearchFilterHelper.EvaluateAnimeTextSearch(anime, txtAnimeSearch.Text);
        }
    }
}

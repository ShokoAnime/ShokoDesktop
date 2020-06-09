﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using NLog;
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
        private readonly List<CancellationTokenSource> runningTasks = new List<CancellationTokenSource>();

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

        public static readonly DependencyProperty OutputTextProperty = DependencyProperty.Register("OutputText",
            typeof(string), typeof(AvdumpFileControl), new UIPropertyMetadata("", null));


        public string OutputText
        {
            get => (string)GetValue(OutputTextProperty);
            set => SetValue(OutputTextProperty, value);
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
                            ed2kDump += "----------------------------------------------------------" + Environment.NewLine;
                            ed2kDump += "This does not mean the data has been uploaded to AniDB yet" + Environment.NewLine;
                            ed2kDump += "----------------------------------------------------------" + Environment.NewLine;
                            ed2kDump += $@"ed2k://|file|{dump.FileName}|{dump.FileSize}|{dump.VideoLocal.Hash}|/" + Environment.NewLine;

                            dump.AVDumpFullResult = ed2kDump;
                        }

                        OutputText = dump.AVDumpFullResult;
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

                        bool areDumped = !dumpList.AVDumps.Any(a => string.IsNullOrEmpty(a.AVDumpFullResult));
                        if (areDumped)
                        {
                            string intersect = null;
                            foreach (VM_AVDump dump in dumpList.AVDumps)
                            {
                                dump.ED2KDump = Utils.GetED2KDump(dump.AVDumpFullResult);
                                massAvDump += dump.ED2KDump + Environment.NewLine;
                                if (intersect == null) intersect = dump.AVDumpFullResult;
                                else
                                {
                                    string[] lines = intersect.Split(Environment.NewLine.ToCharArray(),
                                        StringSplitOptions.RemoveEmptyEntries);
                                    string[] avdumpLines = dump.AVDumpFullResult.Split(
                                        Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    intersect = string.Join(Environment.NewLine, lines.Intersect(avdumpLines));
                                }
                            }

                            OutputText = intersect + Environment.NewLine + Environment.NewLine + massAvDump;
                        }
                        else
                        {
                            string ed2kDump = "Pre-calculated ED2K Dump strings" + Environment.NewLine;
                            ed2kDump += "----------------------------------------------------------" + Environment.NewLine;
                            ed2kDump += "This does not mean the data has been uploaded to AniDB yet" + Environment.NewLine;
                            ed2kDump += "----------------------------------------------------------" + Environment.NewLine;
                            foreach (VM_AVDump dump in dumpList.AVDumps)
                            {
                                dump.ED2KDump = $@"ed2k://|file|{dump.FileName}|{dump.FileSize}|{dump.VideoLocal.Hash}|/";
                                massAvDump += dump.ED2KDump + Environment.NewLine;
                            }

                            ed2kDump += massAvDump;
                            OutputText = ed2kDump;
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
            AvDumpText = "";

            if (string.IsNullOrEmpty(result)) return;

            Utils.CopyToClipboard(result);
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
            lock (runningTasks)
            {
                if (runningTasks.Count > 0)
                {
                    foreach (CancellationTokenSource runningTask in runningTasks)
                        runningTask.Cancel();
                }
            }
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            lock (runningTasks)
            {
                runningTasks.Add(tokenSource);
            }

            Progress<List<VM_AniDB_Anime>> progress = new Progress<List<VM_AniDB_Anime>>(ReportProgress);

            await Task.Run(() =>
            {
                SearchAnime(token, argument, progress);
                lock (runningTasks)
                {
                    runningTasks.Remove(tokenSource);
                }
            }, token);
        }

        private void SearchAnime(CancellationToken token, object argument, IProgress<List<VM_AniDB_Anime>> progress)
        {
            List<VM_AniDB_Anime> tempAnime = new List<VM_AniDB_Anime>();
            if (token.IsCancellationRequested) return;
            progress.Report(tempAnime);
            if (token.IsCancellationRequested) return;
            SearchAnime(token, argument, tempAnime);
            if (token.IsCancellationRequested) return;
            progress.Report(tempAnime);
            return;
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

        public void ReportProgress(List<VM_AniDB_Anime> series)
        {
            if (series == null || series.Count == 0)
            {
                AllAnime.Clear();
                txtAnimeSearch.Text = string.Intern("Loading...");
                txtAnimeSearch.IsEnabled = false;
                txtAnimeSearch.IsReadOnly = true;
                txtAnimeSearch.Focusable = false;

                lbAnime.IsEnabled = false;

                btnClearAnimeSearch.IsEnabled = false;
            }
            else
            {
                AllAnime.Clear();
                series.ForEach(a => AllAnime.Add(a));

                if (AllAnime.Count >= 1)
                    lbAnime.SelectedIndex = 0;

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

            string strOutput = VM_ShokoServer.Instance.ShokoServices.AVDumpFile(vid.VideoLocalID);

            e.Result = strOutput;
        }

        void hlURL_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri(string.Format(Constants.URLS.AniDB_Series_NewRelease, SelectedAnime.AnimeID));
            Process.Start(new ProcessStartInfo
            {
                FileName = uri.AbsoluteUri,
                UseShellExecute = true
            });
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

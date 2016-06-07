using JMMClient.Forms;
using JMMClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for MissingEpisodesControl.xaml
    /// </summary>
    public partial class MissingEpisodesControl : UserControl
    {
        BackgroundWorker workerFiles = new BackgroundWorker();

        public ICollectionView ViewEpisodes { get; set; }
        public ObservableCollection<MissingEpisodeVM> MissingEpisodesCollection { get; set; }

        private List<JMMServerBinary.Contract_MissingEpisode> contracts = new List<JMMServerBinary.Contract_MissingEpisode>();

        public static readonly DependencyProperty EpisodeCountProperty = DependencyProperty.Register("EpisodeCount",
            typeof(int), typeof(MissingEpisodesControl), new UIPropertyMetadata(0, null));

        public int EpisodeCount
        {
            get { return (int)GetValue(EpisodeCountProperty); }
            set { SetValue(EpisodeCountProperty, value); }
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading",
            typeof(bool), typeof(MissingEpisodesControl), new UIPropertyMetadata(false, null));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set
            {
                SetValue(IsLoadingProperty, value);
                IsNotLoading = !IsLoading;
            }
        }

        public static readonly DependencyProperty IsNotLoadingProperty = DependencyProperty.Register("IsNotLoading",
            typeof(bool), typeof(MissingEpisodesControl), new UIPropertyMetadata(true, null));

        public bool IsNotLoading
        {
            get { return (bool)GetValue(IsNotLoadingProperty); }
            set { SetValue(IsNotLoadingProperty, value); }
        }

        public static readonly DependencyProperty ReadyToExportProperty = DependencyProperty.Register("ReadyToExport",
            typeof(bool), typeof(MissingEpisodesControl), new UIPropertyMetadata(false, null));

        public static readonly DependencyProperty StatusMessageProperty = DependencyProperty.Register("StatusMessage",
            typeof(string), typeof(MissingEpisodesControl), new UIPropertyMetadata("", null));

        public string StatusMessage
        {
            get { return (string)GetValue(StatusMessageProperty); }
            set { SetValue(StatusMessageProperty, value); }
        }

        public bool ReadyToExport
        {
            get { return (bool)GetValue(ReadyToExportProperty); }
            set { SetValue(ReadyToExportProperty, value); }
        }

        public MissingEpisodesControl()
        {
            InitializeComponent();

            ReadyToExport = false;
            IsLoading = false;

            MissingEpisodesCollection = new ObservableCollection<MissingEpisodeVM>();
            ViewEpisodes = CollectionViewSource.GetDefaultView(MissingEpisodesCollection);
            ViewEpisodes.SortDescriptions.Add(new SortDescription("AnimeTitle", ListSortDirection.Ascending));
            ViewEpisodes.SortDescriptions.Add(new SortDescription("EpisodeType", ListSortDirection.Ascending));
            ViewEpisodes.SortDescriptions.Add(new SortDescription("EpisodeNumber", ListSortDirection.Ascending));

            btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            btnExport.Click += new RoutedEventHandler(btnExport_Click);

            workerFiles.DoWork += new DoWorkEventHandler(workerFiles_DoWork);
            workerFiles.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerFiles_RunWorkerCompleted);

            btnSelectColumns.Click += new RoutedEventHandler(btnSelectColumns_Click);

            cboAiringFilter.Items.Clear();
            cboAiringFilter.Items.Add(JMMClient.Properties.Resources.Random_All);
            cboAiringFilter.Items.Add(JMMClient.Properties.Resources.MissingEpisodes_StillAiring);
            cboAiringFilter.Items.Add(JMMClient.Properties.Resources.MissingEpisodes_FinishedAiring);
            cboAiringFilter.SelectedIndex = 0;
        }

        void btnSelectColumns_Click(object sender, RoutedEventArgs e)
        {
            MissingEpsColumnsForm dlg = new MissingEpsColumnsForm();
            dlg.Owner = Window.GetWindow(this);
            dlg.ShowDialog();
        }

        void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsLoading = true;
                btnRefresh.IsEnabled = false;
                btnExport.IsEnabled = false;
                ReadyToExport = false;
                this.Cursor = Cursors.Wait;

                StatusMessage = JMMClient.Properties.Resources.Exporting;

                string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string logName = System.IO.Path.Combine(appPath, "AnimeEpisodes.txt");

                string export = "";
                foreach (MissingEpisodeVM missingEp in MissingEpisodesCollection)
                {
                    string[] columns = AppSettings.MissingEpsExportColumns.Split(';');
                    for (int i = 0; i < columns.Length; i++)
                    {
                        if (i == 0 && columns[i] == "1") export += string.Format("{0} , ", missingEp.AnimeTitle);
                        if (i == 1 && columns[i] == "1") export += string.Format("{0} , ", missingEp.AnimeID);
                        if (i == 2 && columns[i] == "1") export += string.Format("{0} , ", missingEp.EpisodeTypeAndNumber);
                        if (i == 3 && columns[i] == "1") export += string.Format("{0} , ", missingEp.EpisodeID);
                        if (i == 4 && columns[i] == "1") export += string.Format("{0} , ", missingEp.GroupFileSummary);
                        if (i == 5 && columns[i] == "1") export += string.Format("{0} , ", missingEp.GroupFileSummarySimple);
                        if (i == 6 && columns[i] == "1") export += string.Format("{0} , ", missingEp.AniDB_SiteURL);
                        if (i == 7 && columns[i] == "1") export += string.Format("{0} , ", missingEp.Episode_SiteURL);

                    }

                    export += Environment.NewLine;
                }

                StreamWriter Tex = new StreamWriter(logName);
                Tex.Write(export);
                Tex.Flush();
                Tex.Close();

                Process.Start(logName);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            IsLoading = false;
            btnRefresh.IsEnabled = true;
            btnExport.IsEnabled = true;
            ReadyToExport = true;
            this.Cursor = Cursors.Arrow;

            StatusMessage = "";
        }

        void workerFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            contracts = e.Result as List<JMMServerBinary.Contract_MissingEpisode>;
            foreach (JMMServerBinary.Contract_MissingEpisode mf in contracts)
                MissingEpisodesCollection.Add(new MissingEpisodeVM(mf));

            EpisodeCount = contracts.Count;
            ReadyToExport = EpisodeCount >= 1;
            btnRefresh.IsEnabled = true;
            IsLoading = false;
        }

        void workerFiles_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                WorkRequest wr = e.Argument as WorkRequest;
                List<JMMServerBinary.Contract_MissingEpisode> contractsTemp = JMMServerVM.Instance.clientBinaryHTTP.GetMissingEpisodes(
                    JMMServerVM.Instance.CurrentUser.JMMUserID.Value, wr.MyGroupsOnly, wr.RegularEpisodesOnly, (int)wr.AiringFilter);
                e.Result = contractsTemp;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            IsLoading = true;
            btnRefresh.IsEnabled = false;
            MissingEpisodesCollection.Clear();
            ReadyToExport = false;
            EpisodeCount = 0;

            AiringState state = (AiringState)cboAiringFilter.SelectedIndex;

            StatusMessage = JMMClient.Properties.Resources.Loading;
            WorkRequest wr = new WorkRequest(chkMyGroupsOnly.IsChecked.Value, chkRegularEpisodesOnly.IsChecked.Value, state);

            workerFiles.RunWorkerAsync(wr);
        }
    }

    class WorkRequest
    {
        public bool MyGroupsOnly { get; set; }
        public bool RegularEpisodesOnly { get; set; }
        public AiringState AiringFilter { get; set; }

        public WorkRequest(bool myGroups, bool regEps, AiringState state)
        {
            MyGroupsOnly = myGroups;
            RegularEpisodesOnly = regEps;
            AiringFilter = state;
        }
    }
}

using JMMClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for CommunityMaint.xaml
    /// </summary>
    public partial class CommunityMaint : UserControl
    {
        public ICollectionView ViewTrakt { get; set; }
        public ObservableCollection<TraktSeriesData> TraktResults { get; set; }

        public static readonly DependencyProperty ShowStartButtonProperty = DependencyProperty.Register("ShowStartButton",
            typeof(bool), typeof(CommunityMaint), new UIPropertyMetadata(true, null));

        public bool ShowStartButton
        {
            get { return (bool)GetValue(ShowStartButtonProperty); }
            set { SetValue(ShowStartButtonProperty, value); }
        }

        public static readonly DependencyProperty ShowPauseButtonProperty = DependencyProperty.Register("ShowPauseButton",
            typeof(bool), typeof(CommunityMaint), new UIPropertyMetadata(false, null));

        public bool ShowPauseButton
        {
            get { return (bool)GetValue(ShowPauseButtonProperty); }
            set { SetValue(ShowPauseButtonProperty, value); }
        }

        public static readonly DependencyProperty ShowStopButtonProperty = DependencyProperty.Register("ShowStopButton",
            typeof(bool), typeof(CommunityMaint), new UIPropertyMetadata(false, null));

        public bool ShowStopButton
        {
            get { return (bool)GetValue(ShowStopButtonProperty); }
            set { SetValue(ShowStopButtonProperty, value); }
        }

        public static readonly DependencyProperty WorkerStatusProperty = DependencyProperty.Register("WorkerStatus",
            typeof(string), typeof(CommunityMaint), new UIPropertyMetadata("", null));

        public string WorkerStatus
        {
            get { return (string)GetValue(WorkerStatusProperty); }
            set { SetValue(WorkerStatusProperty, value); }
        }

        private bool stopWorker = false;
        BackgroundWorker dataWorker = new BackgroundWorker();

        private readonly string FilterTypeAll = JMMClient.Properties.Resources.Random_All;
        private readonly string FilterTypeMissing = JMMClient.Properties.Resources.Community_LinkMissing;
        private readonly string FilterTypeDifferent = JMMClient.Properties.Resources.Community_LinkDifferent;

        public CommunityMaint()
        {
            InitializeComponent();

            TraktResults = new ObservableCollection<TraktSeriesData>();
            ViewTrakt = CollectionViewSource.GetDefaultView(TraktResults);
            //ViewTrakt.SortDescriptions.Add(new SortDescription("SeriesName", ListSortDirection.Ascending));

            btnStart.Click += BtnStart_Click;
            btnPause.Click += BtnPause_Click;
            btnStop.Click += BtnStop_Click;

            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            string cult = appSettings["Culture"];
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cult);

            cboFilterType.Items.Clear();
            cboFilterType.Items.Add(JMMClient.Properties.Resources.Random_All);
            cboFilterType.Items.Add(JMMClient.Properties.Resources.Community_LinkMissing);
            //cboFilterType.Items.Add(FilterTypeIncorrect);
            cboFilterType.Items.Add(JMMClient.Properties.Resources.Community_LinkDifferent);
            cboFilterType.SelectionChanged += new SelectionChangedEventHandler(cboFilterType_SelectionChanged);
            cboFilterType.SelectedIndex = 0;

            dataWorker.DoWork += new DoWorkEventHandler(dataWorker_DoWork);
            dataWorker.WorkerSupportsCancellation = true;
            dataWorker.WorkerReportsProgress = true;
            dataWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(dataWorker_RunWorkerCompleted);
            dataWorker.ProgressChanged += new ProgressChangedEventHandler(dataWorker_ProgressChanged);
        }

        void cboFilterType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewTrakt.Filter = FileFilter;
        }

        private bool FileFilter(object obj)
        {
            TraktSeriesData ser = obj as TraktSeriesData;
            if (ser == null) return false;

            string filterType = cboFilterType.SelectedItem.ToString();

            if (filterType.Equals(FilterTypeAll)) return true;

            if (filterType.Equals(FilterTypeMissing) && !ser.HasUserTraktLink) return true;
            if (filterType.Equals(FilterTypeDifferent) && ser.HasCommTraktLink && !ser.UserTraktLinkMatch) return true;

            return false;
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            ShowStartButton = true;
            ShowStopButton = false;
            //ShowPauseButton = false;

            stopWorker = true;
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            ShowStartButton = true;
            ShowStopButton = true;
            //ShowPauseButton = false;
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            stopWorker = false;
            ShowStartButton = false;
            ShowStopButton = true;
            //ShowPauseButton = true;

            WorkerStatus = JMMClient.Properties.Resources.Community_GettingData;

            TraktResults.Clear();

            TraktWorkerJob job = new TraktWorkerJob();
            job.TraktData = TraktResults;
            job.CheckTraktLinks = chkTraktValid.IsChecked.Value;
            if (job.CheckTraktLinks)
                job.FixTraktLinks = chkTraktValidCleanup.IsChecked.Value;
            job.CheckCommunityLinks = chkCommRec.IsChecked.Value;
            job.MaxProblems = int.MaxValue;
            if (chkLimitSeries.IsChecked.Value && udLimitSeries.Value.HasValue)
                job.MaxProblems = udLimitSeries.Value.Value;

            dataWorker.RunWorkerAsync(job);
        }

        private void EnableDisableControls(bool val)
        {
            //btnLoadFiles.IsEnabled = val;
            //btnPreviewFiles.IsEnabled = val;
            //cboLoadType.IsEnabled = val;
        }

        void dataWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ViewTrakt.Refresh();
            TraktWorkerStatusContainer status = e.UserState as TraktWorkerStatusContainer;
            WorkerStatus = string.Format(JMMClient.Properties.Resources.Community_TraktProgress, status.CurrentAction, status.CurrentSeries, status.TotalSeriesCount, status.CurrentSeriesString);
        }

        void dataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ViewTrakt.Refresh();

            ShowStartButton = true;
            ShowStopButton = false;
            //ShowPauseButton = false;

            WorkerStatus = JMMClient.Properties.Resources.Complete;

            stopWorker = false;
            EnableDisableControls(true);
        }

        private void CommandBinding_RefreshSeries(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            if (obj.GetType() == typeof(TraktSeriesData))
            {
                TraktSeriesData traktSer = obj as TraktSeriesData;

                // Refresh Trakt links
                List<JMMServerBinary.Contract_CrossRef_AniDB_TraktV2> traktxrefs = JMMServerVM.Instance.clientBinaryHTTP.GetTraktCrossRefV2(traktSer.AnimeID);
                List<CrossRef_AniDB_TraktVMV2> links = new List<CrossRef_AniDB_TraktVMV2>();
                foreach (JMMServerBinary.Contract_CrossRef_AniDB_TraktV2 traktxref in traktxrefs)
                    links.Add(new CrossRef_AniDB_TraktVMV2(traktxref));

                traktSer.SetUserTraktLinks(links);
                traktSer.CompareTraktLinks();
            }
        }

        void dataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            TraktWorkerJob job = e.Argument as TraktWorkerJob;
            BackgroundWorker worker = sender as BackgroundWorker;

            // Get all the seies data
            List<JMMServerBinary.Contract_AnimeSeries> sersRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllSeries(JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
            List<AnimeSeriesVM> allSeries = new List<AnimeSeriesVM>();
            foreach (JMMServerBinary.Contract_AnimeSeries contract in sersRaw)
            {
                AnimeSeriesVM ser = new AnimeSeriesVM(contract);
                allSeries.Add(ser);
                if (stopWorker) return;
            }

            List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
            sortCriteria.Add(new SortPropOrFieldAndDirection("SeriesName", false, SortType.eString));
            allSeries = Sorting.MultiSort<AnimeSeriesVM>(allSeries, sortCriteria);

            if (stopWorker) return;

            System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
            {
                WorkerStatus = JMMClient.Properties.Resources.Community_TraktRef;
            });

            // get all the trakt links
            List<CrossRef_AniDB_TraktVMV2> allTraktCrossRefs = new List<CrossRef_AniDB_TraktVMV2>();
            List<JMMServerBinary.Contract_CrossRef_AniDB_TraktV2> traktxrefs = JMMServerVM.Instance.clientBinaryHTTP.GetAllTraktCrossRefs();
            foreach (JMMServerBinary.Contract_CrossRef_AniDB_TraktV2 raw in traktxrefs)
            {
                allTraktCrossRefs.Add(new CrossRef_AniDB_TraktVMV2(raw));
                if (stopWorker) return;
            }

            int counter = 0;
            int problemCount = 0;

            foreach (AnimeSeriesVM ser in allSeries)
            {
                counter++;
                //Thread.Sleep(200);

                dataWorker.ReportProgress(0, new TraktWorkerStatusContainer(JMMClient.Properties.Resources.Community_TraktPopulating, allSeries.Count, counter, ser.SeriesName));

                TraktSeriesData trakt = new TraktSeriesData(ser);

                // populate the Trakt data
                trakt.SetUserTraktLinks(allTraktCrossRefs.Where(x => x.AnimeID == ser.AniDB_ID));

                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate ()
                {
                    job.TraktData.Add(trakt);
                });

                
                if (stopWorker) return;
            }
            if (stopWorker) return;


            int curFile = 0;
            foreach (TraktSeriesData data in job.TraktData)
            {
                if (stopWorker) return;
                //Thread.Sleep(250);

                if (problemCount == job.MaxProblems) return;

                curFile++;
                dataWorker.ReportProgress(0, new TraktWorkerStatusContainer(JMMClient.Properties.Resources.Community_TraktSeriesCheck, job.TraktData.Count, curFile, data.SeriesName));

                if (stopWorker) return;
                if (job.CheckTraktLinks)
                {
                    data.Status = JMMClient.Properties.Resources.Community_TraktDataCheck;
                    bool valid = true;

                    if (data.HasUserTraktLink)
                    {
                        foreach (CrossRef_AniDB_TraktVMV2 xref in data.UserTraktLinks)
                        {
                            bool thisValid = JMMServerVM.Instance.clientBinaryHTTP.CheckTraktLinkValidity(xref.TraktID, job.FixTraktLinks);
                            if (!thisValid)
                            {
                                valid = false;
                                problemCount++;
                            }
                        }
                        data.IsTraktLinkValid = valid;
                    }
                }

                if (stopWorker) return;
                if (job.CheckCommunityLinks)
                {
                    data.Status = JMMClient.Properties.Resources.Community_TraktCompare;

                    List<JMMServerBinary.Contract_Azure_CrossRef_AniDB_Trakt> xrefs = JMMServerVM.Instance.clientBinaryHTTP.GetTraktCrossRefWebCache(data.AnimeID, false);
                    List<CrossRef_AniDB_TraktVMV2> commTraktLinks = new List<CrossRef_AniDB_TraktVMV2>();
                    foreach (JMMServerBinary.Contract_Azure_CrossRef_AniDB_Trakt xref in xrefs)
                    {
                        CrossRef_AniDB_TraktVMV2 xrefComm = new CrossRef_AniDB_TraktVMV2(xref);
                        commTraktLinks.Add(xrefComm);
                    }
                    data.SetCommTraktLinks(commTraktLinks);

                    if (data.HasUserTraktLink)
                    {
                        data.CompareTraktLinks();
                        if (!data.UserTraktLinkMatch && data.HasCommTraktLink) problemCount++;
                    }
                    else
                    { }
                }
            }
        }
    }

    public class TraktSeriesData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                var args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }

        private int animeSeriesID = 0;
        public int AnimeSeriesID
        {
            get { return animeSeriesID; }
            set
            {
                animeSeriesID = value;
                NotifyPropertyChanged("AnimeSeriesID");
            }
        }

        private int animeID = 0;
        public int AnimeID
        {
            get { return animeID; }
            set
            {
                animeID = value;
                NotifyPropertyChanged("AnimeID");
            }
        }

        private string seriesName = string.Empty;
        public string SeriesName
        {
            get { return seriesName; }
            set
            {
                seriesName = value;
                NotifyPropertyChanged("SeriesName");
            }
        }

        private string status = "";
        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                NotifyPropertyChanged("Status");
            }
        }

        #region This series has a Trakt link 

        private bool hasUserTraktLink = false;
        public bool HasUserTraktLink
        {
            get { return hasUserTraktLink; }
            set
            {
                hasUserTraktLink = value;
                NotifyPropertyChanged("HasUserTraktLink");
                SetHasTraktLinkImage();
            }
        }

        public void SetUserTraktLinks(IEnumerable<CrossRef_AniDB_TraktVMV2> links)
        {
            UserTraktLinks.Clear();
            foreach (CrossRef_AniDB_TraktVMV2 xref in links)
                UserTraktLinks.Add(xref);
            HasUserTraktLink = UserTraktLinks.Count > 0;
        }

        private string hasTraktLinkImage = @"/Images/32_clock.png";
        public string HasTraktLinkImage
        {
            get { return hasTraktLinkImage; }
            set
            {
                hasTraktLinkImage = value;
                NotifyPropertyChanged("HasTraktLinkImage");
            }
        }

        public void SetHasTraktLinkImage()
        {
            if (HasUserTraktLink)
                HasTraktLinkImage = @"/Images/16_tick.png";
            else
                HasTraktLinkImage = @"/Images/16_exclamation.png";
        }

        #endregion

        #region This series has an admin approved community link

        private bool hasCommTraktLink = false;
        public bool HasCommTraktLink
        {
            get { return hasCommTraktLink; }
            set
            {
                hasCommTraktLink = value;
                NotifyPropertyChanged("HasCommTraktLink");
                SetHasTraktCommLinkImage();
            }
        }

        public void SetHasTraktCommLinkImage()
        {
            if (HasCommTraktLink)
                HasTraktCommLinkImage = @"/Images/16_tick.png";
            else
                HasTraktCommLinkImage = @"/Images/16_exclamation.png";
        }

        private string hasTraktCommLinkImage = @"/Images/32_clock.png";
        public string HasTraktCommLinkImage
        {
            get { return hasTraktCommLinkImage; }
            set
            {
                hasTraktCommLinkImage = value;
                NotifyPropertyChanged("HasTraktCommLinkImage");
            }
        }

        public void SetCommTraktLinks(IEnumerable<CrossRef_AniDB_TraktVMV2> links)
        {
            CommTraktLinks.Clear();
            foreach (CrossRef_AniDB_TraktVMV2 xref in links)
                CommTraktLinks.Add(xref);
            HasCommTraktLink = CommTraktLinks.Count > 0;
        }

        

        #endregion

        #region This selected Trakt Link Matches the community approved links

        private bool userTraktLinkMatch = false;
        public bool UserTraktLinkMatch
        {
            get { return userTraktLinkMatch; }
            set
            {
                userTraktLinkMatch = value;
                NotifyPropertyChanged("UserTraktLinkMatch");
                SetUserTraktLinkMatchImage();
            }
        }

        public void SetUserTraktLinkMatchImage()
        {
            if (UserTraktLinkMatch)
                UserTraktLinkMatchImage = @"/Images/16_tick.png";
            else
                UserTraktLinkMatchImage = @"/Images/16_exclamation.png";
        }

        private string userTraktLinkMatchImage = @"/Images/32_clock.png";
        public string UserTraktLinkMatchImage
        {
            get { return userTraktLinkMatchImage; }
            set
            {
                userTraktLinkMatchImage = value;
                NotifyPropertyChanged("UserTraktLinkMatchImage");
            }
        }

        public void CompareTraktLinks()
        {
            if (CommTraktLinks.Count > 0)
            {
                // check if the community link matches the links the user has
                if (UserTraktLinks.Count != CommTraktLinks.Count)
                    UserTraktLinkMatch = false;
                else
                {
                    bool match = true;
                    foreach (CrossRef_AniDB_TraktVMV2 xrefAzure in CommTraktLinks)
                    {
                        bool thisMatch = false;
                        foreach (CrossRef_AniDB_TraktVMV2 userRef in UserTraktLinks)
                        {
                            if (xrefAzure.TraktID == userRef.TraktID && xrefAzure.TraktSeasonNumber == userRef.TraktSeasonNumber &&
                                xrefAzure.TraktStartEpisodeNumber == userRef.TraktStartEpisodeNumber && xrefAzure.AniDBStartEpisodeType == userRef.AniDBStartEpisodeType
                                && xrefAzure.AniDBStartEpisodeNumber == userRef.AniDBStartEpisodeNumber)
                            {
                                thisMatch = true;
                                break;
                            }
                        }

                        if (!thisMatch)
                        {
                            // we couldn't find a match for this User Trakt Link so it all fails
                            match = false;
                            break;
                        }
                    }

                    UserTraktLinkMatch = match;
                }
            }
        }

        #endregion

        #region This selected Trakt slug is valid on Trakt 

        private bool isTraktLinkValid = false;
        public bool IsTraktLinkValid
        {
            get { return isTraktLinkValid; }
            set
            {
                isTraktLinkValid = value;
                NotifyPropertyChanged("IsTraktLinkValid");
                SetIsTraktLinkValidImage();
            }
        }

        private string isTraktLinkValidImage = @"/Images/32_clock.png";
        public string IsTraktLinkValidImage
        {
            get { return isTraktLinkValidImage; }
            set
            {
                isTraktLinkValidImage = value;
                NotifyPropertyChanged("IsTraktLinkValidImage");
            }
        }

        public void SetIsTraktLinkValidImage()
        {
            if (IsTraktLinkValid)
                IsTraktLinkValidImage = @"/Images/16_tick.png";
            else
                IsTraktLinkValidImage = @"/Images/16_exclamation.png";
        }

        #endregion

        public ObservableCollection<CrossRef_AniDB_TraktVMV2> UserTraktLinks { get; set; }

        public ObservableCollection<CrossRef_AniDB_TraktVMV2> CommTraktLinks { get; set; }

        public TraktSeriesData()
        { }

        public TraktSeriesData(AnimeSeriesVM ser)
        {
            AnimeSeriesID = ser.AnimeSeriesID.Value;
            AnimeID = ser.AniDB_ID;
            SeriesName = ser.SeriesName;
            Status = "";

            UserTraktLinks = new ObservableCollection<CrossRef_AniDB_TraktVMV2>();
            CommTraktLinks = new ObservableCollection<CrossRef_AniDB_TraktVMV2>();

            SetHasTraktLinkImage();
        }
    }

    public class TraktWorkerJob
    {
        public ObservableCollection<TraktSeriesData> TraktData { get; set; }

        public bool CheckTraktLinks { get; set; }
        public bool FixTraktLinks { get; set; }
        public bool CheckCommunityLinks { get; set; }

        public int MaxProblems { get; set; }

        public TraktWorkerJob()
        {
            CheckTraktLinks = true;
            FixTraktLinks = false;
            CheckCommunityLinks = true;
            MaxProblems = 20;
        }
    }

    public class TraktWorkerStatusContainer
    {
        public string CurrentAction { get; set; }

        public int TotalSeriesCount { get; set; }
        public int CurrentSeries { get; set; }
        public string CurrentSeriesString { get; set; }

        public TraktWorkerStatusContainer()
        {
        }

        public TraktWorkerStatusContainer(string currentAction, int totalSeriesCount, int currentSeries, string currentSeriesString)
        {
            CurrentAction = currentAction;
            TotalSeriesCount = totalSeriesCount;
            CurrentSeries = currentSeries;
            CurrentSeriesString = currentSeriesString;
        }
    }
}

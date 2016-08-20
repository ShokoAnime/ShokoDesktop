using JMMClient.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DevExpress.Xpf.Core;
using CheckBox = System.Windows.Controls.CheckBox;
using Image = System.Windows.Controls.Image;
using Orientation = System.Windows.Controls.Orientation;

namespace JMMClient.Forms
{
    /// <summary>
    /// Interaction logic for DeleteFilesForm.xaml
    /// </summary>
    public partial class DeleteFilesForm : Window
    {
        public static readonly DependencyProperty GroupVideoQualityProperty = DependencyProperty.Register("GroupVideoQuality",
            typeof(GroupVideoQualityVM), typeof(DeleteFilesForm), new UIPropertyMetadata(null, null));

        public static readonly DependencyProperty AnimeIDProperty = DependencyProperty.Register("AnimeID",
            typeof(int), typeof(DeleteFilesForm), new UIPropertyMetadata(0, null));

        public static readonly DependencyProperty FileCountProperty = DependencyProperty.Register("FileCount",
            typeof(int), typeof(DeleteFilesForm), new UIPropertyMetadata(0, null));

        public static readonly DependencyProperty DeleteStatusProperty = DependencyProperty.Register("DeleteStatus",
            typeof(string), typeof(DeleteFilesForm), new UIPropertyMetadata("", null));

        public GroupVideoQualityVM GroupVideoQuality
        {
            get { return (GroupVideoQualityVM)GetValue(GroupVideoQualityProperty); }
            set { SetValue(GroupVideoQualityProperty, value); }
        }

        public int AnimeID
        {
            get { return (int)GetValue(AnimeIDProperty); }
            set { SetValue(AnimeIDProperty, value); }
        }

        public int FileCount
        {
            get { return (int)GetValue(FileCountProperty); }
            set { SetValue(FileCountProperty, value); }
        }

        public string DeleteStatus
        {
            get { return (string)GetValue(DeleteStatusProperty); }
            set { SetValue(DeleteStatusProperty, value); }
        }

        public static readonly DependencyProperty GroupFileSummaryProperty = DependencyProperty.Register("GroupFileSummary",
            typeof(GroupFileSummaryVM), typeof(DeleteFilesForm), new UIPropertyMetadata(null, null));

        public GroupFileSummaryVM GroupFileSummary
        {
            get { return (GroupFileSummaryVM)GetValue(GroupFileSummaryProperty); }
            set { SetValue(GroupFileSummaryProperty, value); }
        }

        public static readonly DependencyProperty SummaryTextProperty = DependencyProperty.Register("SummaryText",
            typeof(string), typeof(DeleteFilesForm), new UIPropertyMetadata("", null));

        public string SummaryText
        {
            get { return (string)GetValue(SummaryTextProperty); }
            set { SetValue(SummaryTextProperty, value); }
        }

        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register("GroupName",
            typeof(string), typeof(DeleteFilesForm), new UIPropertyMetadata("", null));

        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        private BackgroundWorker deleteFilesWorker = new BackgroundWorker();
        public bool FilesDeleted { get; set; }
        private bool inProgress = false;
        private List<VideoDetailedVM> vids = new List<VideoDetailedVM>();

        public DeleteFilesForm()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            deleteFilesWorker.WorkerReportsProgress = true;

            deleteFilesWorker.DoWork += new DoWorkEventHandler(deleteFilesWorker_DoWork);
            deleteFilesWorker.ProgressChanged += new ProgressChangedEventHandler(deleteFilesWorker_ProgressChanged);
            deleteFilesWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(deleteFilesWorker_RunWorkerCompleted);

            btnOK.Click += new RoutedEventHandler(btnOK_Click);
            this.Closing += new CancelEventHandler(DeleteFilesForm_Closing);
            FilesDeleted = false;
        }

        void DeleteFilesForm_Closing(object sender, CancelEventArgs e)
        {
            if (inProgress)
            {
                e.Cancel = true;
                MessageBox.Show(Properties.Resources.DeleteFiles_Wait, Properties.Resources.Stop, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.DialogResult = FilesDeleted;
        }

        void btnOK_Click(object sender, RoutedEventArgs e)
        {
            string msg = string.Format(Properties.Resources.DeleteFiles_Confirm, vids.Count);
            MessageBoxResult res = MessageBox.Show(msg, Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (res == MessageBoxResult.Yes)
            {
                this.Cursor = Cursors.Wait;
                btnOK.Visibility = System.Windows.Visibility.Hidden;
                inProgress = true;
                deleteFilesWorker.RunWorkerAsync(vids);
            }
        }

        void deleteFilesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            inProgress = false;
        }

        void deleteFilesWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            FilesDeleted = true;
            string msg = e.UserState as string;
            DeleteStatus = msg;
        }

        void deleteFilesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<VideoDetailedVM> vids = e.Argument as List<VideoDetailedVM>;
            if (vids == null) return;

            int i = 0;
            foreach (VideoDetailedVM vid in vids)
            {
                i++;
                string msg = string.Format(Properties.Resources.DeleteFiles_Deleting, i, vids.Count);
                deleteFilesWorker.ReportProgress(0, msg);
                //Thread.Sleep(500);
                foreach (VideoLocal_PlaceVM n in vid.Places)
                {
                    string g = dict[n.ImportFolder.CloudID ?? 0].Item1;
                    if (chks[g])
                    {
                        string result = JMMServerVM.Instance.clientBinaryHTTP.DeleteVideoLocalPlaceAndFile(n.VideoLocal_Place_ID);
                        if (result.Length > 0)
                        {
                            deleteFilesWorker.ReportProgress(0, result);
                            return;
                        }
                    }
                }


            }

            deleteFilesWorker.ReportProgress(100, Properties.Resources.Done);
        }
        Dictionary<string, bool> chks=new Dictionary<string, bool>();
        private Dictionary<int, Tuple<string, BitmapImage>> dict=new Dictionary<int, Tuple<string, BitmapImage>>();

        public void InitImportFolders(Dictionary<string, BitmapImage> types)
        {
            WrapPanel.Children.Clear();
            foreach (string s in types.Keys)
            {
                StackPanel st = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                Image im = new Image
                {
                    Height = 24,
                    Width = 24,
                    Source = types[s],
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 5, 0)
                };
                TextBlock tx = new TextBlock
                {
                    Margin = new Thickness(0, 0, 5, 0),
                    Text = s,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.DemiBold
                };
                CheckBox chk = new CheckBox {VerticalAlignment = VerticalAlignment.Center, IsChecked = true};
                chk.Checked += (a, b) =>
                {
                    chks[s] = chk.IsChecked.Value;
                };
                st.Children.Add(im);
                st.Children.Add(tx);
                st.Children.Add(chk);
                WrapPanel.Children.Add(st);
            }
        }

        public void Init(int animeID, GroupVideoQualityVM gvq)
        {
            this.Cursor = Cursors.Wait;

            GroupVideoQuality = gvq;
            AnimeID = animeID;

            // get the videos
            try
            {
                JMMServerVM.Instance.RefreshCloudAccounts();
                dict = JMMServerVM.Instance.FolderProviders.ToDictionary(a => a.CloudID ?? 0, a=>new Tuple<string,BitmapImage>(a.Provider,a.Icon));
                chks = new Dictionary<string, bool>();
                Dictionary<string, BitmapImage> types=new Dictionary<string, BitmapImage>();

                List<JMMServerBinary.Contract_VideoDetailed> vidContracts = JMMServerVM.Instance.clientBinaryHTTP.GetFilesByGroupAndResolution(AnimeID,
                    GroupVideoQuality.GroupName, GroupVideoQuality.Resolution, GroupVideoQuality.VideoSource, GroupVideoQuality.VideoBitDepth, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                vids = new List<VideoDetailedVM>();
                foreach (JMMServerBinary.Contract_VideoDetailed contract in vidContracts)
                {
                    VideoDetailedVM vid = new VideoDetailedVM(contract);
                    foreach (VideoLocal_PlaceVM vv in vid.Places)
                    {
                        Tuple<string, BitmapImage> tup = dict[vv.ImportFolder.CloudID ?? 0];
                        if (!types.ContainsKey(tup.Item1))
                            types.Add(tup.Item1,tup.Item2);
                    }
                    vids.Add(vid);
                }
                
                FileCount = vids.Count;
                lbFiles.ItemsSource = vids;
                GroupName = GroupVideoQuality.GroupName;
                SummaryText = string.Format("{0} - {1}", GroupVideoQuality.VideoSource, GroupVideoQuality.Resolution);
                InitImportFolders(types);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }

        }

        public void Init(int animeID, GroupFileSummaryVM gfs)
        {
            this.Cursor = Cursors.Wait;

            GroupFileSummary = gfs;
            AnimeID = animeID;

            // get the videos
            try
            {
                JMMServerVM.Instance.RefreshCloudAccounts();
                dict = JMMServerVM.Instance.FolderProviders.ToDictionary(a => a.CloudID ?? 0, a => new Tuple<string, BitmapImage>(a.Provider, a.Icon));
                chks = new Dictionary<string, bool>();
                Dictionary<string, BitmapImage> types = new Dictionary<string, BitmapImage>();


                List<JMMServerBinary.Contract_VideoDetailed> vidContracts = JMMServerVM.Instance.clientBinaryHTTP.GetFilesByGroup(AnimeID,
                    GroupFileSummary.GroupName, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);
                vids = new List<VideoDetailedVM>();
                foreach (JMMServerBinary.Contract_VideoDetailed contract in vidContracts)
                {
                    VideoDetailedVM vid = new VideoDetailedVM(contract);
                    foreach (VideoLocal_PlaceVM vv in vid.Places)
                    {
                        Tuple<string, BitmapImage> tup = dict[vv.ImportFolder.CloudID ?? 0];
                        if (!types.ContainsKey(tup.Item1))
                            types.Add(tup.Item1, tup.Item2);
                    }
                    vids.Add(vid);
                }
                FileCount = vids.Count;
                lbFiles.ItemsSource = vids;

                GroupName = GroupFileSummary.GroupName;
                SummaryText = "";
                InitImportFolders(types);

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }

        }


    }
}

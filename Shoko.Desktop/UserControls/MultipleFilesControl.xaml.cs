using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Enums;
using Shoko.Desktop.Forms;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;
using Shoko.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shoko.Models.Enums;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for MultipleFilesControl.xaml
    /// </summary>
    public partial class MultipleFilesControl : UserControl
    {
        public enum MoveDirection
        {
            Up,
            Down
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();

        BackgroundWorker workerFiles = new BackgroundWorker();
        public ICollectionView ViewEpisodes { get; set; }
        public ObservableCollection<VM_AnimeEpisode_User> CurrentEpisodes { get; set; }

        public static readonly DependencyProperty EpisodeCountProperty = DependencyProperty.Register("EpisodeCount",
            typeof(int), typeof(MultipleFilesControl), new UIPropertyMetadata(0, null));

        public int EpisodeCount
        {
            get => (int)GetValue(EpisodeCountProperty);
            set => SetValue(EpisodeCountProperty, value);
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading",
            typeof(bool), typeof(MultipleFilesControl), new UIPropertyMetadata(false, null));

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set
            {
                SetValue(IsLoadingProperty, value);
                IsNotLoading = !IsLoading;
            }
        }

        public static readonly DependencyProperty IsNotLoadingProperty = DependencyProperty.Register("IsNotLoading",
            typeof(bool), typeof(MultipleFilesControl), new UIPropertyMetadata(true, null));

        public bool IsNotLoading
        {
            get => (bool)GetValue(IsNotLoadingProperty);
            set => SetValue(IsNotLoadingProperty, value);
        }

        public static readonly DependencyProperty StatusMessageProperty = DependencyProperty.Register("StatusMessage",
            typeof(string), typeof(MultipleFilesControl), new UIPropertyMetadata("", null));

        public string StatusMessage
        {
            get => (string)GetValue(StatusMessageProperty);
            set => SetValue(StatusMessageProperty, value);
        }

        private List<VM_AnimeEpisode_User> contracts = new List<VM_AnimeEpisode_User>();

        private List<string> AvailableSubGroups = new List<string>();
        private ObservableCollection<FileQualityFilterType> PreferredTypes = new ObservableCollection<FileQualityFilterType>();
        private ObservableCollection<string> PreferredSubGroups = new ObservableCollection<string>();
        private ObservableCollection<string> PreferredSources = new ObservableCollection<string>();
        private ObservableCollection<string> PreferredResolutions = new ObservableCollection<string>();
        private ObservableCollection<string> PreferredAudioCodecs = new ObservableCollection<string>();
        private ObservableCollection<string> PreferredVideoCodecs = new ObservableCollection<string>();

        private ObservableCollection<FileQualityFilterType> RequiredTypes = new ObservableCollection<FileQualityFilterType>();
        private ObservableCollection<string> RequiredSubGroups = new ObservableCollection<string>();
        private ObservableCollection<string> RequiredSources = new ObservableCollection<string>();
        private ObservableCollection<string> RequiredResolutions = new ObservableCollection<string>();
        private ObservableCollection<string> RequiredAudioCodecs = new ObservableCollection<string>();
        private ObservableCollection<string> RequiredVideoCodecs = new ObservableCollection<string>();

        public static readonly DependencyProperty EnableDeleteOnImportProperty = DependencyProperty.Register("EnableDeleteOnImport",
            typeof(bool), typeof(MultipleFilesControl), new UIPropertyMetadata(false, null));
        public bool EnableDeleteOnImport
        {
            get => (bool) GetValue(EnableDeleteOnImportProperty);
            set => SetValue(EnableDeleteOnImportProperty, value);
        }

        public static readonly DependencyProperty AllowDeletionOfImportingFilesProperty = DependencyProperty.Register("AllowDeletionOfImportingFiles",
            typeof(bool), typeof(MultipleFilesControl), new UIPropertyMetadata(false, null));
        public bool AllowDeletionOfImportingFiles
        {
            get => (bool) GetValue(AllowDeletionOfImportingFilesProperty);
            set => SetValue(AllowDeletionOfImportingFilesProperty, value);
        }

        public static readonly DependencyProperty Prefer8BitProperty = DependencyProperty.Register("Prefer8Bit",
            typeof(bool), typeof(MultipleFilesControl), new UIPropertyMetadata(false, null));
        public bool Prefer8Bit
        {
            get => (bool) GetValue(Prefer8BitProperty);
            set => SetValue(Prefer8BitProperty, value);
        }

        public static readonly DependencyProperty Require10BitProperty = DependencyProperty.Register("Require10Bit",
            typeof(bool), typeof(MultipleFilesControl), new UIPropertyMetadata(false, null));
        public bool Require10Bit
        {
            get => (bool) GetValue(Require10BitProperty);
            set => SetValue(Require10BitProperty, value);
        }

        public static readonly DependencyProperty MaxNumberOfFilesProperty = DependencyProperty.Register("MaxNumberOfFiles",
            typeof(int), typeof(MultipleFilesControl), new UIPropertyMetadata(1, null));
        public int MaxNumberOfFiles
        {
            get => (int) GetValue(MaxNumberOfFilesProperty);
            set => SetValue(MaxNumberOfFilesProperty, value);
        }

        public static readonly DependencyProperty RequiredSubStreamCountProperty = DependencyProperty.Register("RequiredSubStreamCount",
            typeof(int), typeof(MultipleFilesControl), new UIPropertyMetadata(0, null));
        public int RequiredSubStreamCount
        {
            get => (int) GetValue(RequiredSubStreamCountProperty);
            set => SetValue(RequiredSubStreamCountProperty, value);
        }

        public static readonly DependencyProperty RequiredAudioStreamCountProperty = DependencyProperty.Register("RequiredAudioStreamCount",
            typeof(int), typeof(MultipleFilesControl), new UIPropertyMetadata(0, null));
        public int RequiredAudioStreamCount
        {
            get => (int) GetValue(RequiredAudioStreamCountProperty);
            set => SetValue(RequiredAudioStreamCountProperty, value);
        }

        public static readonly DependencyProperty RequiredSourcesProperty = DependencyProperty.Register("RequiredSourcesOperator",
            typeof(string), typeof(MultipleFilesControl), new UIPropertyMetadata("IN", null));
        public string RequiredSourcesOperator
        {
            get => (string) GetValue(RequiredSourcesProperty);
            set => SetValue(RequiredSourcesProperty, value);
        }

        public static readonly DependencyProperty RequiredResolutionsProperty = DependencyProperty.Register("RequiredResolutionsOperator",
            typeof(string), typeof(MultipleFilesControl), new UIPropertyMetadata("IN", null));
        public string RequiredResolutionsOperator
        {
            get => (string) GetValue(RequiredResolutionsProperty);
            set => SetValue(RequiredResolutionsProperty, value);
        }

        public static readonly DependencyProperty RequiredAudioCodecsProperty = DependencyProperty.Register("RequiredAudioCodecsOperator",
            typeof(string), typeof(MultipleFilesControl), new UIPropertyMetadata("IN", null));
        public string RequiredAudioCodecsOperator
        {
            get => (string) GetValue(RequiredAudioCodecsProperty);
            set => SetValue(RequiredAudioCodecsProperty, value);
        }

        public static readonly DependencyProperty RequiredVideoCodecsProperty = DependencyProperty.Register("RequiredVideoCodecsOperator",
            typeof(string), typeof(MultipleFilesControl), new UIPropertyMetadata("IN", null));
        public string RequiredVideoCodecsOperator
        {
            get => (string) GetValue(RequiredVideoCodecsProperty);
            set => SetValue(RequiredVideoCodecsProperty, value);
        }

        public static readonly DependencyProperty RequiredSubGroupsProperty = DependencyProperty.Register("RequiredSubGroupsOperator",
            typeof(string), typeof(MultipleFilesControl), new UIPropertyMetadata("IN", null));
        public string RequiredSubGroupsOperator
        {
            get => (string) GetValue(RequiredSubGroupsProperty);
            set => SetValue(RequiredSubGroupsProperty, value);
        }

        public static readonly DependencyProperty RequiredSubStreamCountOperatorProperty = DependencyProperty.Register("RequiredSubStreamCountOperator",
            typeof(string), typeof(MultipleFilesControl), new UIPropertyMetadata("IN", null));
        public string RequiredSubStreamCountOperator
        {
            get => (string) GetValue(RequiredSubStreamCountOperatorProperty);
            set => SetValue(RequiredSubStreamCountOperatorProperty, value);
        }

        public static readonly DependencyProperty RequiredAudioStreamCountOperatorProperty = DependencyProperty.Register("RequiredAudioStreamCountOperator",
            typeof(string), typeof(MultipleFilesControl), new UIPropertyMetadata("IN", null));
        public string RequiredAudioStreamCountOperator
        {
            get => (string) GetValue(RequiredAudioStreamCountOperatorProperty);
            set => SetValue(RequiredAudioStreamCountOperatorProperty, value);
        }

        public MultipleFilesControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            FileQualityPreferences prefs = null;
            try
            {
                prefs = JsonConvert.DeserializeObject<FileQualityPreferences>(VM_ShokoServer.Instance.FileQualityPreferences, new StringEnumConverter());
            }
            catch
            {
                // ignored
            }
            if (prefs != null)
            {
                PreferredTypes = new ObservableCollection<FileQualityFilterType>(prefs.PreferredTypes);
                PreferredSources = new ObservableCollection<string>(prefs.PreferredSources);
                PreferredResolutions = new ObservableCollection<string>(prefs.PreferredResolutions);
                PreferredAudioCodecs = new ObservableCollection<string>(prefs.PreferredAudioCodecs);
                PreferredVideoCodecs = new ObservableCollection<string>(prefs.PreferredVideoCodecs);
                PreferredSubGroups = new ObservableCollection<string>(prefs.PreferredSubGroups);

                RequiredTypes = new ObservableCollection<FileQualityFilterType>(prefs.RequiredTypes);
                RequiredSources = new ObservableCollection<string>(prefs.RequiredSources.Value);
                RequiredResolutions = new ObservableCollection<string>(prefs.RequiredResolutions.Value);
                RequiredAudioCodecs = new ObservableCollection<string>(prefs.RequiredAudioCodecs.Value);
                RequiredVideoCodecs = new ObservableCollection<string>(prefs.RequiredVideoCodecs.Value);
                RequiredSubGroups = new ObservableCollection<string>(prefs.RequiredSubGroups.Value);

                RequiredSourcesOperator = prefs.RequiredSources.Operator.ToString();
                RequiredResolutionsOperator = prefs.RequiredResolutions.Operator.ToString();
                RequiredAudioCodecsOperator = prefs.RequiredAudioCodecs.Operator.ToString();
                RequiredVideoCodecsOperator = prefs.RequiredVideoCodecs.Operator.ToString();
                RequiredSubGroupsOperator = prefs.RequiredSubGroups.Operator.ToString();

                RequiredAudioStreamCountOperator = prefs.RequiredAudioStreamCount.Operator.ToString();
                RequiredSubStreamCountOperator = prefs.RequiredSubStreamCount.Operator.ToString();

                EnableDeleteOnImport = VM_ShokoServer.Instance.FileQualityFilterEnabled;
                AllowDeletionOfImportingFiles = prefs.AllowDeletionOfImportedFiles;
                Prefer8Bit = prefs.Prefer8BitVideo;
                Require10Bit = prefs.Require10BitVideo;
                MaxNumberOfFiles = prefs.MaxNumberOfFilesToKeep;
                RequiredAudioStreamCount = prefs.RequiredAudioStreamCount.Value;
                RequiredSubStreamCount = prefs.RequiredSubStreamCount.Value;

                lbPreferred_Types.ItemsSource = PreferredTypes;
                lbPreferred_Sources.ItemsSource = PreferredSources;
                lbPreferred_Resolutions.ItemsSource = PreferredResolutions;
                lbPreferred_AudioCodecs.ItemsSource = PreferredAudioCodecs;
                lbPreferred_VideoCodecs.ItemsSource = PreferredVideoCodecs;
                lbPreferred_SubGroups.ItemsSource = PreferredSubGroups;

                lbRequired_Types.ItemsSource = RequiredTypes;
                lbRequired_Sources.ItemsSource = RequiredSources;
                lbRequired_Resolutions.ItemsSource = RequiredResolutions;
                lbRequired_AudioCodecs.ItemsSource = RequiredAudioCodecs;
                lbRequired_VideoCodecs.ItemsSource = RequiredVideoCodecs;
                lbRequired_SubGroups.ItemsSource = RequiredSubGroups;

                cmbRequired_Sources_Operator.ItemsSource = new[] { "IN", "NOTIN" };
                cmbRequired_Resolutions_Operator.ItemsSource = new[] { "GREATER_EQ", "LESS_EQ", "EQUALS", "IN", "NOTIN" };
                cmbRequired_AudioCodecs_Operator.ItemsSource = new[] { "IN", "NOTIN" };
                cmbRequired_VideoCodecs_Operator.ItemsSource = new[] { "IN", "NOTIN" };
                cmbRequired_SubGroups_Operator.ItemsSource = new[] { "IN", "NOTIN" };

                cmbAudioStreamCount_Operator.ItemsSource = new[] { "GREATER_EQ", "LESS_EQ", "EQUALS" };
                cmbSubStreamCount_Operator.ItemsSource = new[] { "GREATER_EQ", "LESS_EQ", "EQUALS" };

                cmbRequired_Sources_Operator.SelectionChanged += SaveSettings;
                cmbRequired_Resolutions_Operator.SelectionChanged += SaveSettings;
                cmbRequired_AudioCodecs_Operator.SelectionChanged += SaveSettings;
                cmbRequired_VideoCodecs_Operator.SelectionChanged += SaveSettings;
                cmbRequired_SubGroups_Operator.SelectionChanged += SaveSettings;

                cmbAudioStreamCount_Operator.SelectionChanged += SaveSettings;
                cmbSubStreamCount_Operator.SelectionChanged += SaveSettings;

                chkEnableDeleteOnImport.Checked += SaveSettings;
                chkPrefer8bit.Checked += SaveSettings;
                chkRequire10bit.Checked += SaveSettings;
                chkAllowDeletionOfImportingFiles.Checked += SaveSettings;

                chkEnableDeleteOnImport.Unchecked += SaveSettings;
                chkPrefer8bit.Unchecked += SaveSettings;
                chkRequire10bit.Unchecked += SaveSettings;
                chkAllowDeletionOfImportingFiles.Unchecked += SaveSettings;

                numMaxFilesToKeep.ValueChanged += SaveSettings;
                numMinAudioStreamCount.ValueChanged += SaveSettings;
                numMinSubStreamCount.ValueChanged += SaveSettings;

                cmbTypes_Available.ItemsSource = new[]
                {
                    FileQualityFilterType.RESOLUTION, FileQualityFilterType.SOURCE, FileQualityFilterType.VERSION,
                    FileQualityFilterType.AUDIOSTREAMCOUNT, FileQualityFilterType.VIDEOCODEC,
                    FileQualityFilterType.AUDIOCODEC, FileQualityFilterType.SUBGROUP,
                    FileQualityFilterType.SUBSTREAMCOUNT, FileQualityFilterType.CHAPTER
                };

                AvailableSubGroups = VM_ShokoServer.Instance.ShokoServices.GetAllReleaseGroups();
                cmbPreferred_Subgroups_Available.ItemsSource = AvailableSubGroups;
                cmbRequired_Subgroups_Available.ItemsSource = AvailableSubGroups;
            }

            PreferredTypes.CollectionChanged += SaveSettings;
            PreferredSources.CollectionChanged += SaveSettings;
            PreferredResolutions.CollectionChanged += SaveSettings;
            PreferredSubGroups.CollectionChanged += SaveSettings;
            PreferredVideoCodecs.CollectionChanged += SaveSettings;
            PreferredAudioCodecs.CollectionChanged += SaveSettings;

            txtFileSearch.TextChanged += new TextChangedEventHandler(txtFileSearch_TextChanged);
            btnClearSearch.Click += new RoutedEventHandler(btnClearSearch_Click);

            IsLoading = false;

            CurrentEpisodes = new ObservableCollection<VM_AnimeEpisode_User>();
            ViewEpisodes = CollectionViewSource.GetDefaultView(CurrentEpisodes);
            ViewEpisodes.SortDescriptions.Add(new SortDescription("AnimeName", ListSortDirection.Ascending));
            ViewEpisodes.SortDescriptions.Add(new SortDescription("EpisodeTypeAndNumberAbsolute", ListSortDirection.Ascending));
            ViewEpisodes.Filter = EpisodeSearchFilter;

            workerFiles.DoWork += new DoWorkEventHandler(workerFiles_DoWork);
            workerFiles.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerFiles_RunWorkerCompleted);

            btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

            chkOnlyFinished.IsChecked = AppSettings.MultipleFilesOnlyFinished;

            chkOnlyFinished.Checked += new RoutedEventHandler(chkOnlyFinished_Checked);
        }

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtFileSearch.Clear();
        }

        private bool EpisodeSearchFilter(object obj)
        {
            return (obj == null ||
                (obj as VM_AnimeEpisode_User).EpisodeName.IndexOf(txtFileSearch.Text.Trim(), 0, StringComparison.InvariantCultureIgnoreCase) > -1 ||
                (obj as VM_AnimeEpisode_User).EpisodeNameEnglish.IndexOf(txtFileSearch.Text.Trim(), 0, StringComparison.InvariantCultureIgnoreCase) > -1 ||
                ((obj as VM_AnimeEpisode_User).EpisodeNameRomaji != null && (obj as VM_AnimeEpisode_User).EpisodeNameRomaji.IndexOf(txtFileSearch.Text.Trim(), 0, StringComparison.InvariantCultureIgnoreCase) > -1) ||
                (obj as VM_AnimeEpisode_User).AnimeName.IndexOf(txtFileSearch.Text.Trim(), 0, StringComparison.InvariantCultureIgnoreCase) > -1
                ? true : false);
        }

        private void txtFileSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewEpisodes.Refresh();
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings(object sender, SelectionChangedEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings(object o, NotifyCollectionChangedEventArgs args)
        {
            SaveSettings();
        }

        private void SaveSettings(object o, DependencyPropertyChangedEventArgs args)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            FileQualityPreferences prefs = new FileQualityPreferences
            {
                AllowDeletionOfImportedFiles = AllowDeletionOfImportingFiles,
                PreferredTypes = PreferredTypes.ToList(),
                PreferredAudioCodecs = PreferredAudioCodecs.ToList(),
                PreferredVideoCodecs = PreferredVideoCodecs.ToList(),
                Prefer8BitVideo = Prefer8Bit,
                PreferredResolutions = PreferredResolutions.ToList(),
                PreferredSources = PreferredSources.ToList(),
                PreferredSubGroups = PreferredSubGroups.ToList(),
                MaxNumberOfFilesToKeep = MaxNumberOfFiles,
                RequiredTypes = RequiredTypes.ToList(),
                RequiredResolutions = new FileQualityPreferences.FileQualityTypeListPair<List<string>> { Value = RequiredResolutions.ToList() },
                RequiredVideoCodecs = new FileQualityPreferences.FileQualityTypeListPair<List<string>> { Value = RequiredVideoCodecs.ToList() },
                Require10BitVideo = Require10Bit,
                RequiredAudioCodecs = new FileQualityPreferences.FileQualityTypeListPair<List<string>> { Value = RequiredAudioCodecs.ToList() },
                RequiredAudioStreamCount = new FileQualityPreferences.FileQualityTypeListPair<int> { Value = RequiredAudioStreamCount },
                RequiredSubGroups = new FileQualityPreferences.FileQualityTypeListPair<List<string>> { Value = RequiredSubGroups.ToList() },
                RequiredSubStreamCount = new FileQualityPreferences.FileQualityTypeListPair<int> { Value = RequiredSubStreamCount },
                RequiredSources = new FileQualityPreferences.FileQualityTypeListPair<List<string>> { Value = RequiredSources.ToList() }
            };

            FileQualityFilterOperationType operatorType;
            if (Enum.TryParse(RequiredResolutionsOperator, out operatorType))
                prefs.RequiredResolutions.Operator = operatorType;
            if (Enum.TryParse(RequiredVideoCodecsOperator, out operatorType))
                prefs.RequiredVideoCodecs.Operator = operatorType;
            if (Enum.TryParse(RequiredAudioCodecsOperator, out operatorType))
                prefs.RequiredAudioCodecs.Operator = operatorType;
            if (Enum.TryParse(RequiredAudioStreamCountOperator, out operatorType))
                prefs.RequiredAudioStreamCount.Operator = operatorType;
            if (Enum.TryParse(RequiredSubGroupsOperator, out operatorType))
                prefs.RequiredSubGroups.Operator = operatorType;
            if (Enum.TryParse(RequiredSubStreamCountOperator, out operatorType))
                prefs.RequiredSubStreamCount.Operator = operatorType;
            if (Enum.TryParse(RequiredSourcesOperator, out operatorType))
                prefs.RequiredSources.Operator = operatorType;

            try
            {
                string settings = JsonConvert.SerializeObject(prefs, Formatting.None, new StringEnumConverter());
                VM_ShokoServer.Instance.FileQualityPreferences = settings;
                VM_ShokoServer.Instance.SaveServerSettingsAsync();
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        void chkOnlyFinished_Checked(object sender, RoutedEventArgs e)
        {
            AppSettings.MultipleFilesOnlyFinished = chkOnlyFinished.IsChecked.Value;
        }

        void workerFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                contracts = e.Result as List<VM_AnimeEpisode_User>;
                foreach (VM_AnimeEpisode_User ep in contracts)
                    CurrentEpisodes.Add(ep);

                EpisodeCount = contracts.Count;

                btnRefresh.IsEnabled = true;
                IsLoading = false;
                Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void workerFiles_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                MultipleFilesRefreshOptions opt = e.Argument as MultipleFilesRefreshOptions;
                List<VM_AnimeEpisode_User> eps = VM_ShokoServer.Instance.ShokoServices.GetAllEpisodesWithMultipleFiles(
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID, opt.OnlyFinishedSeries, opt.IgnoreVariations).CastList<VM_AnimeEpisode_User>();
                e.Result = eps;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
                IsLoading = false;
            }
        }

        public void RefreshMultipleFiles()
        {
            if (workerFiles.IsBusy) return;

            IsLoading = true;
            btnRefresh.IsEnabled = false;
            CurrentEpisodes.Clear();
            EpisodeCount = 0;

            StatusMessage = Shoko.Commons.Properties.Resources.Loading;


            MultipleFilesRefreshOptions opt = new MultipleFilesRefreshOptions()
            {
                OnlyFinishedSeries = chkOnlyFinished.IsChecked.Value,
                IgnoreVariations = chkIgnoreVariations.IsChecked.Value
            };

            workerFiles.RunWorkerAsync(opt);
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshMultipleFiles();
        }

        private void CommandBinding_OpenFolder(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    VM_VideoDetailed vid = obj as VM_VideoDetailed;

                    if (File.Exists(vid.GetFullPath()))
                    {
                        Utils.OpenFolderAndSelectFile(vid.GetFullPath());
                    }
                    else
                    {
                        MessageBox.Show(Shoko.Commons.Properties.Resources.MSG_ERR_FileNotFound, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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
                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    VM_VideoDetailed vid = obj as VM_VideoDetailed;
                    //VM_AnimeEpisode_User ep = this.DataContext as VM_AnimeEpisode_User;
                    bool force = true;
                    if (MainWindow.videoHandler.DefaultPlayer.Player.ToString() !=
                        Enum.GetName(typeof(VideoPlayer), VideoPlayer.WindowsDefault))
                    {
                        if (vid.VideoLocal_ResumePosition > 0)
                        {
                            AskResumeVideo ask = new AskResumeVideo(vid.VideoLocal_ResumePosition);
                            ask.Owner = Window.GetWindow(this);
                            if (ask.ShowDialog() == true)
                                force = false;
                        }
                    }
                    MainWindow.videoHandler.PlayVideo(vid,force);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void btnDeleteFilesWithPreferences_Click(object sender, RoutedEventArgs e)
        {
            List<VM_VideoDetailed> list =
                VM_ShokoServer.Instance.ShokoServices.GetMultipleFilesForDeletionByPreferences(VM_ShokoServer.Instance
                    .CurrentUser.JMMUserID).CastList<VM_VideoDetailed>();
            list = list.DistinctBy(a => a.Places.FirstOrDefault(b => !string.IsNullOrEmpty(b.FilePath))?.FilePath).ToList();
            DeleteFilesForm form = new DeleteFilesForm();
            form.Owner = Window.GetWindow(this);
            form.Init(list);
            form.ShowDialog();
        }

        private static void Move<T>(IList<T> list, Selector box, MoveDirection direction)
        {
            int index = box.SelectedIndex;
            if (direction == MoveDirection.Up)
            {
                if (index <= 0) return;
                T old = list[index - 1];
                list[index - 1] = list[index];
                list[index] = old;
                box.SelectedIndex = index - 1;
            }
            else
            {
                if (index >= list.Count) return;
                T old = list[index + 1];
                list[index + 1] = list[index];
                list[index] = old;
                box.SelectedIndex = index + 1;
            }
        }

        private void btnPreferred_Types_Up_Click(object sender, RoutedEventArgs e)
        {
            Move(PreferredTypes, lbPreferred_Types, MoveDirection.Up);
        }

        private void btnPreferred_Types_Down_Click(object sender, RoutedEventArgs e)
        {
            Move(PreferredTypes, lbPreferred_Types, MoveDirection.Down);
        }

        private void btnPreferred_Sources_Up_Click(object sender, RoutedEventArgs e)
        {
            Move(PreferredSources, lbPreferred_Sources, MoveDirection.Up);
        }

        private void btnPreferred_Sources_Down_Click(object sender, RoutedEventArgs e)
        {
            Move(PreferredSources, lbPreferred_Sources, MoveDirection.Down);
        }

        private void btnPreferred_Resolutions_Up_Click(object sender, RoutedEventArgs e)
        {
            Move(PreferredResolutions, lbPreferred_Resolutions, MoveDirection.Up);
        }

        private void btnPreferred_Resolutions_Down_Click(object sender, RoutedEventArgs e)
        {
            Move(PreferredResolutions, lbPreferred_Resolutions, MoveDirection.Down);
        }

        private void btnPreferred_SubGroups_Up_Click(object sender, RoutedEventArgs e)
        {
            Move(PreferredSubGroups, lbPreferred_SubGroups, MoveDirection.Up);
        }

        private void btnPreferred_SubGroups_Down_Click(object sender, RoutedEventArgs e)
        {
            Move(PreferredSubGroups, lbPreferred_SubGroups, MoveDirection.Down);
        }

        private void btnPreferred_SubGroups_Remove_Click(object sender, RoutedEventArgs e)
        {
            int i = lbPreferred_SubGroups.SelectedIndex;
            try
            {
                PreferredSubGroups.RemoveAt(i);
            }
            catch (Exception exception)
            {
                logger.Error(exception);
            }
        }

        private void btnPreferred_SubGroups_Add_Click(object sender, RoutedEventArgs e)
        {
            string item = cmbPreferred_Subgroups_Available.SelectedItem as string;
            if (item == null || PreferredSubGroups.Contains(item.ToLowerInvariant())) return;
            PreferredSubGroups.Add(item.ToLowerInvariant());
        }

        private void btnPreferred_VideoCodecs_Up_Click(object sender, RoutedEventArgs e)
        {
            Move(PreferredVideoCodecs, lbPreferred_VideoCodecs, MoveDirection.Up);
        }

        private void btnPreferred_VideoCodecs_Down_Click(object sender, RoutedEventArgs e)
        {
            Move(PreferredVideoCodecs, lbPreferred_VideoCodecs, MoveDirection.Down);
        }

        private void btnPreferred_AudioCodecs_Up_Click(object sender, RoutedEventArgs e)
        {
            Move(PreferredAudioCodecs, lbPreferred_AudioCodecs, MoveDirection.Up);
        }

        private void btnPreferred_AudioCodecs_Down_Click(object sender, RoutedEventArgs e)
        {
            Move(PreferredAudioCodecs, lbPreferred_AudioCodecs, MoveDirection.Down);
        }

        private void btnRequired_Types_Up_Click(object sender, RoutedEventArgs e)
        {
            Move(RequiredTypes, lbRequired_Types, MoveDirection.Up);
        }

        private void btnRequired_Types_Down_Click(object sender, RoutedEventArgs e)
        {
            Move(RequiredTypes, lbRequired_Types, MoveDirection.Down);
        }

        private void btnRequired_Types_Remove_Click(object sender, RoutedEventArgs e)
        {
            int i = lbRequired_Types.SelectedIndex;
            try
            {
                RequiredTypes.RemoveAt(i);
            }
            catch (Exception exception)
            {
                logger.Error(exception);
            }
        }

        private void btnRequired_Types_Add_Click(object sender, RoutedEventArgs e)
        {
            if(!(cmbTypes_Available.SelectedItem is FileQualityFilterType)) return;
            FileQualityFilterType item = (FileQualityFilterType) cmbTypes_Available.SelectedItem;
            if (RequiredTypes.Contains(item)) return;
            RequiredTypes.Add(item);
        }

        private void btnRequired_Sources_Up_Click(object sender, RoutedEventArgs e)
        {
            Move(RequiredSources, lbRequired_Sources, MoveDirection.Up);
        }

        private void btnRequired_Sources_Down_Click(object sender, RoutedEventArgs e)
        {
            Move(RequiredSources, lbRequired_Sources, MoveDirection.Down);
        }

        private void btnRequired_Resolutions_Up_Click(object sender, RoutedEventArgs e)
        {
            Move(RequiredResolutions, lbRequired_Resolutions, MoveDirection.Up);
        }

        private void btnRequired_Resolutions_Down_Click(object sender, RoutedEventArgs e)
        {
            Move(RequiredResolutions, lbRequired_Resolutions, MoveDirection.Down);
        }

        private void btnRequired_SubGroups_Up_Click(object sender, RoutedEventArgs e)
        {
            Move(RequiredSubGroups, lbRequired_SubGroups, MoveDirection.Up);
        }

        private void btnRequired_SubGroups_Down_Click(object sender, RoutedEventArgs e)
        {
            Move(RequiredSubGroups, lbRequired_SubGroups, MoveDirection.Down);
        }

        private void btnRequired_SubGroups_Remove_Click(object sender, RoutedEventArgs e)
        {
            int i = lbRequired_SubGroups.SelectedIndex;
            try
            {
                RequiredSubGroups.RemoveAt(i);
            }
            catch (Exception exception)
            {
                logger.Error(exception);
            }
        }

        private void btnRequired_SubGroups_Add_Click(object sender, RoutedEventArgs e)
        {
            string item = cmbRequired_Subgroups_Available.SelectedItem as string;
            if (item == null || RequiredSubGroups.Contains(item)) return;
            RequiredSubGroups.Add(item);
        }

        private void btnRequired_VideoCodecs_Up_Click(object sender, RoutedEventArgs e)
        {
            Move(RequiredVideoCodecs, lbRequired_VideoCodecs, MoveDirection.Up);
        }

        private void btnRequired_VideoCodecs_Down_Click(object sender, RoutedEventArgs e)
        {
            Move(RequiredVideoCodecs, lbRequired_VideoCodecs, MoveDirection.Down);
        }

        private void btnRequired_AudioCodecs_Up_Click(object sender, RoutedEventArgs e)
        {
            Move(RequiredAudioCodecs, lbRequired_AudioCodecs, MoveDirection.Up);
        }

        private void btnRequired_AudioCodecs_Down_Click(object sender, RoutedEventArgs e)
        {
            Move(RequiredAudioCodecs, lbRequired_AudioCodecs, MoveDirection.Down);
        }
    }

    public class MultipleFilesRefreshOptions
    {
        public bool OnlyFinishedSeries { get; set; }
        public bool IgnoreVariations { get; set; }
    }
}

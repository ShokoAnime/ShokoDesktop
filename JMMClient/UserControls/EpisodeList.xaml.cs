﻿
using JMMClient.Forms;
using JMMClient.ViewModel;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for EpisodeList.xaml
    /// </summary>
    public partial class EpisodeList : UserControl
    {
        //private readonly string AvEpsOnly = JMMClient.Properties.Resources.Episodes_AvOnly;
        //private readonly string AllEps = JMMClient.Properties.Resources.Episodes_AvAll;

        public static readonly DependencyProperty IsEpisodeSelectedProperty = DependencyProperty.Register("IsEpisodeSelected",
            typeof(bool), typeof(EpisodeList), new UIPropertyMetadata(false, null));

        public bool IsEpisodeSelected
        {
            get { return (bool)GetValue(IsEpisodeSelectedProperty); }
            set { SetValue(IsEpisodeSelectedProperty, value); }
        }

        public static readonly DependencyProperty CurrentEpisodeNumberProperty = DependencyProperty.Register("CurrentEpisodeNumber",
            typeof(int), typeof(EpisodeList), new UIPropertyMetadata(0, null));


        public int CurrentEpisodeNumber
        {
            get { return (int)GetValue(CurrentEpisodeNumberProperty); }
            set { SetValue(CurrentEpisodeNumberProperty, value); }
        }


        private static Logger logger = LogManager.GetCurrentClassLogger();
        private int lastAnimeSeriesID = 0;
        private string lastEpisodeType = "";

        public ObservableCollection<AnimeEpisodeVM> CurrentEpisodes { get; set; }
        public ObservableCollection<AnimeEpisodeTypeVM> CurrentEpisodeTypes { get; set; }

        private EpisodeType episodeType = EpisodeType.Episode;

        public EpisodeList()
        {
            InitializeComponent();

            IsEpisodeSelected = false;
            CurrentEpisodeNumber = 0;

            cboAvailableEpisodes.Items.Clear();
            int idx = 0;
            for (int i = 0; i < AvailableEpisodeTypeContainer.GetAll().Count; i++)
            {
                AvailableEpisodeTypeContainer cont = AvailableEpisodeTypeContainer.GetAll()[i];
                cboAvailableEpisodes.Items.Add(cont);
                if (cont.AvailableEpisodeType == AppSettings.Episodes_Availability) idx = i;
            }
            cboAvailableEpisodes.SelectedIndex = idx;

            cboWatched.Items.Clear();
            idx = 0;
            for (int i = 0; i < WatchedStatusContainer.GetAll().Count; i++)
            {
                WatchedStatusContainer cont = WatchedStatusContainer.GetAll()[i];
                cboWatched.Items.Add(cont);
                if (cont.WatchedStatus == AppSettings.Episodes_WatchedStatus) idx = i;
            }
            cboWatched.SelectedIndex = idx;

            this.Loaded += new RoutedEventHandler(EpisodeList_Loaded);
            cboEpisodeTypeFilter.SelectionChanged += new SelectionChangedEventHandler(cboEpisodeTypeFilter_SelectionChanged);
            cboAvailableEpisodes.SelectionChanged += new SelectionChangedEventHandler(cboAvailableEpisodes_SelectionChanged);
            cboWatched.SelectionChanged += new SelectionChangedEventHandler(cboWatched_SelectionChanged);

            CurrentEpisodes = new ObservableCollection<AnimeEpisodeVM>();
            CurrentEpisodeTypes = new ObservableCollection<AnimeEpisodeTypeVM>();

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(EpisodeList_DataContextChanged);
            //lbEpisodes.SelectionChanged += new SelectionChangedEventHandler(lbEpisodes_SelectionChanged);
            lbEpisodes.SelectedItemChanged += lbEpisodes_SelectedItemChanged;
            lbEpisodes.MouseDoubleClick += new MouseButtonEventHandler(lbEpisodes_MouseDoubleClick);

            btnMarkAllWatched.Click += new RoutedEventHandler(btnMarkAllWatched_Click);
            btnMarkAllUnwatched.Click += new RoutedEventHandler(btnMarkAllUnwatched_Click);
            btnMarkAllPreviousWatched.Click += new RoutedEventHandler(btnMarkAllPreviousWatched_Click);

            lbEpisodes.PreviewMouseWheel += LbEpisodes_PreviewMouseWheel;
        }

        private void LbEpisodes_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                foreach (ScrollViewer sv in Utils.GetScrollViewers(this))
                    sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta / 3);
            }
            catch { }
        }



        void lbEpisodes_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                IsEpisodeSelected = false;

                if (lbEpisodes.SelectedItem != null)
                {
                    AnimeEpisodeVM ep = lbEpisodes.SelectedItem as AnimeEpisodeVM;
                    MainListHelperVM.Instance.LastEpisodeForSeries[ep.AnimeSeriesID] = ep.AnimeEpisodeID;

                    if (ep.EpisodeTypeEnum == EpisodeType.Episode)
                    {
                        IsEpisodeSelected = true;
                        CurrentEpisodeNumber = ep.EpisodeNumber;
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void lbEpisodes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                FrameworkElement ctrl = e.OriginalSource as FrameworkElement;

                if (ctrl == null || lbEpisodes.SelectedItem == null)
                    return;

                // The below while loop, etc. is used to make sure that the double click event is coming from an episode detail.
                // When the ListBox is responsible for rendering the scrollbar, it turns out that the scrollbar will also raise
                // double click events. So we'll see if the EpisodeDetail is an ancestor of the original source of the event
                bool srcWasListItem = false;

                while (ctrl != null)
                {
                    if (ctrl is EpisodeDetail)
                    {
                        srcWasListItem = true;
                        break;
                    }

                    ctrl = ctrl.Parent as FrameworkElement;
                }

                if (!srcWasListItem)
                    return; // The source of the event wasn't an EpisodeDetail

                AnimeEpisodeVM ep = lbEpisodes.SelectedItem as AnimeEpisodeVM;

                if (ep != null)
                {
                    ep.RefreshFilesForEpisode();


                    if (ep.FilesForEpisode.Count == 1)
                    {
                        bool force = true;
                        if (MainWindow.videoHandler.DefaultPlayer.Player.ToString() !=
                            Enum.GetName(typeof(VideoPlayer), VideoPlayer.WindowsDefault))
                        {
                            if (ep.FilesForEpisode[0].VideoLocal_ResumePosition > 0)
                            {
                                AskResumeVideo ask = new AskResumeVideo(ep.FilesForEpisode[0].VideoLocal_ResumePosition);
                                ask.Owner = Window.GetWindow(this);
                                if (ask.ShowDialog() == true)
                                    force = false;
                            }
                        }
                        MainWindow.videoHandler.PlayVideo(ep.FilesForEpisode[0], force);
                    }
                    else if (ep.FilesForEpisode.Count > 1)
                    {
                        Window parentWindow = Window.GetWindow(this);

                        if (AppSettings.AutoFileSingleEpisode)
                        {
                            VideoDetailedVM vid = MainWindow.videoHandler.GetAutoFileForEpisode(ep);
                            if (vid != null)
                            {
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
                                MainWindow.videoHandler.PlayVideo(vid, force);
                            }
                        }
                        else
                        {
                            PlayVideosForEpisodeForm frm = new PlayVideosForEpisodeForm();
                            frm.Owner = parentWindow;
                            frm.Init(ep);
                            frm.ShowDialog();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex.ToString());
            }
        }

        void btnMarkAllPreviousWatched_Click(object sender, RoutedEventArgs e)
        {
            if (lbEpisodes.SelectedItem != null)
            {
                AnimeEpisodeVM ep = lbEpisodes.SelectedItem as AnimeEpisodeVM;
                if (ep == null) return;

                SetWatchedStatusOnSeries(true, ep.EpisodeNumber);
            }
        }

        private void SetWatchedStatusOnSeries(bool watchedStatus, int maxEpisodeNumber)
        {
            try
            {
                AnimeSeriesVM animeSeries = (AnimeSeriesVM)this.DataContext;
                if (animeSeries == null) return;

                this.Cursor = Cursors.Wait;

                AnimeEpisodeTypeVM epType = cboEpisodeTypeFilter.SelectedItem as AnimeEpisodeTypeVM;

                JMMServerVM.Instance.clientBinaryHTTP.SetWatchedStatusOnSeries(animeSeries.AnimeSeriesID.Value, watchedStatus, maxEpisodeNumber,
                    (int)epType.EpisodeType, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

                MainListHelperVM.Instance.UpdateHeirarchy(animeSeries);
                RefreshEpisodes();

                this.Cursor = Cursors.Arrow;

                Window parentWindow = Window.GetWindow(this);
                Utils.PromptToRateSeries(animeSeries, parentWindow);
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Arrow;
                Utils.ShowErrorMessage("SetWatchedStatusOnSeries: " + ex.Message);
                logger.Error(ex, ex.ToString());
            }
        }

        void btnMarkAllUnwatched_Click(object sender, RoutedEventArgs e)
        {
            SetWatchedStatusOnSeries(false, int.MaxValue);
        }

        void btnMarkAllWatched_Click(object sender, RoutedEventArgs e)
        {
            SetWatchedStatusOnSeries(true, int.MaxValue);
        }

        void lbEpisodes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                IsEpisodeSelected = false;

                if (lbEpisodes.SelectedItem != null)
                {
                    AnimeEpisodeVM ep = lbEpisodes.SelectedItem as AnimeEpisodeVM;
                    MainListHelperVM.Instance.LastEpisodeForSeries[ep.AnimeSeriesID] = ep.AnimeEpisodeID;

                    if (ep.EpisodeTypeEnum == EpisodeType.Episode)
                    {
                        IsEpisodeSelected = true;
                        CurrentEpisodeNumber = ep.EpisodeNumber;
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void cboWatched_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbo = (ComboBox)sender;

            object obj = cbo.SelectedItem;
            if (obj == null) return;

            try
            {
                WatchedStatusContainer epcont = cbo.SelectedItem as WatchedStatusContainer;
                AppSettings.Episodes_WatchedStatus = epcont.WatchedStatus;

                RefreshEpisodes();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void cboAvailableEpisodes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbo = (ComboBox)sender;

            object obj = cbo.SelectedItem;
            if (obj == null) return;

            try
            {
                AvailableEpisodeTypeContainer epcont = cbo.SelectedItem as AvailableEpisodeTypeContainer;
                AppSettings.Episodes_Availability = epcont.AvailableEpisodeType;

                RefreshEpisodes();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void RefreshEpisodes()
        {
            try
            {
                AnimeSeriesVM animeSeries = (AnimeSeriesVM)this.DataContext;
                if (animeSeries == null)
                {
                    CurrentEpisodes.Clear();
                    return;
                }

                //if (animeSeries.AnimeSeriesID.HasValue && lastAnimeSeriesID == animeSeries.AnimeSeriesID.Value)
                //	return;
                //else
                //	lastAnimeSeriesID = animeSeries.AnimeSeriesID.Value;


                CurrentEpisodes.Clear();

                foreach (AnimeEpisodeVM ep in animeSeries.AllEpisodes)
                {
                    if (ep.EpisodeType != (int)episodeType) continue;

                    if (AppSettings.Episodes_WatchedStatus != WatchedStatus.All)
                    {
                        if (AppSettings.Episodes_WatchedStatus == WatchedStatus.Watched && ep.IsWatched == 0) continue;
                        if (AppSettings.Episodes_WatchedStatus == WatchedStatus.Unwatched && ep.IsWatched == 1) continue;
                    }

                    if (AppSettings.Episodes_Availability != AvailableEpisodeType.All)
                    {
                        if (AppSettings.Episodes_Availability == AvailableEpisodeType.Available && ep.LocalFileCount == 0) continue;
                        if (AppSettings.Episodes_Availability == AvailableEpisodeType.NoFiles && ep.LocalFileCount > 0) continue;
                    }

                    CurrentEpisodes.Add(ep);
                }

                HighlightEpisode();

                //kick off the property change after the episodes got refreshed
                var origUnwatchedCount = animeSeries.UnwatchedEpisodeCount;
                animeSeries.UnwatchedEpisodeCount = origUnwatchedCount + 1;
                animeSeries.UnwatchedEpisodeCount = origUnwatchedCount;
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public void PopulateToolbars()
        {
            try
            {
                AnimeSeriesVM animeSeries = this.DataContext as AnimeSeriesVM;
                if (animeSeries == null) return;

                CurrentEpisodeTypes.Clear();
                foreach (AnimeEpisodeTypeVM epType in animeSeries.EpisodeTypes)
                    CurrentEpisodeTypes.Add(epType);

                cboEpisodeTypeFilter.ItemsSource = CurrentEpisodeTypes;

                // look for the epiosde type of normal episodes
                int idx = 0;
                for (int i = 0; i < cboEpisodeTypeFilter.Items.Count; i++)
                {
                    AnimeEpisodeTypeVM epType = cboEpisodeTypeFilter.Items[i] as AnimeEpisodeTypeVM;
                    if (epType.EpisodeType == EpisodeType.Episode)
                    {
                        idx = i;
                        break;
                    }
                }

                if (cboEpisodeTypeFilter.Items.Count > 0)
                    cboEpisodeTypeFilter.SelectedIndex = idx;

                RefreshEpisodes();

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void EpisodeList_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                /*if (this.DataContext == null)
				{
					CurrentEpisodeTypes.Clear();
					CurrentEpisodes.Clear();
					return;
				}


				AnimeSeriesVM animeSeries = this.DataContext as AnimeSeriesVM;
				if (animeSeries == null) return;

				if (animeSeries.AnimeSeriesID.HasValue && lastAnimeSeriesID != animeSeries.AnimeSeriesID.Value)
				{
					lastAnimeSeriesID = animeSeries.AnimeSeriesID.Value;
				}

				
				CurrentEpisodeTypes.Clear();
				foreach (AnimeEpisodeTypeVM epType in animeSeries.EpisodeTypes)
					CurrentEpisodeTypes.Add(epType);

				cboEpisodeTypeFilter.ItemsSource = CurrentEpisodeTypes;

				// look for the epiosde type of normal episodes
				int idx = 0;
				for (int i = 0; i < cboEpisodeTypeFilter.Items.Count; i++)
				{
					AnimeEpisodeTypeVM epType = cboEpisodeTypeFilter.Items[i] as AnimeEpisodeTypeVM;
					if (epType.EpisodeType == EpisodeType.Episode)
					{
						idx = i;
						break;
					}
				}

				if (cboEpisodeTypeFilter.Items.Count > 0)
					cboEpisodeTypeFilter.SelectedIndex = idx;

				HighlightEpisode();*/
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private void HighlightEpisode()
        {
            try
            {
                AnimeSeriesVM animeSeries = (AnimeSeriesVM)this.DataContext;
                if (animeSeries == null) return;

                // highlight the appropriate episode
                // check if the last episode belongs to this series
                bool foundEpisode = false;
                int idxEp = 0;
                int idxFirstUnwatchedEp = -1;
                if (MainListHelperVM.Instance.LastEpisodeForSeries.ContainsKey(animeSeries.AnimeSeriesID.Value))
                {
                    for (int i = 0; i < lbEpisodes.Items.Count; i++)
                    {
                        AnimeEpisodeVM ep = lbEpisodes.Items[i] as AnimeEpisodeVM;
                        if (ep.AnimeEpisodeID == MainListHelperVM.Instance.LastEpisodeForSeries[animeSeries.AnimeSeriesID.Value])
                        {
                            idxEp = i;
                            foundEpisode = true;
                            break;
                        }

                        if (ep.HasFiles && !ep.Watched && idxFirstUnwatchedEp < 0)
                        {
                            idxFirstUnwatchedEp = i;
                        }
                    }
                }

                if (!foundEpisode)
                {
                    // try and get the first unwatched file instead
                    if (idxFirstUnwatchedEp >= 0)
                    {
                        idxEp = idxFirstUnwatchedEp;
                    }
                    else
                    {
                        for (int i = 0; i < lbEpisodes.Items.Count; i++)
                        {
                            AnimeEpisodeVM ep = lbEpisodes.Items[i] as AnimeEpisodeVM;
                            if (ep.HasFiles && !ep.Watched)
                            {
                                idxEp = i;
                                break;
                            }
                        }
                    }
                }

                if (lbEpisodes.Items.Count > 0)
                {
                    //lbEpisodes.SelectedIndex = idxEp;
                    lbEpisodes.Focus();
                    //lbEpisodes.ScrollIntoView(lbEpisodes.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void cboEpisodeTypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //return;
            ComboBox cbo = (ComboBox)sender;

            object obj = cbo.SelectedItem;
            if (obj == null) return;

            try
            {
                AnimeEpisodeTypeVM epType = obj as AnimeEpisodeTypeVM;
                AnimeSeriesVM animeSeries = (AnimeSeriesVM)this.DataContext;
                if (animeSeries == null) return;

                episodeType = epType.EpisodeType;

                RefreshEpisodes();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void EpisodeList_Loaded(object sender, RoutedEventArgs e)
        {

        }


    }

    public class HideMassWatchedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //[0] = selected type
            //[1] = animeseries
            //[2] = treeview selected item (selected episode)
            //[3] = unwatchedepisodecount - basically just there to get notifications when watched state changes
            //[4] = IsEpisodeSelected - basically just there to get notifications selection changes

            //return visibility if mark all episodes as watched should be visible or not...

            try
            {
                var selType = values[0] as AnimeEpisodeTypeVM;
                var ser = values[1] as AnimeSeriesVM;

                if (selType == null || ser == null)
                    return Visibility.Collapsed;

                bool visible;

                var strParameter = System.Convert.ToString(parameter);
                if (strParameter == "1")
                    visible = ser.AllEpisodes.Any(x => x.EpisodeTypeEnum == selType.EpisodeType && x.HasFiles);
                else if (strParameter == "2")
                    visible = ser.AllEpisodes.Any(x => x.Unwatched && x.EpisodeTypeEnum == selType.EpisodeType && x.HasFiles);
                else if (strParameter == "3")
                {
                    var ep = values[2] as AnimeEpisodeVM;
                    if (ep == null)
                        visible = false;
                    else
                    {
                        if (ep.EpisodeTypeEnum == EpisodeType.Episode)
                            visible = ser.AllEpisodes.Any(x => x.Unwatched && x.EpisodeTypeEnum == selType.EpisodeType && x.HasFiles && x.EpisodeNumber <= ep.EpisodeNumber);
                        else
                            visible = false;
                    }
                }
                else
                    visible = false;

                return visible ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception)
            {
                return Visibility.Visible;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HideAllConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                foreach (var value in values)
                {
                    if (object.Equals(value, Visibility.Visible))
                        return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
            catch (Exception)
            {
                return Visibility.Visible;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

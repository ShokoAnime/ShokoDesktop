using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace JMMClient.Forms
{
    /// <summary>
    /// Interaction logic for RandomSeriesForm.xaml
    /// </summary>
    public partial class RandomSeriesForm : Window
    {
        private RandomSeriesEpisodeLevel LevelType = RandomSeriesEpisodeLevel.All;
        private object LevelObject = null;
        private static Random rndm = new Random();

        public ICollectionView ViewTagNames { get; set; }
        public ObservableCollection<string> AllTagNames { get; set; }

        public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register("Series",
            typeof(AnimeSeriesVM), typeof(RandomSeriesForm), new UIPropertyMetadata(null, null));

        public AnimeSeriesVM Series
        {
            get { return (AnimeSeriesVM)GetValue(SeriesProperty); }
            set { SetValue(SeriesProperty, value); }
        }

        public static readonly DependencyProperty SeriesExistsProperty = DependencyProperty.Register("SeriesExists",
            typeof(bool), typeof(RandomSeriesForm), new UIPropertyMetadata(false, null));

        public bool SeriesExists
        {
            get { return (bool)GetValue(SeriesExistsProperty); }
            set { SetValue(SeriesExistsProperty, value); }
        }

        public static readonly DependencyProperty SeriesMissingProperty = DependencyProperty.Register("SeriesMissing",
            typeof(bool), typeof(RandomSeriesForm), new UIPropertyMetadata(true, null));

        public bool SeriesMissing
        {
            get { return (bool)GetValue(SeriesMissingProperty); }
            set { SetValue(SeriesMissingProperty, value); }
        }

        public static readonly DependencyProperty TagsExpandedProperty = DependencyProperty.Register("TagsExpanded",
            typeof(bool), typeof(RandomSeriesForm), new UIPropertyMetadata(false, null));

        public bool TagsExpanded
        {
            get { return (bool)GetValue(TagsExpandedProperty); }
            set { SetValue(TagsExpandedProperty, value); }
        }

        public static readonly DependencyProperty TagsCollapsedProperty = DependencyProperty.Register("TagsCollapsed",
            typeof(bool), typeof(RandomSeriesForm), new UIPropertyMetadata(true, null));

        public bool TagsCollapsed
        {
            get { return (bool)GetValue(TagsCollapsedProperty); }
            set { SetValue(TagsCollapsedProperty, value); }
        }

        public static readonly DependencyProperty SelectedTagsProperty = DependencyProperty.Register("SelectedTags",
            typeof(string), typeof(RandomSeriesForm), new UIPropertyMetadata("", null));

        public string SelectedTags
        {
            get { return (string)GetValue(SelectedTagsProperty); }
            set { SetValue(SelectedTagsProperty, value); }
        }

        public static readonly DependencyProperty MatchesFoundProperty = DependencyProperty.Register("MatchesFound",
            typeof(int), typeof(RandomSeriesForm), new UIPropertyMetadata(0, null));

        public int MatchesFound
        {
            get { return (int)GetValue(MatchesFoundProperty); }
            set { SetValue(MatchesFoundProperty, value); }
        }

        public static readonly DependencyProperty SelectedTagsFilterProperty = DependencyProperty.Register("SelectedTagsFilter",
            typeof(string), typeof(RandomSeriesForm), new UIPropertyMetadata(JMMClient.Properties.Resources.Random_Any, null));

        public string SelectedTagsFilter
        {
            get { return (string)GetValue(SelectedTagsFilterProperty); }
            set { SetValue(SelectedTagsFilterProperty, value); }
        }

        public RandomSeriesForm()
        {
            InitializeComponent();

            btnRandomSeries.Click += new RoutedEventHandler(btnRandomSeries_Click);
            btnSelectSeries.Click += new RoutedEventHandler(btnSelectSeries_Click);
            this.Loaded += new RoutedEventHandler(RandomSeriesForm_Loaded);
            btnEditTags.Click += new RoutedEventHandler(btnEditTags_Click);
            btnEditTagsFinish.Click += new RoutedEventHandler(btnEditTagsFinish_Click);
            lbTags.MouseDoubleClick += new MouseButtonEventHandler(lbTags_MouseDoubleClick);

            cboCatFilter.Items.Clear();
            cboCatFilter.Items.Add(JMMClient.Properties.Resources.Random_Any);
            cboCatFilter.Items.Add(JMMClient.Properties.Resources.Random_All);
            cboCatFilter.SelectedIndex = 0;
        }

        void lbTags_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object obj = lbTags.SelectedItem;
            if (obj == null) return;

            string catName = obj.ToString();
            string currentList = txtSelectedTags.Text.Trim();

            // add to the selected list
            int index = currentList.IndexOf(catName, 0, StringComparison.InvariantCultureIgnoreCase);
            if (index > -1) return;

            if (currentList.Length > 0) currentList += ", ";
            currentList += catName;

            txtSelectedTags.Text = currentList;
        }

        void btnEditTagsFinish_Click(object sender, RoutedEventArgs e)
        {
            TagsCollapsed = true;
            TagsExpanded = false;

            SelectedTags = txtSelectedTags.Text.Trim();
            SelectedTagsFilter = cboCatFilter.SelectedItem.ToString();
        }

        void ClearSelectedTags(object sender, RoutedEventArgs e)
        {
            txtSelectedTags.Text = "";
        }

        void btnEditTags_Click(object sender, RoutedEventArgs e)
        {
            TagsCollapsed = false;
            TagsExpanded = true;

            txtSelectedTags.Text = SelectedTags;
            if (SelectedTagsFilter.Equals(JMMClient.Properties.Resources.Random_Any))
                cboCatFilter.SelectedIndex = 0;
            else
                cboCatFilter.SelectedIndex = 1;
        }

        void btnSelectSeries_Click(object sender, RoutedEventArgs e)
        {

            this.DialogResult = true;
            this.Close();
        }

        void RandomSeriesForm_Loaded(object sender, RoutedEventArgs e)
        {
            SetRandomSeries();
        }

        void btnRandomSeries_Click(object sender, RoutedEventArgs e)
        {
            SetRandomSeries();
        }

        public void Init(RandomSeriesEpisodeLevel levelType, object levelObject)
        {
            LevelType = levelType;
            LevelObject = levelObject;

            PopulateTags();
        }

        private void PopulateTags()
        {
            AllTagNames = new ObservableCollection<string>();

            ViewTagNames = CollectionViewSource.GetDefaultView(AllTagNames);
            List<string> tagsRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllTagNames();

            foreach (string cat in tagsRaw)
                AllTagNames.Add(cat);

        }

        private List<AnimeSeriesVM> GetSeriesForGroupFilter()
        {
            List<AnimeSeriesVM> serList = new List<AnimeSeriesVM>();
            try
            {
                if (LevelType != RandomSeriesEpisodeLevel.GroupFilter) return serList;
                GroupFilterVM gf = LevelObject as GroupFilterVM;
                if (gf == null) return serList;

                List<AnimeGroupVM> grps = new List<AnimeGroupVM>(MainListHelperVM.Instance.AllGroups);

                foreach (AnimeGroupVM grp in grps)
                {
                    // ignore sub groups
                    if (grp.AnimeGroupParentID.HasValue) continue;

                    if (gf.EvaluateGroupFilter(grp))
                    {
                        foreach (AnimeSeriesVM ser in grp.AllAnimeSeries)
                        {
                            if (gf.EvaluateGroupFilter(ser))
                            {
                                // tags
                                if (!string.IsNullOrEmpty(SelectedTags))
                                {
                                    string filterParm = SelectedTags.Trim();

                                    string[] cats = filterParm.Split(',');

                                    bool foundCat = false;
                                    if (cboCatFilter.SelectedIndex == 1) foundCat = true; // all

                                    int index = 0;
                                    foreach (string cat in cats)
                                    {
                                        if (cat.Trim().Length == 0) continue;
                                        if (cat.Trim() == ",") continue;

                                        index = ser.TagsString.IndexOf(cat, 0, StringComparison.InvariantCultureIgnoreCase);

                                        if (cboCatFilter.SelectedIndex == 0) // any
                                        {
                                            if (index > -1)
                                            {
                                                foundCat = true;
                                                break;
                                            }
                                        }
                                        else //all
                                        {
                                            if (index < 0)
                                            {
                                                foundCat = false;
                                                break;
                                            }
                                        }
                                    }
                                    if (!foundCat) continue;
                                }

                                if (!ser.IsComplete && chkComplete.IsChecked.Value) continue;

                                if (chkWatched.IsChecked.Value && ser.AllFilesWatched)
                                {
                                    serList.Add(ser);
                                    continue;
                                }

                                if (chkUnwatched.IsChecked.Value && !ser.AnyFilesWatched)
                                {
                                    serList.Add(ser);
                                    continue;
                                }


                                if (chkPartiallyWatched.IsChecked.Value && ser.AnyFilesWatched && !ser.AllFilesWatched)
                                {
                                    serList.Add(ser);
                                    continue;
                                }


                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            return serList;
        }

        private void SetRandomSeries()
        {
            try
            {
                SeriesMissing = true;
                SeriesExists = false;
                MatchesFound = 0;

                List<AnimeSeriesVM> serList = null;
                if (LevelType == RandomSeriesEpisodeLevel.GroupFilter)
                    serList = GetSeriesForGroupFilter();

                if (serList != null && serList.Count > 0)
                {
                    MatchesFound = serList.Count;
                    Series = serList[rndm.Next(0, serList.Count)];
                    SeriesMissing = false;
                    SeriesExists = true;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }
    }
}

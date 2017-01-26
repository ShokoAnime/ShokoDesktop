using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;
using Shoko.Models.Enums;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for RandomEpisodeForm.xaml
    /// </summary>
    public partial class RandomEpisodeForm : Window
    {
        private RandomSeriesEpisodeLevel LevelType = RandomSeriesEpisodeLevel.All;
        private object LevelObject = null;
        private static Random rndm = new Random();
        private static Random eprndm = new Random();

        public ICollectionView ViewTagNames { get; set; }
        public ObservableCollection<string> AllTagNames { get; set; }

        public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register("Series",
            typeof(VM_AnimeSeries_User), typeof(RandomEpisodeForm), new UIPropertyMetadata(null, null));

        public VM_AnimeSeries_User Series
        {
            get { return (VM_AnimeSeries_User)GetValue(SeriesProperty); }
            set { SetValue(SeriesProperty, value); }
        }

        public static readonly DependencyProperty EpisodeProperty = DependencyProperty.Register("Episode",
            typeof(VM_AnimeEpisode_User), typeof(RandomEpisodeForm), new UIPropertyMetadata(null, null));

        public VM_AnimeEpisode_User Episode
        {
            get { return (VM_AnimeEpisode_User)GetValue(EpisodeProperty); }
            set { SetValue(EpisodeProperty, value); }
        }

        public static readonly DependencyProperty SeriesExistsProperty = DependencyProperty.Register("SeriesExists",
            typeof(bool), typeof(RandomEpisodeForm), new UIPropertyMetadata(false, null));

        public bool SeriesExists
        {
            get { return (bool)GetValue(SeriesExistsProperty); }
            set { SetValue(SeriesExistsProperty, value); }
        }

        public static readonly DependencyProperty EpisodeExistsProperty = DependencyProperty.Register("EpisodeExists",
            typeof(bool), typeof(RandomEpisodeForm), new UIPropertyMetadata(false, null));

        public bool EpisodeExists
        {
            get { return (bool)GetValue(EpisodeExistsProperty); }
            set { SetValue(EpisodeExistsProperty, value); }
        }



        public static readonly DependencyProperty SeriesMissingProperty = DependencyProperty.Register("SeriesMissing",
            typeof(bool), typeof(RandomEpisodeForm), new UIPropertyMetadata(true, null));

        public bool SeriesMissing
        {
            get { return (bool)GetValue(SeriesMissingProperty); }
            set { SetValue(SeriesMissingProperty, value); }
        }

        public static readonly DependencyProperty EpisodeMissingProperty = DependencyProperty.Register("EpisodeMissing",
            typeof(bool), typeof(RandomEpisodeForm), new UIPropertyMetadata(true, null));

        public bool EpisodeMissing
        {
            get { return (bool)GetValue(EpisodeMissingProperty); }
            set { SetValue(EpisodeMissingProperty, value); }
        }

        public static readonly DependencyProperty TagsExpandedProperty = DependencyProperty.Register("TagsExpanded",
            typeof(bool), typeof(RandomEpisodeForm), new UIPropertyMetadata(false, null));

        public bool TagsExpanded
        {
            get { return (bool)GetValue(TagsExpandedProperty); }
            set { SetValue(TagsExpandedProperty, value); }
        }

        public static readonly DependencyProperty TagsCollapsedProperty = DependencyProperty.Register("TagsCollapsed",
            typeof(bool), typeof(RandomEpisodeForm), new UIPropertyMetadata(true, null));

        public bool TagsCollapsed
        {
            get { return (bool)GetValue(TagsCollapsedProperty); }
            set { SetValue(TagsCollapsedProperty, value); }
        }

        public static readonly DependencyProperty SelectedTagsProperty = DependencyProperty.Register("SelectedTags",
            typeof(string), typeof(RandomEpisodeForm), new UIPropertyMetadata("", null));

        public string SelectedTags
        {
            get { return (string)GetValue(SelectedTagsProperty); }
            set { SetValue(SelectedTagsProperty, value); }
        }

        public static readonly DependencyProperty MatchesFoundProperty = DependencyProperty.Register("MatchesFound",
            typeof(int), typeof(RandomEpisodeForm), new UIPropertyMetadata(0, null));

        public int MatchesFound
        {
            get { return (int)GetValue(MatchesFoundProperty); }
            set { SetValue(MatchesFoundProperty, value); }
        }

        public static readonly DependencyProperty SelectedTagFilterProperty = DependencyProperty.Register("SelectedTagFilter",
            typeof(string), typeof(RandomEpisodeForm), new UIPropertyMetadata(Properties.Resources.Random_Any, null));

        public string SelectedTagFilter
        {
            get { return (string)GetValue(SelectedTagFilterProperty); }
            set { SetValue(SelectedTagFilterProperty, value); }
        }

        public RandomEpisodeForm()
        {
            InitializeComponent();

            btnRandomEpisode.Click += new RoutedEventHandler(btnRandomSeries_Click);
            Loaded += new RoutedEventHandler(RandomEpisodeForm_Loaded);
            btnEditTags.Click += new RoutedEventHandler(btnEditTags_Click);
            btnEditTagsFinish.Click += new RoutedEventHandler(btnEditTagsFinish_Click);
            lbTags.MouseDoubleClick += new MouseButtonEventHandler(lbTags_MouseDoubleClick);

            cboCatFilter.Items.Clear();
            cboCatFilter.Items.Add(Properties.Resources.Random_Any);
            cboCatFilter.Items.Add(Properties.Resources.Random_All);
            cboCatFilter.SelectedIndex = 0;
        }

        private void CommandBinding_ToggleWatchedStatus(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            Cursor = Cursors.Wait;
            IsEnabled = false;

            bool newStatus = false;

            try
            {
                if (obj.GetType() == typeof(VM_VideoDetailed))
                {
                    VM_VideoDetailed vid = obj as VM_VideoDetailed;
                    newStatus = !vid.Watched;
                    VM_ShokoServer.Instance.ShokoServices.ToggleWatchedStatusOnVideo(vid.VideoLocalID, newStatus, VM_ShokoServer.Instance.CurrentUser.JMMUserID);

                    VM_MainListHelper.Instance.UpdateHeirarchy(vid);
                }

                if (obj.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    VM_AnimeEpisode_User ep = obj as VM_AnimeEpisode_User;
                    newStatus = !ep.Watched;
                    CL_Response<CL_AnimeEpisode_User> response = VM_ShokoServer.Instance.ShokoServices.ToggleWatchedStatusOnEpisode(ep.AnimeEpisodeID,
                        newStatus, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    if (!string.IsNullOrEmpty(response.ErrorMessage))
                    {
                        MessageBox.Show(response.ErrorMessage, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    VM_MainListHelper.Instance.UpdateHeirarchy((VM_AnimeEpisode_User)response.Result);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
                IsEnabled = true;
            }
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
            SelectedTagFilter = cboCatFilter.SelectedItem.ToString();
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
            if (SelectedTagFilter.Equals(Properties.Resources.Random_Any))
                cboCatFilter.SelectedIndex = 0;
            else
                cboCatFilter.SelectedIndex = 1;
        }


        void RandomEpisodeForm_Loaded(object sender, RoutedEventArgs e)
        {
            SetRandomEpisode();
        }

        void btnRandomSeries_Click(object sender, RoutedEventArgs e)
        {
            SetRandomEpisode();
        }

        private void SetRandomEpisode()
        {
            try
            {
                SeriesMissing = true;
                SeriesExists = false;
                EpisodeMissing = true;
                EpisodeExists = false;

                ucPlayEpisode.EpisodeExists = false;
                ucPlayEpisode.EpisodeMissing = true;
                ucPlayEpisode.DataContext = null;

                // first let's get a random series
                // once we have that we'll get a random episode from that series
                // we also have to make sure we ignore series that don't have episodes
                // which match the criteria
                List<int> ignoredSeries = new List<int>();

                List<VM_AnimeSeries_User> serList = null;
                if (LevelType == RandomSeriesEpisodeLevel.GroupFilter)
                    serList = GetSeriesForGroupFilter();

                if (LevelType == RandomSeriesEpisodeLevel.Group)
                    serList = GetSeriesForGroup();

                if (LevelType == RandomSeriesEpisodeLevel.Series)
                {
                    serList = new List<VM_AnimeSeries_User>();
                    VM_AnimeSeries_User ser = LevelObject as VM_AnimeSeries_User;
                    if (ser != null) serList.Add(ser);

                }


                if (serList != null && serList.Count > 0)
                {
                    Dictionary<int, VM_AnimeSeries_User> dictSeries = new Dictionary<int, VM_AnimeSeries_User>();
                    foreach (VM_AnimeSeries_User ser in serList)
                        dictSeries[ser.AnimeSeriesID] = ser;

                    bool needEpisode = true;
                    while (needEpisode)
                    {
                        if (dictSeries.Values.Count == 0)
                            break;

                        List<VM_AnimeSeries_User> tempSerList = new List<VM_AnimeSeries_User>(dictSeries.Values);
                        Series = tempSerList[rndm.Next(0, tempSerList.Count)];

                        // get all the episodes
                        List<VM_AnimeEpisode_User> epList = new List<VM_AnimeEpisode_User>();
                        foreach (VM_AnimeEpisode_User ep in Series.AllEpisodes)
                        {
                            bool useEp = false;
                            if (chkWatched.IsChecked.Value && ep.Watched)
                                useEp = true;

                            if (chkUnwatched.IsChecked.Value && !ep.Watched)
                                useEp = true;

                            if (ep.LocalFileCount == 0)
                                useEp = false;

                            if (ep.EpisodeTypeEnum != enEpisodeType.Episode && ep.EpisodeTypeEnum != enEpisodeType.Special)
                                useEp = false;

                            if (useEp) epList.Add(ep);
                        }

                        if (epList.Count > 0)
                        {
                            Episode = epList[eprndm.Next(0, epList.Count)];
                            needEpisode = false;

                            SeriesMissing = false;
                            SeriesExists = true;
                            EpisodeMissing = false;
                            EpisodeExists = true;

                            ucPlayEpisode.DataContext = Episode;
                        }
                        else
                            dictSeries.Remove(Series.AnimeSeriesID);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        private List<VM_AnimeSeries_User> GetSeriesForGroupFilter()
        {
            List<VM_AnimeSeries_User> serList = new List<VM_AnimeSeries_User>();
            try
            {
                if (LevelType != RandomSeriesEpisodeLevel.GroupFilter) return serList;
                VM_GroupFilter gf = LevelObject as VM_GroupFilter;
                if (gf == null) return serList;

                foreach (VM_AnimeGroup_User grp in VM_MainListHelper.Instance.AllGroupsDictionary.Values)
                {
					// ignore sub groups
					if (grp.AnimeGroupParentID.HasValue) continue;

					foreach (VM_AnimeSeries_User ser in grp.AllAnimeSeries)
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

								foreach (string cat in cats)
								{
									if (cat.Trim().Length == 0) continue;
									if (cat.Trim() == ",") continue;

								    bool fnd = ser.AllTags.Contains(cat, StringComparer.InvariantCultureIgnoreCase);
                                    
									if (cboCatFilter.SelectedIndex == 0) // any
									{
										if (fnd)
										{
											foundCat = true;
											break;
										}
									}
									else //all
									{
										if (!fnd)
										{
											foundCat = false;
											break;
										}
									}
								}
								if (!foundCat) continue;
							}

							serList.Add(ser);
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

        private List<VM_AnimeSeries_User> GetSeriesForGroup()
        {
            List<VM_AnimeSeries_User> serList = new List<VM_AnimeSeries_User>();
            try
            {
                if (LevelType != RandomSeriesEpisodeLevel.Group) return serList;
                VM_AnimeGroup_User grp = LevelObject as VM_AnimeGroup_User;
                if (grp == null) return serList;

                foreach (VM_AnimeSeries_User ser in grp.AllAnimeSeries)
                {

                    // tags
                    if (!string.IsNullOrEmpty(SelectedTags))
                    {
                        string filterParm = SelectedTags.Trim();

                        string[] cats = filterParm.Split(',');

                        bool foundCat = false;
                        if (cboCatFilter.SelectedIndex == 1) foundCat = true; // all

                        foreach (string cat in cats)
                        {
                            if (cat.Trim().Length == 0) continue;
                            if (cat.Trim() == ",") continue;

						    bool fnd = ser.AllTags.Contains(cat, StringComparer.InvariantCultureIgnoreCase);
                            
							if (cboCatFilter.SelectedIndex == 0) // any
							{
								if (fnd)
								{
									foundCat = true;
									break;
								}
							}
							else //all
							{
								if (!fnd)
								{
									foundCat = false;
									break;
								}
							}
						}
						if (!foundCat) continue;
					}

                    serList.Add(ser);
                }

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            return serList;
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
            List<string> tagsRaw = VM_ShokoServer.Instance.ShokoServices.GetAllTagNames();

            foreach (string tag in tagsRaw)
                AllTagNames.Add(tag);

        }
    }
}

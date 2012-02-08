using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace JMMClient.Forms
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

		public ICollectionView ViewCategoryNames { get; set; }
		public ObservableCollection<string> AllCategoryNames { get; set; }

		public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register("Series",
			typeof(AnimeSeriesVM), typeof(RandomEpisodeForm), new UIPropertyMetadata(null, null));

		public AnimeSeriesVM Series
		{
			get { return (AnimeSeriesVM)GetValue(SeriesProperty); }
			set { SetValue(SeriesProperty, value); }
		}

		public static readonly DependencyProperty EpisodeProperty = DependencyProperty.Register("Episode",
			typeof(AnimeEpisodeVM), typeof(RandomEpisodeForm), new UIPropertyMetadata(null, null));

		public AnimeEpisodeVM Episode
		{
			get { return (AnimeEpisodeVM)GetValue(EpisodeProperty); }
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

		public static readonly DependencyProperty CategoriesExpandedProperty = DependencyProperty.Register("CategoriesExpanded",
			typeof(bool), typeof(RandomEpisodeForm), new UIPropertyMetadata(false, null));

		public bool CategoriesExpanded
		{
			get { return (bool)GetValue(CategoriesExpandedProperty); }
			set { SetValue(CategoriesExpandedProperty, value); }
		}

		public static readonly DependencyProperty CategoriesCollapsedProperty = DependencyProperty.Register("CategoriesCollapsed",
			typeof(bool), typeof(RandomEpisodeForm), new UIPropertyMetadata(true, null));

		public bool CategoriesCollapsed
		{
			get { return (bool)GetValue(CategoriesCollapsedProperty); }
			set { SetValue(CategoriesCollapsedProperty, value); }
		}

		public static readonly DependencyProperty SelectedCategoriesProperty = DependencyProperty.Register("SelectedCategories",
			typeof(string), typeof(RandomEpisodeForm), new UIPropertyMetadata("", null));

		public string SelectedCategories
		{
			get { return (string)GetValue(SelectedCategoriesProperty); }
			set { SetValue(SelectedCategoriesProperty, value); }
		}

		public static readonly DependencyProperty MatchesFoundProperty = DependencyProperty.Register("MatchesFound",
			typeof(int), typeof(RandomEpisodeForm), new UIPropertyMetadata(0, null));

		public int MatchesFound
		{
			get { return (int)GetValue(MatchesFoundProperty); }
			set { SetValue(MatchesFoundProperty, value); }
		}

		public static readonly DependencyProperty SelectedCategoryFilterProperty = DependencyProperty.Register("SelectedCategoryFilter",
			typeof(string), typeof(RandomEpisodeForm), new UIPropertyMetadata("Any", null));

		public string SelectedCategoryFilter
		{
			get { return (string)GetValue(SelectedCategoryFilterProperty); }
			set { SetValue(SelectedCategoryFilterProperty, value); }
		}

		public RandomEpisodeForm()
		{
			InitializeComponent();

			btnRandomEpisode.Click += new RoutedEventHandler(btnRandomSeries_Click);
			this.Loaded += new RoutedEventHandler(RandomEpisodeForm_Loaded);
			btnEditCategories.Click += new RoutedEventHandler(btnEditCategories_Click);
			btnEditCategoriesFinish.Click += new RoutedEventHandler(btnEditCategoriesFinish_Click);
			lbCategories.MouseDoubleClick += new MouseButtonEventHandler(lbCategories_MouseDoubleClick);

			cboCatFilter.Items.Clear();
			cboCatFilter.Items.Add("Any");
			cboCatFilter.Items.Add("All");
			cboCatFilter.SelectedIndex = 0;
		}

		void lbCategories_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			object obj = lbCategories.SelectedItem;
			if (obj == null) return;

			string catName = obj.ToString();
			string currentList = txtSelectedCategories.Text.Trim();

			// add to the selected list
			int index = currentList.IndexOf(catName, 0, StringComparison.InvariantCultureIgnoreCase);
			if (index > -1) return;

			if (currentList.Length > 0) currentList += ",";
			currentList += catName;

			txtSelectedCategories.Text = currentList;
		}

		void btnEditCategoriesFinish_Click(object sender, RoutedEventArgs e)
		{
			CategoriesCollapsed = true;
			CategoriesExpanded = false;

			SelectedCategories = txtSelectedCategories.Text.Trim();
			SelectedCategoryFilter = cboCatFilter.SelectedItem.ToString();
		}

		void btnEditCategories_Click(object sender, RoutedEventArgs e)
		{
			CategoriesCollapsed = false;
			CategoriesExpanded = true;

			txtSelectedCategories.Text = SelectedCategories;
			if (SelectedCategoryFilter.Equals("Any"))
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

				List<AnimeSeriesVM> serList = null;
				if (LevelType == RandomSeriesEpisodeLevel.GroupFilter)
					serList = GetSeriesForGroupFilter();

				if (LevelType == RandomSeriesEpisodeLevel.Group)
					serList = GetSeriesForGroup();

				if (LevelType == RandomSeriesEpisodeLevel.Series)
				{
					serList = new List<AnimeSeriesVM>();
					AnimeSeriesVM ser = LevelObject as AnimeSeriesVM;
					if (ser != null) serList.Add(ser);

				}


				if (serList != null && serList.Count > 0)
				{
					Dictionary<int, AnimeSeriesVM> dictSeries = new Dictionary<int, AnimeSeriesVM>();
					foreach (AnimeSeriesVM ser in serList)
						dictSeries[ser.AnimeSeriesID.Value] = ser;
					
					bool needEpisode = true;
					while (needEpisode)
					{
						if (dictSeries.Values.Count == 0) 
							break;

						List<AnimeSeriesVM> tempSerList = new List<AnimeSeriesVM>(dictSeries.Values);
						Series = tempSerList[rndm.Next(0, tempSerList.Count)];

						// get all the episodes
						List<AnimeEpisodeVM> epList = new List<AnimeEpisodeVM>();
						foreach (AnimeEpisodeVM ep in Series.AllEpisodes)
						{
							bool useEp = false;
							if (chkWatched.IsChecked.Value && ep.Watched)
								useEp = true;

							if (chkUnwatched.IsChecked.Value && !ep.Watched)
								useEp = true;

							if (ep.LocalFileCount == 0)
								useEp = false;

							if (ep.EpisodeTypeEnum != EpisodeType.Episode && ep.EpisodeTypeEnum != EpisodeType.Special)
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
							dictSeries.Remove(Series.AnimeSeriesID.Value);
					}
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
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
								// categories
								if (!string.IsNullOrEmpty(SelectedCategories))
								{
									string filterParm = SelectedCategories.Trim();

									string[] cats = filterParm.Split(',');

									bool foundCat = false;
									if (cboCatFilter.SelectedIndex == 1) foundCat = true; // all

									int index = 0;
									foreach (string cat in cats)
									{
										if (cat.Trim().Length == 0) continue;
										if (cat.Trim() == ",") continue;

										index = ser.CategoriesString.IndexOf(cat, 0, StringComparison.InvariantCultureIgnoreCase);

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

								serList.Add(ser);
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

		private List<AnimeSeriesVM> GetSeriesForGroup()
		{
			List<AnimeSeriesVM> serList = new List<AnimeSeriesVM>();
			try
			{
				if (LevelType != RandomSeriesEpisodeLevel.Group) return serList;
				AnimeGroupVM grp = LevelObject as AnimeGroupVM;
				if (grp == null) return serList;

				foreach (AnimeSeriesVM ser in grp.AllAnimeSeries)
				{

					// categories
					if (!string.IsNullOrEmpty(SelectedCategories))
					{
						string filterParm = SelectedCategories.Trim();

						string[] cats = filterParm.Split(',');

						bool foundCat = false;
						if (cboCatFilter.SelectedIndex == 1) foundCat = true; // all

						int index = 0;
						foreach (string cat in cats)
						{
							if (cat.Trim().Length == 0) continue;
							if (cat.Trim() == ",") continue;

							index = ser.CategoriesString.IndexOf(cat, 0, StringComparison.InvariantCultureIgnoreCase);

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

			PopulateCategories();
		}

		private void PopulateCategories()
		{
			AllCategoryNames = new ObservableCollection<string>();

			ViewCategoryNames = CollectionViewSource.GetDefaultView(AllCategoryNames);
			List<string> catsRaw = JMMServerVM.Instance.clientBinaryHTTP.GetAllCategoryNames();

			foreach (string cat in catsRaw)
				AllCategoryNames.Add(cat);

		}
	}
}

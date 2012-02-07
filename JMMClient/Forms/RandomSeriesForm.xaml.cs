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
	/// Interaction logic for RandomSeriesForm.xaml
	/// </summary>
	public partial class RandomSeriesForm : Window
	{
		private RandomSeriesEpisodeLevel LevelType = RandomSeriesEpisodeLevel.All;
		private object LevelObject = null;
		private static Random rndm = new Random();

		public ICollectionView ViewCategoryNames { get; set; }
		public ObservableCollection<string> AllCategoryNames { get; set; }

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

		public static readonly DependencyProperty CategoriesExpandedProperty = DependencyProperty.Register("CategoriesExpanded",
			typeof(bool), typeof(RandomSeriesForm), new UIPropertyMetadata(false, null));

		public bool CategoriesExpanded
		{
			get { return (bool)GetValue(CategoriesExpandedProperty); }
			set { SetValue(CategoriesExpandedProperty, value); }
		}

		public static readonly DependencyProperty CategoriesCollapsedProperty = DependencyProperty.Register("CategoriesCollapsed",
			typeof(bool), typeof(RandomSeriesForm), new UIPropertyMetadata(true, null));

		public bool CategoriesCollapsed
		{
			get { return (bool)GetValue(CategoriesCollapsedProperty); }
			set { SetValue(CategoriesCollapsedProperty, value); }
		}

		public static readonly DependencyProperty SelectedCategoriesProperty = DependencyProperty.Register("SelectedCategories",
			typeof(string), typeof(RandomSeriesForm), new UIPropertyMetadata("", null));

		public string SelectedCategories
		{
			get { return (string)GetValue(SelectedCategoriesProperty); }
			set { SetValue(SelectedCategoriesProperty, value); }
		}

		public static readonly DependencyProperty MatchesFoundProperty = DependencyProperty.Register("MatchesFound",
			typeof(int), typeof(RandomSeriesForm), new UIPropertyMetadata(0, null));

		public int MatchesFound
		{
			get { return (int)GetValue(MatchesFoundProperty); }
			set { SetValue(MatchesFoundProperty, value); }
		}

		public static readonly DependencyProperty SelectedCategoryFilterProperty = DependencyProperty.Register("SelectedCategoryFilter",
			typeof(string), typeof(RandomSeriesForm), new UIPropertyMetadata("Any", null));

		public string SelectedCategoryFilter
		{
			get { return (string)GetValue(SelectedCategoryFilterProperty); }
			set { SetValue(SelectedCategoryFilterProperty, value); }
		}

		public RandomSeriesForm()
		{
			InitializeComponent();

			btnRandomSeries.Click += new RoutedEventHandler(btnRandomSeries_Click);
			btnSelectSeries.Click += new RoutedEventHandler(btnSelectSeries_Click);
			this.Loaded += new RoutedEventHandler(RandomSeriesForm_Loaded);
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

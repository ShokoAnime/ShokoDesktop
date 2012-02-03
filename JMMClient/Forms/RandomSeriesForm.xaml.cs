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

		public RandomSeriesForm()
		{
			InitializeComponent();

			btnRandomSeries.Click += new RoutedEventHandler(btnRandomSeries_Click);
			this.Loaded += new RoutedEventHandler(RandomSeriesForm_Loaded);
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

				List<AnimeSeriesVM> serList = null;
				if (LevelType == RandomSeriesEpisodeLevel.GroupFilter)
					serList = GetSeriesForGroupFilter();

				if (serList != null && serList.Count > 0)
				{
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

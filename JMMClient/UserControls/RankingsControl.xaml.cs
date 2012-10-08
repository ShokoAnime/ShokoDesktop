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
using System.Windows.Navigation;
using System.Windows.Shapes;
using JMMClient.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for RankingsControl.xaml
	/// </summary>
	public partial class RankingsControl : UserControl
	{
		public List<AniDB_AnimeDetailedVM> AllAnime = null;

		public ObservableCollection<AnimeRanking> AllRankings = null;
		//public ICollectionView ViewAnimeRankings { get; set; }

		public ListCollectionView ViewUserRankings { get; set; }

		public RankingsControl()
		{
			InitializeComponent();

			AllAnime = new List<AniDB_AnimeDetailedVM>();
			AllRankings = new ObservableCollection<AnimeRanking>();

			//ViewAnimeRankings = CollectionViewSource.GetDefaultView(AllAnimeRankings);

			ViewUserRankings = new ListCollectionView(AllRankings);
			GroupByYearUserRating(ViewUserRankings);
		
			
		}

		private void SortByOverallUserRating(ListCollectionView view)
		{
			view.SortDescriptions.Clear();
			view.GroupDescriptions.Clear();

			view.SortDescriptions.Add(new SortDescription("UserRating", ListSortDirection.Descending));
			view.SortDescriptions.Add(new SortDescription("Year", ListSortDirection.Descending));
			view.SortDescriptions.Add(new SortDescription("AnimeName", ListSortDirection.Ascending));
		}

		private void GroupByYearUserRating(ListCollectionView view)
		{
			view.SortDescriptions.Clear();
			view.GroupDescriptions.Clear();

			view.GroupDescriptions.Add(new PropertyGroupDescription("Year"));

			view.SortDescriptions.Add(new SortDescription("Year", ListSortDirection.Descending));
			view.SortDescriptions.Add(new SortDescription("UserRating", ListSortDirection.Descending));
			view.SortDescriptions.Add(new SortDescription("AnimeName", ListSortDirection.Ascending));
		}

		public void Init()
		{
			AllAnime = MainListHelperVM.Instance.AllAnimeDetailedDictionary.Values.ToList();

			List<AnimeRanking> rankings = new List<AnimeRanking>();

			int i = 0;
			foreach (AniDB_AnimeDetailedVM anime in AllAnime)
			{
				i++;
				AnimeRanking ranking = new AnimeRanking()
				{
					AnimeName = anime.AniDB_Anime.MainTitle,
					Ranking = 1,
					Rating = String.Format("{0:0.00}", anime.AniDB_Anime.AniDBRating),
					UserRating = anime.UserRating,
					Year = anime.AniDB_Anime.BeginYear
				};
				AllRankings.Add(ranking);
				//if (i == 50) break;
			}

			/*List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
			sortCriteria.Add(new SortPropOrFieldAndDirection("Year", true, SortType.eInteger));
			sortCriteria.Add(new SortPropOrFieldAndDirection("UserRating", true, SortType.eInteger));
			rankings = Sorting.MultiSort<AnimeRanking>(rankings, sortCriteria);
			foreach (AnimeRanking rnk in rankings)
				AllRankings.Add(rnk);*/

			ViewUserRankings.Refresh();
		}
	}

	public class AnimeRanking
	{
		public string AnimeName { get; set; }
		public int Ranking { get; set; }
		public string Rating { get; set; }
		public decimal UserRating { get; set; }
		public int Year { get; set; }
	}
}

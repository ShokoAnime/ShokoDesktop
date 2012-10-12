using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class AnimeRatingVM : BindableObject
	{
		public int AnimeID { get; set; }

		public string AnimeName { get; set; }
		public string Rating { get; set; }
		public int Year { get; set; }

		private decimal userRating = 0;
		public decimal UserRating
		{
			get { return userRating; }
			set
			{
				userRating = value;
				base.RaisePropertyChanged("UserRating");
			}
		}

		private AniDB_AnimeDetailedVM animeDetailed = null;
		public AniDB_AnimeDetailedVM AnimeDetailed
		{
			get { return animeDetailed; }
			set
			{
				animeDetailed = value;
				base.RaisePropertyChanged("AnimeDetailed");
			}
		}

		private AnimeSeriesVM animeSeries = null;
		public AnimeSeriesVM AnimeSeries
		{
			get { return animeSeries; }
			set
			{
				animeSeries = value;
				base.RaisePropertyChanged("AnimeSeries");
			}
		}

		public AnimeRatingVM()
		{
		}

		public void Populate(JMMServerBinary.Contract_AnimeRating contract)
		{
			this.AnimeID = contract.AnimeID;
			this.AnimeDetailed = new AniDB_AnimeDetailedVM();
			this.AnimeDetailed.Populate(contract.AnimeDetailed, AnimeID);
			if (contract.AnimeSeries != null)
				this.AnimeSeries = new AnimeSeriesVM(contract.AnimeSeries);

			AnimeName = AnimeDetailed.AniDB_Anime.MainTitle;
			Rating = String.Format("{0:0.00}", AnimeDetailed.AniDB_Anime.AniDBRating);
			UserRating = AnimeDetailed.UserRating;
			Year = AnimeDetailed.AniDB_Anime.BeginYear;
		}

		public AnimeRatingVM(JMMServerBinary.Contract_AnimeRating contract)
		{
			Populate(contract);
		}
	}
}

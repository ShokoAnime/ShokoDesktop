using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace JMMClient.ViewModel 
{
	public class AniDB_Anime_RelationVM : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

		public int AniDB_Anime_RelationID { get; set; }
		public string RelationType { get; set; }
		public int RelatedAnimeID { get; set; }

		public AniDB_AnimeVM AniDB_Anime { get; set; }
		public AnimeSeriesVM AnimeSeries { get; set; }

		private string displayName = "";
		public string DisplayName
		{
			get { return displayName; }
			set
			{
				displayName = value;
				NotifyPropertyChanged("DisplayName");
			}
		}

		private int animeID = 0;
		public int AnimeID
		{
			get { return animeID; }
			set
			{
				animeID = value;
				NotifyPropertyChanged("AnimeID");
			}
		}

		private string aniDB_SiteURL = "";
		public string AniDB_SiteURL
		{
			get { return aniDB_SiteURL; }
			set
			{
				aniDB_SiteURL = value;
				NotifyPropertyChanged("AniDB_SiteURL");
			}
		}

		private bool localSeriesExists = false;
		public bool LocalSeriesExists
		{
			get { return localSeriesExists; }
			set
			{
				localSeriesExists = value;
				NotifyPropertyChanged("LocalSeriesExists");
			}
		}

		private bool animeInfoExists = false;
		public bool AnimeInfoExists
		{
			get { return animeInfoExists; }
			set
			{
				animeInfoExists = value;
				NotifyPropertyChanged("AnimeInfoExists");
			}
		}

		private bool animeInfoNotExists = false;
		public bool AnimeInfoNotExists
		{
			get { return animeInfoNotExists; }
			set
			{
				animeInfoNotExists = value;
				NotifyPropertyChanged("AnimeInfoNotExists");
			}
		}

		private bool showCreateSeriesButton = false;
		public bool ShowCreateSeriesButton
		{
			get { return showCreateSeriesButton; }
			set
			{
				showCreateSeriesButton = value;
				NotifyPropertyChanged("ShowCreateSeriesButton");
			}
		}

		private string posterPath = "";
		public string PosterPath
		{
			get { return posterPath; }
			set
			{
				posterPath = value;
				NotifyPropertyChanged("PosterPath");
			}
		}

		private int sortPriority = 100;
		public int SortPriority
		{
			get { return sortPriority; }
			set
			{
				sortPriority = value;
				NotifyPropertyChanged("SortPriority");
			}
		}

		public AniDB_Anime_RelationVM()
		{
		}

		public void PopulateAnime(JMMServerBinary.Contract_AniDBAnime animeContract)
		{
			if (animeContract != null)
				AniDB_Anime = new AniDB_AnimeVM(animeContract);

			EvaluateProperties();
		}

		public void PopulateSeries(JMMServerBinary.Contract_AnimeSeries seriesContract)
		{
			if (seriesContract != null)
				AnimeSeries = new AnimeSeriesVM(seriesContract);

			EvaluateProperties();
		}

		public void EvaluateProperties()
		{
			if (AniDB_Anime != null)
			{
				DisplayName = AniDB_Anime.FormattedTitle;
				AnimeInfoExists = true;
				PosterPath = AniDB_Anime.PosterPath;
			}
			else
			{
				DisplayName = "Data Missing";
				AnimeInfoExists = false;
				PosterPath = string.Format("pack://application:,,,/{0};component/Images/blankposter.png", Constants.AssemblyName);
			}

			AnimeInfoNotExists = !AnimeInfoExists;

			if (AnimeSeries != null)
				LocalSeriesExists = true;
			else
				LocalSeriesExists = false;

			if (!localSeriesExists && AnimeInfoExists) ShowCreateSeriesButton = true;
			else ShowCreateSeriesButton = false;
		}

		public void Populate(JMMServerBinary.Contract_AniDB_Anime_Relation details)
		{
			this.AniDB_Anime_RelationID = details.AniDB_Anime_RelationID;
			this.AnimeID = details.AnimeID;
			this.RelationType = details.RelationType;
			this.RelatedAnimeID = details.RelatedAnimeID;

			AniDB_SiteURL = string.Format(Constants.URLS.AniDB_Series, RelatedAnimeID);

			SortPriority = int.MaxValue;
			if (RelationType.Equals("Prequel", StringComparison.InvariantCultureIgnoreCase)) SortPriority = 1;
			if (RelationType.Equals("Sequel", StringComparison.InvariantCultureIgnoreCase)) SortPriority = 2;
			

			PopulateAnime(details.AniDB_Anime);
			PopulateSeries(details.AnimeSeries);
		}
	}
}

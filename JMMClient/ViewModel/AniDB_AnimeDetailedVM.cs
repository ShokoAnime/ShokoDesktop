using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace JMMClient.ViewModel
{
	public class AniDB_AnimeDetailedVM : INotifyPropertyChanged
	{
		private int AnimeID { get; set; }

		public AniDB_AnimeVM AniDB_Anime { get; set; }
		public AniDB_VoteVM UserVote { get; set; }

		public List<AnimeCategoryVM> AnimeCategories { get; set; }
		public List<AnimeCategoryVM> AnimeCategoriesSummary { get; set; }
		public List<AnimeTagVM> AnimeTags { get; set; }
		public List<AnimeTagVM> AnimeTagsSummary { get; set; }

		public List<AnimeTitleVM> AnimeTitles { get; set; }
		public List<AnimeTitleVM> AnimeTitlesSummary { get; set; }
		public List<AnimeTitleVM> AnimeTitlesMain { get; set; }
		public List<AnimeTitleVM> AnimeTitlesOfficial { get; set; }
		public List<AnimeTitleVM> AnimeTitlesSynonym { get; set; }
		public List<AnimeTitleVM> AnimeTitlesShort { get; set; }

		public string Stat_AllVideoQuality { get; set; }
		public string Stat_AllVideoQuality_Episodes { get; set; }
		public string Stat_AudioLanguages { get; set; }
		public string Stat_SubtitleLanguages { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

	
		public AniDB_AnimeDetailedVM()
		{
			AnimeTitles = new List<AnimeTitleVM>();
			AnimeTitlesSummary = new List<AnimeTitleVM>();
			AnimeTitlesMain = new List<AnimeTitleVM>();
			AnimeTitlesOfficial = new List<AnimeTitleVM>();
			AnimeTitlesSynonym = new List<AnimeTitleVM>();
			AnimeTitlesShort = new List<AnimeTitleVM>();

			AnimeCategories = new List<AnimeCategoryVM>();
			AnimeCategoriesSummary = new List<AnimeCategoryVM>();

			AnimeTags = new List<AnimeTagVM>();
			AnimeTagsSummary = new List<AnimeTagVM>();
			
		}

		/*public string FormattedTitle
		{
			get
			{
				foreach (NamingLanguage nlan in Languages.PreferredNamingLanguages)
				{
					string thisLanguage = nlan.Language.Trim().ToUpper();
					// Romaji and English titles will be contained in MAIN and/or OFFICIAL
					// we won't use synonyms for these two languages
					if (thisLanguage == "X-JAT" || thisLanguage == "EN")
					{
						// first try the  Main title
						if (AnimeTitlesMain[0].Language.Trim().ToUpper() == thisLanguage) return AnimeTitlesMain[0].Title;
					}

					// now check official
					// now try the official title
					for (int i = 0; i < AnimeTitlesOfficial.Count; i++)
					{
						if (AnimeTitlesOfficial[i].Language.Trim().ToUpper() == thisLanguage) return AnimeTitlesOfficial[i].Title;
					}

					// try synonyms
					if (JMMServerVM.Instance.LanguageUseSynonyms)
					{
						for (int i = 0; i < AnimeTitlesSynonym.Count; i++)
						{
							if (AnimeTitlesSynonym[i].Language.Trim().ToUpper() == thisLanguage) return AnimeTitlesSynonym[i].Title;
						}
					}
					

				}

				// otherwise just use the main title
				return AnimeTitlesMain[0].Title;
			}
		}*/

		private bool userHasVoted = false;
		public bool UserHasVoted
		{
			get { return userHasVoted; }
			set
			{
				userHasVoted = value;
				NotifyPropertyChanged("UserHasVoted");
			}
		}

		private bool userHasNotVoted = false;
		public bool UserHasNotVoted
		{
			get { return userHasNotVoted; }
			set
			{
				userHasNotVoted = value;
				NotifyPropertyChanged("UserHasNotVoted");
			}
		}

		private decimal userRating = 0;
		public decimal UserRating
		{
			get { return userRating; }
			set
			{
				userRating = value;
				NotifyPropertyChanged("UserRating");
			}
		}

		private string userRatingFormatted = "";
		public string UserRatingFormatted
		{
			get { return userRatingFormatted; }
			set
			{
				userRatingFormatted = value;
				NotifyPropertyChanged("UserRatingFormatted");
			}
		}

		public void RefreshBase()
		{
			JMMServerBinary.Contract_AniDB_AnimeDetailed contract = JMMServerVM.Instance.clientBinaryHTTP.GetAnimeDetailed(AnimeID);
			Populate(contract, AnimeID);
		}

		public void Populate(JMMServerBinary.Contract_AniDB_AnimeDetailed contract, int animeID)
		{
			AnimeID = animeID;

			AnimeTitles = new List<AnimeTitleVM>();
			AnimeTitlesSummary = new List<AnimeTitleVM>();
			AnimeTitlesMain = new List<AnimeTitleVM>();
			AnimeTitlesOfficial = new List<AnimeTitleVM>();
			AnimeTitlesSynonym = new List<AnimeTitleVM>();
			AnimeTitlesShort = new List<AnimeTitleVM>();

			AnimeCategories = new List<AnimeCategoryVM>();
			AnimeCategoriesSummary = new List<AnimeCategoryVM>();

			AnimeTags = new List<AnimeTagVM>();
			AnimeTagsSummary = new List<AnimeTagVM>();

			try
			{
				AniDB_Anime = new AniDB_AnimeVM(contract.AniDBAnime);
				UserVote = null;
				if (contract.UserVote != null)
					UserVote = new AniDB_VoteVM(contract.UserVote);

				UserHasVoted = UserVote != null;
				UserHasNotVoted = UserVote == null;

				if (UserVote == null)
					UserRating = 0;
				else
					UserRating = UserVote.VoteValue;

				UserRatingFormatted = Utils.FormatAniDBRating((double)UserRating);
				if (UserVote != null)
				{
					UserRatingFormatted += " (";
					if (UserVote.VoteType == 1) UserRatingFormatted += Properties.Resources.VoteTypeAnimePermanent;
					if (UserVote.VoteType == 2) UserRatingFormatted += Properties.Resources.VoteTypeAnimeTemporary;
					UserRatingFormatted += ")";
				}

				this.Stat_AllVideoQuality = contract.Stat_AllVideoQuality;
				this.Stat_AllVideoQuality_Episodes = contract.Stat_AllVideoQuality_Episodes;
				this.Stat_AudioLanguages = contract.Stat_AudioLanguages;
				this.Stat_SubtitleLanguages = contract.Stat_SubtitleLanguages;

				foreach (JMMServerBinary.Contract_AnimeCategory cat in contract.Categories)
				{
					AnimeCategoryVM vcat = new AnimeCategoryVM(cat);
					AnimeCategories.Add(vcat);

				}
				AnimeCategories.Sort();

				int i = 0;
				foreach (AnimeCategoryVM cat in AnimeCategories)
				{
					if (i <= 5)
						AnimeCategoriesSummary.Add(cat);
					else
						break;
					i++;
				}

				foreach (JMMServerBinary.Contract_AnimeTag tag in contract.Tags)
				{
					AnimeTagVM vtag = new AnimeTagVM(tag);
					AnimeTags.Add(vtag);
				}
				AnimeTags.Sort();

				i = 0;
				foreach (AnimeTagVM tag in AnimeTags)
				{
					if (i <= 5)
						AnimeTagsSummary.Add(tag);
					else
						break;
					i++;
				}

				foreach (JMMServerBinary.Contract_AnimeTitle title in contract.AnimeTitles)
				{
					AnimeTitleVM vtitle = new AnimeTitleVM(title);
					AnimeTitles.Add(vtitle);

					if (title.TitleType.Trim().ToUpper() == Constants.AnimeTitleType.Main.ToUpper())
						AnimeTitlesMain.Add(vtitle);

					if (title.TitleType.Trim().ToUpper() == Constants.AnimeTitleType.Official.ToUpper())
						AnimeTitlesOfficial.Add(vtitle);

					if (title.TitleType.Trim().ToUpper() == Constants.AnimeTitleType.Synonym.ToUpper())
						AnimeTitlesSynonym.Add(vtitle);

					if (title.TitleType.Trim().ToUpper() == Constants.AnimeTitleType.ShortName.ToUpper())
						AnimeTitlesShort.Add(vtitle);
				}
				i = 0;
				foreach (AnimeTitleVM title in AnimeTitlesOfficial)
				{
					if (i <= 5)
						AnimeTitlesSummary.Add(title);
					else
						break;
					i++;
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}

	}
}

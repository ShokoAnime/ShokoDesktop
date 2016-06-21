using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace JMMClient.ViewModel
{
    public class AniDB_AnimeDetailedVM : INotifyPropertyChanged
    {
        private int AnimeID { get; set; }

        public AniDB_AnimeVM AniDB_Anime { get; set; }
        public AniDB_VoteVM UserVote { get; set; }

        public List<AnimeTagVM> AnimeTags { get; set; }
        public List<AnimeTagVM> AnimeTagsSummary { get; set; }
        public List<CustomTagVM> CustomTags { get; set; }
        public ICollectionView ViewCustomTags { get; set; }

        public List<AnimeTitleVM> AnimeTitles { get; set; }
        public List<AnimeTitleVM> AnimeTitlesSummary { get; set; }
        public List<AnimeTitleVM> AnimeTitlesMain { get; set; }
        public List<AnimeTitleVM> AnimeTitlesOfficial { get; set; }
        public List<AnimeTitleVM> AnimeTitlesSynonym { get; set; }
        public List<AnimeTitleVM> AnimeTitlesShort { get; set; }

		public HashSet<string> Stat_AllVideoQuality { get; set; }
		public HashSet<string> Stat_AllVideoQuality_Episodes { get; set; }
		public HashSet<string> Stat_AudioLanguages { get; set; }
		public HashSet<string> Stat_SubtitleLanguages { get; set; }

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

            AnimeTags = new List<AnimeTagVM>();
            AnimeTagsSummary = new List<AnimeTagVM>();
            CustomTags = new List<CustomTagVM>();
            ViewCustomTags = CollectionViewSource.GetDefaultView(CustomTags);
        }

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

            AnimeTags = new List<AnimeTagVM>();
            AnimeTagsSummary = new List<AnimeTagVM>();
            CustomTags.Clear();

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

				this.Stat_AllVideoQuality = new HashSet<string>(contract.Stat_AllVideoQuality);
				this.Stat_AllVideoQuality_Episodes = new HashSet<string>(contract.Stat_AllVideoQuality_Episodes);
				this.Stat_AudioLanguages = new HashSet<string>(contract.Stat_AudioLanguages);
				this.Stat_SubtitleLanguages = new HashSet<string>(contract.Stat_SubtitleLanguages);

                foreach (JMMServerBinary.Contract_AnimeTag tag in contract.Tags)
                {
                    AnimeTagVM vtag = new AnimeTagVM(tag);
                    AnimeTags.Add(vtag);
                }
                //AnimeTags.Sort();

                List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
                sortCriteria.Add(new SortPropOrFieldAndDirection("Weight", true, SortType.eInteger));
                AnimeTags = Sorting.MultiSort<AnimeTagVM>(AnimeTags, sortCriteria);

                int i = 0;
                /*
                foreach (AnimeTagVM tag in AnimeTags)
				{
					if (i <= 5)
						AnimeTagsSummary.Add(tag);
					else
						break;
					i++;
				}
                */

                foreach (JMMServerBinary.Contract_CustomTag ctag in contract.CustomTags)
                {
                    CustomTagVM vtag = new CustomTagVM(ctag);
                    CustomTags.Add(vtag);
                }
                CustomTags.Sort();
                ViewCustomTags.Refresh();

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

using System;

namespace JMMClient.ViewModel
{
    public class AniDB_EpisodeVM
    {
        public int AniDB_EpisodeID { get; set; }
        public int EpisodeID { get; set; }
        public int AnimeID { get; set; }
        public int LengthSeconds { get; set; }
        public string Rating { get; set; }
        public string Votes { get; set; }
        public int EpisodeNumber { get; set; }
        public int EpisodeType { get; set; }
        public string RomajiName { get; set; }
        public string EnglishName { get; set; }
        public int AirDate { get; set; }
        public DateTime DateTimeUpdated { get; set; }

        public AniDB_EpisodeVM()
        {
        }

        public AniDB_EpisodeVM(JMMClient.JMMServerBinary.Contract_AniDB_Episode contract)
        {
            this.AniDB_EpisodeID = contract.AniDB_EpisodeID;
            this.EpisodeID = contract.EpisodeID;
            this.AnimeID = contract.AnimeID;
            this.LengthSeconds = contract.LengthSeconds;
            this.Rating = contract.Rating;
            this.Votes = contract.Votes;
            this.EpisodeNumber = contract.EpisodeNumber;
            this.EpisodeType = contract.EpisodeType;
            this.RomajiName = contract.RomajiName;
            this.EnglishName = contract.EnglishName;
            this.AirDate = contract.AirDate;
            this.DateTimeUpdated = contract.DateTimeUpdated;

        }

        public string EpisodeName
        {
            get
            {
                if (!string.IsNullOrEmpty(EnglishName)) return EnglishName;
                return RomajiName;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", EpisodeNumber, EpisodeName);
        }
    }
}

namespace JMMClient.ViewModel
{
    public class CrossRef_AniDB_TraktEpisodeVM
    {
        public int CrossRef_AniDB_Trakt_EpisodeID { get; set; }
        public int AnimeID { get; set; }
        public int AniDBEpisodeID { get; set; }
        public string TraktID { get; set; }
        public int Season { get; set; }
        public int EpisodeNumber { get; set; }

        public CrossRef_AniDB_TraktEpisodeVM()
        {
        }

        public CrossRef_AniDB_TraktEpisodeVM(JMMServerBinary.Contract_CrossRef_AniDB_Trakt_Episode contract)
        {
            this.AnimeID = contract.AnimeID;
            this.AniDBEpisodeID = contract.AniDBEpisodeID;
            this.CrossRef_AniDB_Trakt_EpisodeID = contract.CrossRef_AniDB_Trakt_EpisodeID;
            this.TraktID = contract.TraktID;
            this.Season = contract.Season;
            this.EpisodeNumber = contract.EpisodeNumber;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1} = {2}", AnimeID, AniDBEpisodeID, EpisodeNumber);
        }
    }
}

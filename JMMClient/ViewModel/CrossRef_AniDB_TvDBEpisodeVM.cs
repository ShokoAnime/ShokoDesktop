namespace JMMClient.ViewModel
{
    public class CrossRef_AniDB_TvDBEpisodeVM
    {
        public int CrossRef_AniDB_TvDB_EpisodeID { get; set; }
        public int AnimeID { get; set; }
        public int AniDBEpisodeID { get; set; }
        public int TvDBEpisodeID { get; set; }

        public CrossRef_AniDB_TvDBEpisodeVM()
        {
        }

        public CrossRef_AniDB_TvDBEpisodeVM(JMMServerBinary.Contract_CrossRef_AniDB_TvDB_Episode contract)
        {
            this.AnimeID = contract.AnimeID;
            this.AniDBEpisodeID = contract.AniDBEpisodeID;
            this.CrossRef_AniDB_TvDB_EpisodeID = contract.CrossRef_AniDB_TvDB_EpisodeID;
            this.TvDBEpisodeID = contract.TvDBEpisodeID;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1} = {2}", AnimeID, AniDBEpisodeID, TvDBEpisodeID);
        }
    }
}

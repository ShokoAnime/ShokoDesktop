namespace JMMClient.ViewModel
{
    public class CrossRef_AniDB_MALResultVM
    {
        public int AnimeID { get; set; }
        public int MALID { get; set; }
        public int CrossRefSource { get; set; }
        public string MALTitle { get; set; }
        public int StartEpisodeType { get; set; }
        public int StartEpisodeNumber { get; set; }
        public int IsAdminApproved { get; set; }

        public string SiteURL
        {
            get
            {
                return string.Format(Constants.URLS.MAL_Series, MALID);
            }
        }

        public string StartEpisodeTypeString
        {
            get
            {
                return EnumTranslator.EpisodeTypeTranslated((EpisodeType)StartEpisodeType);
            }
        }

        public bool AdminApproved
        {
            get
            {
                return IsAdminApproved == 1;
            }
        }

        public CrossRef_AniDB_MALResultVM()
        {
        }

        public CrossRef_AniDB_MALResultVM(JMMServerBinary.Contract_CrossRef_AniDB_MALResult contract)
        {
            this.AnimeID = contract.AnimeID;
            this.MALID = contract.MALID;
            this.CrossRefSource = contract.CrossRefSource;
            this.MALTitle = contract.MALTitle;
            this.StartEpisodeType = contract.StartEpisodeType;
            this.StartEpisodeNumber = contract.StartEpisodeNumber;
            this.IsAdminApproved = contract.IsAdminApproved;
        }

        public override string ToString()
        {
            return string.Format("{0} --- {1}", MALID, MALTitle);
        }
    }
}

namespace JMMClient.ViewModel
{
    public class CrossRef_AniDB_OtherVM
    {
        public int CrossRef_AniDB_OtherID { get; set; }
        public int AnimeID { get; set; }
        public string CrossRefID { get; set; }
        public int CrossRefSource { get; set; }
        public int CrossRefType { get; set; }

        public CrossRef_AniDB_OtherVM()
        {
        }

        public CrossRef_AniDB_OtherVM(JMMServerBinary.Contract_CrossRef_AniDB_Other contract)
        {
            this.CrossRef_AniDB_OtherID = contract.CrossRef_AniDB_OtherID;
            this.AnimeID = contract.AnimeID;
            this.CrossRefID = contract.CrossRefID;
            this.CrossRefSource = contract.CrossRefSource;
            this.CrossRefType = contract.CrossRefType;
        }

        public override string ToString()
        {
            return string.Format("{0} = {1} Type {2}", AnimeID, CrossRefID, CrossRefType);
        }
    }
}

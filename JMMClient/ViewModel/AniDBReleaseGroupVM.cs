namespace JMMClient.ViewModel
{
    public class AniDBReleaseGroupVM
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public bool UserCollecting { get; set; }
        public int FileCount { get; set; }

        public AniDBReleaseGroupVM()
        {
        }

        public AniDBReleaseGroupVM(JMMServerBinary.Contract_AniDBReleaseGroup contract)
        {
            this.GroupID = contract.GroupID;
            this.GroupName = contract.GroupName;
            this.UserCollecting = contract.UserCollecting;
            this.FileCount = contract.FileCount;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1} (Collecting: {2}) (Files: {3})", GroupID, GroupName, UserCollecting, FileCount);
        }
    }
}

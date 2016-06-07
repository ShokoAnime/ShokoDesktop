namespace JMMClient.ViewModel
{
    public class VideoLocalRenamedVM
    {
        public int VideoLocalID { get; set; }
        public VideoLocalVM VideoLocal { get; set; }
        public string NewFileName { get; set; }
        public bool Success { get; set; }

        public string StatusImage
        {
            get
            {
                if (Success) return @"/Images/16_tick.png";

                return @"/Images/16_exclamation.png";
            }
        }

        public VideoLocalRenamedVM(JMMServerBinary.Contract_VideoLocalRenamed contract)
        {
            this.VideoLocalID = contract.VideoLocalID;
            this.NewFileName = contract.NewFileName;
            this.VideoLocal = new VideoLocalVM(contract.VideoLocal);
            this.Success = contract.Success;
        }

        public VideoLocalRenamedVM()
        {
            this.VideoLocalID = -1;
            this.NewFileName = "";
            this.VideoLocal = null;
            this.Success = false;
        }
    }
}

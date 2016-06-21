namespace JMMClient.ViewModel
{
    public class AnimeEpisodeDisplayVM : AnimeEpisodeVM
    {
        private string displayTypeLabel = "";
        public string DisplayTypeLabel
        {
            get { return displayTypeLabel; }
            set
            {
                displayTypeLabel = value;
                NotifyPropertyChanged("DisplayTypeLabel");
            }
        }

        private int episodeOrder = 0;
        public int EpisodeOrder
        {
            get { return episodeOrder; }
            set
            {
                episodeOrder = value;
                NotifyPropertyChanged("EpisodeOrder");
                SetDisplayDetails();
            }
        }

        private void SetDisplayDetails()
        {
            // Episode Type
            if (EpisodeOrder == 0) DisplayTypeLabel = "Previous Episode";
            else if (EpisodeOrder == 1) DisplayTypeLabel = "Next Episode";
            else DisplayTypeLabel = "";

            // Display Options
        }

        public AnimeEpisodeDisplayVM(JMMServerBinary.Contract_AnimeEpisode contract) : base(contract)
        {
            //other stuff here
        }
    }
}

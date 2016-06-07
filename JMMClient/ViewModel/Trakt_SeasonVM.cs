using System.Collections.Generic;

namespace JMMClient.ViewModel
{
    public class Trakt_SeasonVM
    {
        public int Trakt_SeasonID { get; set; }
        public int Trakt_ShowID { get; set; }
        public int Season { get; set; }
        public string URL { get; set; }
        public List<Trakt_EpisodeVM> Episodes { get; set; }

        public Trakt_SeasonVM(JMMServerBinary.Contract_Trakt_Season contract)
        {
            this.Trakt_SeasonID = contract.Trakt_SeasonID;
            this.Trakt_ShowID = contract.Trakt_ShowID;
            this.Season = contract.Season;
            this.URL = contract.URL;
            Episodes = new List<Trakt_EpisodeVM>();

            foreach (JMMServerBinary.Contract_Trakt_Episode ep in contract.Episodes)
                Episodes.Add(new Trakt_EpisodeVM(ep));
        }
    }
}

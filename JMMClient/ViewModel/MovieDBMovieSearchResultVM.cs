namespace JMMClient.ViewModel
{
    public class MovieDBMovieSearchResultVM
    {
        public int MovieID { get; set; }
        public string MovieName { get; set; }
        public string OriginalName { get; set; }
        public string Overview { get; set; }

        public string SiteURL
        {
            get
            {
                return string.Format(Constants.URLS.MovieDB_Series, MovieID);
            }
        }

        public MovieDBMovieSearchResultVM()
        {
        }

        public MovieDBMovieSearchResultVM(JMMServerBinary.Contract_MovieDBMovieSearchResult contract)
        {
            this.MovieID = contract.MovieID;
            this.MovieName = contract.MovieName;
            this.OriginalName = contract.OriginalName;
            this.Overview = contract.Overview;
        }

        public override string ToString()
        {
            return string.Format("{0} --- {1} ({2})", MovieID, MovieName, OriginalName);
        }
    }
}

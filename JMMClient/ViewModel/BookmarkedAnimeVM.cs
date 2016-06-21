using System;
using System.ComponentModel;

namespace JMMClient.ViewModel
{
    public class BookmarkedAnimeVM : INotifyPropertyChanged
    {
        public int? BookmarkedAnimeID { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                var args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }

        private AniDB_AnimeVM aniDB_Anime = null;
        public AniDB_AnimeVM AniDB_Anime
        {
            get { return aniDB_Anime; }
            set
            {
                aniDB_Anime = value;
                NotifyPropertyChanged("AniDB_Anime");
            }
        }

        private int animeID = 0;
        public int AnimeID
        {
            get { return animeID; }
            set
            {
                animeID = value;
                NotifyPropertyChanged("AnimeID");
            }
        }

        private int priority = 0;
        public int Priority
        {
            get { return priority; }
            set
            {
                priority = value;
                NotifyPropertyChanged("Priority");
            }
        }

        private string notes = "";
        public string Notes
        {
            get { return notes; }
            set
            {
                notes = value;
                NotifyPropertyChanged("Notes");
            }
        }

        private int downloading = 0;
        public int Downloading
        {
            get { return downloading; }
            set
            {
                downloading = value;
                NotifyPropertyChanged("Downloading");
                DownloadingBool = downloading == 1;
                NotDownloadingBool = downloading == 0;
            }
        }

        private bool downloadingBool = false;
        public bool DownloadingBool
        {
            get { return downloadingBool; }
            set
            {
                downloadingBool = value;
                NotifyPropertyChanged("DownloadingBool");
            }
        }

        private bool notDownloadingBool = false;
        public bool NotDownloadingBool
        {
            get { return notDownloadingBool; }
            set
            {
                notDownloadingBool = value;
                NotifyPropertyChanged("NotDownloadingBool");
            }
        }

        public BookmarkedAnimeVM()
        {

        }

        public BookmarkedAnimeVM(JMMServerBinary.Contract_BookmarkedAnime contract)
        {
            Populate(contract);
        }

        public void Populate(JMMServerBinary.Contract_BookmarkedAnime contract)
        {
            this.BookmarkedAnimeID = contract.BookmarkedAnimeID;
            this.AnimeID = contract.AnimeID;
            this.Priority = contract.Priority;
            this.Notes = contract.Notes;
            this.Downloading = contract.Downloading;

            if (contract.Anime != null)
                AniDB_Anime = new AniDB_AnimeVM(contract.Anime);
        }

        public bool Save()
        {
            JMMServerBinary.Contract_BookmarkedAnime ba = new JMMServerBinary.Contract_BookmarkedAnime();
            ba.BookmarkedAnimeID = this.BookmarkedAnimeID;
            ba.AnimeID = this.AnimeID;
            ba.Priority = this.Priority;
            ba.Notes = this.Notes;
            ba.Downloading = this.Downloading;
            JMMServerBinary.Contract_BookmarkedAnime_SaveResponse resp = JMMServerVM.Instance.clientBinaryHTTP.SaveBookmarkedAnime(ba);

            if (!string.IsNullOrEmpty(resp.ErrorMessage))
            {
                Utils.ShowErrorMessage(resp.ErrorMessage);
                return false;
            }
            else
                this.Populate(resp.BookmarkedAnime);

            return true;
        }
    }
}

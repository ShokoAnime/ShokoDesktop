using System.ComponentModel;
using Shoko.Commons.Notification;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_BookmarkedAnime : CL_BookmarkedAnime, INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        public new VM_AniDB_Anime Anime
        {
            get { return (VM_AniDB_Anime)base.Anime; }
            set
            {
                base.Anime = this.SetField(base.Anime, value);
            }
        }

        public new int AnimeID
        {
            get { return base.AnimeID; }
            set
            {
                base.AnimeID = this.SetField(base.AnimeID, value);
            }
        }

        public new int Priority
        {
            get { return base.Priority; }
            set
            {
                base.Priority = this.SetField(base.Priority, value);
            }
        }

        public new string Notes
        {
            get { return base.Notes; }
            set
            {
                base.Notes = this.SetField(base.Notes, value);
            }
        }

         public new int Downloading
        {
            get { return base.Downloading; }
            set
            {
                base.Downloading= this.SetField(base.Downloading,value, ()=>Downloading, ()=>DownloadingBool);
            }
        }

        public bool DownloadingBool => Downloading==1;


        public void Populate(CL_BookmarkedAnime contract)
        {
            BookmarkedAnimeID = contract.BookmarkedAnimeID;
            AnimeID = contract.AnimeID;
            Priority = contract.Priority;
            Notes = contract.Notes;
            Downloading = contract.Downloading;
            Anime = (VM_AniDB_Anime)contract.Anime;
        }
        public bool Save()
        {

            CL_Response<CL_BookmarkedAnime> resp = VM_ShokoServer.Instance.ShokoServices.SaveBookmarkedAnime(this);

            if (!string.IsNullOrEmpty(resp.ErrorMessage))
            {
                Utils.ShowErrorMessage(resp.ErrorMessage);
                return false;
            }
            Populate(resp.Result);

            return true;
        }
    }
}

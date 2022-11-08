using System.ComponentModel;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Notification;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_BookmarkedAnime : CL_BookmarkedAnime, INotifyPropertyChanged, INotifyPropertyChangedExt
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
                this.SetField(()=>base.Anime,(r)=> base.Anime = r, value);
            }
        }

        public new int AnimeID
        {
            get { return base.AnimeID; }
            set
            {
                this.SetField(()=>base.AnimeID,(r)=> base.AnimeID = r, value);
            }
        }

        public new int Priority
        {
            get { return base.Priority; }
            set
            {
                this.SetField(()=>base.Priority,(r)=> base.Priority = r, value);
            }
        }

        public new string Notes
        {
            get { return base.Notes; }
            set
            {
                this.SetField(()=>base.Notes,(r)=> base.Notes = r, value);
            }
        }

         public new int Downloading
        {
            get { return base.Downloading; }
            set
            {
                this.SetField(()=>base.Downloading,(r)=>base.Downloading=r, value, ()=>Downloading, ()=>DownloadingBool);
            }
        }

        [JsonIgnore, XmlIgnore]
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

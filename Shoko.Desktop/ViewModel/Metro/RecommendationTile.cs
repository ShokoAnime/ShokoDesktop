using System;
using System.ComponentModel;
using Shoko.Commons.Notification;
using Shoko.Desktop.ViewModel.Server;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Metro
{
    public class RecommendationTile : INotifyPropertyChangedExt
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        private string animeName = null;
        public string AnimeName { get => animeName; set => this.SetField(()=> animeName, value); }

        private string details = null;
        public string Details { get => details; set => this.SetField(()=> details, value); }

        private string picture = null;
        public string Picture { get => picture; set => this.SetField(()=> picture, value); }

        private long height = 0L;
        public long Height { get => height; set => this.SetField(()=> height, value); }

        private string tileSize = null;
        public string TileSize { get => tileSize; set => this.SetField(()=> tileSize, value); }

        private VM_AnimeSeries_User animeSeries = null;
        public VM_AnimeSeries_User AnimeSeries { get => animeSeries; set => this.SetField(()=> animeSeries, value); }

        private string url = null;
        public string URL { get => url; set => this.SetField(()=> url, value); }

        private string source = null;
        public string Source { get => source; set => this.SetField(()=> source, value); }

        private string description = null;
        public string Description { get => description; set => this.SetField(()=> description, value); }

        private int animeID = 0;
        public int AnimeID { get => animeID; set => this.SetField(()=> animeID, value); }

        private int similarAnimeID = 0;
        public int SimilarAnimeID { get => similarAnimeID; set => this.SetField(()=> similarAnimeID, value); }

        private bool hasSeries = false;
        public bool HasSeries { get => hasSeries; set => this.SetField(()=> hasSeries, value); }
    }
}

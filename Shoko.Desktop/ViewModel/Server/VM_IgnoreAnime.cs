using Shoko.Models.Enums;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_IgnoreAnime : CL_IgnoreAnime
    {

        public new VM_AniDB_Anime Anime
        {
            get { return (VM_AniDB_Anime)base.Anime; }
            set { base.Anime = value; }
        }



        public string IgnoreTypeAsString
        {
            get
            {

                switch ((IgnoreAnimeType)IgnoreType)
                {
                    case IgnoreAnimeType.RecDownload: return "Recommendations - Download";
                    case IgnoreAnimeType.RecWatch: return "Recommendations - Watch";
                }

                return "Recommendations - Download";
            }

        }

        public string DisplayName => Anime != null ? Anime.FormattedTitle : $"Anime ID: {AnimeID}";
    }
}

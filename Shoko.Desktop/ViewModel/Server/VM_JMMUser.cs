using System.Collections.Generic;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Models.Server;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_JMMUser : JMMUser
    {
        [JsonIgnore, XmlIgnore]
        public bool IsAdminUser => IsAdmin == 1;

        [JsonIgnore, XmlIgnore]
        public bool IsAniDBUserBool => IsAniDBUser == 1;

        [JsonIgnore, XmlIgnore]
        public bool CanEditSettings => CanEditServerSettings.HasValue && CanEditServerSettings.Value == 1;

        [JsonIgnore, XmlIgnore]
        public bool IsTraktUserBool => IsTraktUser == 1;

        public override string ToString()
        {
            return $"{Username} - {IsAdmin} ({IsAniDBUser}) - {HideCategories}";
        }

        private bool EvaluateTags(HashSet<string> allcats)
        {
            return !allcats.Overlaps(this.GetHideCategories());
        }

        public bool EvaluateGroup(VM_AnimeGroup_User grp)
        {
            return EvaluateTags(grp.Stat_AllTags);
        }

        public bool EvaluateSeries(VM_AnimeSeries_User ser)
        {
            // make sure the user has not filtered this out
            return EvaluateTags(ser.AllTags);
        }

        public bool EvaluateAnime(VM_AniDB_Anime anime)
        {
            // make sure the user has not filtered this out
            return EvaluateTags(anime.GetAllTags());
        }
    }
}

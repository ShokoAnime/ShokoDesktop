using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Commons.Notification;
using Shoko.Commons.Properties;
using Shoko.Desktop.Properties;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models;
using Shoko.Models.Client;
using Shoko.Models.Server;
using Formatting = Shoko.Commons.Utils.Formatting;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AniDB_AnimeDetailed : CL_AniDB_AnimeDetailed, INotifyPropertyChanged, INotifyPropertyChangedExt
    {
        private int AnimeID => AniDBAnime.AnimeID;

        public new VM_AniDB_Anime AniDBAnime
        {
            get { return (VM_AniDB_Anime) base.AniDBAnime; }

            set { base.AniDBAnime = value; }
        }

        public new List<VM_AnimeTitle> AnimeTitles
        {
            get { return base.AnimeTitles.CastList<VM_AnimeTitle>(); }
            set
            {
                base.AnimeTitles = value.CastList<CL_AnimeTitle>();
                AnimeTitlesMain = value.Where(a => a.TitleType.Trim().ToUpper() == Constants.AnimeTitleType.Main.ToUpper()).ToList();
                AnimeTitlesOfficial = value.Where(a => a.TitleType.Trim().ToUpper() == Constants.AnimeTitleType.Official.ToUpper()).ToList();
                AnimeTitlesSynonym = value.Where(a => a.TitleType.Trim().ToUpper() == Constants.AnimeTitleType.Synonym.ToUpper()).ToList();
                AnimeTitlesShort = value.Where(a => a.TitleType.Trim().ToUpper() == Constants.AnimeTitleType.ShortName.ToUpper()).ToList();
                AnimeTitlesSummary = AnimeTitlesOfficial.Take(5).ToList();

            }
        }
        public new List<VM_AnimeTag> Tags
        {
            get { return base.Tags.CastList<VM_AnimeTag>(); }
            set { base.Tags = value.OrderByDescending(a => a.Weight).ToList().CastList<CL_AnimeTag>(); }
        }

        [JsonIgnore, XmlIgnore]
        public List<VM_AnimeTag> TagsSummary => Tags.Take(5).ToList();

        public new List<VM_CustomTag> CustomTags
        {
            get { return base.CustomTags.CastList<VM_CustomTag>(); }
            set
            {
                base.CustomTags = value.OrderBy(a => a.TagName.ToLowerInvariant()).ToList().CastList<CustomTag>();
                ViewCustomTags = CollectionViewSource.GetDefaultView(CustomTags);
                ViewCustomTags.Refresh();
            }
        }
        [JsonIgnore, XmlIgnore]
        public ICollectionView ViewCustomTags { get; set; }

        [JsonIgnore, XmlIgnore]
        public List<VM_AnimeTitle> AnimeTitlesSummary { get; set; }
        [JsonIgnore, XmlIgnore]
        public List<VM_AnimeTitle> AnimeTitlesMain { get; set; }
        [JsonIgnore, XmlIgnore]
        public List<VM_AnimeTitle> AnimeTitlesOfficial { get; set; }
        [JsonIgnore, XmlIgnore]
        public List<VM_AnimeTitle> AnimeTitlesSynonym { get; set; }
        [JsonIgnore, XmlIgnore]
        public List<VM_AnimeTitle> AnimeTitlesShort { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }


        public VM_AniDB_AnimeDetailed()
        {
            
        }

        public new AniDB_Vote UserVote
        {
            get { return base.UserVote; }
            set { this.SetField(()=>base.UserVote,(r)=> base.UserVote = r, value, () => UserHasVoted, () => UserHasNotVoted, ()=>UserRating, ()=>UserRatingFormatted); }
        }

        [JsonIgnore, XmlIgnore]
        public bool UserHasVoted => UserVote!=null;

        [JsonIgnore, XmlIgnore]
        public bool UserHasNotVoted => UserVote==null;

        [JsonIgnore, XmlIgnore]
        public decimal UserRating
        {
            get
            {
                if (UserVote == null)
                    return 0m;
                return Convert.ToDecimal(UserVote.VoteValue) / 100m;
            }
            set
            {
                if (UserVote == null)
                    return;
                UserVote.VoteValue = Convert.ToInt32(value / 100);
            }
        }

        [JsonIgnore, XmlIgnore]
        public string UserRatingFormatted
        {
            get
            {
                string val = Formatting.FormatAniDBRating((double)UserRating);
                if (UserVote != null)
                {
                    val += " (";
                    if (UserVote.VoteType == 1) val += Resources.VoteTypeAnimePermanent;
                    if (UserVote.VoteType == 2) val += Resources.VoteTypeAnimeTemporary;
                    val += ")";
                }
                return val;
            }
        }



        public void Populate(VM_AniDB_AnimeDetailed contract)
        {
            AniDBAnime = contract.AniDBAnime;
            UserVote = contract.UserVote;
            Stat_AllVideoQuality =contract.Stat_AllVideoQuality;
            Stat_AllVideoQuality_Episodes = contract.Stat_AllVideoQuality_Episodes;
            Stat_AudioLanguages = contract.Stat_AudioLanguages;
            Stat_SubtitleLanguages = contract.Stat_SubtitleLanguages;
            Tags = contract.Tags;
            CustomTags = contract.CustomTags;
            AnimeTitles = contract.AnimeTitles;
            AnimeTitles.Add(new VM_AnimeTitle()
            {
                AnimeID = AnimeID,
                Language = Constants.AniDBLanguageType.Romaji,
                Title = AniDBAnime.MainTitle,
                TitleType = Constants.AnimeTitleType.Main
            });
        }

    }
}

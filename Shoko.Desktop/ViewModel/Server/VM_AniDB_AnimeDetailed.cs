using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Shoko.Commons.Extensions;
using Shoko.Commons.Notification;
using Shoko.Commons.Properties;
using Shoko.Commons.Utils;
using Shoko.Desktop.Properties;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models;
using Shoko.Models.Client;
using Shoko.Models.Server;

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
        public new List<CL_AnimeTag> Tags
        {
            get { return base.Tags; }
            set { base.Tags = value.OrderByDescending(a => a.Weight).ToList(); }
        }

        public new List<CustomTag> CustomTags
        {
            get { return base.CustomTags; }
            set
            {
                base.CustomTags = value.OrderBy(a => a.TagName.ToLowerInvariant()).ToList();
                ViewCustomTags = CollectionViewSource.GetDefaultView(CustomTags);
                ViewCustomTags.Refresh();
            }
        }
        public ICollectionView ViewCustomTags { get; set; }

        public List<VM_AnimeTitle> AnimeTitlesSummary { get; set; }
        public List<VM_AnimeTitle> AnimeTitlesMain { get; set; }
        public List<VM_AnimeTitle> AnimeTitlesOfficial { get; set; }
        public List<VM_AnimeTitle> AnimeTitlesSynonym { get; set; }
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
            set { base.UserVote = this.SetField(base.UserVote, value, () => UserHasVoted, () => UserHasNotVoted, ()=>UserRating, ()=>UserRatingFormatted); }
        }

        public bool UserHasVoted => UserVote!=null;

        public bool UserHasNotVoted => UserVote==null;

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

        public void RefreshBase()
        {
            Populate((VM_AniDB_AnimeDetailed)VM_ShokoServer.Instance.ShokoServices.GetAnimeDetailed(AnimeID));
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
        }

    }
}

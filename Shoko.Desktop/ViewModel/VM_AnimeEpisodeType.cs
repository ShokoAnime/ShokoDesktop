using System;
using System.Collections.Generic;
using System.Linq;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Enums;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel
{
    public class VM_AnimeEpisodeType : IListWrapper
    {
        public int ObjectType => 3;
        public bool IsEditable => false;
        public enum SortMethod { EpisodeTypeDescription = 0, EpisodeType };
        public static SortMethod SortType { get; set; }

        public enEpisodeType EpisodeType { get; set; }
        public string EpisodeTypeDescription { get; set; }
        VM_AnimeSeries_User AnimeSeries { get;  }

        public VM_AnimeEpisodeType()
        {

        }

        public string EpisodeSummary => $"{GetDirectChildren().Count} Episodes";

        public VM_AnimeEpisodeType(VM_AnimeSeries_User series, VM_AnimeEpisode_User ep)
        {
            AnimeSeries = series;
            EpisodeType = (enEpisodeType)ep.EpisodeType;
            EpisodeTypeDescription = EpisodeType.EpisodeTypeTranslated();
        }



        public List<IListWrapper> GetDirectChildren()
        {
            try
            {
                return AnimeSeries.AllEpisodes.Where(a=>a.EpisodeTypeEnum==EpisodeType).OrderBy(a=>a.EpisodeNumber).CastList<IListWrapper>();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            return new List<IListWrapper>();
        }
    }
}

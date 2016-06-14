using JMMClient.ViewModel;
using System.Collections.Generic;

namespace JMMClient
{
    /// <summary>
    /// This class is uses a wrapper so we can store either an AnimeGroup or AnimeSeries record
    /// This is needed because a a group contain both sub groups and series, episodes etc, which we need to
    /// display in the same listbox
    /// </summary>
    public abstract class MainListWrapper
    {
        // common


        public MainListWrapper()
        {
        }



        public int ObjectType
        {
            get
            {
                if (this.GetType() == typeof(GroupFilterVM))
                    return 0;
                else if (this.GetType() == typeof(AnimeGroupVM))
                    return 1;
                else if (this.GetType() == typeof(AnimeSeriesVM))
                    return 2;
                else if (this.GetType() == typeof(AnimeEpisodeTypeVM))
                    return 3;
                else if (this.GetType() == typeof(AnimeEpisodeVM))
                    return 4;
                else if (this.GetType() == typeof(VideoDetailedVM))
                    return 5;
                else if (this.GetType() == typeof(VideoLocalVM))
                    return 5;

                return 1;
            }
        }

        public bool IsEditable
        {
            get
            {
                if (this.GetType() == typeof(GroupFilterVM))
                    return false;
                else if (this.GetType() == typeof(AnimeGroupVM))
                    return true;
                else if (this.GetType() == typeof(AnimeSeriesVM))
                    return true;
                else if (this.GetType() == typeof(AnimeEpisodeTypeVM))
                    return false;
                else if (this.GetType() == typeof(AnimeEpisodeVM))
                    return false;
                else if (this.GetType() == typeof(VideoDetailedVM))
                    return false;

                return false;
            }
        }

        public abstract List<MainListWrapper> GetDirectChildren();

    }
}

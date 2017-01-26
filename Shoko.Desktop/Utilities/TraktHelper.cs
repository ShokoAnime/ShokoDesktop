using System;
using NLog;
using Shoko.Desktop.VideoPlayers;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;

namespace Shoko.Desktop.Utilities
{
    class TraktHelper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public enum ScrobblePlayingStatus
        {
            Start = 1,
            Pause = 2,
            Stop = 3
        }
        public enum ScrobblePlayingType
        {
            movie = 1,
            episode = 2
        }

        public void TraktScrobble(ScrobblePlayingStatus scrobblePlayingStatus, VideoInfo info, int position, int duration, bool logOutput = false)
        {
            try
            {
                VM_VideoDetailed vd = (VM_VideoDetailed)VM_ShokoServer.Instance.ShokoServices.GetVideoDetailed(info.VideoLocalId, VM_ShokoServer.Instance.CurrentUser.JMMUserID);

                if (vd == null)
                {
                    if (logOutput)
                        logger.Debug("Trakt scrobbling video detail = null");

                    return;
                }

                CL_AnimeEpisode_User ep =
                    VM_ShokoServer.Instance.ShokoServices.GetEpisodeByAniDBEpisodeID(vd.AnimeEpisodeID, VM_ShokoServer.Instance.CurrentUser.JMMUserID);

                if (ep != null)
                {
                    double percentagePlayed = (int)Math.Round((double)(100 * position) / duration);

                    if (logOutput)
                    {
                        logger.Debug("Trakt is scrobbling for anime episode id: " + ep.AnimeEpisodeID);
                        logger.Debug("Trakt is scrobbling with played percentage: " + (int)percentagePlayed);
                    }

                    VM_ShokoServer.Instance.ShokoServices.TraktScrobble(ep.AnimeEpisodeID,
                        (int)ScrobblePlayingType.episode, (int)percentagePlayed, (int)scrobblePlayingStatus);
                }

                if (logOutput)
                    logger.Debug("Trakt scrobbling has finished scrobbling");
            }
            catch (Exception e)
            {
                if (logOutput)
                    logger.Debug("Error in VideoHandler.TraktScrobble: {0}", e.ToString());
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JMMClient.JMMServerBinary;
using JMMClient.VideoPlayers;
using NLog;

namespace JMMClient.Utilities
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
                Contract_VideoDetailed vd =
                    JMMServerVM.Instance.clientBinaryHTTP.GetVideoDetailed(info.VideoLocalId,
                        JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

                if (vd == null)
                {
                    if (logOutput)
                        logger.Debug("Trakt scrobbling video detail = null");

                    return;
                }

                Contract_AnimeEpisode ep =
                    JMMServerVM.Instance.clientBinaryHTTP.GetEpisodeByAniDBEpisodeID(vd.AnimeEpisodeID, JMMServerVM.Instance.CurrentUser.JMMUserID.Value);

                if (ep != null)
                {
                    double percentagePlayed = (int)Math.Round((double)(100 * position) / duration);

                    if (logOutput)
                    {
                        logger.Debug("Trakt is scrobbling for anime episode id: " + ep.AnimeEpisodeID);
                        logger.Debug("Trakt is scrobbling with played percentage: " + (int)percentagePlayed);
                    }

                    JMMServerVM.Instance.clientBinaryHTTP.TraktScrobble(ep.AnimeEpisodeID,
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

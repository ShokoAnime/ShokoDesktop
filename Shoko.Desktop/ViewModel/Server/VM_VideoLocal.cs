using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_VideoLocal : CL_VideoLocal, IListWrapper
    {
        public int ObjectType { get; } = 5;
        public bool IsEditable { get; } = false;
        public bool IsLocalFile => this.IsLocalFile();
        public string FileDirectory => this.GetFileDirectories();
        public bool IsHashed => this.IsHashed();
        public string FormattedFileSize => this.GetFormattedFileSize();

        // ReSharper disable once EmptyConstructor
        public VM_VideoLocal()
        {
        }

        public string FullPath
        {
            get => Places.FirstOrDefault(a => !string.IsNullOrEmpty(a.GetFullPath())).GetFullPath();
        }


        public string ClosestAnimeMatchString
        {
            get
            {
                // if this file is manually linked to a series already
                // then use that main title
                List<VM_AnimeEpisode_User> eps = VM_ShokoServer.Instance.ShokoServices.GetEpisodesForFile(VideoLocalID, VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeEpisode_User>();

                if (eps.Count > 0)
                {
                    eps[0].RefreshAnime();
                    return eps[0].AniDB_Anime.MainTitle;
                }

                string match = Path.GetFileNameWithoutExtension(FileName);
                if (string.IsNullOrEmpty(match))
                    return match;
                //remove any group names or CRC's
                while (true)
                {
                    int pos = match.IndexOf('[');
                    if (pos >= 0)
                    {
                        int endPos = match.IndexOf(']', pos);
                        if (endPos >= 0)
                        {
                            string rubbish = match.Substring(pos, endPos - pos + 1);
                            match = match.Replace(rubbish, "");
                        }
                        else break;
                    }
                    else break;
                }

                //remove any video information
                while (true)
                {
                    int pos = match.IndexOf('(');
                    if (pos >= 0)
                    {
                        int endPos = match.IndexOf(')', pos);
                        if (endPos >= 0)
                        {
                            string rubbish = match.Substring(pos, endPos - pos + 1);
                            match = match.Replace(rubbish, "");
                        }
                        else break;
                    }
                    else break;
                }

                //if (match.Length < 16)
                //	return match;
                //else
                //	return match.Substring(0, 16);

                return match;
            }
        }


        public List<IListWrapper> GetDirectChildren()
        {
            return null;
        }

        public List<VM_AnimeEpisode_User> GetEpisodes()
        {
            try
            {
                return VM_ShokoServer.Instance.ShokoServices.GetEpisodesForFile(VideoLocalID, VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeEpisode_User>();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }

            return new List<VM_AnimeEpisode_User>();
        }


    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_VideoLocal : CL_VideoLocal, IListWrapper
    {
        [JsonIgnore, XmlIgnore]
        public int ObjectType => 5;
        [JsonIgnore, XmlIgnore]
        public bool IsEditable => false;
        [JsonIgnore, XmlIgnore]
        public bool IsLocalFile => this.IsLocalFile();
        [JsonIgnore, XmlIgnore]
        public bool IsAccessibleFromServer => Places.Any(a => a.ImportFolder?.IsNotCloud() ?? false);
        [JsonIgnore, XmlIgnore]
        public string FileDirectory => string.Join(",", Places.Select(place => place.GetLocalFileSystemFullPath()));

        [JsonIgnore, XmlIgnore]
        public string ServerPath => string.Join(",",
            Places.Select(
                place => place.ImportFolder?.ImportFolderLocation == null || place.FilePath == null
                    ? ""
                    : $"{place.ImportFolder.ImportFolderLocation}{place.FilePath}"));

        [JsonIgnore, XmlIgnore]
        public bool IsHashed => this.IsHashed();
        [JsonIgnore, XmlIgnore]
        public string FormattedFileSize => this.GetFormattedFileSize();

        // ReSharper disable once EmptyConstructor
        public VM_VideoLocal()
        {
        }

        [JsonIgnore, XmlIgnore]
        public string FullPath => Places.FirstOrDefault(a => !string.IsNullOrEmpty(a.GetFullPath())).GetFullPath();

        [JsonIgnore, XmlIgnore]
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
                    return eps[0].AniDBAnime.MainTitle;
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

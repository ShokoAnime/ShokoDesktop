using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using DevExpress.Xpf.Editors.Helpers;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Desktop.Utilities;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_VideoLocal : CL_VideoLocal, IListWrapper
    {
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public int ObjectType => 5;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsEditable => false;
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsLocalFile => this.IsLocalFile();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsAccessibleFromServer => Places.Any(a => a.ImportFolder?.IsNotCloud() ?? false);
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string FileDirectory => string.Join(",", Places.Select(place => place.GetLocalFileSystemFullPath()));

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string ServerPath => string.Join(",",
            Places.Select(
                place => place.ImportFolder?.ImportFolderLocation == null || place.FilePath == null
                    ? ""
                    : $"{place.ImportFolder.ImportFolderLocation}{place.FilePath}"));

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string ED2KUri => $"ed2k://|file|{FileName}|{FileSize}|{Hash}|/";

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public bool IsHashed => this.IsHashed();
        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string FormattedFileSize => this.GetFormattedFileSize();

        // ReSharper disable once EmptyConstructor
        public VM_VideoLocal()
        {
        }

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public string FullPath => Places.FirstOrDefault(a => !string.IsNullOrEmpty(a.GetFullPath())).GetFullPath();

        [ScriptIgnore, JsonIgnore, XmlIgnore]
        public new string FileName => Places.FirstOrDefault(a => !string.IsNullOrEmpty(a.FilePath))?.GetFileName() ?? string.Empty;

        [ScriptIgnore, JsonIgnore, XmlIgnore]
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

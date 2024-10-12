using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Shoko.Commons.Extensions;
using Shoko.Commons.Notification;
using Shoko.Commons.Properties;
using Shoko.Desktop.Properties;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_VideoDetailed : CL_VideoDetailed, IListWrapper, INotifyPropertyChanged, INotifyPropertyChangedExt
    {


        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }


        #region Editable members

        public new int CrossRefSource
        {
            get => base.CrossRefSource;
            set => this.SetField(()=>base.CrossRefSource,(r)=> base.CrossRefSource = r, value, () => CrossRefSource, () => IsAutoAssociation);
        }

        [JsonIgnore, XmlIgnore]
        public bool IsAutoAssociation => base.CrossRefSource == 1;
        [JsonIgnore, XmlIgnore]
        public bool Ignored => base.VideoLocal_IsIgnored == 1;
        [JsonIgnore, XmlIgnore]
        public bool Variation => base.VideoLocal_IsVariation == 1;
        [JsonIgnore, XmlIgnore]
        public string FileName => VideoLocal_FileName;
        [JsonIgnore, XmlIgnore]
        public string FullPath => this.GetFullPath();
        [JsonIgnore, XmlIgnore]
        public bool FileIsAvailable => this.GetFileIsAvailable();
        [JsonIgnore, XmlIgnore]
        public string VideoInfoSummary => this.GetVideoInfoSummary();
        [JsonIgnore, XmlIgnore]
        public string FormattedFileSize => this.GetFormattedFileSize();


        #region Video Properties
        [JsonIgnore, XmlIgnore]
        public string VideoResolution => this.GetVideoResolution();
        [JsonIgnore, XmlIgnore]
        public string VideoCodec => this.GetVideoCodec();
        [JsonIgnore, XmlIgnore]
        public string AudioCodec => this.GetAudioCodec();
        [JsonIgnore, XmlIgnore]
        public bool IsBluRay => this.IsBluRay();
        [JsonIgnore, XmlIgnore]
        public bool IsDVD => this.IsDVD();
        [JsonIgnore, XmlIgnore]
        public bool IsHD => this.IsHD();
        [JsonIgnore, XmlIgnore]
        public bool IsFullHD => this.IsFullHD();
        [JsonIgnore, XmlIgnore]
        public bool IsHi08P => this.IsHi08P();
        [JsonIgnore, XmlIgnore]
        public bool IsHi10P => this.IsHi10P();
        [JsonIgnore, XmlIgnore]
        public bool IsHi12P => this.IsHi12P();
        [JsonIgnore, XmlIgnore]
        public int BitDepth => this.GetBitDepth();
        [JsonIgnore, XmlIgnore]
        public bool IsDualAudio => this.IsDualAudio();
        [JsonIgnore, XmlIgnore]
        public bool IsMultiAudio => this.IsMultiAudio();
        [JsonIgnore, XmlIgnore]
        public bool IsChaptered => AniDB_File_IsChaptered == 1;

        #endregion

        [JsonIgnore, XmlIgnore]
        public bool HasReleaseGroup => this.HasReleaseGroup();
        [JsonIgnore, XmlIgnore]
        public string ReleaseGroupName => this.GetReleaseGroupName();
        [JsonIgnore, XmlIgnore]
        public string ReleaseGroupAniDBURL => this.GetReleaseGroupAniDBURL();
        [JsonIgnore, XmlIgnore]
        public bool HasAniDBFile => this.HasAniDBFile();
        [JsonIgnore, XmlIgnore]
        public string AniDB_SiteURL => this.GetAniDB_SiteURL();

        public new int VideoLocal_IsWatched
        {
            get => base.VideoLocal_IsWatched;
            set => this.SetField(()=>base.VideoLocal_IsWatched,(r)=> base.VideoLocal_IsWatched = r, value, ()=>VideoLocal_IsWatched, ()=>Watched);
        }

        public new int VideoLocal_IsIgnored
        {
            get => base.VideoLocal_IsIgnored;
            set => this.SetField(()=>base.VideoLocal_IsIgnored,(r)=> base.VideoLocal_IsIgnored = r, value, () => VideoLocal_IsIgnored, () => Ignored);
        }

      


        public new int VideoLocal_IsVariation
        {
            get => base.VideoLocal_IsVariation;
            set => this.SetField(()=>base.VideoLocal_IsVariation,(r)=> base.VideoLocal_IsVariation = r, value, ()=>VideoLocal_IsVariation, ()=>Variation);
        }

        [JsonIgnore, XmlIgnore]
        public bool Watched => VideoLocal_IsWatched==1;


        private bool showMoreDetails;
        [JsonIgnore, XmlIgnore]
        public bool ShowMoreDetails
        {
            get => showMoreDetails;
            set => this.SetField(()=>showMoreDetails,value);
        }

        public new DateTime? VideoLocal_WatchedDate
        {
            get => base.VideoLocal_WatchedDate;
            set => this.SetField(()=>base.VideoLocal_WatchedDate,(r)=> base.VideoLocal_WatchedDate = r, value);
        }

        [JsonIgnore, XmlIgnore]
        public string LastWatchedDescription
        {
            get
            {
                if (VideoLocal_WatchedDate.HasValue)
                {
                    DateTime today = DateTime.Now;
                    DateTime yesterday = today.AddDays(-1);

                    if (VideoLocal_WatchedDate.Value.Day == today.Day && VideoLocal_WatchedDate.Value.Month == today.Month && VideoLocal_WatchedDate.Value.Year == today.Year)
                    {
                        return Resources.Today;
                    }
                    if (VideoLocal_WatchedDate.Value.Day == yesterday.Day && VideoLocal_WatchedDate.Value.Month == yesterday.Month && VideoLocal_WatchedDate.Value.Year == yesterday.Year)
                    {
                        return Resources.Yesterday;
                    }

                    return VideoLocal_WatchedDate.Value.ToString("dd MMM yyyy", Commons.Culture.Global);
                }
                return string.Empty;
            }

        }



        #endregion

       
        public VM_VideoDetailed()
        {
            ReleaseGroup = null;
        }

        public void Populate(VM_VideoDetailed contract)
        {
            ReleaseGroup = null;
            AnimeEpisodeID = contract.AnimeEpisodeID;
            Places = contract.Places;
            Percentage = contract.Percentage;
            EpisodeOrder = contract.EpisodeOrder;
            CrossRefSource = contract.CrossRefSource;
            VideoLocalID = contract.VideoLocalID;
            VideoLocal_FileName = contract.VideoLocal_FileName;
            VideoLocal_ResumePosition = contract.VideoLocal_ResumePosition;
            VideoLocal_Hash = contract.VideoLocal_Hash;
            VideoLocal_FileSize = contract.VideoLocal_FileSize;
            VideoLocal_IsWatched = contract.VideoLocal_IsWatched;
            VideoLocal_WatchedDate = contract.VideoLocal_WatchedDate;
            VideoLocal_IsIgnored = contract.VideoLocal_IsIgnored;
            VideoLocal_IsVariation = contract.VideoLocal_IsVariation;
            VideoLocal_MD5 = contract.VideoLocal_MD5;
            VideoLocal_SHA1 = contract.VideoLocal_SHA1;
            VideoLocal_CRC32 = contract.VideoLocal_CRC32;
            VideoLocal_HashSource = contract.VideoLocal_HashSource;
            VideoInfo_VideoCodec = contract.VideoInfo_VideoCodec;
            VideoInfo_VideoBitrate = contract.VideoInfo_VideoBitrate;
            VideoInfo_VideoBitDepth = contract.VideoInfo_VideoBitDepth;
            VideoInfo_VideoFrameRate = contract.VideoInfo_VideoFrameRate;
            VideoInfo_VideoResolution = contract.VideoInfo_VideoResolution;
            VideoInfo_AudioCodec = contract.VideoInfo_AudioCodec;
            VideoInfo_AudioBitrate = contract.VideoInfo_AudioBitrate;
            VideoInfo_Duration = contract.VideoInfo_Duration;
            AniDB_Anime_GroupName = contract.AniDB_Anime_GroupName;
            AniDB_Anime_GroupNameShort = contract.AniDB_Anime_GroupNameShort;
            AniDB_AnimeID = contract.AniDB_AnimeID;
            AniDB_CRC = contract.AniDB_CRC;
            AniDB_Episode_Rating = contract.AniDB_Episode_Rating;
            AniDB_Episode_Votes = contract.AniDB_Episode_Votes;
            AniDB_File_AudioCodec = contract.AniDB_File_AudioCodec;
            AniDB_File_Description = contract.AniDB_File_Description;
            AniDB_File_FileExtension = contract.AniDB_File_FileExtension;
            AniDB_File_LengthSeconds = contract.AniDB_File_LengthSeconds;
            AniDB_File_ReleaseDate = contract.AniDB_File_ReleaseDate;
            AniDB_File_Source = contract.AniDB_File_Source;
            AniDB_File_VideoCodec = contract.AniDB_File_VideoCodec;
            AniDB_File_VideoResolution = contract.AniDB_File_VideoResolution;
            AniDB_FileID = contract.AniDB_FileID;
            AniDB_GroupID = contract.AniDB_GroupID;
            AniDB_MD5 = contract.AniDB_MD5;
            AniDB_SHA1 = contract.AniDB_SHA1;
            AniDB_File_FileVersion = contract.AniDB_File_FileVersion;
            AniDB_File_IsChaptered = contract.AniDB_File_IsChaptered;
            AniDB_File_IsCensored = contract.AniDB_File_IsCensored;
            AniDB_File_IsDeprecated = contract.AniDB_File_IsDeprecated;
            LanguagesAudio = contract.LanguagesAudio;
            LanguagesSubtitle = contract.LanguagesSubtitle;
            Media = contract.Media;
            ReleaseGroup = contract.ReleaseGroup;

        }

        public List<IListWrapper> GetDirectChildren()
        {
            return new List<IListWrapper>();
        }

        [JsonIgnore, XmlIgnore]
        public string VideoFilename => System.IO.Path.GetFileName(Places.FirstOrDefault()?.GetFileName());

        [JsonIgnore, XmlIgnore]
        public int ObjectType => 5;
        [JsonIgnore, XmlIgnore]
        public bool IsEditable => false;

        [JsonIgnore, XmlIgnore] public int PlaceCount => Places.Count;
    }
}

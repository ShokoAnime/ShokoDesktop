using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            get { return base.CrossRefSource; }
            set
            {
                base.CrossRefSource = this.SetField(base.CrossRefSource, value, () => CrossRefSource, () => IsAutoAssociation);
            }
        }

        public bool IsAutoAssociation => base.CrossRefSource == 1;
        public bool Ignored => base.VideoLocal_IsIgnored == 1;
        public bool Variation => base.VideoLocal_IsVariation == 1;
        public string FileName => VideoLocal_FileName;
        public string FullPath => this.GetFullPath();
        public bool FileIsAvailable => this.GetFileIsAvailable();
        public string VideoInfoSummary => this.GetVideoInfoSummary();
        public string FormattedFileSize => this.GetFormattedFileSize();


        #region Video Properties
        public string VideoResolution => this.GetVideoResolution();
        public string VideoCodec => this.GetVideoCodec();
        public string AudioCodec => this.GetAudioCodec();
        public bool IsBluRay => this.IsBluRay();
        public bool IsDVD => this.IsDVD();
        public bool IsHD => this.IsHD();
        public bool IsFullHD => this.IsFullHD();
        public bool IsHi08P => this.IsHi08P();
        public bool IsHi10P => this.IsHi10P();
        public bool IsHi12P => this.IsHi12P();
        public int BitDepth => this.GetBitDepth();

        #endregion

        public bool HasReleaseGroup => this.HasReleaseGroup();
        public string ReleaseGroupName => this.GetReleaseGroupName();
        public string ReleaseGroupAniDBURL => this.GetReleaseGroupAniDBURL();
        public bool HasAniDBFile => this.HasAniDBFile();
        public string AniDB_SiteURL => this.GetAniDB_SiteURL();

        public new int VideoLocal_IsWatched
        {
            get { return base.VideoLocal_IsWatched; }
            set
            {
                base.VideoLocal_IsWatched = this.SetField(base.VideoLocal_IsWatched, value, ()=>VideoLocal_IsWatched, ()=>Watched);
            }
        }

        public new int VideoLocal_IsIgnored
        {
            get { return base.VideoLocal_IsIgnored; }
            set
            {
                base.VideoLocal_IsIgnored = this.SetField(base.VideoLocal_IsIgnored, value, () => VideoLocal_IsIgnored, () => Ignored);
            }
        }

      


        public new int VideoLocal_IsVariation
        {
            get { return base.VideoLocal_IsVariation; }
            set
            {
                base.VideoLocal_IsVariation = this.SetField(base.VideoLocal_IsVariation,value, ()=>VideoLocal_IsVariation, ()=>Variation);
            }
        }

        public bool Watched => VideoLocal_IsWatched==1;


        private bool showMoreDetails;
        public bool ShowMoreDetails
        {
            get { return showMoreDetails; }
            set
            {
                showMoreDetails = this.SetField(showMoreDetails, value);
            }
        }

        public new DateTime? VideoLocal_WatchedDate
        {
            get
            {
                return base.VideoLocal_WatchedDate;
            }
            set { base.VideoLocal_WatchedDate = this.SetField(base.VideoLocal_WatchedDate, value); }
        }


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
            LanguagesAudio = contract.LanguagesAudio;
            LanguagesSubtitle = contract.LanguagesSubtitle;
            Media = contract.Media;
            ReleaseGroup = contract.ReleaseGroup;

        }

        public List<IListWrapper> GetDirectChildren()
        {
            return new List<IListWrapper>();
        }

        public int ObjectType { get; } = 5;
        public bool IsEditable { get; } = false;
    }
}

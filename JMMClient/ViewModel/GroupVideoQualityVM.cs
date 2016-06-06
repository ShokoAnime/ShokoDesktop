using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class GroupVideoQualityVM
	{
		public string GroupName { get; set; }
		public string GroupNameShort { get; set; }
		public int Ranking { get; set; }
		public string Resolution { get; set; }
		public string VideoSource { get; set; }
		public int VideoBitDepth { get; set; }
		public int FileCountNormal { get; set; }
		public bool NormalComplete { get; set; }
		public int FileCountSpecials { get; set; }
		public bool SpecialsComplete { get; set; }
        public double TotalFileSize { get; set; }
        public long TotalRunningTime { get; set; }

		public List<int> NormalEpisodeNumbers { get; set; }
		public string NormalEpisodeNumberSummary { get; set; }

		public bool HasAnySpecials
		{
			get
			{
				return FileCountSpecials > 0;
			}
		}

        public string TotalFileSizeFormatted
        {
            get
            {
                return Utils.FormatFileSize(TotalFileSize);
            }
        }

        public string AverageFileSizeFormatted
        {
            get
            {
                if (TotalRunningTime <= 0) return "N/A";

                double avgBitRate = TotalFileSize / TotalRunningTime;
                return Utils.FormatBitRate(avgBitRate);
            }
        }

		public GroupVideoQualityVM(JMMServerBinary.Contract_GroupVideoQuality contract)
		{
			this.GroupName = contract.GroupName;
			this.GroupNameShort = contract.GroupNameShort;
			this.Ranking = contract.Ranking;
			this.Resolution = contract.Resolution;
			this.VideoSource = contract.VideoSource;
			this.FileCountNormal = contract.FileCountNormal;
			this.FileCountSpecials = contract.FileCountSpecials;
			this.NormalComplete = contract.NormalComplete;
			this.SpecialsComplete = contract.SpecialsComplete;
            this.TotalFileSize = contract.TotalFileSize;
            this.TotalRunningTime = contract.TotalRunningTime;

			this.NormalEpisodeNumbers = contract.NormalEpisodeNumbers;
			this.NormalEpisodeNumberSummary = contract.NormalEpisodeNumberSummary;

			this.VideoBitDepth = contract.VideoBitDepth;
		}

		public string PrettyDescription
		{
			get
			{
				return this.ToString();
			}
		}

		public override string ToString()
		{
			return string.Format("{0} - {1}/{2} - {3}/{4} Files", GroupNameShort, Resolution, VideoSource, FileCountNormal, FileCountSpecials);
		}

		public bool IsBluRay
		{
			get
			{
				return VideoSource.ToUpper().Contains("BLU");
			}
		}

		public bool IsDVD
		{
			get
			{
				return VideoSource.ToUpper().Contains("DVD");
			}
		}

		public bool IsHD
		{
			get
			{
				return (GetVideoWidth() >= 1280 && GetVideoWidth() < 1920);
			}
		}

		public bool IsFullHD
		{
			get
			{
				return (GetVideoWidth() >= 1920);
			}
		}

        public bool IsHi08P
        {
            get
            {
                return VideoBitDepth == 8;
            }
        }

        public bool IsHi10P
		{
			get
			{
				return VideoBitDepth == 10;
			}
		}

        public bool IsHi12P
        {
            get
            {
                return VideoBitDepth == 12;
            }
        }

        private int GetVideoWidth()
		{
			int videoWidth = 0;
			if (Resolution.Trim().Length > 0)
			{
				string[] dimensions = Resolution.Split('x');
				if (dimensions.Length > 0) int.TryParse(dimensions[0], out videoWidth);
			}
			return videoWidth;
		}

		private int GetVideoHeight()
		{
			int videoHeight = 0;
			if (Resolution.Trim().Length > 0)
			{
				string[] dimensions = Resolution.Split('x');
				if (dimensions.Length > 1) int.TryParse(dimensions[1], out videoHeight);
			}
			return videoHeight;
		}
	}
}

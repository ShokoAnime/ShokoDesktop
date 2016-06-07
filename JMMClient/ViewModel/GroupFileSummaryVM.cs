using System.Collections.Generic;

namespace JMMClient.ViewModel
{
    public class GroupFileSummaryVM
    {
        public string GroupName { get; set; }
        public string GroupNameShort { get; set; }
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

        public GroupFileSummaryVM(JMMServerBinary.Contract_GroupFileSummary contract)
        {
            this.GroupName = contract.GroupName;
            this.GroupNameShort = contract.GroupNameShort;
            this.FileCountNormal = contract.FileCountNormal;
            this.FileCountSpecials = contract.FileCountSpecials;
            this.NormalComplete = contract.NormalComplete;
            this.SpecialsComplete = contract.SpecialsComplete;
            this.TotalFileSize = contract.TotalFileSize;
            this.TotalRunningTime = contract.TotalRunningTime;

            this.NormalEpisodeNumbers = contract.NormalEpisodeNumbers;
            this.NormalEpisodeNumberSummary = contract.NormalEpisodeNumberSummary;
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
            return string.Format("{0} - {1}/{2} Files", GroupNameShort, FileCountNormal, FileCountSpecials);
        }
    }
}

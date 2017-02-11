using Shoko.Commons.Extensions;
using Shoko.Models.Client;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_DuplicateFile : CL_DuplicateFile
    {
        public string LocalFilePath1 => this.GetLocalFilePath1();
        public string LocalFilePath2 => this.GetLocalFilePath2();

        public string LocalFileName1 => this.GetLocalFileName1();

        public string LocalFileName2 => this.GetLocalFileName2();

        public string LocalFileDirectory1 => this.GetLocalFileDirectory1();
        public string LocalFileDirectory2 => this.GetLocalFileDirectory2();

        public string EpisodeNumberAndName => this.GetEpisodeNumberAndName();
    }
}

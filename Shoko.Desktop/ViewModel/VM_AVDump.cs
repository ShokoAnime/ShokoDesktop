using System.ComponentModel;
using System.IO;
using Shoko.Commons.Extensions;
using Shoko.Commons.Utils;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Desktop.ViewModel.Server;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel
{
    public class VM_AVDump : INotifyPropertyChangedExt
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }


        private string fullPath = "";
        public string FullPath
        {
            get { return fullPath; }
            set
            {
                fullPath = this.SetField(fullPath,value);
            }
        }

        private long fileSize;
        public long FileSize
        {
            get { return fileSize; }
            set
            {
                fileSize = this.SetField(fileSize, value);
            }
        }

        private string eD2KDump = "";
        public string ED2KDump
        {
            get { return eD2KDump; }
            set
            {
                eD2KDump = this.SetField(ED2KDump, value);
            }
        }

        private string aVDumpFullResult = "";
        public string AVDumpFullResult
        {
            get { return aVDumpFullResult; }
            set
            {
                aVDumpFullResult = this.SetField(aVDumpFullResult, value);
            }
        }

        private bool hasBeenDumped;
        public bool HasBeenDumped
        {
            get { return hasBeenDumped; }
            set
            {
                hasBeenDumped = this.SetField(hasBeenDumped, value);
            }
        }

        private bool isBeingDumped;
        public bool IsBeingDumped
        {
            get { return isBeingDumped; }
            set
            {
                isBeingDumped = this.SetField(isBeingDumped, value);
            }
        }

        private string dumpStatus = "";
        public string DumpStatus
        {
            get { return dumpStatus; }
            set
            {
                dumpStatus = this.SetField(dumpStatus, value);
            }
        }

        private VM_VideoLocal videoLocal;
        public VM_VideoLocal VideoLocal
        {
            get { return videoLocal; }
            set
            {
                videoLocal = this.SetField(videoLocal, value);
            }
        }


        public string FileName => Path.GetFileName(FullPath);

        public bool FileIsAvailable => File.Exists(FullPath);

        public bool FileIsNotAvailable => !File.Exists(FullPath);

        public string FormattedFileSize => Formatting.FormatFileSize(FileSize);

        public VM_AVDump()
        {
        }

        /*public AVDumpVM(VideoDetailedVM vid)
		{
			this.FullPath = vid.FullPath;
			this.FileSize = vid.VideoLocal_FileSize;
			this.ED2KDump = "";
			this.AVDumpFullResult = "";
			this.HasBeenDumped = false;
			this.IsBeingDumped = false;
			this.DumpStatus = "To be processed";
		}*/

        public VM_AVDump(VM_VideoLocal vid)
        {
            FullPath = vid.GetLocalFileSystemFullPath();
            FileSize = vid.FileSize;
            ED2KDump = "";
            AVDumpFullResult = "";HasBeenDumped = false;
            IsBeingDumped = false;
            DumpStatus = "To be processed";
            VideoLocal = vid;
        }
    }
}

using System;
using System.ComponentModel;
using System.IO;

namespace JMMClient.ViewModel
{
    public class AVDumpVM : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                var args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }


        private string fullPath = "";
        public string FullPath
        {
            get { return fullPath; }
            set
            {
                fullPath = value;
                NotifyPropertyChanged("FullPath");
            }
        }

        private long fileSize = 0;
        public long FileSize
        {
            get { return fileSize; }
            set
            {
                fileSize = value;
                NotifyPropertyChanged("FileSize");
            }
        }

        private string eD2KDump = "";
        public string ED2KDump
        {
            get { return eD2KDump; }
            set
            {
                eD2KDump = value;
                NotifyPropertyChanged("ED2KDump");
            }
        }

        private string aVDumpFullResult = "";
        public string AVDumpFullResult
        {
            get { return aVDumpFullResult; }
            set
            {
                aVDumpFullResult = value;
                NotifyPropertyChanged("AVDumpFullResult");
            }
        }

        private bool hasBeenDumped = false;
        public bool HasBeenDumped
        {
            get { return hasBeenDumped; }
            set
            {
                hasBeenDumped = value;
                NotifyPropertyChanged("HasBeenDumped");
            }
        }

        private bool isBeingDumped = false;
        public bool IsBeingDumped
        {
            get { return isBeingDumped; }
            set
            {
                isBeingDumped = value;
                NotifyPropertyChanged("IsBeingDumped");
            }
        }

        private string dumpStatus = "";
        public string DumpStatus
        {
            get { return dumpStatus; }
            set
            {
                dumpStatus = value;
                NotifyPropertyChanged("DumpStatus");
            }
        }

        private VideoLocalVM videoLocal = null;
        public VideoLocalVM VideoLocal
        {
            get { return videoLocal; }
            set
            {
                videoLocal = value;
                NotifyPropertyChanged("VideoLocal");
            }
        }


        public string FileName
        {
            get
            {
                return Path.GetFileName(FullPath);
            }
        }

        public bool FileIsAvailable
        {
            get
            {
                return File.Exists(FullPath);
            }
        }

        public bool FileIsNotAvailable
        {
            get
            {
                return !File.Exists(FullPath);
            }
        }

        public string FormattedFileSize
        {
            get
            {
                return Utils.FormatFileSize(FileSize);
            }
        }

        public AVDumpVM()
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

        public AVDumpVM(VideoLocalVM vid)
        {
            this.FullPath = vid.FullPath;
            this.FileSize = vid.FileSize;
            this.ED2KDump = "";
            this.AVDumpFullResult = "";
            this.HasBeenDumped = false;
            this.IsBeingDumped = false;
            this.DumpStatus = "To be processed";
            this.VideoLocal = vid;
        }
    }
}

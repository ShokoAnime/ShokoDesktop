using JMMClient.ViewModel;
using System.Collections.Generic;

namespace JMMClient.UserControls
{
    public class MultipleVideos
    {
        public int SelectedCount { get; set; }
        public List<int> VideoLocalIDs { get; set; }
        public List<VideoLocalVM> VideoLocals { get; set; }

        public bool SomeAreLocal
        {
            get
            {
                foreach(VideoLocalVM vidLocal in VideoLocals)
                { 
                    if (!string.IsNullOrEmpty(vidLocal.LocalFileSystemFullPath))                        
                        return true;
                }
                return false;
            }
        }

        public bool SomeHaveHashes
        {
            get
            {
                foreach (VideoLocalVM vidLocal in VideoLocals)
                {
                    if (!string.IsNullOrEmpty(vidLocal.Hash))
                        return true;
                }
                return false;
            }
        }
        public bool AllHaveHashes
        {
            get
            {
                foreach (VideoLocalVM vidLocal in VideoLocals)
                {
                    if (string.IsNullOrEmpty(vidLocal.Hash))
                        return false;
                }
                return true;
            }
        }
    }
}

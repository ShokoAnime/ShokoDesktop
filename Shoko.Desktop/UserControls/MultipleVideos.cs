using System.Collections.Generic;
using System.Linq;
using Shoko.Commons.Extensions;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Enums;

namespace Shoko.Desktop.UserControls
{
    public class MultipleVideos
    {
        public int SelectedCount { get; set; }
        public List<int> VideoLocalIDs { get; set; }
        public List<VM_VideoLocal> VideoLocals { get; set; }

        public bool SomeAreLocal
        {
            get
            {
                foreach(VM_VideoLocal vidLocal in VideoLocals)
                {
                    if (!string.IsNullOrEmpty(Commons.Extensions.Models.GetLocalFileSystemFullPath(vidLocal)))
                        return true;
                }
                return false;
            }
        }

        public bool SomeHaveHashes
        {
            get
            {
                foreach (VM_VideoLocal vidLocal in VideoLocals)
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
                foreach (VM_VideoLocal vidLocal in VideoLocals)
                {
                    if (string.IsNullOrEmpty(vidLocal.Hash))
                        return false;
                }
                return true;
            }
        }
    }
}

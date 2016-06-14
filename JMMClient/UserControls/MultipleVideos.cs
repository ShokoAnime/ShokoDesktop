using JMMClient.ViewModel;
using System.Collections.Generic;

namespace JMMClient.UserControls
{
    public class MultipleVideos
    {
        public int SelectedCount { get; set; }
        public List<int> VideoLocalIDs { get; set; }
        public List<VideoLocalVM> VideoLocals { get; set; }

    }
}

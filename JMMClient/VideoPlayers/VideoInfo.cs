using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JMMClient.ViewModel;

namespace JMMClient.VideoPlayers
{
    public class VideoInfo
    {
        public long Duration { get; set; }
        public long ResumePosition { get; set; }
        public int VideoLocalId { get; set; }
        public string Uri { get; set; }
        public bool IsUrl { get; set; }
        public bool IsPlaylist { get; set; }
        public List<string> SubtitlePaths { get; set; }
        public bool WasWatched { get; set; }
        public VideoDetailedVM VideoDetailed { get; set; }
        public VideoLocalVM VideoLocal { get; set; }
        public int ChangePositionTimeout { get; set; } = 3000; //3 seconds

        private System.Threading.Timer _changePositionTimer=null;

        public void ChangePosition(long position)
        {
            if ((ResumePosition == position) || (position==0))
                return;
            ResumePosition = position;
            if (_changePositionTimer == null)
                _changePositionTimer = new Timer(Change, null, ChangePositionTimeout, Timeout.Infinite);
            else
                _changePositionTimer.Change(ChangePositionTimeout, Timeout.Infinite);
        }

        private void Change(object o)
        {
            if (VideoDetailed != null)
                VideoDetailed.VideoLocal_ResumePosition = ResumePosition;
            if (VideoLocal != null)
                VideoLocal.ResumePosition = ResumePosition;
            JMMServerVM.Instance.clientBinaryHTTP.SetResumePosition(VideoLocalId,JMMServerVM.Instance.CurrentUser.JMMUserID.Value,ResumePosition);
        }
    }
}

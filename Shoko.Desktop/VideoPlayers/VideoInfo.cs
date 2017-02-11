using System.Collections.Generic;
using System.Threading;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.VideoPlayers
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
        public VM_VideoDetailed VideoDetailed { get; set; }
        public VM_VideoLocal VideoLocal { get; set; }
        public int ChangePositionTimeout { get; set; } = 3000; //3 seconds

        private Timer _changePositionTimer=null;

        public void ChangePosition(long position)
        {
            if ((ResumePosition == position) || (position==0))
                return;
            ResumePosition = position;
            if (VideoDetailed != null)
                VideoDetailed.VideoLocal_ResumePosition = ResumePosition;
            if (VideoLocal != null)
                VideoLocal.ResumePosition = ResumePosition;
            if (_changePositionTimer == null)
                _changePositionTimer = new Timer(Change, null, ChangePositionTimeout, Timeout.Infinite);
            else
                _changePositionTimer.Change(ChangePositionTimeout, Timeout.Infinite);
        }

        private void Change(object o)
        {
            VM_ShokoServer.Instance.ShokoServices.SetResumePosition(VideoLocalId,ResumePosition, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
        }
        public void ForceChange(long position)
        {
            if (_changePositionTimer != null)
            {
                _changePositionTimer.Dispose();
                _changePositionTimer = null;
            }
            VM_ShokoServer.Instance.ShokoServices.SetResumePosition(VideoLocalId, position, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
        }
    }
}

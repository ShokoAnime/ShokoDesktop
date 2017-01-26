using Shoko.Commons.Extensions;
using Shoko.Models.Server;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AniDB_Episode : AniDB_Episode
    {
        public string EpisodeName => this.GetEpisodeName();
    }
}

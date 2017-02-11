using Shoko.Commons.Extensions;
using Shoko.Models.TvDB;

namespace Shoko.Desktop.ViewModel.Server
{
    // ReSharper disable once InconsistentNaming
    public class VM_TvDB_Language : TvDB_Language
    {
        public string LanguageFlagImage => this.GetLanguageFlagImage();
    }
}

using Shoko.Commons.Extensions;
using Shoko.Models.Client;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AnimeTitle : CL_AnimeTitle
    {
        public string FlagImage => this.GetFlagImage();
        public string LanguageDescription => this.GetLanguageDescription();
    }
}

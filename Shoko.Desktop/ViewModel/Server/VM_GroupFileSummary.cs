using Shoko.Commons.Extensions;
using Shoko.Models.Client;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_GroupFileSummary : CL_GroupFileSummary
    {

        public bool HasAnySpecials => this.HasAnySpecials();

        public string TotalFileSizeFormatted => this.GetTotalFileSizeFormatted();

        public string AverageFileSizeFormatted => this.GetAverageFileSizeFormatted();
        public string PrettyDescription => this.GetPrettyDescription();
    }
}

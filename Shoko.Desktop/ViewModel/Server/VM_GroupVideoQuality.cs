using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shoko.Commons.Extensions;
using Shoko.Models.Client;

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_GroupVideoQuality : CL_GroupVideoQuality
    {
        public bool HasAnySpecials => this.GetHasAnySpecials();
        public string TotalFileSizeFormatted => this.GetTotalFileSizeFormatted();
        public string AverageFileSizeFormatted => this.GetAverageFileSizeFormatted();
        public string PrettyDescription => this.GetPrettyDescription();
        public bool IsBluRay => this.IsBluRay();
        public bool IsDVD => this.IsDVD();
        public bool IsHD => this.IsHD();
        public bool IsFullHD => this.IsFullHD();
        public bool IsHi08P => this.IsHi08P();
        public bool IsHi10P => this.IsHi10P();
        public bool IsHi12P => this.IsHi12P();
        public bool IsDualAudio => this.IsDualAudio();
        public bool IsMultiAudio => this.IsMultiAudio();
    }
}

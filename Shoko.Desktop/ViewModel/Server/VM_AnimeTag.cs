using Shoko.Models.Client;

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_AnimeTag : CL_AnimeTag
    {
        public new string TagName
        {
            get => base.TagName == null ? null : string.Intern(base.TagName);
            set => base.TagName = value == null ? null : string.Intern(value);
        }

        public new string TagDescription
        {
            get => base.TagDescription == null ? null : string.Intern(base.TagDescription);
            set => base.TagDescription = value == null ? null : string.Intern(value);
        }
    }
}
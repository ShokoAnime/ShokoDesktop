using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
    public class CustomTagVM : IComparable<CustomTagVM>
    {
        public int CustomTagID { get; set; }
        public string TagName { get; set; }
        public string TagDescription { get; set; }

		public CustomTagVM()
		{
		}

        public CustomTagVM(JMMServerBinary.Contract_CustomTag contract)
		{
            this.CustomTagID = contract.CustomTagID.Value;
            this.TagName = contract.TagName;
			this.TagDescription = contract.TagDescription;
		}

		public override string ToString()
		{
			return string.Format("{0} - {1}", TagName, TagDescription);
		}

        public int CompareTo(CustomTagVM obj)
        {
            return this.TagName.CompareTo(obj.TagName);
        }
    }
}

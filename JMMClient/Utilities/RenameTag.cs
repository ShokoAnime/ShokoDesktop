using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient
{
	public class RenameTag
	{
		public string Name { get; set; }
		public string Tag { get; set; }

		public RenameTag()
		{
		}

		public RenameTag(string name, string tag)
		{
			Name = name;
			Tag = tag;
		}
	}
}

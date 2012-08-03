using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient
{
	public class RenameTest
	{
		public string Name { get; set; }
		public string Test { get; set; }

		public RenameTest()
		{
		}

		public RenameTest(string name, string test)
		{
			Name = name;
			Test = test;
		}
	}
}

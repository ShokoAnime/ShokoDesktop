using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class TvDB_LanguageVM
	{
		public string Name { get; set; }
		public string Abbreviation { get; set; }

		public string LanguageFlagImage
		{
			get
			{
				return Languages.GetFlagImage(Abbreviation.Trim().ToUpper());
			}
		}

		public TvDB_LanguageVM(JMMServerBinary.Contract_TvDBLanguage contract)
		{
			this.Name = contract.Name;
			this.Abbreviation = contract.Abbreviation;
		}
	}
}

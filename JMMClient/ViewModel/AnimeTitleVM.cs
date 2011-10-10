using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class AnimeTitleVM
	{
		public int AnimeID { get; set; }
		public string TitleType { get; set; }
		public string Language { get; set; }
		public string Title { get; set; }

		


		public string FlagImage
		{
			get
			{
				return Languages.GetFlagImage(Language.Trim().ToUpper());
			}
		}

		public string LanguageDescription
		{
			get
			{
				return Languages.GetLanguageDescription(Language.Trim().ToUpper());

			}
		}

		public AnimeTitleVM()
		{
		}

		public AnimeTitleVM(JMMServerBinary.Contract_AnimeTitle contract)
		{
			this.AnimeID = contract.AnimeID;
			this.TitleType = contract.TitleType;
			this.Language = contract.Language;
			this.Title = contract.Title;
		}

		public override string ToString()
		{
			return string.Format("{0} - {1} ({2}) - {3}", AnimeID, TitleType, Language, Title);
		}
	}
}

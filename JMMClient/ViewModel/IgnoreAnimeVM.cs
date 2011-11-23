using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class IgnoreAnimeVM
	{
		public int IgnoreAnimeID { get; set; }
		public int JMMUserID { get; set; }
		public int AnimeID { get; set; }
		public int IgnoreType { get; set; }

		public AniDB_AnimeVM Anime { get; set; }

		public IgnoreAnimeVM(JMMServerBinary.Contract_IgnoreAnime contract)
		{
			this.IgnoreAnimeID = contract.IgnoreAnimeID;
			this.JMMUserID = contract.JMMUserID;
			this.AnimeID = contract.AnimeID;
			this.IgnoreType = contract.IgnoreType;

			if (contract.Anime != null)
				Anime = new AniDB_AnimeVM(contract.Anime);
		}

		public string IgnoreTypeAsString
		{
			get
			{

				switch ((IgnoreAnimeType)IgnoreType)
				{
					case IgnoreAnimeType.RecDownload: return "Recommendations - Download";
					case IgnoreAnimeType.RecWatch: return "Recommendations - Watch";
				}

				return "Recommendations - Download";
			}
		
		}

		public string DisplayName
		{
			get
			{
				if (Anime != null)
					return Anime.FormattedTitle;
				else
					return string.Format("Anime ID: {0}", AnimeID);
			}
		}

		public override string ToString()
		{
			return string.Format("{0} - {1} - {2} - {3}", IgnoreAnimeID, JMMUserID, AnimeID, IgnoreType);
		}
	}
}

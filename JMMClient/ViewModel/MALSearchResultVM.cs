using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace JMMClient.ViewModel
{
	public class MALSearchResultVM
	{
		public int id { get; set; }
		public string title { get; set; }
		public string english { get; set; }
		public string synonyms { get; set; }
		public int episodes { get; set; }
		public decimal score { get; set; }
		public string animeType { get; set; }
		public string status { get; set; }
		public string start_date { get; set; }
		public string end_date { get; set; }
		public string synopsis { get; set; }
		public string image { get; set; }

		public string SiteURL
		{
			get
			{
				return string.Format(Constants.URLS.MAL_Series, id);
			}
		}

		public string Overview
		{
			get
			{
				return Utils.ReparseDescription(synopsis);
			}
		}

		public string PosterPath
		{
			get
			{
				if (string.IsNullOrEmpty(image))
					return string.Format("pack://application:,,,/{0};component/Images/blankposter.png", Constants.AssemblyName);

				return image;
			}
		}

		public MALSearchResultVM()
		{
		}

		public MALSearchResultVM(JMMServerBinary.Contract_MALAnimeResponse contract)
		{
			this.id = contract.id;
			this.title = contract.title;
			this.english = contract.english;
			this.synonyms = contract.synonyms;
			this.episodes = contract.episodes;
			this.score = contract.score;
			this.animeType = contract.animeType;
			this.status = contract.status;
			this.start_date = contract.start_date;
			this.end_date = contract.end_date;
			this.synopsis = contract.synopsis;
			this.image = contract.image;
			
		}

		public override string ToString()
		{
			return string.Format("{0} --- {1} ({2})", id, title, animeType);
		}
	}
}

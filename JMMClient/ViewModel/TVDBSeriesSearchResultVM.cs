using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class TVDBSeriesSearchResultVM
	{
		public string Id { get; set; }
		public int SeriesID { get; set; }
		public string Overview { get; set; }
		public string SeriesName { get; set; }
		public string Banner { get; set; }
		public string Language { get; set; }

		public string BannerURL
		{
			get
			{
				if (string.IsNullOrEmpty(Banner)) return "";

				// check if we have this banner locally

				// otherwise just use the URL
				return string.Format(Constants.URLS.TvDB_Images, Banner);
			}
		}

		public string SeriesURL
		{
			get
			{
				return string.Format(Constants.URLS.TvDB_Series, SeriesID);
			}
		}

		public string LanguageFlagImage
		{
			get
			{
				return Languages.GetFlagImage(Language.Trim().ToUpper());
			}
		}

		public TVDBSeriesSearchResultVM()
		{
		}

		public TVDBSeriesSearchResultVM(JMMServerBinary.Contract_TVDBSeriesSearchResult contract)
		{
			this.Id = contract.Id;
			this.SeriesID = contract.SeriesID;
			this.Overview = contract.Overview;
			this.SeriesName = contract.SeriesName;
			this.Banner = contract.Banner;
			this.Language = contract.Language;
		}

		public override string ToString()
		{
			return string.Format("{0} --- {1} ({2})", SeriesID, SeriesName, Language);
		}
	}
}

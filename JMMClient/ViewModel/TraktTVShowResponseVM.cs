using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
	public class TraktTVShowResponseVM
	{
		public string title { get; set; }
		public string year { get; set; }
		public string url { get; set; }
		public string first_aired { get; set; }
		public string country { get; set; }
		public string overview { get; set; }
		public string tvdb_id { get; set; }

		public string TraktID
		{
			get
			{
				if (string.IsNullOrEmpty(url)) return "";

				int pos = url.LastIndexOf("/");
				if (pos < 0) return "";

				string id = url.Substring(pos + 1, url.Length - pos - 1);
				return id;
			}
		}

		public TraktTVShowResponseVM()
		{
		}

		public TraktTVShowResponseVM(JMMServerBinary.Contract_TraktTVShowResponse contract)
		{
			this.title = contract.title;
			this.year = contract.year;
			this.url = contract.url;
			this.first_aired = contract.first_aired;
			this.country = contract.country;
			this.overview = contract.overview;
			this.tvdb_id = contract.tvdb_id;
		}

		public override string ToString()
		{
			return string.Format("{0} ({1}) - {2}", title, year, overview);
		}
	}
}

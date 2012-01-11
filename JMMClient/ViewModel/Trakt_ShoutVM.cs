using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace JMMClient.ViewModel
{
	public class Trakt_ShoutVM
	{
		public int ShoutType { get; set; } // episode, show
		public string Text { get; set; }
		public bool Spoiler { get; set; }
		public DateTime? Inserted { get; set; }

		public string Episode_Season { get; set; }
		public string Episode_Number { get; set; }
		public string Episode_Title { get; set; }
		public string Episode_Overview { get; set; }
		public string Episode_Url { get; set; }
		public string Episode_Screenshot { get; set; }

		public TraktTVShowResponseVM TraktShow { get; set; }
		public int? AnimeSeriesID { get; set; }
		public AniDB_AnimeVM Anime { get; set; }

		public bool HasAnimeSeries
		{
			get
			{
				return AnimeSeriesID.HasValue;
			}
		}

		public string EpisodeDescription
		{
			get
			{
				return string.Format("{0}x{1} - {2}", Episode_Season, Episode_Number, Episode_Title);
			}
		}

		public string ImagePathForDisplay
		{
			get
			{
				if (!string.IsNullOrEmpty(FullImagePath) && File.Exists(FullImagePath)) return FullImagePath;

				// use fanart instead
				if (Anime != null && !string.IsNullOrEmpty(Anime.FanartPathOnlyExisting))
					return Anime.FanartPathOnlyExisting;

				return @"/Images/EpisodeThumb_NotFound.png";
			}
		}

		public string OnlineImagePath
		{
			get
			{
				if (string.IsNullOrEmpty(Episode_Screenshot)) return "";
				return Episode_Screenshot;
			}
		}

		public string FullImagePath
		{
			get
			{
				// typical EpisodeImage url
				// http://vicmackey.trakt.tv/images/episodes/3228-1-1.jpg

				// get the TraktID from the URL
				// http://trakt.tv/show/11eyes/season/1/episode/1 (11 eyes)

				if (string.IsNullOrEmpty(Episode_Screenshot)) return "";

				// on Trakt, if the episode doesn't have a proper screenshot, they will return the
				// fanart instead, we will ignore this
				int pos = Episode_Screenshot.IndexOf(@"episodes/");
				if (pos <= 0) return "";

				string traktID = TraktShow.TraktID;
				traktID = traktID.Replace("/", @"\");

				string imageName = Episode_Screenshot.Substring(pos + 9, Episode_Screenshot.Length - pos - 9);
				imageName = imageName.Replace("/", @"\");

				string relativePath = Path.Combine("episodes", traktID);
				relativePath = Path.Combine(relativePath, imageName);

				return Path.Combine(Utils.GetTraktImagePath(), relativePath);
			}
		}

		public string ShowTitle
		{
			get
			{
				if (Anime != null)
					return Anime.FormattedTitle;

				return TraktShow.title;
			}
		}

		public string ShoutDateString
		{
			get
			{
				if (Inserted.HasValue)
					//return Inserted.Value.ToString("dd MMM yyyy", Globals.Culture);
					return Inserted.Value.ToString("dd MMM yyyy", Globals.Culture) + ", " + Inserted.Value.ToShortTimeString();
				else
					return "";
			}
		}

		public Trakt_ShoutVM(JMMServerBinary.Contract_Trakt_Shout contract)
		{
			this.ShoutType = contract.ShoutType;
			this.Text = contract.Text;
			this.Spoiler = contract.Spoiler;
			this.Inserted = contract.Inserted;

			this.Episode_Season = contract.Episode_Season;
			this.Episode_Number = contract.Episode_Number;
			this.Episode_Title = contract.Episode_Title;

			if (!string.IsNullOrEmpty(Episode_Title) && Episode_Title.Length > 30)
				Episode_Title = Episode_Title.Substring(0, 30) + "...";

			this.Episode_Overview = contract.Episode_Overview;
			this.Episode_Url = contract.Episode_Url;
			this.Episode_Screenshot = contract.Episode_Screenshot;
			this.AnimeSeriesID = contract.AnimeSeriesID;

			if (contract.TraktShow != null)
				this.TraktShow = new TraktTVShowResponseVM(contract.TraktShow);

			if (contract.Anime != null)
				this.Anime = new AniDB_AnimeVM(contract.Anime);

			Console.Write(this.FullImagePath);
			Console.Write(this.OnlineImagePath);
			Console.Write(this.ImagePathForDisplay);

			//this.Text = "Sora asks Haru to take her measurements so she can have a uniform made. The next day Nao comes over to bring Haruka some mosquito repellent, since Sora is scared of mosquitoes, and then leaves almost immediately. The next day Ryouhei invites Haruka to the school rooftop to take a peek at the girls cleaning the pool. Haruka warns Ryouhei, though, not to peek at Nao, and leaves. Much later Ryouhei tells Nao that Haruka, “the prince she has been waiting for,” has a crush on her; but she thinks he does not like her. The truth is that something happened between the two many summers ago: Nao, trying to escape the noise of her arguing parents, ran to Haruka's house. At that time Haruka was sleeping at their veranda, and was surprised to see Nao on top of him with her clothes undone. Until now Nao feels guilty for what she did. However, Ryouhei, even Akira and Kazuha, is into the act of bringing Haruka and Nao together. They did so by making the two meet at school pool on a Sunday. When they were changing after the swimming lesson, Haruka rushes into the girls' locker room when Nao was scared by a black cat inside a box. The school supervisor hears their chatter, but the two were able to hide inside a box before he catches them. Thinking it was just the cat, he leaves. There, Nao gets to know that Haruka does not hate her, only that he was just surprised at the events of that summer day, dispelling her assumptions. Meanwhile, Sora shows her hatred of Nao, whom she thinks is the reason why Haruka has been preoccupied the past couple of days, at home on her laptop.";
		}
	}
}

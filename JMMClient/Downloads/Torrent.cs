using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.Downloads
{
	public class Torrent
	{
		private const int StatusStarted = 1;
		private const int StatusChecking = 2;
		private const int StatusStartAfterCheck = 4;
		private const int StatusChecked = 8;
		private const int StatusError = 16;
		private const int StatusPaused = 32;
		private const int StatusQueued = 64;
		private const int StatusLoaded = 128;

		public string Hash { get; set; }
		public int Status { get; set; }
		public string Name { get; set; }

		/// <summary>
		/// in bytes
		/// </summary>
		public long Size { get; set; }

		/// <summary>
		/// integer in per mils
		/// </summary>
		public long PercentProgress { get; set; }

		public string PercentProgressFormatted
		{
			get
			{
				double pro = (double)PercentProgress / (double)10;
				return String.Format("{0:0.0}%", pro);
			}
		}

		/// <summary>
		/// integer in bytes
		/// </summary>
		public long Downloaded { get; set; }

		/// <summary>
		/// integer in bytes
		/// </summary>
		public long Uploaded { get; set; }


		/// <summary>
		/// integer in per mils
		/// </summary>
		public long Ratio { get; set; }

		/// <summary>
		/// integer in bytes per second
		/// </summary>
		public long UploadSpeed { get; set; }

		/// <summary>
		/// integer in bytes per second
		/// </summary>
		public long DownloadSpeed { get; set; }

		/// <summary>
		/// integer in seconds
		/// </summary>
		public long ETA { get; set; }
		public string Label { get; set; }
		public long PeersConnected { get; set; }
		public long PeersInSwarm { get; set; }
		public long SeedsConnected { get; set; }
		public long SeedsInSwarm { get; set; }

		/// <summary>
		/// integer in 1/65535ths
		/// </summary>
		public long Availability { get; set; }

		public long TorrentQueueOrder { get; set; }

		/// <summary>
		/// integer in bytes
		/// </summary>
		public long Remaining { get; set; }

		public Torrent()
		{
		}

		public Torrent(object[] row)
		{
			this.Hash = row[0].ToString();
			this.Status = int.Parse(row[1].ToString());
			this.Name = row[2].ToString();
			this.Size = long.Parse(row[3].ToString());
			this.PercentProgress = long.Parse(row[4].ToString());
			this.Downloaded = long.Parse(row[5].ToString());
			this.Uploaded = long.Parse(row[6].ToString());
			this.Ratio = long.Parse(row[7].ToString());
			this.UploadSpeed = long.Parse(row[8].ToString());
			this.DownloadSpeed = long.Parse(row[9].ToString());
			this.ETA = long.Parse(row[10].ToString());
			this.Label = row[11].ToString();
			this.PeersConnected = long.Parse(row[12].ToString());
			this.PeersInSwarm = long.Parse(row[13].ToString());
			this.SeedsConnected = long.Parse(row[14].ToString());
			this.SeedsInSwarm = long.Parse(row[15].ToString());
			this.Availability = long.Parse(row[16].ToString());
			this.TorrentQueueOrder = long.Parse(row[17].ToString());
			this.Remaining = long.Parse(row[18].ToString());
		}

		public override string ToString()
		{
			return string.Format("Torrent: {0} - {1} - {2}", Name, PercentProgressFormatted, Status);
		}

		public string SeedsFormatted
		{
			get
			{
				return String.Format("{0} ({1})", SeedsConnected, SeedsInSwarm);
			}
		}

		public string PeersFormatted
		{
			get
			{
				return String.Format("{0} ({1})", PeersConnected, PeersInSwarm);
			}
		}

		public string SizeFormatted
		{
			get
			{
				return Utils.FormatByteSize(Size);
			}
		}

		public string DownloadSpeedFormatted
		{
			get
			{
				return Utils.FormatByteSize((long)DownloadSpeed) + "/sec";
			}
		}

		public string UploadSpeedFormatted
		{
			get
			{
				return Utils.FormatByteSize((long)UploadSpeed) + "/sec";
			}
		}

		public string DownloadedFormatted
		{
			get
			{
				return Utils.FormatByteSize((long)Downloaded);
			}
		}

		public string UploadedFormatted
		{
			get
			{
				return Utils.FormatByteSize((long)Uploaded);
			}
		}

		public string RatioFormatted
		{
			get
			{
				double temp = (double)Ratio / (double)1000;
				return String.Format("{0:0.000}", temp);
			}
		}

		public bool IsRunning
		{
			get
			{
				if (Status == 137 || Status == 200 || Status == 201) return true;
				return false;
			}
		}

		public bool IsNotRunning
		{
			get
			{
				if (Status == 136) return true;
				return false;
			}
		}

		public bool IsPaused
		{
			get
			{
				int paused = Status & StatusPaused;
				if (paused > 0) return true;
				return false;
			}
		}

		public string StatusFormatted
		{
			get
			{
				if (Status == 201 && Remaining > 0) return "Downloading";
				if (Status == 201 && Remaining == 0) return "Seeding";
				if (Status == 137 && Remaining > 0) return "[F] Downloading";
				if (Status == 137 && Remaining == 0) return "[F] Seeding";

				if (Status == 200 && Remaining > 0) return "Queued";
				if (Status == 200 && Remaining == 0) return "Queued Seed";

				if (Status == 136 && Remaining == 0) return "Finished";
				if (Status == 136 && Remaining > 0) return "Stopped";

				int paused = Status & StatusPaused;
				if (paused > 0)
					return "Paused";

				return "";
			}
		}

		public string StatusImage
		{
			get
			{
				if (Status == 201 && Remaining > 0) return @"/Images/Torrents/tor_downloading.png";
				if (Status == 201 && Remaining == 0) return @"/Images/Torrents/tor_seeding.png";
				if (Status == 137 && Remaining > 0) return @"/Images/Torrents/tor_downloading.png";
				if (Status == 137 && Remaining == 0) return @"/Images/Torrents/tor_seeding.png";

				if (Status == 200 && Remaining > 0) return @"/Images/Torrents/tor_queued.png";
				if (Status == 200 && Remaining == 0) return @"/Images/Torrents/tor_queuedseed.png";

				if (Status == 136 && Remaining == 0) return @"/Images/Torrents/tor_finished.png";
				if (Status == 136 && Remaining > 0) return @"/Images/Torrents/tor_stopped.png";

				int paused = Status & StatusPaused;
				if (paused > 0) return @"/Images/Torrents/tor_paused.png";

				return @"/Images/16_key.png";
			}
		}

		public string ListDisplay
		{
			get
			{
				if (StatusFormatted.Length > 0)
					return string.Format("{0}: {1} - {2}", StatusFormatted.ToUpper(), Name, PercentProgressFormatted);
				else
					return string.Format("{0} - {1}", Name, PercentProgressFormatted);
			}
		}

	}
}

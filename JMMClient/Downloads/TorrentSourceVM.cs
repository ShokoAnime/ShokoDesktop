using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace JMMClient.Downloads
{
	public class TorrentSourceVM : INotifyPropertyChanged
	{
		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

		public TorrentSourceType TorrentSource { get; set; }

		private bool isEnabled = true;
		public bool IsEnabled
		{
			get { return isEnabled; }
			set
			{
				isEnabled = value;
				NotifyPropertyChanged("IsEnabled");
				IsDisabled = !value;
			}
		}

		private bool isDisabled = false;
		public bool IsDisabled
		{
			get { return isDisabled; }
			set
			{
				isDisabled = value;
				NotifyPropertyChanged("IsDisabled");
			}
		}

		public string TorrentSourceName 
		{
			get
			{
				return EnumTranslator.TorrentSourceTranslated(TorrentSource);
			}
		}

		public string TorrentSourceNameShort
		{
			get
			{
				return EnumTranslator.TorrentSourceTranslatedShort(TorrentSource);
			}
		}

		public TorrentSourceVM()
		{
		}

		public TorrentSourceVM(TorrentSourceType tsType, bool isEnabled)
		{
			TorrentSource = tsType;
			IsEnabled = isEnabled;
		}

		public void PopulateTorrentDownloadLink(ref TorrentLinkVM torLink)
		{
			if (TorrentSource == TorrentSourceType.BakaBT)
			{
				if (string.IsNullOrEmpty(torLink.TorrentDownloadLink))
				{
					TorrentsBakaBT bakbt = new TorrentsBakaBT();
					bakbt.PopulateTorrentLink(ref torLink);
				}
			}
			return;
		}

		public List<TorrentLinkVM> BrowseTorrents()
		{
			List<TorrentLinkVM> links = new List<TorrentLinkVM>();

			if (TorrentSource == TorrentSourceType.Nyaa)
			{
				TorrentsNyaa nyaa = new TorrentsNyaa();
				List<TorrentLinkVM> ttLinks = nyaa.BrowseTorrents();
				links.AddRange(ttLinks);
			}

            if (TorrentSource == TorrentSourceType.Sukebei)
            {
                TorrentsSukebei sukebei = new TorrentsSukebei();
                List<TorrentLinkVM> ttLinks = sukebei.BrowseTorrents();
                links.AddRange(ttLinks);
            }

            if (TorrentSource == TorrentSourceType.TokyoToshokanAnime)
			{
				TorrentsTokyoToshokan tt = new TorrentsTokyoToshokan(TorrentSourceType.TokyoToshokanAnime);
				List<TorrentLinkVM> ttLinks = tt.BrowseTorrents();
				links.AddRange(ttLinks);
			}

			if (TorrentSource == TorrentSourceType.TokyoToshokanAll)
			{
				TorrentsTokyoToshokan tt = new TorrentsTokyoToshokan(TorrentSourceType.TokyoToshokanAll);
				List<TorrentLinkVM> ttLinks = tt.BrowseTorrents();
				links.AddRange(ttLinks);
			}

			if (TorrentSource == TorrentSourceType.AnimeSuki)
			{
				TorrentsAnimeSuki suki = new TorrentsAnimeSuki();
				List<TorrentLinkVM> sukiLinks = suki.BrowseTorrents();
				links.AddRange(sukiLinks);
			}



			if (TorrentSource == TorrentSourceType.BakaBT)
			{
				TorrentsBakaBT bakbt = new TorrentsBakaBT();
				List<TorrentLinkVM> bakauLinks = bakbt.BrowseTorrents();
				links.AddRange(bakauLinks);
			}

			if (TorrentSource == TorrentSourceType.AnimeBytes)
			{
				TorrentsAnimeBytes abytes = new TorrentsAnimeBytes();
				List<TorrentLinkVM> abytesLinks = abytes.BrowseTorrents();
				links.AddRange(abytesLinks);
			}

			foreach (TorrentLinkVM torLink in links)
				NLog.LogManager.GetCurrentClassLogger().Trace(torLink.ToStringMatch());

			return links;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using JMMClient.ImageDownload;

namespace JMMClient.ViewModel
{
	public class Trakt_ImagePosterVM : INotifyPropertyChanged
	{
		public int Trakt_ImagePosterID { get; set; }
		public int Trakt_ShowID { get; set; }
		public int Season { get; set; }
		public string ImageURL { get; set; }
		public int Enabled { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

		public string FullImagePathPlain
		{
			get
			{
				// typical url
				// http://vicmackey.trakt.tv/images/seasons/3228-1.jpg
				// http://vicmackey.trakt.tv/images/posters/1130.jpg

				if (string.IsNullOrEmpty(ImageURL)) return "";

				int pos = ImageURL.IndexOf(@"images/");
				if (pos <= 0) return "";

				string relativePath = ImageURL.Substring(pos + 7, ImageURL.Length - pos - 7);
				relativePath = relativePath.Replace("/", @"\");

				string filename = Path.Combine(Utils.GetTraktImagePath(), relativePath);

				return filename;
			}
		}

		public string FullImagePath
		{
			get
			{
				if (!File.Exists(FullImagePathPlain))
				{
					ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.Trakt_Poster, this, false);
					MainWindow.imageHelper.DownloadImage(req);
					if (File.Exists(FullImagePathPlain)) return FullImagePathPlain;
				}

				return FullImagePathPlain;
			}
		}

		public string FullThumbnailPath
		{
			get
			{
				return FullImagePath;
			}
		}

		private bool isImageEnabled = false;
		public bool IsImageEnabled
		{
			get { return isImageEnabled; }
			set
			{
				isImageEnabled = value;
				NotifyPropertyChanged("IsImageEnabled");
			}
		}

		private bool isImageDisabled = false;
		public bool IsImageDisabled
		{
			get { return isImageDisabled; }
			set
			{
				isImageDisabled = value;
				NotifyPropertyChanged("IsImageDisabled");
			}
		}

		private bool isImageDefault = false;
		public bool IsImageDefault
		{
			get { return isImageDefault; }
			set
			{
				isImageDefault = value;
				NotifyPropertyChanged("IsImageDefault");
			}
		}

		private bool isImageNotDefault = false;
		public bool IsImageNotDefault
		{
			get { return isImageNotDefault; }
			set
			{
				isImageNotDefault = value;
				NotifyPropertyChanged("IsImageNotDefault");
			}
		}

		public Trakt_ImagePosterVM(JMMServerBinary.Contract_Trakt_ImagePoster contract)
		{
			this.Trakt_ImagePosterID = contract.Trakt_ImagePosterID;
			this.Trakt_ShowID = contract.Trakt_ShowID;
			this.Season = contract.Season;
			this.ImageURL = contract.ImageURL;
			this.Enabled = contract.Enabled;

			IsImageEnabled = Enabled == 1;
			IsImageDisabled = Enabled != 1;
		}
	}
}

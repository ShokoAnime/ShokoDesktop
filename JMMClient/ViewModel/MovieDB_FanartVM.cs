using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using JMMClient.ImageDownload;

namespace JMMClient.ViewModel
{
	public class MovieDB_FanartVM : INotifyPropertyChanged
	{
		public int MovieDB_FanartID { get; set; }
		public string ImageID { get; set; }
		public int MovieId { get; set; }
		public string ImageType { get; set; }
		public string ImageSize { get; set; }
		public string URL { get; set; }
		public int ImageWidth { get; set; }
		public int ImageHeight { get; set; }
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
				//strip out the base URL
				int pos = URL.IndexOf('/', 10);
				string fname = URL.Substring(pos + 1, URL.Length - pos - 1);
				fname = fname.Replace("/", @"\");
				string filename = Path.Combine(Utils.GetMovieDBImagePath(), fname);

				return filename;
			}
		}

		public string FullImagePath
		{
			get
			{
				if (!File.Exists(FullImagePathPlain))
				{
					ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.MovieDB_FanArt, this, false);
					MainWindow.imageHelper.DownloadImage(req);
					if (File.Exists(FullImagePathPlain)) return FullImagePathPlain;
				}

				return FullImagePathPlain;
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

		public MovieDB_FanartVM(JMMServerBinary.Contract_MovieDB_Fanart contract)
		{
			this.MovieDB_FanartID = contract.MovieDB_FanartID;
			this.ImageID = contract.ImageID;
			this.MovieId = contract.MovieId;
			this.ImageType = contract.ImageType;
			this.ImageSize = contract.ImageSize;
			this.URL = contract.URL;
			this.ImageWidth = contract.ImageWidth;
			this.ImageHeight = contract.ImageHeight;
			this.Enabled = contract.Enabled;

			IsImageEnabled = Enabled == 1;
			IsImageDisabled = Enabled != 1;
		}
	}
}

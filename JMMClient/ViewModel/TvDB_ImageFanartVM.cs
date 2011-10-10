using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

namespace JMMClient.ViewModel
{
	public class TvDB_ImageFanartVM : INotifyPropertyChanged
	{
		public int TvDB_ImageFanartID { get; set; }
		public int Id { get; set; }
		public int SeriesID { get; set; }
		public string BannerPath { get; set; }
		public string BannerType { get; set; }
		public string BannerType2 { get; set; }
		public string Colors { get; set; }
		public string Language { get; set; }
		public string ThumbnailPath { get; set; }
		public string VignettePath { get; set; }
		public int Enabled { get; set; }
		public int Chosen { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

		public string FullImagePath
		{
			get
			{
				string fname = BannerPath;
				fname = BannerPath.Replace("/", @"\");
				return Path.Combine(Utils.GetTvDBImagePath(), fname);
			}
		}

		public string FullThumbnailPath
		{
			get
			{
				string fname = ThumbnailPath;
				fname = ThumbnailPath.Replace("/", @"\");
				return Path.Combine(Utils.GetTvDBImagePath(), fname);
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

		public TvDB_ImageFanartVM(JMMServerBinary.Contract_TvDB_ImageFanart contract)
		{
			this.TvDB_ImageFanartID = contract.TvDB_ImageFanartID;
			this.Id = contract.Id;
			this.SeriesID = contract.SeriesID;
			this.BannerPath = contract.BannerPath;
			this.BannerType = contract.BannerType;
			this.BannerType2 = contract.BannerType2;
			this.Colors = contract.Colors;
			this.Language = contract.Language;
			this.ThumbnailPath = contract.ThumbnailPath;
			this.VignettePath = contract.VignettePath;
			this.Enabled = contract.Enabled;
			this.Chosen = contract.Chosen;

			IsImageEnabled = Enabled == 1;
			IsImageDisabled = Enabled != 1;
		}
	}
}

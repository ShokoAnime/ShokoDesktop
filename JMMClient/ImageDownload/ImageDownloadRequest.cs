using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ImageDownload
{
	public class ImageDownloadRequest
	{
		public ImageEntityType ImageType { get; set; }
		public object ImageData { get; set; }
		public bool ForceDownload { get; set; }

		public ImageDownloadRequest(ImageEntityType imageType, object data, bool forceDownload)
		{
			this.ImageType = imageType;
			this.ImageData = data;
			this.ForceDownload = forceDownload;
		}

	}

	public class ImageDownloadEventArgs : EventArgs
	{
		public readonly string Status = string.Empty;
		public readonly ImageDownloadRequest Req = null;
		public readonly ImageDownloadEventType EventType;
		public readonly ImageEntityType ImageType;

		public ImageDownloadEventArgs(string status, ImageDownloadRequest req, ImageDownloadEventType eventType)
		{
			Status = status;
			Req = req;
			EventType = eventType;
			ImageType = Req.ImageType; ;
		}
	}

	public class QueueUpdateEventArgs : EventArgs
	{
		public readonly int queueCount;

		public QueueUpdateEventArgs(int queueCount)
		{
			this.queueCount = queueCount;
		}
	}

	
}

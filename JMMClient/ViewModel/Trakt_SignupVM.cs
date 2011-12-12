using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace JMMClient.ViewModel
{
	public class Trakt_SignupVM : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

		private string username = "";
		public string Username
		{
			get { return username; }
			set
			{
				username = value;
				NotifyPropertyChanged("Username");
			}
		}

		private string password = "";
		public string Password
		{
			get { return password; }
			set
			{
				password = value;
				NotifyPropertyChanged("Password");
			}
		}

		private string email = "";
		public string Email
		{
			get { return email; }
			set
			{
				email = value;
				NotifyPropertyChanged("Email");
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using JMMClient.ViewModel;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for TraktShoutsShowControl.xaml
	/// </summary>
	public partial class TraktShoutsShowControl : UserControl
	{
		public ObservableCollection<Trakt_ShoutUserVM> CurrentShouts { get; set; }

		public TraktShoutsShowControl()
		{
			InitializeComponent();

			CurrentShouts = new ObservableCollection<Trakt_ShoutUserVM>();

			this.DataContextChanged += new DependencyPropertyChangedEventHandler(TraktShoutsShowControl_DataContextChanged);
		}

		void TraktShoutsShowControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			//RefreshShouts();
		}

		public void RefreshShouts()
		{
			try
			{
				AnimeSeriesVM animeSeries = (AnimeSeriesVM)this.DataContext;
				if (animeSeries == null)
				{
					CurrentShouts.Clear();
					return;
				}


				CurrentShouts.Clear();

				List<JMMServerBinary.Contract_Trakt_ShoutUser> rawShouts = JMMServerVM.Instance.clientBinaryHTTP.GetTraktShoutsForAnime(animeSeries.AniDB_ID);
				foreach (JMMServerBinary.Contract_Trakt_ShoutUser contract in rawShouts)
				{
					Trakt_ShoutUserVM shout = new Trakt_ShoutUserVM(contract);
					CurrentShouts.Add(shout);
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}
	}
}

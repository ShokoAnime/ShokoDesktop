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
using System.Windows.Shapes;
using JMMClient.ViewModel;

namespace JMMClient.Forms
{
	/// <summary>
	/// Interaction logic for SelectAniDBTitle.xaml
	/// </summary>
	public partial class SelectAniDBTitleForm : Window
	{
		public static readonly DependencyProperty AllTitlesProperty = DependencyProperty.Register("AllTitles",
			typeof(List<AnimeTitleVM>), typeof(SelectAniDBTitleForm), new UIPropertyMetadata(null, null));

		public List<AnimeTitleVM> AllTitles
		{
			get { return (List<AnimeTitleVM>)GetValue(AllTitlesProperty); }
			set { SetValue(AllTitlesProperty, value); }
		}

		public string SelectedTitle = "";

		public SelectAniDBTitleForm()
		{
			InitializeComponent();

			btnClose.Click += new RoutedEventHandler(btnClose_Click);
		}

		void btnClose_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			SelectedTitle = "";
			this.Close();
		}

		public void Init(List<AnimeTitleVM> titles)
		{
			AllTitles = titles;
		}

		private void CommandBinding_UseThis(object sender, ExecutedRoutedEventArgs e)
		{
			Window parentWindow = Window.GetWindow(this);

			object obj = e.Parameter;
			if (obj == null) return;

			try
			{
				if (obj.GetType() == typeof(AnimeTitleVM))
				{
					this.Cursor = Cursors.Wait;
					AnimeTitleVM title = obj as AnimeTitleVM;
					SelectedTitle = title.Title;

					this.DialogResult = true;
					this.Close();
				}
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
		}
	}
}

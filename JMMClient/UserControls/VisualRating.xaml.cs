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

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for VisualRating.xaml
	/// </summary>
	public partial class VisualRating : UserControl
	{
		public static readonly DependencyProperty RatingProperty = DependencyProperty.Register("Rating",
			typeof(decimal), typeof(VisualRating), new UIPropertyMetadata((decimal)-1, ratingChangedCallback));

		public decimal Rating
		{
			get { return (decimal)GetValue(RatingProperty); }
			set { SetValue(RatingProperty, value); }
		}

		public static readonly DependencyProperty ImageSizeProperty = DependencyProperty.Register("ImageSize",
			typeof(double), typeof(VisualRating), new UIPropertyMetadata((double)20, ratingChangedCallback));

		public double ImageSize
		{
			get { return (double)GetValue(ImageSizeProperty); }
			set { SetValue(ImageSizeProperty, value); }
		}

		public VisualRating()
		{
			InitializeComponent();
		}

		private static void ratingChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			try
			{
				string packUriFullStar = string.Format("pack://application:,,,/{0};component/Images/star_48.png", Constants.AssemblyName);
				string packUriHalfStar = string.Format("pack://application:,,,/{0};component/Images/star_half_48.png", Constants.AssemblyName);
				string packUriStarOff = string.Format("pack://application:,,,/{0};component/Images/star_off_48.png", Constants.AssemblyName);

				VisualRating input = (VisualRating)d;

				decimal rating = decimal.Parse(e.NewValue.ToString());


				// first star
				if (rating > (decimal)1.5)
					input.imgStar1.Source = new ImageSourceConverter().ConvertFromString(packUriFullStar) as ImageSource;
				if (rating > (decimal)0.5 && rating <= (decimal)1.5)
					input.imgStar1.Source = new ImageSourceConverter().ConvertFromString(packUriHalfStar) as ImageSource;
				if (rating <= (decimal)0.5)
					input.imgStar1.Source = new ImageSourceConverter().ConvertFromString(packUriStarOff) as ImageSource;

				// second star
				if (rating > (decimal)3.5)
					input.imgStar2.Source = new ImageSourceConverter().ConvertFromString(packUriFullStar) as ImageSource;
				if (rating > (decimal)2.5 && rating <= (decimal)3.5)
					input.imgStar2.Source = new ImageSourceConverter().ConvertFromString(packUriHalfStar) as ImageSource;
				if (rating <= (decimal)2.5)
					input.imgStar2.Source = new ImageSourceConverter().ConvertFromString(packUriStarOff) as ImageSource;

				// third star
				if (rating > (decimal)5.5)
					input.imgStar3.Source = new ImageSourceConverter().ConvertFromString(packUriFullStar) as ImageSource;
				if (rating > (decimal)4.5 && rating <= (decimal)5.5)
					input.imgStar3.Source = new ImageSourceConverter().ConvertFromString(packUriHalfStar) as ImageSource;
				if (rating <= (decimal)4.5)
					input.imgStar3.Source = new ImageSourceConverter().ConvertFromString(packUriStarOff) as ImageSource;

				// fourth star
				if (rating > (decimal)7.5)
					input.imgStar4.Source = new ImageSourceConverter().ConvertFromString(packUriFullStar) as ImageSource;
				if (rating > (decimal)6.5 && rating <= (decimal)7.5)
					input.imgStar4.Source = new ImageSourceConverter().ConvertFromString(packUriHalfStar) as ImageSource;
				if (rating <= (decimal)6.5)
					input.imgStar4.Source = new ImageSourceConverter().ConvertFromString(packUriStarOff) as ImageSource;

				// fifth star
				if (rating > (decimal)9.5)
					input.imgStar5.Source = new ImageSourceConverter().ConvertFromString(packUriFullStar) as ImageSource;
				if (rating > (decimal)8.5 && rating <= (decimal)9.5)
					input.imgStar5.Source = new ImageSourceConverter().ConvertFromString(packUriHalfStar) as ImageSource;
				if (rating <= (decimal)8.5)
					input.imgStar5.Source = new ImageSourceConverter().ConvertFromString(packUriStarOff) as ImageSource;
			}
			catch (Exception ex)
			{
				Utils.ShowErrorMessage(ex);
			}
			
		}
	}
}

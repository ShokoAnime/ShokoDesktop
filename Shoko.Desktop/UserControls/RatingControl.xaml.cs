using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Shoko.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for RatingControl.xaml
    /// </summary>
    public partial class RatingControl : UserControl
    {

        public static readonly DependencyProperty RatingValueProperty =
           DependencyProperty.Register("RatingValue", typeof(double), typeof(RatingControl),
                                       new FrameworkPropertyMetadata(0.0,
                                                                     FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                                     RatingValueChanged));

        public double RatingValue
        {
            get { return (double)GetValue(RatingValueProperty); }
            set
            {
                if (value < 0)
                {
                    SetValue(RatingValueProperty, 0);
                    HoverRatingValue = 0;
                }
                else if (value > _maxValue)
                {
                    SetValue(RatingValueProperty, _maxValue);
                    HoverRatingValue = _maxValue;
                }
                else
                {
                    SetValue(RatingValueProperty, value);
                    HoverRatingValue = value;
                }


            }
        }

        public static readonly DependencyProperty HoverRatingValueProperty =
           DependencyProperty.Register("HoverRatingValue", typeof(double), typeof(RatingControl),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null));

        public double HoverRatingValue
        {
            get { return (double)GetValue(HoverRatingValueProperty); }
            set
            {
                if (value < 0)
                {
                    SetValue(HoverRatingValueProperty, 0);
                }
                else if (value > _maxValue)
                {
                    SetValue(HoverRatingValueProperty, _maxValue);
                }
                else
                {
                    SetValue(HoverRatingValueProperty, value);
                }


            }
        }

        private double _maxValue = 10;

        public delegate void RatingValueChangedHandler(RatingValueEventArgs ev);
        public event RatingValueChangedHandler OnRatingValueChangedEvent;
        protected void OnRatingValueChanged(RatingValueEventArgs ev)
        {
            if (OnRatingValueChangedEvent != null)
            {
                OnRatingValueChangedEvent(ev);
            }
        }

        /*public static readonly DependencyProperty HoverRatingProperty = DependencyProperty.Register("HoverRating",
			typeof(double), typeof(RatingControl), new UIPropertyMetadata((double)0, null));

		public double HoverRating
		{
			get { return (double)GetValue(HoverRatingProperty); }
			set { SetValue(HoverRatingProperty, value); }
		}*/

        public RatingControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);
        }



        private static void RatingValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            RatingControl parent = sender as RatingControl;

            try
            {
                NumberStyles style = NumberStyles.Number;
                CultureInfo culture = CultureInfo.CreateSpecificCulture(AppSettings.Culture);

                double ratingValue = -1;
                double.TryParse(e.NewValue.ToString(), style, culture, out ratingValue);

                int numberOfButtonsToHighlight = (int)(2 * ratingValue);

                UIElementCollection children = ((StackPanel)(parent.Content)).Children;
                ToggleButton button = null;

                for (int i = 0; i < numberOfButtonsToHighlight; i++)
                {
                    button = children[i] as ToggleButton;
                    if (button != null)
                        button.IsChecked = true;

                }

                for (int i = numberOfButtonsToHighlight; i < children.Count; i++)
                {
                    button = children[i] as ToggleButton;
                    if (button != null)
                        button.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }

        private void RatingButtonClickEventHandler(Object sender, RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;

            NumberStyles style = NumberStyles.Number;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-GB");

            double newRating = -1;
            double.TryParse((String)button.Tag, style, culture, out newRating);

            if (RatingValue == newRating && newRating == 0.5)
            {
                RatingValue = 0.0;
            }
            else
            {
                RatingValue = newRating;
            }
            HoverRatingValue = RatingValue;
            e.Handled = true;

            OnRatingValueChanged(new RatingValueEventArgs(RatingValue));
        }

        private void RatingButtonMouseEnterEventHandler(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;

            NumberStyles style = NumberStyles.Number;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-GB");

            double hoverRating = -1;
            double.TryParse((String)button.Tag, style, culture, out hoverRating);

            if (hoverRating >= 0)
                HoverRatingValue = hoverRating;

            int numberOfButtonsToHighlight = (int)(2 * hoverRating);

            UIElementCollection children = RatingContentPanel.Children;

            ToggleButton hlbutton = null;

            for (int i = 0; i < numberOfButtonsToHighlight; i++)
            {
                hlbutton = children[i] as ToggleButton;
                if (hlbutton != null)
                    hlbutton.IsChecked = true;
            }

            for (int i = numberOfButtonsToHighlight; i < children.Count; i++)
            {
                hlbutton = children[i] as ToggleButton;
                if (hlbutton != null)
                    hlbutton.IsChecked = false;
            }
        }

        private void RatingButtonMouseLeaveEventHandler(object sender, System.Windows.Input.MouseEventArgs e)
        {
            double ratingValue = RatingValue;
            int numberOfButtonsToHighlight = (int)(2 * ratingValue);

            HoverRatingValue = ratingValue;

            UIElementCollection children = RatingContentPanel.Children;
            ToggleButton button = null;

            for (int i = 0; i < numberOfButtonsToHighlight; i++)
            {
                button = children[i] as ToggleButton;
                if (button != null)
                    button.IsChecked = true;
            }

            for (int i = numberOfButtonsToHighlight; i < children.Count; i++)
            {
                button = children[i] as ToggleButton;
                if (button != null)
                    button.IsChecked = false;
            }
        }
    }

    public class RatingValueEventArgs : EventArgs
    {
        public readonly double RatingValue;

        public RatingValueEventArgs(double ratingValue)
        {
            RatingValue = ratingValue;
        }
    }
}

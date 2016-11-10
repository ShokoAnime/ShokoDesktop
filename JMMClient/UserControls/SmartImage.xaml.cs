using NLog;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for SmartImage.xaml
    /// </summary>
    public partial class SmartImage : UserControl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static readonly DependencyProperty UriSourceProperty = DependencyProperty.Register("UriSource",
            typeof(string), typeof(SmartImage), new UIPropertyMetadata("", uriSourceChangedCallback));

        public static readonly DependencyProperty BitmapHeightProperty = DependencyProperty.Register("BitmapHeight",
            typeof(int), typeof(SmartImage), new UIPropertyMetadata(-1, bitmapHeightChangedCallback));

        public static readonly DependencyProperty BitmapBorderThicknessProperty = DependencyProperty.Register("BitmapBorderThickness",
            typeof(int), typeof(SmartImage), new UIPropertyMetadata(-1, bitmapBorderThicknessChangedCallback));


        private static string uriSource = "";
        private static int bitmapHeight = -1;
        private static int bitmapBorderThickness = -1;

        public SmartImage()
        {
            InitializeComponent();

            MainWindow.imageHelper.ImageDownloadEvent += new ImageDownload.ImageDownloader.ImageDownloadEventHandler(imageHelper_ImageDownloadEvent);
        }

        void imageHelper_ImageDownloadEvent(ImageDownload.ImageDownloadEventArgs ev)
        {
            // check if the image has been downloaded since we loaded this control
            //if (ev.Req.Filename != uriSource) return;
            //if (ev.EventType != ImageDownloadEventType.Complete) return;

            //SetImage(this);
        }


        public string UriSource
        {
            get { return (string)GetValue(UriSourceProperty); }
            set { SetValue(UriSourceProperty, value); }
        }

        private static void uriSourceChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            uriSource = e.NewValue as string;
            SmartImage input = (SmartImage)d;
            SetImage(input);
        }

        public int BitmapHeight
        {
            get { return (int)GetValue(BitmapHeightProperty); }
            set { SetValue(BitmapHeightProperty, value); }
        }

        private static void bitmapHeightChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bitmapHeight = int.Parse(e.NewValue.ToString());
            SmartImage input = (SmartImage)d;
            SetImage(input);
        }

        public int BitmapBorderThickness
        {
            get { return (int)GetValue(BitmapBorderThicknessProperty); }
            set { SetValue(BitmapBorderThicknessProperty, value); }
        }

        private static void bitmapBorderThicknessChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bitmapBorderThickness = int.Parse(e.NewValue.ToString());
            SmartImage input = (SmartImage)d;
            SetImage(input);
        }

        private static void SetImage(SmartImage input)
        {
            if (string.IsNullOrEmpty(uriSource)) return;
            if (bitmapHeight < 0) return;
            if (bitmapBorderThickness < 0) return;

            try
            {
                // do this so that we can handle the completion of an image download
                // via the event
                System.Threading.Thread thread = new System.Threading.Thread(
                    new System.Threading.ThreadStart(
                      delegate ()
                      {
                          input.imgSmart.Dispatcher.Invoke(
                            System.Windows.Threading.DispatcherPriority.Normal,
                            new Action(
                              delegate ()
                              {
                                  string packUriBlank = string.Format("pack://application:,,,/{0};component/Images/blankposter.png", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

                                  BitmapImage bmp = new BitmapImage();
                                  bmp.BeginInit();
                                  if (File.Exists(uriSource))
                                      bmp.UriSource = new Uri(uriSource);
                                  else
                                      bmp.UriSource = new Uri(packUriBlank);
                                  bmp.DecodePixelHeight = bitmapHeight;
                                  bmp.EndInit();

                                  input.imgSmart.Height = bitmapHeight;
                                  input.bdrSmart.Padding = new Thickness(bitmapBorderThickness);
                                  input.imgSmart.Source = bmp;
                              }
                          ));
                      }
                  ));

                thread.Start();
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }
        }

    }
}

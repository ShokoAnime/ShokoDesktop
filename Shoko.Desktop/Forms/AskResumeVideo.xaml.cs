using System;
using System.Windows;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for DialogText.xaml
    /// </summary>
    public partial class AskResumeVideo : Window
    {

        public AskResumeVideo(long position)
        {
            InitializeComponent();
            txtDesc.Text=string.Format(Shoko.Commons.Properties.Resources.Resume_Message,TimeSpan.FromMilliseconds(position).ToString("hh\\:mm\\:ss"));
            btnConfirm.Click += BtnConfirm_Click;
            btnCancel.Click += BtnCancel_Click;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}



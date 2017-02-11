using Shoko.Desktop.Properties;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Shoko.Commons.Properties;

namespace Shoko.Desktop.UserControls
{
    public class Info : MarkupExtension
    {
        /// <summary>
        /// Either a title text or a resource key that can be used
        /// to look up the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Either a tooltips main text or a resource key that can be used
        /// to look up the text.
        /// </summary>
        public string Body { get; set; }


        /// <summary>
        /// Empty default constructor.
        /// </summary>
        public Info()
        {
        }

        /// <summary>
        /// Inits the <see cref="Info"/> markup extension
        /// with title and body.
        /// </summary>
        public Info(string title, string body)
        {
            Title = title;
            Body = body;
        }

        /// <summary>
        /// Performs a lookup for the defined <see cref="Title"/> and
        /// <see cref="Info"/> and creates the tooltip control.
        /// </summary>
        /// <returns>
        /// A <see cref="ToolTip"/> that contains the
        /// <see cref="InfoPopup"/> control.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            //create the user control that 
            InfoPopup popup = new InfoPopup();


            if (!String.IsNullOrEmpty(Title))
            {
                //look up title - if the string is not a
                //resource key, use it directly
                var result = Resources.ResourceManager.GetObject(Title) ?? Title;
                popup.HeaderText = (string)result;
            }

            if (!String.IsNullOrEmpty(Body))
            {
                //look up body text - if the string is not a
                //resource key, use it directly
                var result = Resources.ResourceManager.GetObject(Body) ?? Body;
                popup.BodyText = (string)result;
            }

            //create tooltip and make sure it's not visible
            ToolTip tt = new ToolTip();
            tt.HasDropShadow = false;
            tt.BorderThickness = new Thickness(0);
            tt.Background = Brushes.Transparent;
            tt.Content = popup;

            return tt;
        }
    }
}

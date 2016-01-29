using JMMClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace JMMClient
{
    public class CharacterTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                if (item.GetType() == typeof(AniDB_CharacterVM))
                {
                    AniDB_CharacterVM chr = item as AniDB_CharacterVM;

                    if (chr.CharType.Equals(Constants.CharacterType.MAIN, StringComparison.InvariantCultureIgnoreCase))
                        return element.FindResource("CharacterTemplateMain") as DataTemplate;
                    else
                        return element.FindResource("CharacterTemplate") as DataTemplate;
                }


            }

            return null;
        }
    }
}

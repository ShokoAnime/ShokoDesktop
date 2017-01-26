using System;
using System.Windows;
using System.Windows.Controls;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.DataTemplateSelectors
{
    public class CharacterTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                if (item.GetType() == typeof(VM_AniDB_Character))
                {
                    VM_AniDB_Character chr = item as VM_AniDB_Character;

                    if (chr.CharType.Equals(Models.Constants.CharacterType.MAIN, StringComparison.InvariantCultureIgnoreCase))
                        return element.FindResource("CharacterTemplateMain") as DataTemplate;
                    else
                        return element.FindResource("CharacterTemplate") as DataTemplate;
                }


            }

            return null;
        }
    }
}

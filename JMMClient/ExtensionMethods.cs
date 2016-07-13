using System;
using System.Collections;
using System.Collections.Generic;

namespace JMMClient
{
    public static class ExtensionMethods
    {
        public static T SureGet<T>(this Dictionary<int, T> dict, int val) where T:class
        {
            if (dict.ContainsKey(val))
                return dict[val];
            return null;
        }
        public static bool SubContains(this IEnumerable<string> list, string part)
        {
            foreach (string n in list)
            {
                if (n.IndexOf(part, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }
        /*static public bool SetSelectedItem(this TreeView treeView, object item)
		{
			return SetSelected(treeView, item);
		}

		static private bool SetSelected(ItemsControl parent, object child)
		{
			if (parent == null || child == null)
			{
				return false;
			}

			TreeViewItem childNode = parent.ItemContainerGenerator.ContainerFromItem(child) as TreeViewItem;

			if (childNode != null)
			{
				childNode.Focus();
				return childNode.IsSelected = true;
			}

			if (parent.Items.Count > 0)
			{
				foreach (object childItem in parent.Items)
				{
					ItemsControl childControl = parent.ItemContainerGenerator.ContainerFromItem(childItem) as ItemsControl;

					if (SetSelected(childControl, child))
					{
						return true;
					}
				}
			}

			return false;
		}*/
    }
}

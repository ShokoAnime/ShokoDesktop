using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace JMMClient
{
	static class ExtensionMethods
	{
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

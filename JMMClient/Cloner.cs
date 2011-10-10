using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace JMMClient
{
	public class Cloner
	{
		public static void Clone(object source, object destination)
		{
			Type type = source.GetType();
			FieldInfo[] myObjectFields = type.GetFields(
			BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

			foreach (FieldInfo fi in myObjectFields)
			{
				try
				{
					fi.SetValue(destination, fi.GetValue(source));
				}
				catch { }
			}
		}
	}
}

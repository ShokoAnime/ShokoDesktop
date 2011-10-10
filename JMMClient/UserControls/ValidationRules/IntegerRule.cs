using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Globalization;

namespace JMMClient
{
	class IntegerRule : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			int parameter = 0;

			try
			{
				if (((string)value).Length > 0)
				{
					parameter = int.Parse((String)value);
				}
			}
			catch (Exception e)
			{
				return new ValidationResult(false, "Must be an integer");
			}

			return new ValidationResult(true, null);
		}
	}
}

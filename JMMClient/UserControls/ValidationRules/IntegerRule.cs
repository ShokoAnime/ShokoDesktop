using System;
using System.Globalization;
using System.Windows.Controls;

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

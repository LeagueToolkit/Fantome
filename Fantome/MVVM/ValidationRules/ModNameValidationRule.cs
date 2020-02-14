using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace Fantome.MVVM.ValidationRules
{
    public class ModNameValidationRule : ValidationRule
    {
        private static readonly char[] INVALID_CHARACTERS = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string modName = value as string;

            if (string.IsNullOrEmpty(modName))
            {
                return new ValidationResult(false, "Name cannot be empty");
            }
            else if (modName.Any(x => INVALID_CHARACTERS.Contains(x)))
            {
                return new ValidationResult(false, "Remove special characters from Name");
            }
            else
            {
                return new ValidationResult(true, "");
            }
        }
    }
}

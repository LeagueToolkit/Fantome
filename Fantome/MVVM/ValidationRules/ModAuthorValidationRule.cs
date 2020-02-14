using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace Fantome.MVVM.ValidationRules
{
    public class ModAuthorValidationRule : ValidationRule
    {
        private static readonly char[] INVALID_CHARACTERS = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string modAuthor = value as string;

            if (string.IsNullOrEmpty(modAuthor))
            {
                return new ValidationResult(false, "Author cannot be empty");
            }
            else if (modAuthor.Any(x => INVALID_CHARACTERS.Contains(x)))
            {
                return new ValidationResult(false, "Remove special characters from Author");
            }
            else
            {
                return new ValidationResult(true, "");
            }
        }
    }
}

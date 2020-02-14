using System;
using System.Globalization;
using System.Windows.Controls;

namespace Fantome.MVVM.ValidationRules
{
    public class ModVersionValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string versionString = value as string;
            
            if(string.IsNullOrEmpty(versionString))
            {
                return new ValidationResult(false, "Version cannot be empty");
            }
            else if(!Version.TryParse(versionString, out Version _))
            {
                return new ValidationResult(false, "Not a valid version. Use 1.0 format");
            }
            else
            {
                return new ValidationResult(true, "");
            }
        }
    }
}

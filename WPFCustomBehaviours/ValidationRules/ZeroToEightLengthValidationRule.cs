using System.Globalization;
using System.Windows.Controls;

namespace VideoCompressorGUI.WPFCustomBehaviours.ValidationRules;

public class ZeroToEightLengthValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return string.IsNullOrWhiteSpace((value ?? "").ToString())
            ? new ValidationResult(false, "Name ist notwendig.")
            : ((string) value).Length > 8 
                ? new ValidationResult(false, "Name zu lang.") 
                : ValidationResult.ValidResult;
    }
}
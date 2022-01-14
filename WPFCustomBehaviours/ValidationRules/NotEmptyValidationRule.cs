using System.Globalization;
using System.Windows.Controls;

namespace VideoCompressorGUI.WPFCustomBehaviours.ValidationRules;

public class NotEmptyValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return string.IsNullOrWhiteSpace((value ?? "").ToString())
            ? new ValidationResult(false, "Field is required.")
            : ((string) value).Length > 8 
                ? new ValidationResult(false, "Name too long.") 
                : ValidationResult.ValidResult;
    }
}
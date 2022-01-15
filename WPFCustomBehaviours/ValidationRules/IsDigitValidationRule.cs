using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace VideoCompressorGUI.WPFCustomBehaviours.ValidationRules;

public class IsDigitValidationRule : ValidationRule
{
    private Regex onlyDigitsRegex = new("[^0-9]+");

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        => string.IsNullOrWhiteSpace((value ?? "").ToString())
            ? new ValidationResult(false, "Zahl ist notwendig")
            : ((string)value).Length > 7
                ? new ValidationResult(false, "Zahl zu lang.")
                : onlyDigitsRegex.Match((string)value).Success
                    ? new ValidationResult(false, "Nur Zahlen")
                    : ValidationResult.ValidResult;
}
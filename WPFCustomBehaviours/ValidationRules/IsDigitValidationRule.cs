using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using VideoCompressorGUI.Properties;

namespace VideoCompressorGUI.WPFCustomBehaviours.ValidationRules
{
    public class IsDigitValidationRule : ValidationRule
    {
        private Regex onlyDigitsRegex = new("[^0-9]+");

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            => string.IsNullOrWhiteSpace((value ?? "").ToString())
                ? new ValidationResult(false, Resources.DigitRequired)
                : ((string)value).Length > 7
                    ? new ValidationResult(false, Resources.DigitTooLong)
                    : onlyDigitsRegex.Match((string)value).Success
                        ? new ValidationResult(false, Resources.OnlyDigits)
                        : ValidationResult.ValidResult;
    }
}
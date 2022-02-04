using System.Globalization;
using System.IO;
using System.Windows.Controls;
using VideoCompressorGUI.Properties;

namespace VideoCompressorGUI.WPFCustomBehaviours.ValidationRules
{
    public class ValidFileNameValidationRule : ValidationRule
    {
        public string FolderWithoutFile { get; set; }
        public string FileEnding { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace((value ?? "").ToString()))
                return new ValidationResult(false, Resources.EnterFilename);

            if (File.Exists(FolderWithoutFile + "/" + (string)value + FileEnding))
                return new ValidationResult(false, Resources.EnterFilename);
                
                
            return ValidationResult.ValidResult;
        }
    }
}
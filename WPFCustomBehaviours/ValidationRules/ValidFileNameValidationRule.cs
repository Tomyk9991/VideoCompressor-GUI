using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace VideoCompressorGUI.WPFCustomBehaviours.ValidationRules;

public class ValidFileNameValidationRule : ValidationRule
{
    public string FolderWithoutFile { get; set; }
    public string FileEnding { get; set; }
    
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (string.IsNullOrWhiteSpace((value ?? "").ToString()))
            return new ValidationResult(false, "Dateinamen eingeben");

        if (File.Exists(FolderWithoutFile + "/" + (string)value + FileEnding))
            return new ValidationResult(false, "Dateiname vergeben");
                
                
        return ValidationResult.ValidResult;
    }
}
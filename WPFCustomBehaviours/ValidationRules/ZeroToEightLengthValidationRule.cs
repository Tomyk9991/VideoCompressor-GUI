using System;
using System.Globalization;
using System.Windows.Controls;
using VideoCompressorGUI.Properties;
using VideoCompressorGUI.SettingsLoadables;

namespace VideoCompressorGUI.WPFCustomBehaviours.ValidationRules
{
    public class ZeroToEightLengthValidationRule : ValidationRule
    {
        public CompressPresetCollection collection;
        public CompressPreset IgnorePreset { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var validation = string.IsNullOrWhiteSpace((value ?? "").ToString())
                ? new ValidationResult(false, Resources.NameRequired)
                : ((string)value).Length > 8
                    ? new ValidationResult(false, Resources.NameTooLong)
                    : ValidationResult.ValidResult;

            if (IgnorePreset != null)
            {
                if (collection == null)
                    return validation;
            
                bool contains = collection.Contains(t => t != IgnorePreset && 
                                                         String.Equals(t.PresetName, ((string)value), StringComparison.CurrentCultureIgnoreCase));

                return contains ? new ValidationResult(false, Resources.NameAlreadyUsed) : validation;
            }
        
            return validation;
        }
    }
}
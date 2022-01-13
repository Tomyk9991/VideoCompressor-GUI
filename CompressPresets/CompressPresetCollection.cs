using System.Collections.Generic;
using VideoCompressorGUI.Settings;

namespace VideoCompressorGUI.CompressPresets;

[System.Serializable]
public class CompressPresetCollection : ISettingsLoadable<CompressPresetCollection>
{
    public List<CompressPreset> CompressPresets { get; set; }
    
    public CompressPresetCollection CreateDefault()
    {
        return new CompressPresetCollection
        {
            CompressPresets = new List<CompressPreset>
            {
                new("Discord", false, null, true),
            }
        };
    }
}
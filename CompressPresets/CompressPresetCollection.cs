using System;
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
                new("Discord", true, 4500, false, false, null),
            }
        };
    }

    public bool Contains(Func<CompressPreset, bool> func)
    {
        foreach (CompressPreset preset in CompressPresets)
        {
            if (func.Invoke(preset))
            {
                return true;
            }
        }

        return false;
    }
}
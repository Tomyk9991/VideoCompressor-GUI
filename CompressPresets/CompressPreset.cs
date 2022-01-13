using System.Collections.Generic;

namespace VideoCompressorGUI.CompressPresets;

[System.Serializable]
public class CompressPreset
{
    public string PresetName { get; set; }

    public bool UseBitrate { get; set; } = true;
    public int? Bitrate { get; set; }
    
    public bool UseTargetSizeCalculation { get; set; }

    public CompressPreset(string presetName, bool useBitrate, int? bitrate, bool useTargetSizeCalculation)
    {
        PresetName = presetName;
        UseBitrate = useBitrate;
        Bitrate = bitrate;
        UseTargetSizeCalculation = useTargetSizeCalculation;
    }
}
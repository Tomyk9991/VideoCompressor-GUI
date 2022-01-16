using System.Collections.Generic;

namespace VideoCompressorGUI.CompressPresets;

[System.Serializable]
public class CompressPreset
{
    public string PresetName { get; set; }

    public bool UseBitrate { get; set; } = true;
    public int? Bitrate { get; set; }
    
    public bool UseTargetSizeCalculation { get; set; }
    public bool AskLater { get; set; }
    public int? TargetSize { get; set; }
    
    public CompressPreset(string presetName, bool useBitrate, int? bitrate, bool useTargetSizeCalculation, bool askLater, int? targetSize)
    {
        PresetName = presetName;
        UseBitrate = useBitrate;
        Bitrate = bitrate;
        UseTargetSizeCalculation = useTargetSizeCalculation;
        TargetSize = targetSize;
        AskLater = askLater;
    }

    public override string ToString()
    {
        return "{\n\tPreset name: " + PresetName + "\n\tuse bitrate: " + UseBitrate + "\n\tbitrate: " + Bitrate +
               "\n\tuse target size calculation: " + UseTargetSizeCalculation + "\n\task later: " + AskLater + "\n\ttarget size: " + TargetSize + "\n}";
    }
}
using System;

namespace VideoCompressorGUI.SettingsLoadables
{
    [System.Serializable]
    public class CompressPreset
    {
        public string PresetName { get; set; }

        public CodecDTO Codec { get; set; }

        public bool UseBitrate { get; set; } = true;
        public int? Bitrate { get; set; }


        public bool UseTargetSizeCalculation { get; set; }
        public bool AskLater { get; set; }
        public int? TargetSize { get; set; }

        public CompressPreset(string presetName, CodecDTO codec, bool useBitrate, int? bitrate,
            bool useTargetSizeCalculation, bool askLater, int? targetSize)
        {
            PresetName = presetName;
            this.Codec = codec;
            UseBitrate = useBitrate;
            Bitrate = bitrate;
            UseTargetSizeCalculation = useTargetSizeCalculation;
            TargetSize = targetSize;
            AskLater = askLater;
        }

        public override string ToString()
        {
            return "{\n\tPreset name: " + PresetName + "\n\tCodec: " + Codec + "\n\tuse bitrate: " + UseBitrate +
                   "\n\tbitrate: " + Bitrate +
                   "\n\tuse target size calculation: " + UseTargetSizeCalculation + "\n\task later: " + AskLater +
                   "\n\ttarget size: " + TargetSize + "\n}";
        }

        public int CalculateBitrateWithFixedTargetSize(double videoLengthInSeconds)
        {
            int targetBitSize = this.TargetSize.Value * 1000 * 8;
            return (int)(targetBitSize / videoLengthInSeconds);
        }

        public CompressPreset Copy()
        {
            return new CompressPreset(String.Copy(this.PresetName), this.Codec, this.UseBitrate, this.Bitrate,
                this.UseTargetSizeCalculation, this.AskLater, this.TargetSize);
        }

        public override bool Equals(object? obj)
        {
            if (obj is CompressPreset other)
            {
                return this.PresetName == other.PresetName &&
                       this.AskLater == other.AskLater &&
                       this.UseBitrate == other.UseBitrate &&
                       this.Bitrate == other.Bitrate &&
                       this.UseTargetSizeCalculation == other.UseTargetSizeCalculation &&
                       this.TargetSize == other.TargetSize &&
                       this.Codec == other.Codec;
            }

            return false;
        }
    }
}
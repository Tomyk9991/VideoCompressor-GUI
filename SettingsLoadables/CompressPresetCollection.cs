using System;
using System.Collections.Generic;

namespace VideoCompressorGUI.SettingsLoadables
{
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

        /// <summary>
        /// Gets the preset by name
        /// </summary>
        /// <returns>Return preset with corresponding name. If not found, it will return the first preset</returns>
        public CompressPreset GetByName(string cacheLatestSelectedPresetName)
        {
            CompressPreset preset = this.CompressPresets[0];

            for (int i = 0; i < this.CompressPresets.Count; i++)
            {
                if (this.CompressPresets[i].PresetName.ToLower() == cacheLatestSelectedPresetName.ToLower())
                {
                    return this.CompressPresets[i];
                }
            }

            return preset;
        }
    }
}


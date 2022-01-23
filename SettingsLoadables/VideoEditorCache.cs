using System;

namespace VideoCompressorGUI.SettingsLoadables
{
    [System.Serializable]
    public class VideoEditorCache : ISettingsLoadable<VideoEditorCache>
    {
        public string LatestSelectedPresetName { get; set; }
        
        public VideoEditorCache CreateDefault()
        {
            return new VideoEditorCache
            {
                LatestSelectedPresetName = "Discord"
            };
        }
    }
}
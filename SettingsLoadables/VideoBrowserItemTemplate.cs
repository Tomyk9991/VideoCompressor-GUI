using VideoCompressorGUI.Utils.DataStructures;

namespace VideoCompressorGUI.SettingsLoadables
{
    [System.Serializable]
    public class VideoBrowserItemTemplate : ISettingsLoadable<VideoBrowserItemTemplate>
    {
        public Bitfield8 BitField { get; set; }
        
        public VideoBrowserItemTemplate CreateDefault()
        {
            return new VideoBrowserItemTemplate
            {
                BitField = (Bitfield8) 0b00000001
            };
        }
    }
}
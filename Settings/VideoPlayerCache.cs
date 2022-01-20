namespace VideoCompressorGUI.Settings;

public class VideoPlayerCache : ISettingsLoadable<VideoPlayerCache>
{
    public double Volume { get; set; }

    public VideoPlayerCache(double volume)
    {
        Volume = volume;
    }

    public VideoPlayerCache()
    {
    }

    public VideoPlayerCache CreateDefault()
    {
        return new VideoPlayerCache
        {
            Volume = 50.0d
        };
    }
}
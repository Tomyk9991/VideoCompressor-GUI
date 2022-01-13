using System;
using System.Collections.Generic;
using System.IO;

namespace VideoCompressorGUI.Settings;

[System.Serializable]
public class GeneralSettingsData : ISettingsLoadable<GeneralSettingsData>
{
    public bool AutomaticallyUseNewestVideos { get; set; }
    public string PathToNewestVideos { get; set; }
    public DateTime LatestTimeWatched;
    
    public GeneralSettingsData CreateDefault()
    {
        return new GeneralSettingsData
        {
            AutomaticallyUseNewestVideos = false,
            PathToNewestVideos = "",
            LatestTimeWatched = DateTime.Now
        };
    }

    public List<string> FetchNewFiles()
    {
        List<string> newFiles = new List<string>();
        
        foreach (var file in Directory.EnumerateFiles(PathToNewestVideos, "*.mp4", SearchOption.AllDirectories))
        {
            FileInfo fileInfo = new FileInfo(file);
            if (fileInfo.CreationTime > this.LatestTimeWatched)
                newFiles.Add(file);
        }

        
        return newFiles;
    }
}
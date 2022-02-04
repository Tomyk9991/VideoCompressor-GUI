using System;
using System.Collections.Generic;
using System.IO;

namespace VideoCompressorGUI.SettingsLoadables
{
    [System.Serializable]
    public class GeneralSettingsData : ISettingsLoadable<GeneralSettingsData>
    {
        public bool AutomaticallyUseNewestVideos { get; set; }
        public string PathToNewestVideos { get; set; }
        public DateTime LatestTimeWatched { get; set; }
        public string Language { get; set; }
    
        public bool OpenExplorerAfterCompress { get; set; }
        public bool DeleteOriginalFileAfterCompress { get; set; }
        public bool RemoveFromItemsList { get; set; }
        public bool OpenExplorerAfterLastCompression { get; set; }
        public bool ShowThumbnailForUpperThumb { get; set; }
        
        public string FFmpegPath { get; set; }

        public GeneralSettingsData CreateDefault()
        {
            return new GeneralSettingsData
            {
                AutomaticallyUseNewestVideos = false,
                PathToNewestVideos = "",
                LatestTimeWatched = DateTime.Now,
                FFmpegPath = "",
                
                Language = "en-EN",
            
                OpenExplorerAfterCompress = false,
                DeleteOriginalFileAfterCompress = false,
                RemoveFromItemsList = false,
                ShowThumbnailForUpperThumb = false,
                OpenExplorerAfterLastCompression = false
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
        
        public static List<string> ValidateFFmpegPath(string path)
        {
            List<string> neededFiles = new()
            {
                "avcodec", "avdevice", "avfilter", "avformat", "avutil", "ffmpeg", "ffplay", "ffprobe", "postproc",
                "swresample", "swscale"
            };

            if (!Directory.Exists(path)) return neededFiles;
            
            DirectoryInfo dir = new DirectoryInfo(path);

            foreach (FileInfo file in dir.EnumerateFiles())
            {
                for (int i = neededFiles.Count - 1; i >= 0; i--)
                {
                    if (file.Name.ToLower().Contains(neededFiles[i]))
                    {
                        neededFiles.RemoveAt(i);
                    }
                }
            }
            
            return neededFiles;
        }
    }
}
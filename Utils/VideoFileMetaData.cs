using System;
using FFmpeg.NET;

namespace VideoCompressorGUI.Utils;

public class VideoFileMetaData
{
    public string File { get; set; }
    public string ThumbnailPath { get; set; }
    public MetaData MetaData { get; set; }
    public DateTime CreatedOn { get; set; }

    public VideoFileMetaData(string file, string thumbnailPath, MetaData metaData, DateTime createdOn)
    {
        File = file;
        ThumbnailPath = thumbnailPath;
        MetaData = metaData;
        CreatedOn = createdOn;
    }
}
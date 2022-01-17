using System;
using FFmpeg.NET;
using ffmpegCompressor;

namespace VideoCompressorGUI.Utils;

public class VideoFileMetaData
{
    public string File { get; set; }
    public string ThumbnailPath { get; set; }
    public MetaData MetaData { get; set; }
    public DateTime CreatedOn { get; set; }
    
    public CompressData CompressData { get; set; }
    public CutStartEndParameter CutSeek { get; set; }

    public VideoFileMetaData(string file, string thumbnailPath, MetaData metaData, DateTime createdOn)
    {
        File = file;
        ThumbnailPath = thumbnailPath;
        MetaData = metaData;
        CreatedOn = createdOn;
        CompressData = new CompressData();
        CutSeek = new CutStartEndParameter(TimeSpan.Zero, metaData.Duration);
    }
}
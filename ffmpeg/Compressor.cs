using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FFmpeg.NET;
using VideoCompressorGUI.CompressPresets;
using VideoCompressorGUI.Utils;

namespace ffmpegCompressor;

public class CutStartEndParameter
{
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }

    public CutStartEndParameter(TimeSpan start, TimeSpan end)
    {
        Start = start;
        End = end;
    }
}

public class Compressor
{
    private Engine ffmpeg;
    private Mp4FileValidator validator = new();

    public event Action<VideoFileMetaData, double> OnCompressProgress;
    public event Action<VideoFileMetaData> OnCompressFinished;
    
    public Compressor()
    {
        this.ffmpeg = new Engine();
    }

    public async Task<string> GetThumbnail(string file)
    {
        if (!this.validator.Validate(file))
            throw new ArgumentException("File hasn't the supported format");

        InputFile inputFile = new InputFile(file);
        string generatedPath = TempFolder.GenerateNewName(".jpg");
        OutputFile outputFile = new OutputFile(generatedPath);
        ConversionOptions options = new ConversionOptions { Seek = TimeSpan.FromSeconds(0) };

        await this.ffmpeg.GetThumbnailAsync(inputFile, outputFile, options, CancellationToken.None);
        return generatedPath;
    }

    public async Task<MetaData> GetMetaData(string file)
    {
        if (!this.validator.Validate(file))
            throw new ArgumentException("File hasn't the supported format");

        InputFile inputFile = new InputFile(file);
        return await this.ffmpeg.GetMetaDataAsync(inputFile, CancellationToken.None);
    }

    public async Task<MediaFile> Compress(CompressPreset preset, VideoFileMetaData videoFile, CompressOptions compressOptions)
    {
        InputFile inputFile = new InputFile(videoFile.File);
        OutputFile outputFile = new OutputFile(compressOptions.OutputPath);
        
        ConversionOptions options = this.ffmpeg.BuildConversionOptions(videoFile, preset);

        if (videoFile.CutSeek != null)
        {
            options.CutMedia(videoFile.CutSeek.Start, (videoFile.CutSeek.End - videoFile.CutSeek.Start));
        }
        
        this.ffmpeg.Progress += (_, args) =>
        {
            OnCompressProgress?.Invoke(videoFile, args.ProcessedDuration.TotalSeconds / (videoFile.CutSeek.End - videoFile.CutSeek.Start).TotalSeconds);
        };

        this.ffmpeg.Complete += (_, _) =>
        {
            OnCompressFinished?.Invoke(videoFile);
        };

        return await this.ffmpeg.ConvertAsync(inputFile, outputFile, options, CancellationToken.None);
    }
}
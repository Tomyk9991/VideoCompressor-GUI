using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FFmpeg.NET;
using VideoCompressorGUI.CompressPresets;
using VideoCompressorGUI.Utils;

namespace ffmpegCompressor;
    

public class Compressor
{
    private Engine ffmpeg;
    private Mp4FileValidator validator = new();

    public event Action<double> OnCompressProgress;
    public event Action OnCompressFinished;
    
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

    public async Task<MediaFile> Compress(CompressPreset preset, VideoFileMetaData videoFile)
    {
        InputFile inputFile = new InputFile(videoFile.File);
        string outputName = Path.GetDirectoryName(videoFile.File) + "/Output.mp4";
        OutputFile outputFile = new OutputFile(outputName);

        Console.WriteLine(preset.Bitrate);

        ConversionOptions options = this.ffmpeg.BuildConversionOptions(videoFile, preset);
        
        this.ffmpeg.Progress += (_, args) =>
        {
            OnCompressProgress?.Invoke((double) args.ProcessedDuration.TotalSeconds / args.TotalDuration.TotalSeconds);
        };

        this.ffmpeg.Complete += (_, _) =>
        {
            OnCompressFinished?.Invoke();
        };

        return await this.ffmpeg.ConvertAsync(inputFile, outputFile, options, CancellationToken.None);
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using FFmpeg.NET;
using VideoCompressorGUI.Utils;

namespace ffmpegCompressor;
    

public class Compressor
{
    private Engine ffmpeg;
    private Mp4FileValidator validator = new();
    
    public Compressor()
    {
        this.ffmpeg = new Engine();
        TempFolder.Clear();
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
}
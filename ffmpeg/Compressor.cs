using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FFmpeg.NET;
using VideoCompressorGUI.SettingsLoadables;
using VideoCompressorGUI.Utils;

namespace VideoCompressorGUI.ffmpeg
{
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

        public static event Action<VideoFileMetaData> OnAnyCompressionStarted;
        public static event Action<VideoFileMetaData> OnAnyCompressionFinished;
        

        public event Action<VideoFileMetaData, double> OnCompressProgress;
        public event Action<VideoFileMetaData> OnCompressFinished;
    
        public Compressor()
        {
            string exePath = Path.GetDirectoryName(typeof(Compressor).Assembly.Location) + "/libs/bin/ffmpeg.exe";
            this.ffmpeg = new Engine(exePath);
        }

        public async Task<string> GetThumbnail(string file)
        {
            if (!this.validator.Validate(file))
                throw new ArgumentException("File hasn't the supported format");

            InputFile inputFile = new InputFile(file);
            string generatedPath = TempFolder.GenerateThumbnailName(inputFile.FileInfo.CreationTime, file, ".jpg");

            if (File.Exists(generatedPath))
            {
                return generatedPath;
            }
        
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
            TimeSpan snippetLength = (videoFile.CutSeek.End - videoFile.CutSeek.Start);
            
            ConversionOptions options = this.ffmpeg.BuildConversionOptions(videoFile, compressOptions, preset, snippetLength);

            if (videoFile.CutSeek != null)
            {
                options.CutMedia(videoFile.CutSeek.Start, snippetLength);
            }
            
            
            this.ffmpeg.Progress += (_, args) =>
            {
                OnCompressProgress?.Invoke(videoFile, args.ProcessedDuration.TotalSeconds / (videoFile.CutSeek.End - videoFile.CutSeek.Start).TotalSeconds);
            };

            this.ffmpeg.Complete += (_, _) =>
            {
                OnCompressFinished?.Invoke(videoFile);
                
                videoFile.CompressData.IsCompressing = false;
                OnAnyCompressionFinished?.Invoke(videoFile);
            };

            videoFile.CompressData.IsCompressing = true;
            OnAnyCompressionStarted?.Invoke(videoFile);
            
            return await this.ffmpeg.ConvertAsync(inputFile, outputFile, options, CancellationToken.None);
        }
    }
}
namespace ffmpegCompressor;

public class CompressOptions
{
    public string OutputPath { get; set; }

    public CompressOptions(string outputPath)
    {
        OutputPath = outputPath;
    }
}
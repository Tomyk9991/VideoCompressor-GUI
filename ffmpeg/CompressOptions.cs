
using VideoCompressorGUI.ffmpeg.ExtensionOptions;

namespace VideoCompressorGUI.ffmpeg
{
    public class CompressOptions
    {
        public string OutputPath { get; set; }
        public ExtensionOption ExtensionOption { get; set; }

        public CompressOptions(string outputPath, ExtensionOption option)
        {
            OutputPath = outputPath;
            ExtensionOption = option;
        }
    }
}
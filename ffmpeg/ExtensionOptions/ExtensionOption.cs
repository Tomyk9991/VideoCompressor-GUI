namespace VideoCompressorGUI.ffmpeg.ExtensionOptions
{
    public abstract class ExtensionOption
    {
        public virtual string BuildExtraArguments()
        {
            return "";
        }

        public static ExtensionOption FromFileEnding(string fileEnding)
        {
            return fileEnding switch
            {
                ".gif" => new GifExtensionOption(),
                _ => new DefaultExtensionOption()
            };
        }
    }
}
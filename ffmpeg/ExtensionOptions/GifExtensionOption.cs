namespace VideoCompressorGUI.ffmpeg.ExtensionOptions
{
    public class GifExtensionOption : ExtensionOption
    {
        public int FPS { get; set; }
        public int Scale { get; set; }

        public GifExtensionOption()
        {
            FPS = 30;
            Scale = 400;
        }
        
        public GifExtensionOption(int fps, int scale)
        {
            this.FPS = fps;
            this.Scale = scale;
        }
        
        public override string BuildExtraArguments()
        {
            return $"-y -filter_complex \"fps={FPS},scale={Scale}:-1:flags=lanczos,split[s0][s1];[s0]palettegen=max_colors=32[p];[s1][p]paletteuse=dither=bayer\"";
        }
    }
}
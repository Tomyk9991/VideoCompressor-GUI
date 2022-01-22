using System;

namespace VideoCompressorGUI.Utils
{
    public interface IFileValidator
    {
        public bool Validate(ReadOnlySpan<char> file);
    }

    public class Mp4FileValidator : IFileValidator
    {
        private static string[] EXTENSIONS = {"mp4"};
    
        public bool Validate(ReadOnlySpan<char> file)
        {
            foreach (ReadOnlySpan<char> a in EXTENSIONS)
            {
                if (file.EndsWith(a))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
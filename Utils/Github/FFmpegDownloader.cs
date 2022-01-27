using System.Net;
using System.Threading.Tasks;

namespace VideoCompressorGUI.Utils.Github
{
    public class FFmpegDownloader : GithubReleaseDownloader
    {
        // "https://api.github.com/repos/BtbN/FFmpeg-Builds/releases";
        private const string API_URL = "https://api.github.com/repos/BtbN/FFmpeg-Builds/releases";

        public FFmpegDownloader() : base(API_URL)
        {
            
        }
        
        public override async Task Download(string savingPath)
        {
            string assetSearch = "win64-gpl-shared-";
            
            await base.FetchData(assetSearch);
            await base.Download(savingPath);
        }

    }
}
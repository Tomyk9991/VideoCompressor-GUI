using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using VideoCompressorGUI.ContentControls.Settingspages.InfoTab;

namespace VideoCompressorGUI.Utils.Github
{
    public class GithubReleaseCheck : GithubReleaseDownloader
    {
        public event Action OnDownloadFinished;
        
        private static readonly string RELEASE_URL =
            "https://api.github.com/repos/tomyk9991/videocompressor-gui/releases";

        public GithubReleaseCheck() : base(RELEASE_URL)
        {
            
        }
        
        public bool Check()
        {
            if (Response == null)
                throw new ArgumentNullException("Github response is null. Fetch first");

            return this.Response.Version.CompareTo(typeof(GithubReleaseCheck).Assembly.GetName().Version) > 0;
        }

        public async Task DownloadNewest()
        {
            Assembly currentAssembly = typeof(AboutSettings).Assembly;
            string downloadPath = Path.GetDirectoryName(currentAssembly.Location) + "\\update\\";
            Directory.CreateDirectory(downloadPath);
            
            string zipPath = downloadPath + Response.Name + ".zip";
            
            if (!File.Exists(zipPath))
                await base.Download(zipPath);

            this.OnDownloadFinished?.Invoke();
        }

    }
}
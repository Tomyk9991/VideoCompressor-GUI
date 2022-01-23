using System;
using Newtonsoft.Json.Linq;

namespace VideoCompressorGUI.Utils.Github
{
    public class GithubResponse
    {
        public string Name { get; private set; }
        public Version Version { get; private set; }
        public bool Prerelease { get; private set; }
        public string DownloadZipURL { get; private set; }
        public string ChangeLogs { get; private set; }
        
        public GithubResponse()
        {
            
        }
        
        public GithubResponse(JObject body)
        {
            this.Name = (string) body["name"];
            this.Version = new Version(Name.Split(" ")[1]);
            this.Prerelease = (bool)body["prerelease"];

            JArray assets = (JArray) body["assets"];
            JObject assetObject = null;
            foreach (JObject obj in assets.Children())
            {
                assetObject = obj;
                break;
            }

            this.DownloadZipURL = (string) assetObject["browser_download_url"];
            this.ChangeLogs = (string) body["body"];

            this.ChangeLogs = this.ChangeLogs.Replace("\r\n", Environment.NewLine);
        }

    }
}
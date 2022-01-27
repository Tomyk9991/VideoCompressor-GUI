using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VideoCompressorGUI.Utils.Logger;

namespace VideoCompressorGUI.Utils.Github
{
    public class GithubReleaseDownloader
    {
        public string URL { get; set; }
        public GithubResponse Response { get; set; }

        public event Action<int> OnDownloadProgressChanged;

        public event Action OnDownloadFinished;
        public event Action OnDownloadStarted;
        
        
        public GithubReleaseDownloader(string url)
        {
            this.URL = url;
        }
        
        public virtual async Task Download(string path)
        {
            if (this.Response == null || this.Response.DownloadZipURL == "")
                throw new Exception("Unexpected response from Github. Cannot download the release");
            
            using var client = new WebClient();
            client.DownloadProgressChanged += (sender, args) =>
            {
                OnDownloadProgressChanged?.Invoke(args.ProgressPercentage);
            };

            this.OnDownloadStarted?.Invoke();
            await client.DownloadFileTaskAsync(this.Response.DownloadZipURL, path);
            this.OnDownloadFinished?.Invoke();
        }
        
        public async Task<GithubResponse> FetchData(string expectedAssetNameContents = "")
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");
            
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, this.URL);
            
            try
            {
                HttpResponseMessage message = await client.SendAsync(requestMessage);
                string str = await message.Content.ReadAsStringAsync();
                JArray releases = JArray.Parse(str);
                JObject body = null;
                
                foreach (JObject obj in releases.Children())
                {
                    body = obj;
                    break;
                }

                this.Response = expectedAssetNameContents == "" ? new GithubResponse(body) : new GithubResponse(body, expectedAssetNameContents);
                return this.Response;
            }
            catch (Exception e)
            {
                Log.Warn(e.Message);
                return null;
            }
        }
    }
}
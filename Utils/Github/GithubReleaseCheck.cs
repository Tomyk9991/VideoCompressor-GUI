using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VideoCompressorGUI.ContentControls.Settingspages.InfoTab;

namespace VideoCompressorGUI.Utils.Github
{
    public class GithubReleaseCheck
    {
        public event Action OnDownloadFinished;
        
        private static readonly string RELEASE_URL =
            "https://api.github.com/repos/tomyk9991/videocompressor-gui/releases";

        public GithubResponse Response { get; private set; }
        
        public bool Check()
        {
            if (Response == null)
                throw new ArgumentNullException("Github response is null. Fetch first");

            return this.Response.Version.CompareTo(typeof(GithubReleaseCheck).Assembly.GetName().Version) > 0;
        }

        public async Task<GithubResponse> FetchData()
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");
            
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, RELEASE_URL);

            
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

                this.Response = new GithubResponse(body);
                return this.Response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task DownloadNewest()
        {
            if (this.Response == null || this.Response.DownloadZipURL == "") return;
            
            Assembly currentAssembly = typeof(AboutSettings).Assembly;
            string downloadPath = Path.GetDirectoryName(currentAssembly.Location) + "\\update\\";

            Directory.CreateDirectory(downloadPath);

            if (!File.Exists(downloadPath + Response.Name + ".zip"))
            {
                using var client = new WebClient();
                await client.DownloadFileTaskAsync(this.Response.DownloadZipURL, downloadPath + Response.Name + ".zip");
            }
            
            this.OnDownloadFinished?.Invoke();
        }
    }
}
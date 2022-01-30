using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using MaterialDesignThemes.Wpf;
using VideoCompressorGUI.Utils;
using VideoCompressorGUI.Utils.Github;
using VideoCompressorGUI.WPFCustomBehaviours.ValueConverters;

namespace VideoCompressorGUI.ContentControls.Settingspages.InfoTab
{
    public partial class AboutSettings : UserControl
    {
        private bool updateNext = false;
        public AboutSettings()
        {
            InitializeComponent();
            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly()?.ManifestModule.Name);
            ni.Visible = true;

            iconImage.Source = ni.Icon.ToImageSource();
            
            versionTextBlock.Text = "Version: " + typeof(AboutSettings).Assembly.GetName().Version;
        }
        
        private void OpenURL(string url)
        {
            System.Diagnostics.Process.Start("explorer", url);
        }

        private void Github_Click(object sender, RoutedEventArgs e)
        {
            OpenURL("https://github.com/Tomyk9991");
        }

        private void SourceCode_Click(object sender, RoutedEventArgs e)
        {
            OpenURL("https://github.com/Tomyk9991/VideoCompressor-GUI");
        }

        private void SetButtonLoadingAnimation(bool state)
        {
            ButtonProgressAssist.SetIsIndicatorVisible(updateButton, state);
            updateButton.IsEnabled = !state;
        }

        private async void OnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!updateNext)
            {
                GithubReleaseCheck checker = new GithubReleaseCheck();
                SetButtonLoadingAnimation(true);
            
                GithubResponse response = await checker.FetchData();
                
                if (checker.Check())
                {
                    RenderMDText(response.ChangeLogs);
                    checker.OnDownloadFinished += () =>
                    {
                        SetButtonLoadingAnimation(false);
                        updateButton.Content = "Update durchführen (Neustart erforderlich)";
                        updateNext = true;
                    };
                    
                    await checker.DownloadNewest();
                }
                else
                {
                    RenderMDText("Keine neuen Versionen verfügbar");
                    SetButtonLoadingAnimation(false);
                }
            }
            else
            {
                // https://stackoverflow.com/questions/22891580/update-c-sharp-application-replace-exe-file
                DeleteOldFiles();
                MoveFiles();
                CopyUpdateFiles();
                RestartApplication();
            }
        }

        private void RenderMDText(string text)
        {
            
            text = text.Replace("\\r\\n", Environment.NewLine);
            TextToFlowDocumentConverter converter = (TextToFlowDocumentConverter) this.TryFindResource("TextToFlowDocumentConverter");
            FlowDocument document = (FlowDocument) converter.Convert(text, typeof(FlowDocument), null, null);

            markdownRenderer.Document = document;
        }
        
        
        public static void DeleteOldFiles()
        {
            string[] excludeStrings = { "\\temp\\", "\\settings\\", "\\update\\", "\\ffmpeg.exe", "\\Restart.bat" };
            string path = Path.GetDirectoryName(typeof(AboutSettings).Assembly.Location);
            
            DirectoryInfo root = new DirectoryInfo(path);
            var files = root.GetFiles("*_OLD.*", SearchOption.AllDirectories).Where(p => excludeStrings.All(excludeString => !p.FullName.Contains(excludeString)));
            
            foreach (var file in files)
            {
                File.Delete(file.FullName);
            }
        }
        
        private static void MoveFiles()
        {
            string[] excludeStrings = { "\\temp\\", "\\settings\\", "\\update\\", "\\ffmpeg.exe", "\\Restart.bat" };
            string path = Path.GetDirectoryName(typeof(AboutSettings).Assembly.Location);
            
            DirectoryInfo root = new DirectoryInfo(path);
            var files = root.GetFiles("*.*", SearchOption.AllDirectories).Where(p => excludeStrings.All(excludeString => !p.FullName.Contains(excludeString)));
            
            foreach (var file in files)
            {
                string folderPath = Path.GetDirectoryName(file.FullName);
                string name = Path.GetFileNameWithoutExtension(file.FullName);
                string extension = Path.GetExtension(file.FullName);

                string dest = folderPath + "\\" + name + "_OLD" + extension;
                
                File.Move(file.FullName, dest, true);
            }
        }

        private static void CopyUpdateFiles()
        {
            string path = Path.GetDirectoryName(typeof(AboutSettings).Assembly.Location);
            string fileName = "";

            DirectoryInfo info = new DirectoryInfo(path + "\\update\\");

            foreach (FileInfo file in info.EnumerateFiles())
            {
                if (file.Name.ToLower().Contains("release"))
                {
                    fileName = file.FullName;
                    break;
                }
            }

            if (File.Exists(fileName))
            {
                ZipFile.ExtractToDirectory(fileName, path);
                File.Delete(fileName);
            }
        }

        private static void RestartApplication()
        {
            ProcessStartInfo Info = new ProcessStartInfo
            {
                Arguments = "/C choice /C Y /N /D Y /T 1 & START \"\" \"" + Assembly.GetEntryAssembly().Location.Replace("dll", "exe") + "\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            };
            
            Process.Start(Info);
            Process.GetCurrentProcess().Kill();
        }
    }
    
}

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Ookii.Dialogs.Wpf;
using VideoCompressorGUI.SettingsLoadables;
using VideoCompressorGUI.Utils.Github;

namespace VideoCompressorGUI.ContentControls.Settingspages.GeneralSettingsTab
{
    public partial class GeneralSettings : UserControl
    {
        public static DependencyProperty selectedPathProperty =
            DependencyProperty.Register("SelectedPath", typeof(string), typeof(GeneralSettings));

        public string SelectedPath
        {
            get => (string)GetValue(selectedPathProperty);
            set => SetValue(selectedPathProperty, value);
        }

        public GeneralSettings()
        {
            InitializeComponent();
            var settingsLoad = SettingsFolder.Load<GeneralSettingsData>();

            ApplySettingsLoad(settingsLoad);
        }
        
        private void GeneralSettings_OnLoaded(object sender, RoutedEventArgs e)
        {
            HandleOnMissingFiles();
        }

        private void HandleOnMissingFiles()
        {
            List<string> missingFiles = ValidateFFmpegPath(ffmpegPathTextBox.Text);

            bool missing = missingFiles.Count > 0;
            
            missingFilesTextBox.Text = missingFiles.Count switch
            {
                0 => "",
                1 => "Es fehlt folgende Datei: " + Environment.NewLine + missingFiles[0],
                _ => "Es fehlen folgende Dateien: " + Environment.NewLine + "    - " + string.Join(Environment.NewLine + "    - ", missingFiles)
            };

            this.downloadFFmpegButton.IsEnabled = missing;
        }

        private void ApplySettingsLoad(GeneralSettingsData settings)
        {
            this.collapsibleGroupBoxNewestVideo.IsVisibleContent = settings.AutomaticallyUseNewestVideos;
            this.SelectedPath = settings.PathToNewestVideos;
            this.openExplorerAfterCompressCheckbox.IsChecked = settings.OpenExplorerAfterCompress;
            this.ffmpegPathTextBox.Text = settings.FFmpegPath ?? "";
            this.deleteOriginalFileAfterCompressCheckbox.IsChecked = settings.DeleteOriginalFileAfterCompress;
            this.removeFromItemsListAfterCompressCheckbox.IsChecked = settings.RemoveFromItemsList;
            this.openExplorerAfterLastCompressCheckbox.IsChecked = settings.OpenExplorerAfterLastCompression;
        }

        private void SelectNewVideoWatcherPath_OnClick(object sender, MouseButtonEventArgs e)
        {
            SelectPath();
        }

        private void SelectPathButton_OnClick(object sender, RoutedEventArgs e)
        {
            SelectPath();
        }

        private void SelectPath()
        {
            var dialog = new VistaFolderBrowserDialog();

            if ((bool)dialog.ShowDialog(Window.GetWindow(this)))
            {
                var selectedFolder = dialog.SelectedPath;

                if (selectedFolder != "")
                    this.SelectedPath = selectedFolder;
            }
        }

        public static List<string> ValidateFFmpegPath(string path)
        {
            List<string> neededFiles = new()
            {
                "avcodec", "avdevice", "avfilter", "avformat", "avutil", "ffmpeg", "ffplay", "ffprobe", "postproc",
                "swresample", "swscale"
            };

            if (!Directory.Exists(path)) return neededFiles;
            
            DirectoryInfo dir = new DirectoryInfo(path);

            foreach (FileInfo file in dir.EnumerateFiles())
            {
                for (int i = neededFiles.Count - 1; i >= 0; i--)
                {
                    if (file.Name.ToLower().Contains(neededFiles[i]))
                    {
                        neededFiles.RemoveAt(i);
                    }
                }
            }
            
            return neededFiles;
        }

        private void SelectFFmpegPath_OnClick(object sender, MouseButtonEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();

            if ((bool)dialog.ShowDialog(Window.GetWindow(this)))
            {
                var selectedFolder = dialog.SelectedPath;
                if (selectedFolder != "")
                {
                    this.ffmpegPathTextBox.Text = selectedFolder;
                    HandleOnMissingFiles();
                }
                
            }
        }

        private void GeneralSettings_OnUnloaded(object sender, RoutedEventArgs e)
        {
            GeneralSettingsData data = new GeneralSettingsData
            {
                LatestTimeWatched = DateTime.Now,
                AutomaticallyUseNewestVideos =
                    collapsibleGroupBoxNewestVideo.IsVisibleContent && this.SelectedPath != "",
                PathToNewestVideos = collapsibleGroupBoxNewestVideo.IsVisibleContent ? this.SelectedPath : "",
                OpenExplorerAfterCompress = openExplorerAfterCompressCheckbox.IsChecked.Value,
                FFmpegPath = ffmpegPathTextBox.Text,
                DeleteOriginalFileAfterCompress = deleteOriginalFileAfterCompressCheckbox.IsChecked.Value,
                RemoveFromItemsList = removeFromItemsListAfterCompressCheckbox.IsChecked.Value,
                OpenExplorerAfterLastCompression = openExplorerAfterLastCompressCheckbox.IsChecked.Value
            };

            SettingsFolder.Save(data);
        }

        private void DeletePathButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.SelectedPath = "";
        }

        private async void DownloadFFmpegButton_OnClick(object sender, RoutedEventArgs e)
        {
            string path = this.ffmpegPathTextBox.Text;
            string zipFilePath = path + "\\ffmpeg.zip";
            
            FFmpegDownloader downloader = new FFmpegDownloader();
            downloader.OnDownloadProgressChanged += (int percentage) =>
            {
                ButtonProgressAssist.SetValue(downloadFFmpegButton, percentage);
            };

            downloader.OnDownloadStarted += () =>
            {
                downloadFFmpegButton.IsEnabled = false;
            };
            
            downloader.OnDownloadFinished += () =>
            {
                ButtonProgressAssist.SetValue(downloadFFmpegButton, 0);
                downloadFFmpegButton.IsEnabled = true;
            };
            
            await downloader.Download(zipFilePath);
            
            ZipFile.ExtractToDirectory(zipFilePath, path);

            string firstCreatedFolder = Directory.GetDirectories(path)[0];
            string binFolderPath  = firstCreatedFolder + "\\bin\\";
            DirectoryInfo binfolder = new DirectoryInfo(binFolderPath);

            foreach (FileInfo file in binfolder.GetFiles())
            {
                File.Move(file.FullName, path + "\\" + file.Name);
            }

            Directory.Delete(firstCreatedFolder, true);
            File.Delete(zipFilePath);

            HandleOnMissingFiles();
        }
    }
}
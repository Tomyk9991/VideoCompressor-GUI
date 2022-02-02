using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Ookii.Dialogs.Wpf;
using VideoCompressorGUI.SettingsLoadables;
using VideoCompressorGUI.Utils.Github;
using VideoCompressorGUI.Utils.Logger;

namespace VideoCompressorGUI.ContentControls.Components
{
    public partial class FailLoad : UserControl
    {
        private string lastHeader = "";
        public FailLoad(string header = "FFmpeg konnte nicht geladen werden")
        {
            InitializeComponent();
            lastHeader = header;
        }

        private void CheckPath(string path)
        {
            headerTextBlock.Text = this.lastHeader;
            
            List<string> missingFiles = GeneralSettingsData.ValidateFFmpegPath(path);
            failMessageTextBlock.Text = missingFiles.Count == 0 ? "" :
                missingFiles.Count == 1 ? "Es fehlt folgende Datei: " + Environment.NewLine + missingFiles[0] :
                "Es fehlen folgende Dateien: " + Environment.NewLine + "    - " + string.Join(Environment.NewLine + "    - ", missingFiles);

            failFirstTextBlock.Text = "Verzeichnis: \'" + path + "'";
            
            if (missingFiles.Count == 0)
            {
                GeneralSettingsData settings = SettingsFolder.Load<GeneralSettingsData>();
                settings.FFmpegPath = path;
                SettingsFolder.Save(settings);
                
                ((MainWindow)Application.Current.MainWindow).PopContentControl();
            }
        }

        private void FailLoad_OnLoaded(object sender, RoutedEventArgs e)
        {
            Log.Warn("Fail Control loaded");
            
            GeneralSettingsData settings = SettingsFolder.Load<GeneralSettingsData>();
            CheckPath(settings.FFmpegPath);
            CheckPath(ffmpegPathTextBox.Text);
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
                    CheckPath(selectedFolder);
                }
            }
        }

        private async void DownloadFFmpegButton_OnClick(object sender, RoutedEventArgs e)
        {
            string path = this.ffmpegPathTextBox.Text;
            string zipFilePath = path + "\\ffmpeg.zip";

            Directory.CreateDirectory(path);
            
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
                Console.WriteLine(file.FullName, path + "\\" + file.Name);
                File.Move(file.FullName, path + "\\" + file.Name);
            }

            Directory.Delete(firstCreatedFolder, true);
            File.Delete(zipFilePath);

            CheckPath(this.ffmpegPathTextBox.Text);
        }
    }
}
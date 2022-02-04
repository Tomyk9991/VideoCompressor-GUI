using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using VideoCompressorGUI.ffmpeg;
using VideoCompressorGUI.SettingsLoadables;
using VideoCompressorGUI.Utils;

namespace VideoCompressorGUI.ContentControls
{
    public partial class VideoEditorControl : UserControl
    {
        private CompressPreset currentlySelectedPreset;
        private VideoFileMetaData currentlySelectedVideoFile;

        private CompressPresetCollection presets;
        private VideoEditorCache cache;

        public VideoEditorControl(List<string> files)
        {
            InitializeComponent();

            informationParent.Visibility = Visibility.Collapsed;
            
            ((MainWindow)Application.Current.MainWindow).OnWindowClosing += args => SaveCache();

            this.videoBrowser.UpdateSource(files);
            
            
            Compressor.OnAnyCompressionStarted += file =>
            {
                if (file == this.currentlySelectedVideoFile)
                {
                    Dispatcher.Invoke(() =>
                    {
                        compressButton.IsEnabled = false;
                    });
                }
            };

            Compressor.OnAnyCompressionFinished += file =>
            {
                if (file == this.currentlySelectedVideoFile)
                {
                    Dispatcher.Invoke(() =>
                    {
                        compressButton.IsEnabled = true;
                    });
                }
            };
            

            this.videoBrowser.OnSelectionChanged += (a) =>
            {
                this.currentlySelectedVideoFile = a;
                compressButton.IsEnabled = a != null;

                if (this.currentlySelectedVideoFile != null)
                    compressButton.IsEnabled = !this.currentlySelectedVideoFile.CompressData.IsCompressing;

                ShowVideoFileInformation(this.currentlySelectedVideoFile);
                this.videoPlayer.UpdateSource(a);
            };
        }

        private void ShowVideoFileInformation(VideoFileMetaData file)
        {
            if (file == null)
            {
                informationParent.Visibility = Visibility.Collapsed;
                return;
            }

            informationParent.Visibility = Visibility.Visible;

            fileNameTextBox.Text = Path.GetFileName(file.File);
            fileDurationTextBox.Text = file.MetaData.Duration.TotalSeconds.ToMinutesAndSecondsFromSeconds() 
                                       + (file.MetaData.Duration.TotalMinutes < 1 ? $" {Properties.Resources.Seconds}" :  $" {Properties.Resources.Minutes}");
            fileSizeTextBox.Text = MathHelper.BytesToString(file.MetaData.FileInfo.Length);
            fileFPSTextBox.Text = (int)file.MetaData.VideoData.Fps + "FPS";
        }

        private void VideoEditorControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            presets = SettingsFolder.Load<CompressPresetCollection>();
            cache = SettingsFolder.Load<VideoEditorCache>();

            this.currentlySelectedPreset = presets.GetByName(cache.LatestSelectedPresetName);

            FillContextMenu(presets);
        }

        private void VideoEditorControl_OnUnloaded(object sender, RoutedEventArgs e) => SaveCache();

        private void SaveCache() => SettingsFolder.Save(this.cache);

        private void FillContextMenu(CompressPresetCollection collection)
        {
            buttonCompressContextMenu.Items.Clear();

            for (int i = 0; i < collection.CompressPresets.Count; i++)
            {
                var menuItem = new MenuItem
                {
                    Header = collection.CompressPresets[i].PresetName,
                    Icon = new PackIcon
                    {
                        Kind = collection.CompressPresets[i].PresetName == currentlySelectedPreset.PresetName
                            ? PackIconKind.Check
                            : PackIconKind.None
                    }
                };

                menuItem.Click += (sender, args) =>
                {
                    var menuItem = (MenuItem)sender;
                    var newPreset = collection.CompressPresets.First(p => p.PresetName == (string)menuItem.Header);
                    OnPresetChanged(newPreset);

                    foreach (MenuItem item in buttonCompressContextMenu.Items)
                    {
                        ((PackIcon)item.Icon).Kind =
                            (string)item.Header == newPreset.PresetName ? PackIconKind.Check : PackIconKind.None;
                    }
                };

                buttonCompressContextMenu.Items.Add(menuItem);
            }
        }

        private void OnPresetChanged(CompressPreset newPreset)
        {
            this.cache.LatestSelectedPresetName = newPreset.PresetName;
            this.currentlySelectedPreset = newPreset;
        }

        private void ContextMenu_OnClick(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            ContextMenu contextMenu = b.ContextMenu;


            contextMenu.PlacementTarget = b;
            contextMenu.IsOpen = true;

            e.Handled = true;
        }

        private void InitCompressDialog_OnClick(object sender, RoutedEventArgs e)
        {
            compressOptionsDialog.Visibility = Visibility.Visible;
            compressOptionsDialog.InitCompressDialog(currentlySelectedPreset, currentlySelectedVideoFile, videoBrowser);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FFmpeg.NET.Events;
using ffmpegCompressor;
using MaterialDesignThemes.Wpf;
using VideoCompressorGUI.CompressPresets;
using VideoCompressorGUI.Settings;
using VideoCompressorGUI.Utils;

namespace VideoCompressorGUI.ContentControls;

public partial class VideoEditorControl : UserControl
{
    private CompressPreset currentlySelectedPreset = null;
    private CompressPresetCollection presets;
    private VideoFileMetaData currentlySelectedVideoFile;

    public VideoEditorControl(List<string> files)
    {
        InitializeComponent();
        presets = SettingsFolder.Load<CompressPresetCollection>();

        this.currentlySelectedPreset = presets.CompressPresets[0];
        FillContextMenu(presets);

        this.videoBrowser.UpdateSource(files);
        this.videoBrowser.OnSelectionChanged += (a) =>
        {
            this.currentlySelectedVideoFile = a;

            compressButton.IsEnabled = a != null;
            
            this.videoPlayer.UpdateSource(a);
        };
    }

    private void FillContextMenu(CompressPresetCollection collection)
    {
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
                var newPreset = collection.CompressPresets.First(p => p.PresetName == menuItem.Header);
                OnPresetChanged(newPreset);

                foreach (MenuItem item in buttonCompressContextMenu.Items)
                {
                    ((PackIcon)item.Icon).Kind =
                        item.Header == newPreset.PresetName ? PackIconKind.Check : PackIconKind.None;
                }
            };

            buttonCompressContextMenu.Items.Add(menuItem);
        }
    }

    private void OnPresetChanged(CompressPreset newPreset)
    {
        this.currentlySelectedPreset = newPreset;
    }

    private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Compressor compressor = new Compressor();
        
        compressor.OnCompressProgress += OnCompressProgress;
        compressor.OnCompressFinished += OnCompressFinished;
        
        await compressor.Compress(currentlySelectedPreset, currentlySelectedVideoFile);
    }

    private void OnCompressFinished(VideoFileMetaData file)
    {
        Dispatcher.Invoke(() =>
        {
            file.CompressData.Progress = 1.0d;
        });
    }

    private void OnCompressProgress(VideoFileMetaData file, double percentage)
    {
        Dispatcher.Invoke(() =>
        {
            file.CompressData.Progress = percentage;
            file.CompressData.ProgressColor = CompressData.FromPercentage(percentage);
        });
    }

    private void ContextMenu_OnClick(object sender, RoutedEventArgs e)
    {
        Button b = (Button)sender;
        ContextMenu contextMenu = b.ContextMenu;


        contextMenu.PlacementTarget = b;
        contextMenu.IsOpen = true;

        e.Handled = true;
    }
}
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
    private CompressPresetCollection preset;
    private VideoFileMetaData currentlySelectedVideoFile;

    public VideoEditorControl(List<string> files)
    {
        InitializeComponent();
        preset = SettingsFolder.Load<CompressPresetCollection>();

        this.currentlySelectedPreset = preset.CompressPresets[0];
        FillContextMenu(preset);

        this.videoBrowser.UpdateSource(files);
        this.videoBrowser.OnSelectionChanged += (a) =>
        {
            this.currentlySelectedVideoFile = a;
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
        textblockProgress.Text = "Beginne Komprimierung...";
        
        compressor.OnCompressProgress += OnCompressProgress;
        compressor.OnCompressFinished += OnCompressFinished;
        
        await compressor.Compress(currentlySelectedPreset, currentlySelectedVideoFile);
    }

    private void OnCompressFinished()
    {
        Dispatcher.Invoke(() =>
        {
            textblockProgress.Text = "";
        });
    }

    private void OnCompressProgress(double percentage)
    {
        Dispatcher.Invoke(() =>
        {
            textblockProgress.Text = (percentage * 100.0d).ToString("F") + "%";
            
            this.currentlySelectedVideoFile.CompressData.Progress = percentage;
            this.currentlySelectedVideoFile.CompressData.ProgressColor = CompressData.FromPercentage(percentage);
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
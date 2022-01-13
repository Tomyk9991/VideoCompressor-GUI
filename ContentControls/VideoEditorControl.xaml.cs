using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using VideoCompressorGUI.CompressPresets;
using VideoCompressorGUI.Settings;

namespace VideoCompressorGUI.ContentControls;

public partial class VideoEditorControl : UserControl
{
    private List<string> files;

    private CompressPreset currentlySelectedPreset = null;
    private CompressPresetCollection preset;

    public VideoEditorControl(List<string> files)
    {
        InitializeComponent();
        preset = SettingsFolder.Load<CompressPresetCollection>();

        this.currentlySelectedPreset = preset.CompressPresets[0];
        FillContextMenu(preset);

        this.files = files;

        this.videoBrowser.UpdateSource(files);
        this.videoBrowser.OnSelectionChanged += this.videoPlayer.UpdateSource;
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

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("Komprimiere...");
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
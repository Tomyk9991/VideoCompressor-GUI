using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ffmpegCompressor;
using MaterialDesignThemes.Wpf;
using Ookii.Dialogs.Wpf;
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
        
        this.videoBrowser.UpdateSource(files);
        
        this.videoBrowser.OnSelectionChanged += (a) =>
        {
            this.currentlySelectedVideoFile = a;
            compressButton.IsEnabled = a != null;
            
            this.videoPlayer.UpdateSource(a);
        };
    }
    
    private void VideoEditorControl_OnLoaded(object sender, RoutedEventArgs e)
    {
        presets = SettingsFolder.Load<CompressPresetCollection>();
        this.currentlySelectedPreset = presets.CompressPresets[0];
        FillContextMenu(presets);
    }

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


    private void OnCompressFinished(VideoFileMetaData file)
    {
        Dispatcher.Invoke(() =>
        {
            file.CompressData.Progress = 1.0d;
            file.CompressData.ProgressColor = CompressData.FromPercentage(1.0d);
            
            var animation = new DoubleAnimation
            {
                From = 1.0d,
                To = 1.2d,
                BeginTime = TimeSpan.FromSeconds(0),
                Duration = TimeSpan.FromSeconds(1.0),
                FillBehavior = FillBehavior.Stop
            };
            
            animation.Completed += (s, e) =>
            {
                file.CompressData.Progress = 0.0d;
                file.CompressData.ProgressColor = CompressData.FromPercentage(0.0d);
            };

            animation.BeginAnimation(OpacityProperty, animation);
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

    private void OnSelectFolderPath(object sender, MouseButtonEventArgs e)
    {
        string newPath = SelectPath();
        folderPathTextBox.Text = newPath == "" && folderPathTextBox.Text != "" ? folderPathTextBox.Text : newPath;
    }
    
    private string SelectPath()
    {
        var dialog = new VistaFolderBrowserDialog();

        if ((bool)dialog.ShowDialog(Window.GetWindow(this)))
        {
            return dialog.SelectedPath;
        }

        return "";
    }
    
    private void OnFileNameChanged(object sender, TextChangedEventArgs e)
    {
        ValidateCanCompress();
    }
    
    private void InitCompressDialog_OnClick(object sender, RoutedEventArgs e)
    {
        compressOptionsDialog.Visibility = Visibility.Visible;

        
        targetSizeQuestionParent.Visibility =
            currentlySelectedPreset.AskLater ? Visibility.Visible : Visibility.Collapsed;
        
        string folderWithoutFile = Path.GetDirectoryName(currentlySelectedVideoFile.File);
        folderPathTextBox.Text = folderWithoutFile;

        validFileNameValidationRule.FolderWithoutFile = folderWithoutFile;
        validFileNameValidationRule.FileEnding = fileEndingTextBox.Text;

        string builtName = Path.GetFileNameWithoutExtension(currentlySelectedVideoFile.File).BuildNameFromString();

        while (!validFileNameValidationRule.Validate(builtName, null).IsValid) 
            builtName = builtName.BuildNameFromString();
        
        filenameTextBox.Text = builtName;
    }

    private void OutsideDialog_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        compressOptionsDialog.Visibility = Visibility.Collapsed;
    }

    private void InsideDialog_OnClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    private async void DialogCompress_OnClick(object sender, RoutedEventArgs e)
    {
        Compressor compressor = new Compressor();
        
        compressor.OnCompressProgress += OnCompressProgress;
        compressor.OnCompressFinished += OnCompressFinished;

        string folderWithoutFile = Path.GetDirectoryName(currentlySelectedVideoFile.File);
        string fileEnding = fileEndingTextBox.Text;
        string builtName = filenameTextBox.Text;
        
        
        CompressOptions options = new CompressOptions(folderWithoutFile + "/" + builtName + fileEnding);
        
        compressOptionsDialog.Visibility = Visibility.Collapsed;
        await compressor.Compress(currentlySelectedPreset, currentlySelectedVideoFile, options);
    }

    private void DialogCancel_OnClick(object sender, RoutedEventArgs e)
    {
        folderPathTextBox.Text = "";
        filenameTextBox.Text = "";
        targetSizeTextbox.Text = "";
        
        compressOptionsDialog.Visibility = Visibility.Collapsed;
    }

    private void ValidateCanCompress()
    {
        var r1 = validFileNameValidationRule.Validate(filenameTextBox.Text, CultureInfo.CurrentCulture);
        var r2 = folderPathTextBox.Text != "";

        dialogCompressButton.IsEnabled = r1.IsValid && r2;
    }
}
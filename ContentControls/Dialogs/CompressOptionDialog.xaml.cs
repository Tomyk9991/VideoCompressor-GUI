using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ffmpegCompressor;
using Microsoft.VisualBasic.FileIO;
using Ookii.Dialogs.Wpf;
using VideoCompressorGUI.CompressPresets;
using VideoCompressorGUI.ContentControls.Components;
using VideoCompressorGUI.ContentControls.Settingspages;
using VideoCompressorGUI.Settings;
using VideoCompressorGUI.Utils;

namespace VideoCompressorGUI.ContentControls.Dialogs;

public partial class CompressOptionDialog : UserControl
{
    private CompressPreset currentlySelectedPreset;
    private VideoFileMetaData currentlySelectedVideoFile;
    private VideoBrowser videoBrowser;
    
    private GeneralSettingsData generalSettings;
    private PathRuleCollection pathRuleCollection;
    
    public CompressOptionDialog()
    {
        InitializeComponent();
        
        ((MainWindow)Application.Current.MainWindow).OnKeyPressed += async (e, _) =>
        {
            if (ValidateCanCompress() && e.Key == Key.Enter)
            {
                await Compress();
            }
        };
    }
    
    private void CompressOptionDialog_OnLoaded(object sender, RoutedEventArgs e)
    {
        generalSettings = SettingsFolder.Load<GeneralSettingsData>();
        pathRuleCollection = SettingsFolder.Load<PathRuleCollection>();
    }

    public void InitCompressDialog(CompressPreset preset, VideoFileMetaData file, VideoBrowser videoBrowser)
    {
        this.currentlySelectedPreset = preset;
        this.currentlySelectedVideoFile = file;
        this.videoBrowser = videoBrowser;
        
        Dispatcher.DelayInvoke(() =>
        {
            filenameTextBox.Focus();
            filenameTextBox.CaretIndex = filenameTextBox.Text.Length;
        }, TimeSpan.FromMilliseconds(1));
        
        targetSizeQuestionParent.Visibility =
            currentlySelectedPreset.AskLater ? Visibility.Visible : Visibility.Collapsed;
        
        string folderWithoutFile = Path.GetDirectoryName(currentlySelectedVideoFile.File);

        if (pathRuleCollection.ContainsDirectory(folderWithoutFile, out string mappedPath))
        {
            folderWithoutFile = mappedPath;
        }
        
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
        this.Visibility = Visibility.Collapsed;
    }

    private void InsideDialog_OnClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
    
    private void OnFileNameChanged(object sender, TextChangedEventArgs e)
    {
        ValidateCanCompress();
    }

    private void TargetSizeTextbox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        ValidateCanCompress();
    }

    private async void DialogCompress_OnClick(object sender, RoutedEventArgs e)
    {
        await Compress();
    }

    private async Task Compress()
    {
        Compressor compressor = new Compressor();
        
        compressor.OnCompressProgress += OnCompressProgress;
        compressor.OnCompressFinished += OnCompressFinished;

        string folderWithoutFile = folderPathTextBox.Text;
        string fileEnding = fileEndingTextBox.Text;
        string builtName = filenameTextBox.Text;

        if (this.currentlySelectedPreset.AskLater)
            this.currentlySelectedPreset.TargetSize = int.Parse(this.targetSizeTextbox.Text);
        
        
        CompressOptions options = new CompressOptions(folderWithoutFile + "/" + builtName + fileEnding);
            
        this.Visibility = Visibility.Collapsed;
        
        compressor.OnCompressFinished += file =>
        {
            if (generalSettings.OpenExplorerAfterCompress)
                UtilMethods.OpenExplorerAndSelectFile(options.OutputPath);

            if (generalSettings.DeleteOriginalFileAfterCompress)
            {
                Console.WriteLine("Move file to Bin: " + file.File);
                FileSystem.DeleteFile(file.File, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin,
                    UICancelOption.DoNothing);
            }

            if (generalSettings.DeleteOriginalFileAfterCompress || generalSettings.RemoveFromItemsList)
            {
                Dispatcher.Invoke(() =>
                {
                    int count = this.videoBrowser.RemoveItem(file);

                    if (count == 0 && generalSettings.OpenExplorerAfterLastCompression)
                        UtilMethods.OpenExplorerAndSelectFile(options.OutputPath);
                });
            }
        };
        
        await compressor.Compress(currentlySelectedPreset, currentlySelectedVideoFile, options);
    }
    
    private void OnCompressProgress(VideoFileMetaData file, double percentage)
    {
        Dispatcher.Invoke(() =>
        {
            file.CompressData.Progress = percentage;
            file.CompressData.ProgressColor = CompressData.FromPercentage(percentage);
        });
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
    
    private void DialogCancel_OnClick(object sender, RoutedEventArgs e)
    {
        folderPathTextBox.Text = "";
        filenameTextBox.Text = "";
        targetSizeTextbox.Text = "";
        
        this.Visibility = Visibility.Collapsed;
    }
    
    private void OnSelectFolderPath(object sender, MouseButtonEventArgs e)
    {
        string newPath = SelectPath();
        folderPathTextBox.Text = newPath == "" && folderPathTextBox.Text != "" ? folderPathTextBox.Text : newPath;
    }
    
    private bool ValidateCanCompress()
    {
        var r1 = validFileNameValidationRule.Validate(filenameTextBox.Text, CultureInfo.CurrentCulture);
        var r2 = folderPathTextBox.Text != "";
        var r3 = isDigitValidationRule.Validate(targetSizeTextbox.Text, CultureInfo.CurrentCulture);

        bool result = r1.IsValid && 
                      r2 && 
                      (r3.IsValid && targetSizeQuestionParent.Visibility == Visibility.Visible || targetSizeQuestionParent.Visibility == Visibility.Collapsed);
        
        dialogCompressButton.IsEnabled = result;

        return result;
    }
    
    private string SelectPath()
    {
        var dialog = new VistaFolderBrowserDialog();
        
        if ((bool)dialog.ShowDialog(Window.GetWindow(this)))
        {
            string selectedPath = dialog.SelectedPath;
            validFileNameValidationRule.FolderWithoutFile = selectedPath;
            
            return selectedPath;
        }

        return "";
    }
}
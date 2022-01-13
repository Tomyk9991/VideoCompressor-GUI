using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using VideoCompressorGUI.Settings;

namespace VideoCompressorGUI.ContentControls.Settingspages;

public partial class GeneralSettings : UserControl
{
    public static DependencyProperty selectedPathProperty = DependencyProperty.Register("SelectedPath", typeof(string), typeof(GeneralSettings));
    
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

    private void ApplySettingsLoad(GeneralSettingsData settings)
    {
        this.collapsibleGroupBoxNewestVideo.IsVisibleContent = settings.AutomaticallyUseNewestVideos;
        this.SelectedPath = settings.PathToNewestVideos;
    }

    private void SelectedPath_OnClick(object sender, MouseButtonEventArgs e)
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
            Console.WriteLine(selectedFolder);
            this.SelectedPath = selectedFolder;
        }
    }

    private void GeneralSettings_OnUnloaded(object sender, RoutedEventArgs e)
    {
        GeneralSettingsData data = new GeneralSettingsData
        {
            AutomaticallyUseNewestVideos = collapsibleGroupBoxNewestVideo.IsVisibleContent && this.SelectedPath != "",
            PathToNewestVideos = collapsibleGroupBoxNewestVideo.IsVisibleContent ? this.SelectedPath : "",
            LatestTimeWatched = DateTime.Now
        };

        SettingsFolder.Save(data);
    }

    private void DeletePathButton_OnClick(object sender, RoutedEventArgs e)
    {
        this.SelectedPath = "";
    }
}
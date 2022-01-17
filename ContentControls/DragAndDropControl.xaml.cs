using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using VideoCompressorGUI.Settings;
using VideoCompressorGUI.Utils;

namespace VideoCompressorGUI.ContentControls;

public partial class DragAndDropControl : UserControl
{
    private Mp4FileValidator validator = new();
    
    public DragAndDropControl()
    {
        InitializeComponent();

        Dispatcher.DelayInvoke(() =>
        {
            var loadedGeneralSettings = SettingsFolder.Load<GeneralSettingsData>();
            
            if (loadedGeneralSettings.AutomaticallyUseNewestVideos)
            {
                List<string> files = loadedGeneralSettings.FetchNewFiles();
                
                loadedGeneralSettings.LatestTimeWatched = DateTime.Now;
                SettingsFolder.Save(loadedGeneralSettings);
            
                if (files.Count != 0)
                    HandleFiles(files);
            }
            
            // HandleFiles(new List<string>
            // {
            //     "F:/Videos/Valorant/smart teleport.mp4",
            //     "F:/Videos/Valorant/absolute clean C hold.mp4",
            //     "F:/Videos/Valorant/buggy flash.mp4",
            //     "F:/Videos/Valorant/juicy.mp4"
            // });
            //
            // HandleFiles(new List<string>
            // {
            //     "F:/Videos/Testing/test.mp4",
            // });
        }, TimeSpan.FromMilliseconds(1));
    }

    private void HandleFiles(List<string> files)
    {
        bool wasInvalid = false;
        for (int i = 0; i < files.Count; i++)
        {
            if (!this.validator.Validate(files[i]))
            {
                this.snackbar.IsActive = true;
                var extension = Path.GetExtension(files[i]);
                this.snackbar.MessageQueue.DiscardDuplicates = true;

                this.snackbar.MessageQueue.Enqueue(extension + " not supported", null, null, null, false, false,
                    TimeSpan.FromSeconds(1.5));
                wasInvalid = true;
                break;
            }
        }

        if (!wasInvalid)
        {
            ((MainWindow)Application.Current.MainWindow).PushContentControl(new VideoEditorControl(files));
        }
    }

    private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        var dialog = new VistaOpenFileDialog
        {
            Multiselect = true,
            Filter = "Supported video formats (*.mp4)|*.mp4",
        };

        if ((bool)dialog.ShowDialog(Window.GetWindow(this)))
        {
            var selectedFiles = dialog.FileNames;
            HandleFiles(selectedFiles.ToList());
        }
    }

    private void UIElement_OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            HandleFiles(droppedFiles.ToList());
        }
    }
}
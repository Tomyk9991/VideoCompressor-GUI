using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using VideoCompressorGUI.Utils;

namespace VideoCompressorGUI.ContentControls;

public partial class DragAndDropControl : UserControl
{
    private Mp4FileValidator validator = new();
    private ContentControl contentControl;
    
    public DragAndDropControl(ContentControl contentControl)
    {
        InitializeComponent();
        this.contentControl = contentControl;
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
            VideoEditorControl editor = new VideoEditorControl(this.contentControl, files);
            this.contentControl.Content = editor;
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

    private async void UIElement_OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            HandleFiles(droppedFiles.ToList());
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using Unosquare.FFME;
using VideoCompressorGUI.ContentControls.Components;
using VideoCompressorGUI.SettingsLoadables;
using VideoCompressorGUI.Utils;
using VideoCompressorGUI.Utils.Logger;

namespace VideoCompressorGUI.ContentControls
{
    public partial class DragAndDropWindowControl : UserControl
    {
        private Mp4FileValidator validator = new();

        public DragAndDropWindowControl()
        {
            InitializeComponent();
        }

        private void DragAndDropWindowControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void Init()
        {
            Dispatcher.DelayInvoke(() =>
            {
                var loadedGeneralSettings = SettingsFolder.Load<GeneralSettingsData>();
                
                GeneralSettingsData.ValidateFFmpegPath(loadedGeneralSettings.FFmpegPath);
                
                if (GeneralSettingsData.ValidateFFmpegPath(loadedGeneralSettings.FFmpegPath).Count > 0)
                {
                    ((MainWindow)Application.Current.MainWindow).PushContentControl(new FailLoad(), false);
                    return;
                }

                Log.Info($"Loading ffmpeg from path: {loadedGeneralSettings.FFmpegPath}");
                Library.FFmpegDirectory = loadedGeneralSettings.FFmpegPath;
                Task.Run(async () => {await Library.LoadFFmpegAsync();}).GetAwaiter().GetResult();
                
                if (loadedGeneralSettings.AutomaticallyUseNewestVideos)
                {
                    List<string> files = loadedGeneralSettings.FetchNewFiles();
                
                    if (files.Count != 0)
                        HandleFiles(files);
                }
                
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
                ((MainWindow)Application.Current.MainWindow).PushContentControl(new VideoEditorControl(files), true);
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
}
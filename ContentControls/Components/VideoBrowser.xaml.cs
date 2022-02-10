using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using FFmpeg.NET;
using Ookii.Dialogs.Wpf;
using VideoCompressorGUI.ffmpeg;
using VideoCompressorGUI.SettingsLoadables;
using VideoCompressorGUI.Utils;
using VideoCompressorGUI.Utils.Logger;

namespace VideoCompressorGUI.ContentControls.Components
{
    public partial class VideoBrowser : UserControl
    {
        private HashSet<string> files = new();
        private ObservableCollection<VideoFileMetaData> videoFileMetaDatas = new();

        private object syncLock = new();

        private VideoFileMetaData currentlyContextMenuOpen;

        private readonly Mp4FileValidator validator = new();
        private readonly Compressor compressor = new();
        private List<FileSystemWatcherReferenceCounter> fileSystemWatchers = new();

        private VideoBrowserItemTemplate template;
        
        public event Action<VideoFileMetaData> OnSelectionChanged;

        public VideoBrowser()
        {
            InitializeComponent();

            LoadTemplate();
            
            BindingOperations.EnableCollectionSynchronization(videoFileMetaDatas, syncLock);
            TempFolder.ClearOnTimeExpired();

            this.listboxFiles.ItemsSource = videoFileMetaDatas;
        }
        
        private void VideoBrowser_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.LoadTemplate();
        }

        private void LoadTemplate()
        {
            template = SettingsFolder.Load<VideoBrowserItemTemplate>();
            foreach (var vid in videoFileMetaDatas)
            {
                vid.ShowButtonField = template.BitField;
            }
        }

        private void ListboxFiles_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VideoFileMetaData association = (sender as ListBox).SelectedItem as VideoFileMetaData;
            
            currentlyContextMenuOpen = association;

            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.OnSelectionChanged?.Invoke(association);
        }
        
        private void ListboxFiles_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                HandleFiles(droppedFiles);
            }
        }

        private void MenuItemOnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (!this.currentlyContextMenuOpen.CompressData.IsCompressing)
                RemoveItem(this.currentlyContextMenuOpen);
        }
        
        private void DeleteListItem_OnClick(object sender, RoutedEventArgs e)
        {
            VideoFileMetaData tag = (VideoFileMetaData)((Button)sender).Tag;
            RemoveItem(tag);
        }
        
        private void FolderListItem_OnClick(object sender, RoutedEventArgs e)
        {
            UtilMethods.OpenExplorerAndSelectFile((string)((Button)sender).Tag);
            e.Handled = true;
        }

        public void UpdateSource(List<string> newFiles)
        {
            loadingProgressBar.Visibility = Visibility.Visible;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (_, _) =>
            {
                ExtractMetaData(newFiles).GetAwaiter().GetResult();
            };

            worker.RunWorkerCompleted += (_, _) =>
            {
                loadingProgressBar.Visibility = Visibility.Collapsed;
                dragAndDrop.Visibility = this.listboxFiles.Items.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
                
                CreateSystemFileWatchers(newFiles);
                
                if (videoFileMetaDatas.Count == 1)
                {
                    this.listboxFiles.SelectedIndex = 0;
                    this.OnSelectionChanged?.Invoke(videoFileMetaDatas[0]);
                }
            };
            
            worker.RunWorkerAsync();
        }

        private void CreateSystemFileWatchers(List<string> newFiles)
        {
            foreach (string newFile in newFiles)
            {
                bool needAdd = true;

                for (int i = 0; i < fileSystemWatchers.Count; i++)
                {
                    if (fileSystemWatchers[i].Watcher.Path == Path.GetDirectoryName(newFile))
                    {
                        needAdd = false;
                        fileSystemWatchers[i].Increase(newFile);
                        break;
                    }
                }

                if (needAdd)
                {
                    var path = Path.GetDirectoryName(newFile);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Log.Info("Adding watcher to: " + path);
                    Console.ResetColor();
                    FileSystemWatcher watcher = new FileSystemWatcher(path);

                    watcher.Filter = "*.mp4";
                    watcher.EnableRaisingEvents = true;

                    fileSystemWatchers.Add(new FileSystemWatcherReferenceCounter(watcher, newFile));

                    watcher.Deleted += (_, args) =>
                    {
                        var watcherToRemove =
                            fileSystemWatchers.FirstOrDefault(t => t.Watcher.Path == Path.GetDirectoryName(args.FullPath));

                        if (watcherToRemove == null) return;
                        
                        int refCount = watcherToRemove.Decrease(args.FullPath);

                        Dispatcher.Invoke(() =>
                        {
                            VideoFileMetaData metaData = videoFileMetaDatas.FirstOrDefault(t => t.File == args.FullPath);
                            RemoveItem(metaData, false);
                        });
                        
                        if (refCount == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Log.Info("Remove watcher: " + watcherToRemove.Watcher.Path);
                            Console.ResetColor();

                            fileSystemWatchers.Remove(watcherToRemove);
                        }
                    };
                }
            }
        }

        private async Task ExtractMetaData(List<string> newFiles)
        {
            if (newFiles == null) return;
            
            List<Task> tasks = new List<Task>();
            foreach (string file in newFiles)
            {
                this.files.Add(file);
                tasks.Add(Task.Run(async () =>
                {
                    var thumbnail = new List<Task<string>> { compressor.GetThumbnail(file) };
                    var metaData = new List<Task<MetaData>> { compressor.GetMetaData(file) };
                    DateTime now = DateTime.Now;
                
                    var nonGenericTasks = thumbnail.Concat(metaData.Cast<Task>());
                    await Task.WhenAll(nonGenericTasks);

                    lock (syncLock)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            bool needAdd = videoFileMetaDatas.All(data => data.File != file);

                            if (needAdd)
                                videoFileMetaDatas.Add(new VideoFileMetaData(file, thumbnail[0].Result, metaData[0].Result, now, template.BitField));
                        });
                    }
                }));
            }
            
            await Task.WhenAll(tasks);
        }

        private void HandleFiles(string[] newFiles)
        {
            bool wasInvalid = false;
            for (int i = 0; i < newFiles.Length; i++)
            {
                if (!this.validator.Validate(newFiles[i]))
                {
                    this.snackbar.IsActive = true;
                    string extension = Path.GetExtension(newFiles[i]);
                    this.snackbar.MessageQueue.DiscardDuplicates = true;

                    this.snackbar.MessageQueue.Enqueue(extension + " " + Properties.Resources.NotSupported, null, null, null, false, false,
                        TimeSpan.FromSeconds(1.5));
                    wasInvalid = true;
                    break;
                }
            }

            if (!wasInvalid)
            {
                List<string> filteredStrings = new List<string>();
                
                foreach (var file in newFiles)
                {
                    if (!this.files.Contains(file))
                    {
                        filteredStrings.Add(file);
                        this.files.Add(file);
                    }
                }

                this.UpdateSource(filteredStrings);
            }
        }

        /// <summary>Removes the item from the list</summary>
        /// <returns>The amount of items in the list after the remove</returns>
        public int RemoveItem(VideoFileMetaData target, bool checkWatchers = true)
        {
            if (target == null)
            {
                return this.videoFileMetaDatas.Count;
            }

            if (checkWatchers)
            {
                var watcherToRemove = fileSystemWatchers.FirstOrDefault(t => t.Watcher.Path == Path.GetDirectoryName(target.File));
                int? refCount = watcherToRemove?.Decrease(target.File);
                
                if (refCount is 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Log.Info("Remove watcher: " + watcherToRemove.Watcher.Path);
                    Console.ResetColor();
                    
                    fileSystemWatchers.Remove(watcherToRemove);
                }
            }
            
            this.files.Remove(target.File);
            this.videoFileMetaDatas.Remove(target);

            dragAndDrop.Visibility = this.listboxFiles.Items.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

            var currentlySelected = (VideoFileMetaData) listboxFiles.SelectedItem;
            
            if (currentlySelected == target || currentlySelected == null)
            {
                this.OnSelectionChanged?.Invoke(null);
            }
            
            return this.videoFileMetaDatas.Count;
        }

        private void BrowseFile_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var dialog = new VistaOpenFileDialog
            {
                Multiselect = true,
                Filter = "Supported video formats (*.mp4)|*.mp4",
            };

            if ((bool)dialog.ShowDialog(Window.GetWindow(this)))
            {
                var selectedFiles = dialog.FileNames;
                HandleFiles(selectedFiles);
            }
        }
    }
}


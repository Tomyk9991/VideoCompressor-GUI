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
using VideoCompressorGUI.ffmpeg;
using VideoCompressorGUI.Utils;
using VideoCompressorGUI.Utils.Logger;

namespace VideoCompressorGUI.ContentControls.Components
{
    public partial class VideoBrowser : UserControl
    {
        private HashSet<string> files = new();
        private ObservableCollection<VideoFileMetaData> videoFileMetaData = new();

        private object syncLock = new();

        private VideoFileMetaData currentlyContextMenuOpen = null;

        private readonly Mp4FileValidator validator = new();
        private readonly Compressor compressor = new();
        private List<FileSystemWatcherReferenceCounter> fileSystemWatchers = new();
        
        public event Action<VideoFileMetaData> OnSelectionChanged;

        public VideoBrowser()
        {
            InitializeComponent();
            
            BindingOperations.EnableCollectionSynchronization(videoFileMetaData, syncLock);
            TempFolder.ClearOnTimeExpired();
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

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            RemoveItem(this.currentlyContextMenuOpen);
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
            worker.DoWork += (s, e) =>
            {
                ExtractMetaData(newFiles).GetAwaiter().GetResult();
            };

            worker.RunWorkerCompleted += (_, _) =>
            {
                loadingProgressBar.Visibility = Visibility.Collapsed;
                this.listboxFiles.ItemsSource = Array.Empty<Object>();

                videoFileMetaData = new ObservableCollection<VideoFileMetaData>(videoFileMetaData.OrderByDescending(t1 => t1.CreatedOn));
                this.listboxFiles.ItemsSource = videoFileMetaData;

                CreateSystemFileWatchers(newFiles);
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
                        fileSystemWatchers[i].Increase();
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

                    fileSystemWatchers.Add(new FileSystemWatcherReferenceCounter(watcher));

                    watcher.Deleted += (sender, args) =>
                    {
                        var watcherToRemove =
                            fileSystemWatchers.FirstOrDefault(t => t.Watcher.Path == Path.GetDirectoryName(args.FullPath));

                        if (watcherToRemove == null) return;
                        
                        uint refCount = watcherToRemove.Decrease();

                        Dispatcher.Invoke(() =>
                        {
                            VideoFileMetaData metaData = videoFileMetaData.FirstOrDefault(t => t.File == args.FullPath);
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
                            bool needAdd = videoFileMetaData.All(data => data.File != file);

                            if (needAdd)
                                videoFileMetaData.Add(new VideoFileMetaData(file, thumbnail[0].Result, metaData[0].Result, now));
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

                    this.snackbar.MessageQueue.Enqueue(extension + " not supported", null, null, null, false, false,
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
                return this.videoFileMetaData.Count;
            }

            if (checkWatchers)
            {
                var watcherToRemove = fileSystemWatchers.FirstOrDefault(t => t.Watcher.Path == Path.GetDirectoryName(target.File));
                uint? refCount = watcherToRemove?.Decrease();
                
                if (refCount is 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Log.Info("Remove watcher: " + watcherToRemove.Watcher.Path);
                    Console.ResetColor();
                    
                    fileSystemWatchers.Remove(watcherToRemove);
                }
            }
            
            this.currentlyContextMenuOpen = target;

            this.files.Remove(this.currentlyContextMenuOpen.File);
            this.videoFileMetaData.Remove(this.currentlyContextMenuOpen);

            this.currentlyContextMenuOpen = null;

            videoFileMetaData = new ObservableCollection<VideoFileMetaData>(videoFileMetaData.OrderByDescending(t1 => t1.CreatedOn));

            this.listboxFiles.ItemsSource = Array.Empty<Object>();
            this.listboxFiles.ItemsSource = videoFileMetaData;
            
            this.OnSelectionChanged?.Invoke(null);
            return this.videoFileMetaData.Count;
        }
    }
}


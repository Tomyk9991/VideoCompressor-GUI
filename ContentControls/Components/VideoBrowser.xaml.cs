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
using ffmpegCompressor;
using VideoCompressorGUI.Utils;

namespace VideoCompressorGUI.ContentControls.Components;

public partial class VideoBrowser : UserControl
{
    private HashSet<string> files = new();
    private ObservableCollection<VideoFileMetaData> videoFileMetaData = new();

    private object syncLock = new();

    private VideoFileMetaData currentlyContextMenuOpen = null;

    private readonly Mp4FileValidator validator = new();
    private readonly Compressor compressor = new();
    
    public event Action<VideoFileMetaData> OnSelectionChanged;

    public VideoBrowser()
    {
        InitializeComponent();
        
        BindingOperations.EnableCollectionSynchronization(videoFileMetaData, syncLock);
        TempFolder.Clear();
    }
    
    public void Initialize(List<string> newFiles)
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
        };
        
        worker.RunWorkerAsync();
    }

    public void UpdateSource(List<string> files)
    {
        Initialize(files);
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
                        videoFileMetaData.Add(new VideoFileMetaData(file, thumbnail[0].Result, metaData[0].Result, now));
                    });
                }
            }));
        }
        
        await Task.WhenAll(tasks);
    }
    
    private void ListboxFiles_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        VideoFileMetaData association = (sender as ListBox).SelectedItem as VideoFileMetaData;
        
        currentlyContextMenuOpen = association;

        if (Mouse.LeftButton == MouseButtonState.Pressed)
            this.OnSelectionChanged?.Invoke(association);
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

    /// <summary>
    /// Removes the item from the list
    /// </summary>
    /// <param name="target"></param>
    /// <returns>The amount of items in the list after the remove</returns>
    public int RemoveItem(VideoFileMetaData target)
    {
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

    private void FolderListItem_OnClick(object sender, RoutedEventArgs e)
    {
        UtilMethods.OpenExplorerAndSelectFile((string)((Button)sender).Tag);
        e.Handled = true;
    }
}
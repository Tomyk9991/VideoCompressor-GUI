using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using FFmpeg.NET;
using ffmpegCompressor;
using VideoCompressorGUI.Utils;

namespace VideoCompressorGUI.ContentControls.Components;

public partial class VideoBrowser : UserControl
{
    private List<string> files = new();
    private List<VideoFileMetaData> videoFileMetaData = new();

    private VideoFileMetaData currentlyContextMenuOpen = null;

    private readonly Mp4FileValidator validator = new();
    private readonly Compressor compressor = new();
    
    public event Action<VideoFileMetaData> OnSelectionChanged;
    
    private readonly DispatcherTimer ticker = new()
    {
        Interval = TimeSpan.FromSeconds(0.33)
    };
    
    public VideoBrowser()
    {
        InitializeComponent();
    }
    
    public void Initialize(List<string> newFiles)
    {
        progressBar.Visibility = Visibility.Visible;
        ticker.Tick -= (o, args) => { };
        ticker.Tick += (o, e) =>
        {
            this.listboxFiles.ItemsSource = Array.Empty<Object>();
            this.listboxFiles.ItemsSource = videoFileMetaData;
        };
        
        ticker.Start();
        
        BackgroundWorker worker = new BackgroundWorker();
        worker.DoWork += (s, e) =>
        {
            ExtractMetaData(newFiles).GetAwaiter().GetResult();

            Console.WriteLine("Finished importing...");
            Dispatcher.Invoke(() =>
            {
                progressBar.Visibility = Visibility.Collapsed;
                this.listboxFiles.ItemsSource = Array.Empty<Object>();
                
                videoFileMetaData.Sort((t1, t2) => t2.CreatedOn.CompareTo(t1.CreatedOn));
                this.listboxFiles.ItemsSource = videoFileMetaData;
            });
            
            this.ticker.Stop();
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

                lock (videoFileMetaData)
                {
                    videoFileMetaData.Add(new VideoFileMetaData(file, thumbnail[0].Result, metaData[0].Result, now));
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
        this.files.Remove(this.currentlyContextMenuOpen.File);
        this.videoFileMetaData.Remove(this.currentlyContextMenuOpen);

        this.currentlyContextMenuOpen = null;
        
        videoFileMetaData.Sort((t1, t2) => t2.CreatedOn.CompareTo(t1.CreatedOn));
        
        this.listboxFiles.ItemsSource = Array.Empty<Object>();
        this.listboxFiles.ItemsSource = videoFileMetaData;
    }
}
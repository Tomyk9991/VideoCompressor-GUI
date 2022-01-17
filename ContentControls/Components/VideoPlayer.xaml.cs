using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using VideoCompressorGUI.Keybindings;
using VideoCompressorGUI.Utils;

namespace VideoCompressorGUI.ContentControls.Components;

public partial class VideoPlayer : UserControl
{
    private VideoFileMetaData currentlySelectedVideo;

    private DispatcherTimer timerVideoTime = new()
    {
        Interval = TimeSpan.FromSeconds(0.1)
    };

    private bool isPlayingVideo = false;
    private double playbackSpeed = 1.0d;

    public VideoPlayer()
    {
        InitializeComponent();

        PlaybackSpeedKeyBinding pbsKB = new PlaybackSpeedKeyBinding(this.snackbarNotifier, this.videoPlayer);
        ResumeHaltKeyBinding rhKB = new ResumeHaltKeyBinding(TogglePlayPause);

        MainWindow instance = (MainWindow)Application.Current.MainWindow;
        instance.OnKeyPressed += pbsKB.OnKeybinding;
        instance.OnKeyPressed += rhKB.OnKeybinding;

        this.videoPlaybackSlider.BlockValueOverrideOnDrag = true;
        this.videoPlaybackSlider.OnEndedMainDrag += d =>
        {
            OnPlaybackMainValueChanged(d);
        };
        this.videoPlaybackSlider.OnUpperThumbChanged += (d) =>
        {
            this.currentlySelectedVideo.CutSeek.End = d * currentlySelectedVideo.MetaData.Duration;
            UpdateDistanceForTextSpan(d);
        };

        this.videoPlaybackSlider.OnLowerThumbChanged += (d) =>
        {
            UpdateDistanceForTextSpan(d);

            this.currentlySelectedVideo.CutSeek.Start = d * currentlySelectedVideo.MetaData.Duration;

            if (isPlayingVideo)
            {
                TogglePlayPause();
            }
            
            OnPlaybackMainValueChanged(d, false);
        };

        this.videoPlayerParent.Visibility = Visibility.Hidden;
    }

    private void VideoPlayer_OnLoaded(object sender, RoutedEventArgs e)
    {
        SetVolume(50.0d);
    }


    private void Element_MediaEnded(object sender, RoutedEventArgs e)
    {
        // Wird nur ausgeführt, wenn das Element auf natürliche Art fertiggestellt wird
        UpperVideoThumbLimitReached();
    }

    #region Resume / Pause

    private void VideoPlayer_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        TogglePlayPause();
    }

    private void TogglePlayPause()
    {
        if (isPlayingVideo)
            videoPlayer.PauseAnimated(Dispatcher, playPauseIcon);
        else
            videoPlayer.PlayAnimated(Dispatcher, playPauseIcon);

        videoPlayer.Focus();
        isPlayingVideo = !isPlayingVideo;
    }

    #endregion

    public void UpdateSource(VideoFileMetaData association)
    {
        if (association == null || this.currentlySelectedVideo == association)
        {
            this.videoPlayer.Source = null;
            videoPlayerParent.Visibility = Visibility.Collapsed;
            timerVideoTime.Tick -= (o, args) => { };
            
            return;
        }
        
        

        this.currentlySelectedVideo = association;
        this.videoPlayer.Source = new Uri(association.File);

        isPlayingVideo = false;
        TogglePlayPause();

        Dispatcher.DelayInvoke(() => { videoPlayer.Focus(); }, TimeSpan.FromMilliseconds(100));

        textblockTotalTime.Text = association.MetaData.Duration.TotalSeconds.ToMinutesAndSecondsFromSeconds();
        UpdateDistanceForTextSpan(0.0d);
        
        timerVideoTime.Tick -= (o, args) => { };
        timerVideoTime.Tick += OnTimerTick;
        timerVideoTime.Start();

        videoPlayerParent.Visibility = Visibility.Visible;
    }

    #region Video playback

    private void UpdateDistanceForTextSpan(double _)
    {
        // calculate percentage value to duration
        double lowerTime = this.videoPlaybackSlider.LowerThumb * currentlySelectedVideo.MetaData.Duration.TotalSeconds;
        double upperTime = this.videoPlaybackSlider.UpperThumb * currentlySelectedVideo.MetaData.Duration.TotalSeconds;

        double distance = (upperTime - lowerTime);
        this.videoPlaybackSlider.spanTextBlock.Text = distance >= 1 
            ? distance.ToMinutesAndSecondsFromSeconds() 
            : distance.ToMilliSecondsFromSeconds();
    }
    
    //gets called from videorangeslider, when user drags the main value
    private void OnPlaybackMainValueChanged(double percentage, bool shouldToggle = true)
    {
        videoPlayer.Position = TimeSpan.FromMilliseconds(percentage * currentlySelectedVideo.MetaData.Duration.TotalMilliseconds);

        if (shouldToggle && !isPlayingVideo)
            TogglePlayPause();
    }

    //Wird aufgerufen, unabhängig davon, ob das Video zu Ende ist, oder der Thumb erreicht wurde
    private void UpperVideoThumbLimitReached()
    {
        double startInMilliseconds = videoPlaybackSlider.LowerThumb *
                                     currentlySelectedVideo.MetaData.Duration.TotalMilliseconds;
        
        this.videoPlayer.Position = TimeSpan.FromMilliseconds(startInMilliseconds);
    }

    private void OnTimerTick(object sender, EventArgs e)
    {
        if (videoPlayer.NaturalDuration.HasTimeSpan &&
            videoPlayer.NaturalDuration.TimeSpan.TotalSeconds > 0)
        {
            videoPlaybackSlider.Value = (videoPlayer.Position /
                                        currentlySelectedVideo.MetaData.Duration);

            textblockPlayedTime.Text = videoPlayer.Position.TotalSeconds.ToMinutesAndSecondsFromSeconds();
            
            double endInMilliseconds = videoPlaybackSlider.UpperThumb *
                                       currentlySelectedVideo.MetaData.Duration.TotalMilliseconds;
            
            if ((int)videoPlayer.Position.TotalMilliseconds >= (int) endInMilliseconds)
            {
                UpperVideoThumbLimitReached();
            }
        }
    }

    #endregion

    #region Sound

    private void SoundVolumeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateVolumeLabel(e.NewValue);
    }

    private void SetVolume(double newValue)
    {
        UpdateVolumeLabel(newValue);
        soundVolumeSlider.Value = newValue;
    }

    private void UpdateVolumeLabel(double newValue)
    {
        if (soundVolumeSlider != null && volumeTextBlock != null)
        {
            double actualWidth = soundVolumeSlider.ActualWidth;
            double percentage = newValue / 100.0d;
            double value = percentage * (actualWidth - volumeTextBlock.ActualWidth);


            volumeTextBlock.Margin = new Thickness(value, 0, 0, 0);

            volumeTextBlock.Text = (int)newValue + "%";
            videoPlayer.Volume = percentage;
        }
    }

    #endregion
}
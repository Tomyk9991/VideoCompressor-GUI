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

    private DateTime latestSelectionStartChange = DateTime.Now;
    

    private bool isPlayingVideo = false;
    private double playbackSpeed = 1.0d;
    private bool shouldUpdatePlayback = true;

    public VideoPlayer()
    {
        InitializeComponent();

        PlaybackSpeedKeyBinding pbsKB = new PlaybackSpeedKeyBinding(this.snackbarNotifier, this.videoPlayer);
        ResumeHaltKeyBinding rhKB = new ResumeHaltKeyBinding(TogglePlayPause);
        MainWindow.OnKeyPressed += pbsKB.OnKeybinding;
        MainWindow.OnKeyPressed += rhKB.OnKeybinding;


        this.videoPlayerParent.Visibility = Visibility.Hidden;
    }

    private void VideoPlayer_OnLoaded(object sender, RoutedEventArgs e)
    {
        SetVolume(50.0d);
    }


    private void Element_MediaEnded(object sender, RoutedEventArgs e)
    {
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

    #region Timeline

    private void Timeline_OnLowerValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        => RangeSliderValueChanged(e.NewValue, textBlockLowerValue, true);

    private void Timeline_OnUpperValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        => RangeSliderValueChanged(e.NewValue, textBlockUpperValue, false);

    private void RangeSliderValueChanged(double newValue, TextBlock target, bool isLower)
    {
        if (target != null)
        {
            if (isLower)
            {
                this.videoPlaybackSlider.SelectionStart = newValue / 100.0;
                
                double startInSeconds = videoPlaybackSlider.SelectionStart *
                                        currentlySelectedVideo.MetaData.Duration.TotalSeconds;

                double endInSeconds = videoPlaybackSlider.SelectionEnd *
                                      currentlySelectedVideo.MetaData.Duration.TotalSeconds;

                if (startInSeconds != endInSeconds && DateTime.Now - latestSelectionStartChange > TimeSpan.FromMilliseconds(125))
                {
                    latestSelectionStartChange = DateTime.Now;
                    this.videoPlayer.Position = TimeSpan.FromSeconds(startInSeconds);
                }
            }
            else
                this.videoPlaybackSlider.SelectionEnd = newValue / 100.0;

            target.Text = ConvertPercentageToClockOutput(newValue / 100.0);
        }
    }

    private string ConvertPercentageToClockOutput(double percentage)
    {
        if (this.currentlySelectedVideo != null)
        {
            double totalSeconds = this.currentlySelectedVideo.MetaData.Duration.TotalSeconds;
            return (percentage * totalSeconds).ToMinutesAndSecondsFromSeconds();
        }

        return "";
    }

    #endregion

    public void UpdateSource(VideoFileMetaData association)
    {
        if (association == null || this.currentlySelectedVideo == association) return;

        this.currentlySelectedVideo = association;
        this.videoPlayer.Source = new Uri(association.File);

        textBlockLowerValue.Text = 0.0d.ToMinutesAndSecondsFromSeconds();
        textBlockUpperValue.Text =
            this.currentlySelectedVideo.MetaData.Duration.TotalSeconds.ToMinutesAndSecondsFromSeconds();

        isPlayingVideo = false;
        TogglePlayPause();

        Dispatcher.DelayInvoke(() => { videoPlayer.Focus(); }, TimeSpan.FromMilliseconds(100));

        textblockTotalTime.Text = association.MetaData.Duration.TotalSeconds.ToMinutesAndSecondsFromSeconds();

        timerVideoTime.Tick -= (o, args) => { };
        timerVideoTime.Tick += OnTimerTick;
        timerVideoTime.Start();

        videoPlayerParent.Visibility = Visibility.Visible;
    }

    #region Video playback

    private void OnTimerTick(object sender, EventArgs e)
    {
        if (shouldUpdatePlayback && videoPlayer.NaturalDuration.HasTimeSpan &&
            videoPlayer.NaturalDuration.TimeSpan.TotalSeconds > 0)
        {
            videoPlaybackSlider.Value = videoPlayer.Position.TotalSeconds /
                                        currentlySelectedVideo.MetaData.Duration.TotalSeconds;


            textblockPlayedTime.Text = videoPlayer.Position.TotalSeconds.ToMinutesAndSecondsFromSeconds();

            double startInSeconds = videoPlaybackSlider.SelectionStart *
                                    currentlySelectedVideo.MetaData.Duration.TotalSeconds;

            double endInSeconds = videoPlaybackSlider.SelectionEnd *
                                  currentlySelectedVideo.MetaData.Duration.TotalSeconds;

            if (startInSeconds != endInSeconds && (int)videoPlayer.Position.TotalSeconds >= (int)endInSeconds)
            {
                this.videoPlayer.Position = TimeSpan.FromSeconds(startInSeconds);
            }
        }
    }

    private void VideoPlaybackSlider_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        shouldUpdatePlayback = false;
    }

    private void VideoPlaybackSlider_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        shouldUpdatePlayback = false;

        videoPlayer.Position =
            TimeSpan.FromSeconds(videoPlaybackSlider.Value * currentlySelectedVideo.MetaData.Duration.TotalSeconds);

        if (!isPlayingVideo)
        {
            this.isPlayingVideo = true;
            videoPlayer.PlayAnimated(Dispatcher, playPauseIcon);
        }

        shouldUpdatePlayback = true;
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
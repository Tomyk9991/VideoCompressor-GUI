using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using Unosquare.FFME.Common;
using VideoCompressorGUI.ffmpeg;
using VideoCompressorGUI.Keybindings;
using VideoCompressorGUI.SettingsLoadables;
using VideoCompressorGUI.Utils;

namespace VideoCompressorGUI.ContentControls.Components
{
    public partial class VideoPlayer : UserControl
    {
        private VideoFileMetaData currentlySelectedVideo;
        private static readonly double updateTimeInterval = 0.05;
        
        private bool isPlayingVideo = false;
        private bool canSeek = true;

        private DispatcherTimer thumbnailTimer;

        public VideoPlayer()
        {
            InitializeComponent();

            var settings = SettingsFolder.Load<GeneralSettingsData>();
            PrepareThumbnailUse(settings.ShowThumbnailForUpperThumb);

            PlaybackSpeedKeyBinding pbsKB = new PlaybackSpeedKeyBinding(this.snackbarNotifier, this.videoPlayer);
            ResumeHaltKeyBinding rhKB = new ResumeHaltKeyBinding(TogglePlayPause);
            LeftMouseClickMouseBinding lmKB = new LeftMouseClickMouseBinding(TogglePlayPause, this.videoPlayer);

            MainWindow instance = (MainWindow)Application.Current.MainWindow;
            instance.OnKeyPressed += pbsKB.OnKeybinding;
            instance.OnKeyPressed += rhKB.OnKeybinding;
            instance.OnMousePressed += lmKB.OnMouseClick;
            

            this.videoPlaybackSlider.BlockValueOverrideOnDrag = true;
            this.videoPlaybackSlider.OnEndedMainDrag += (d) => OnPlaybackMainValueChanged(d, true);
            
            this.videoPlaybackSlider.OnUpperThumbChanged += (d, actualPixelX) =>
            {
                this.currentlySelectedVideo.CutSeek.End = d * currentlySelectedVideo.MetaData.Duration;
                this.videoPlaybackSlider.upperThumbText.Text = (d * currentlySelectedVideo.MetaData.Duration)
                    .TotalSeconds.ToMinutesAndSecondsFromSeconds();

                ValidateLowerUpperThumbDistance(this.currentlySelectedVideo.CutSeek);


                if (settings.ShowThumbnailForUpperThumb)
                {
                    thumbnailPreview.Visibility = Visibility.Visible;
                    thumbnailPreview.Margin = new Thickness(actualPixelX, 0, 0, 35);
                }
                
                if (settings.ShowThumbnailForUpperThumb && !thumbnailPreview.IsSeeking)
                {
                    thumbnailTimer.Stop();
                    thumbnailTimer.Start();
                    thumbnailPreview.Position = d * currentlySelectedVideo.MetaData.Duration;
                }
            };

            this.videoPlaybackSlider.OnLowerThumbChanged += (d) =>
            {
                this.currentlySelectedVideo.CutSeek.Start = d * currentlySelectedVideo.MetaData.Duration;
                this.videoPlaybackSlider.lowerThumbText.Text = (d * currentlySelectedVideo.MetaData.Duration)
                    .TotalSeconds.ToMinutesAndSecondsFromSeconds();

                ValidateLowerUpperThumbDistance(this.currentlySelectedVideo.CutSeek);

                if (isPlayingVideo)
                {
                    TogglePlayPause();
                }

                OnPlaybackMainValueChanged(d, true);
            };

            videoPlayer.MediaOpened += (s, a) =>
            {
                isPlayingVideo = false;
                TogglePlayPause();
            };

            this.videoPlayer.SeekingStarted += (_, _) => { this.canSeek = false; };
            this.videoPlayer.SeekingEnded += (_, _) => { this.canSeek = true; };

            this.videoPlayer.PositionChanged += OnVideoPlayerPositionChanged;

            SetCanUseControlUI(false);

            ((MainWindow)Application.Current.MainWindow).OnWindowClosing += _ => SavePersistentStates();
        }
        
        private void VideoPlayer_OnLoaded(object sender, RoutedEventArgs e)
        {
            var playerCache = SettingsFolder.Load<VideoPlayerCache>();
            var settings = SettingsFolder.Load<GeneralSettingsData>();
            
            PrepareThumbnailUse(settings.ShowThumbnailForUpperThumb);
            SetVolume(playerCache.Volume);
        }

        private void VideoPlayer_OnUnloaded(object sender, RoutedEventArgs e)
        {
            SavePersistentStates();
        }

        private void PrepareThumbnailUse(bool state)
        {
            if (state)
            {
                this.thumbnailTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                    
                thumbnailTimer.Tick += (_, _) =>
                {
                    thumbnailTimer.Stop();
                    thumbnailPreview.Visibility = Visibility.Hidden;
                };

                return;
            }

            this.thumbnailTimer = null;
        }

        private void SetCanUseControlUI(bool state)
        {
            var white = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            var gray = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
            
            this.videoPlaybackSlider.IsEnabled = state;
            
            this.playPauseIcon.IsHitTestVisible = state;
            this.playPauseIcon.Foreground = state ? white : gray;

            resumeStopIcon.IsHitTestVisible = state;
            resumeStopIcon.Foreground = state ? white : gray;
            
            this.textblockPlayedTime.Text = 0.0d.ToMinutesAndSecondsFromSeconds();
            this.textblockPlayedTime.Foreground = state ? white : gray;
            
            this.textblockTotalTime.Text = 0.0d.ToMinutesAndSecondsFromSeconds();
            this.textblockTotalTime.Foreground = state ? white : gray;
            
            this.soundVolumeSlider.IsHitTestVisible = state;
            this.soundVolumeSlider.Foreground = state ? white : gray;
            
            this.volumeIcon.IsHitTestVisible = state;
            this.volumeIconActually.Foreground = state ? white : gray; 
            
            this.toLowerThumbIcon.IsHitTestVisible = state;
            this.toLowerThumbIcon.Foreground = state ? white : gray;
            
            
            this.videoPlaybackSlider.SetEnabledColors(state);

            noVideoSelectedTextBlock.Visibility = state ? Visibility.Hidden : Visibility.Visible;
        }

        private void OnVideoPlayerPositionChanged(object? sender, PositionChangedEventArgs e)
        {
            if (videoPlayer.NaturalDuration is { TotalSeconds: > 0 })
            {
                videoPlaybackSlider.Value = videoPlayer.ActualPosition.Value /
                                            currentlySelectedVideo.MetaData.Duration;

                textblockPlayedTime.Text = videoPlayer.ActualPosition.Value.TotalSeconds.ToMinutesAndSecondsFromSeconds();

                var end = videoPlaybackSlider.UpperThumb *
                          currentlySelectedVideo.MetaData.Duration;

                var timeSpanInterval = TimeSpan.FromSeconds(updateTimeInterval);
                
                if (videoPlayer.ActualPosition.Value + timeSpanInterval >= end)
                    UpperVideoThumbLimitReached();
            }
        }

        private void ValidateLowerUpperThumbDistance(CutStartEndParameter cutStartEndParameter)
        {
            double distance = cutStartEndParameter.End.TotalSeconds - cutStartEndParameter.Start.TotalSeconds;
            bool result = distance >= 1;

            this.videoPlaybackSlider.upperThumbText.Visibility = result ? Visibility.Visible : Visibility.Collapsed;

            if (!result)
                this.videoPlaybackSlider.lowerThumbText.Text = distance.ToMilliSecondsFromSeconds();
        }

        private void SavePersistentStates()
        {
            var playerCache = new VideoPlayerCache(soundVolumeSlider.Value);
            SettingsFolder.Save(playerCache);
        }

        private void Element_MediaEnded(object? sender, EventArgs eventArgs)
        {
            // Wird nur ausgef??hrt, wenn das Element auf nat??rliche Art fertiggestellt wird
            UpperVideoThumbLimitReached();
        }

        #region Resume / Pause


        private void TogglePlayPause()
        {
            if (isPlayingVideo)
                videoPlayer.PauseAnimated(Dispatcher, playPauseIcon);
            else
                videoPlayer.PlayAnimated(Dispatcher, playPauseIcon);

            videoPlayer.Focus();
            isPlayingVideo = !isPlayingVideo;

            resumeStopIcon.Kind = isPlayingVideo ? PackIconKind.Pause : PackIconKind.Play;
        }

        #endregion

        public void UpdateSource(VideoFileMetaData association)
        {
            if (association == null || this.currentlySelectedVideo == association)
            {
                this.videoPlayer.Close();
                SetCanUseControlUI(false);
                return;
            }
            
            SetCanUseControlUI(true);

            this.currentlySelectedVideo = association;
            this.videoPlaybackSlider.ResetThumbs();
            
            this.videoPlayer.Open(new Uri(association.File));
            this.thumbnailPreview.Open(new Uri(association.File));
            
            Dispatcher.DelayInvoke(() => { videoPlayer.Focus(); }, TimeSpan.FromMilliseconds(1000));

            textblockTotalTime.Text = association.MetaData.Duration.TotalSeconds.ToMinutesAndSecondsFromSeconds();

            videoPlayerParent.Visibility = Visibility.Visible;
        }

        #region Video playback

        //gets called from videorangeslider, when user drags the main value
        private void OnPlaybackMainValueChanged(double percentage, bool ignore)
        {
            if (canSeek || ignore)
            {
                videoPlayer.Seek(
                    TimeSpan.FromMilliseconds(percentage * currentlySelectedVideo.MetaData.Duration.TotalMilliseconds));
                
                if(isPlayingVideo) // due to a bug its needed
                    videoPlayer.Play();
            }
        }

        //Wird aufgerufen, unabh??ngig davon, ob das Video zu Ende ist, oder der Thumb erreicht wurde
        private void UpperVideoThumbLimitReached()
        {
            if (canSeek)
            {
                this.videoPlayer.Seek(TimeSpan.FromMilliseconds(videoPlaybackSlider.LowerThumb *
                                                                currentlySelectedVideo.MetaData.Duration
                                                                    .TotalMilliseconds));
            }
        }

        private void ToLowerThumbIcon_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (toLowerThumbIcon.IsHitTestVisible)
            {
                UpperVideoThumbLimitReached();
            }
        }
        #endregion

        #region Sound

        private void SoundVolumeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            videoPlayer.Volume = e.NewValue / 100.0d;;
        }

        private void SetVolume(double newValue)
        {
            soundVolumeSlider.Value = newValue;
        }

        #endregion

        private void ResumeStopIcon_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.resumeStopIcon.IsHitTestVisible)
                TogglePlayPause();
        }

        private void VolumeMouseLeave(object sender, MouseEventArgs e)
        {
            AnimateGridDefinitionColumnWidth(vlmDef, 20, 5, () =>
            {
                soundVolumeSlider.Visibility = Visibility.Collapsed;
                volumeIcon.Visibility = Visibility.Visible;

                vlmDef.Width = new GridLength(5, GridUnitType.Star);
            });
        }

        private void AnimateGridDefinitionColumnWidth(ColumnDefinition def, double from, double to, Action complete)
        {
            var animation = new GridLengthAnimation()
            {
                From = new GridLength(from, GridUnitType.Star),
                To = new GridLength(to, GridUnitType.Star),
                BeginTime = TimeSpan.FromSeconds(0),
                Duration = TimeSpan.FromSeconds(0.1),
                FillBehavior = FillBehavior.Stop
            };

            animation.Completed += (s, e) => { complete(); };

            def.BeginAnimation(ColumnDefinition.WidthProperty, animation);
        }

        private void VolumeCollapsedMouseOver(object sender, MouseEventArgs e)
        {
            AnimateGridDefinitionColumnWidth(vlmDef, 5, 20, () =>
            {
                soundVolumeSlider.Visibility = Visibility.Visible;
                volumeIcon.Visibility = Visibility.Collapsed;

                vlmDef.Width = new GridLength(20, GridUnitType.Star);
            });
        }
    }
}
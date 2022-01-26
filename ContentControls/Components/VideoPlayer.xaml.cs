using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using FFmpeg.AutoGen;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using Unosquare.FFME;
using VideoCompressorGUI.ffmpeg;
using VideoCompressorGUI.Keybindings;
using VideoCompressorGUI.SettingsLoadables;
using VideoCompressorGUI.Utils;

namespace VideoCompressorGUI.ContentControls.Components
{
    public partial class VideoPlayer : UserControl
    {
        private VideoFileMetaData currentlySelectedVideo;

        private DispatcherTimer timerVideoTime = new()
        {
            Interval = TimeSpan.FromSeconds(0.1)
        };

        private bool isPlayingVideo = false;

        public VideoPlayer()
        {
            InitializeComponent();
            
            PlaybackSpeedKeyBinding pbsKB = new PlaybackSpeedKeyBinding(this.snackbarNotifier, this.videoPlayer);
            ResumeHaltKeyBinding rhKB = new ResumeHaltKeyBinding(TogglePlayPause);

            MainWindow instance = (MainWindow)Application.Current.MainWindow;
            instance.OnKeyPressed += pbsKB.OnKeybinding;
            instance.OnKeyPressed += rhKB.OnKeybinding;

            this.videoPlaybackSlider.BlockValueOverrideOnDrag = true;
            this.videoPlaybackSlider.OnEndedMainDrag += d => { OnPlaybackMainValueChanged(d); };
            this.videoPlaybackSlider.OnUpperThumbChanged += (d) =>
            {
                this.currentlySelectedVideo.CutSeek.End = d * currentlySelectedVideo.MetaData.Duration;
                this.videoPlaybackSlider.upperThumbText.Text = (d * currentlySelectedVideo.MetaData.Duration)
                    .TotalSeconds.ToMinutesAndSecondsFromSeconds();

                ValidateLowerUpperThumbDistance(this.currentlySelectedVideo.CutSeek);
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

                OnPlaybackMainValueChanged(d, false);
            };

            videoPlayer.MediaOpened += (s, a) =>
            {
                isPlayingVideo = false;
                TogglePlayPause();
            };
            
            
            this.videoPlayerParent.Visibility = Visibility.Hidden;

            ((MainWindow)Application.Current.MainWindow).OnWindowClosing += _ => SavePersistentStates();
        }

        private void ValidateLowerUpperThumbDistance(CutStartEndParameter cutStartEndParameter)
        {
            double distance = cutStartEndParameter.End.TotalSeconds - cutStartEndParameter.Start.TotalSeconds;
            bool result = distance >= 1;

            this.videoPlaybackSlider.upperThumbText.Visibility = result ? Visibility.Visible : Visibility.Collapsed;

            if (!result)
                this.videoPlaybackSlider.lowerThumbText.Text = distance.ToMilliSecondsFromSeconds();
        }


        private void VideoPlayer_OnLoaded(object sender, RoutedEventArgs e)
        {
            var playerCache = SettingsFolder.Load<VideoPlayerCache>();
            SetVolume(playerCache.Volume);
        }

        private void VideoPlayer_OnUnloaded(object sender, RoutedEventArgs e)
        {
            SavePersistentStates();
        }

        private void SavePersistentStates()
        {
            var playerCache = new VideoPlayerCache(soundVolumeSlider.Value);
            SettingsFolder.Save(playerCache);
        }

        private void Element_MediaEnded(object? sender, EventArgs eventArgs)
        {
            // Wird nur ausgeführt, wenn das Element auf natürliche Art fertiggestellt wird
            UpperVideoThumbLimitReached();
        }

        #region Resume / Pause

        private void VideoPlayer_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("mouse up");
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

            resumeStopIcon.Kind = isPlayingVideo ? PackIconKind.Pause : PackIconKind.Play;
        }

        #endregion

        public void UpdateSource(VideoFileMetaData association)
        {
            if (association == null || this.currentlySelectedVideo == association)
            {
                this.videoPlayer.Close();
                videoPlayerParent.Visibility = Visibility.Collapsed;
                timerVideoTime.Tick -= (o, args) => { };

                return;
            }

            this.currentlySelectedVideo = association;
            this.videoPlaybackSlider.ResetThumbs();


            this.videoPlayer.Open(new Uri(association.File));

            Dispatcher.DelayInvoke(() =>
            {
                videoPlayer.Focus();
            }, TimeSpan.FromMilliseconds(1000));

            textblockTotalTime.Text = association.MetaData.Duration.TotalSeconds.ToMinutesAndSecondsFromSeconds();

            timerVideoTime.Tick -= (o, args) => { };
            timerVideoTime.Tick += OnTimerTick;
            timerVideoTime.Start();

            videoPlayerParent.Visibility = Visibility.Visible;
        }

        #region Video playback

        //gets called from videorangeslider, when user drags the main value
        private void OnPlaybackMainValueChanged(double percentage, bool shouldToggle = true)
        {
            videoPlayer.Position =
                TimeSpan.FromMilliseconds(percentage * currentlySelectedVideo.MetaData.Duration.TotalMilliseconds);

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

        private void ToLowerThumbIcon_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            UpperVideoThumbLimitReached();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            
            if (videoPlayer.NaturalDuration is { TotalSeconds: > 0 })
            {
                videoPlaybackSlider.Value = (videoPlayer.Position /
                                             currentlySelectedVideo.MetaData.Duration);

                textblockPlayedTime.Text = videoPlayer.Position.TotalSeconds.ToMinutesAndSecondsFromSeconds();

                double endInMilliseconds = videoPlaybackSlider.UpperThumb *
                                           currentlySelectedVideo.MetaData.Duration.TotalMilliseconds;

                if ((int)videoPlayer.Position.TotalMilliseconds >= (int)endInMilliseconds)
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

        private void ResumeStopIcon_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            TogglePlayPause();
        }

        private void VolumeMouseLeave(object sender, MouseEventArgs e)
        {
            AnimateGridDefinitionColumnWidth(vlmDef, 20, 5, () =>
            {
                volumeTextBlock.Visibility = Visibility.Collapsed;
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
            
            animation.Completed += (s, e) =>
            {
                complete();
            };
            
            def.BeginAnimation(ColumnDefinition.WidthProperty, animation);
        }

        private void VolumeCollapsedMouseOver(object sender, MouseEventArgs e)
        {
            AnimateGridDefinitionColumnWidth(vlmDef, 5, 20, () =>
            {
                volumeTextBlock.Visibility = Visibility.Visible;
                soundVolumeSlider.Visibility = Visibility.Visible;
                volumeIcon.Visibility = Visibility.Collapsed;
                
                vlmDef.Width = new GridLength(20, GridUnitType.Star);
            });
        }
    }
}
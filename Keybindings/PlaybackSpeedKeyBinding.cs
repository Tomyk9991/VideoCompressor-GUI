using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace VideoCompressorGUI.Keybindings
{
    public class PlaybackSpeedKeyBinding : IKeyBinding
    {
        private DateTime lastSpeedModified = DateTime.Now;
        private double playbackSpeed = 1.0d;

        private Snackbar snackbarNotifier;
        private MediaElement videoPlayer;
    
        public PlaybackSpeedKeyBinding(Snackbar snackbarNotifier, MediaElement videoPlayer)
        {
            this.snackbarNotifier = snackbarNotifier;
            this.videoPlayer = videoPlayer;
        }
    
        public void OnKeybinding(KeyEventArgs e, IInputElement input)
        {
            if (input is MediaElement)
            {
                //shift + .
                if ((Keyboard.Modifiers & ModifierKeys.Shift) > 0 && e.Key == Key.OemPeriod)
                {
                    if (DateTime.Now - lastSpeedModified > TimeSpan.FromSeconds(1))
                    {
                        lastSpeedModified = DateTime.Now;
                        playbackSpeed = Math.Min(playbackSpeed + 0.25d, 2.0d);
                        
                        snackbarNotifier.MessageQueue.Clear();
                        snackbarNotifier.MessageQueue.Enqueue("Wiedergabegeschwindigkeit: " + playbackSpeed.ToString("F"), null, null, null, false, false,
                            TimeSpan.FromSeconds(0.5));
                        
                        videoPlayer.SpeedRatio = playbackSpeed;
                    }
                }
                
                if ((Keyboard.Modifiers & ModifierKeys.Shift) > 0 && e.Key == Key.OemComma)
                {
                    if (DateTime.Now - lastSpeedModified > TimeSpan.FromSeconds(1))
                    {
                        lastSpeedModified = DateTime.Now;

                        playbackSpeed = Math.Max(playbackSpeed - 0.25d, 0.25d);
                        
                        snackbarNotifier.MessageQueue.Clear();
                        snackbarNotifier.MessageQueue.Enqueue("Wiedergabegeschwindigkeit: " + playbackSpeed.ToString("F"), null, null, null, false, false,
                            TimeSpan.FromSeconds(0.5));
                        
                        videoPlayer.SpeedRatio = playbackSpeed;
                    }
                }
            }
        }

    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using FFmpeg.NET;
using MaterialDesignThemes.Wpf;
using VideoCompressorGUI.CompressPresets;

namespace VideoCompressorGUI.Utils;

public static class ClassExtensions
{
    private static int clickedTimes = 0;
    private static object mutex = new object();
    
    public static void PlayAnimated(this MediaElement videoPlayer, Dispatcher dispatcher, PackIcon playPauseIcon)
    {
        playPauseIcon.Width = 100;
        playPauseIcon.Height = 100;
        playPauseIcon.Kind = PackIconKind.Play;
        ToggleHelper(videoPlayer, dispatcher, playPauseIcon, true);
    }

    public static Color LerpTo(this Color from, Color to, double value)
    {
        from.R = (byte)MathHelper.Lerp(from.R, to.R, value);
        from.G = (byte)MathHelper.Lerp(from.G, to.G, value);
        from.B = (byte)MathHelper.Lerp(from.B, to.B, value);

        return from;
    }

    public static ConversionOptions BuildConversionOptions(this Engine ffmpeg, VideoFileMetaData file, CompressPreset preset)
    {
        ConversionOptions options = new ConversionOptions
        {
            VideoBitRate = preset.Bitrate,
            VideoFps = (int) file.MetaData.VideoData.Fps
        };

        return options;
    }

    public static void PauseAnimated(this MediaElement videoPlayer, Dispatcher dispatcher, PackIcon playPauseIcon)
    {
        playPauseIcon.Width = 100;
        playPauseIcon.Height = 100;
        playPauseIcon.Kind = PackIconKind.Pause;
        ToggleHelper(videoPlayer, dispatcher, playPauseIcon, false);
    }

    private static void ToggleHelper(MediaElement videoPlayer, Dispatcher dispatcher, PackIcon playPauseIcon, bool play)
    {
        if (play)
            videoPlayer.Play();
        else
            videoPlayer.Pause();

        Interlocked.Increment(ref clickedTimes);

        if (playPauseIcon.Visibility != Visibility.Visible)
        {
            playPauseIcon.Visibility = Visibility.Visible;
            var animation = new DoubleAnimation
            {
                From = 0.0d,
                To = 1.0d,
                BeginTime = TimeSpan.FromSeconds(0),
                Duration = TimeSpan.FromSeconds(0.1),
                FillBehavior = FillBehavior.Stop
            };
            animation.Completed += (s, e) => playPauseIcon.Opacity = 1.0d;

            playPauseIcon.BeginAnimation(UIElement.OpacityProperty, animation);
        }
        
        dispatcher.DelayInvoke(() =>
        {
            if (clickedTimes == 1)
            {
                playPauseIcon.Visibility = Visibility.Hidden;
            }

            Interlocked.Decrement(ref clickedTimes);
        }, TimeSpan.FromMilliseconds(300));
    }

    public static string ToMilliSecondsFromSeconds(this double value)
    {
        if (value is > 0 and < 1.0d)
        {
            return  (value * 1000.0d).ToString("000") + "ms";
        }

        return "INVALID";
    }
    
    public static string ToMinutesAndSecondsFromSeconds(this double value)
    {
        TimeSpan span = TimeSpan.FromSeconds((int) value);
        return $"{PrefixIfNeeded(span.Minutes)}:{PrefixIfNeeded(span.Seconds)}";
    }
    
    private static string PrefixIfNeeded(int value)
        => value <= 9 ? "0" + value : value.ToString();
    
    
    public static void PrintArray<T>(this T[] a)
    {
        Console.WriteLine("[{0}]", string.Join("\n", a));
    }
    
    public static void PrintArray<T>(this IEnumerable<T> a)
    {
        Console.WriteLine("[{0}]", string.Join("\n", a));
    }

    public static void DelayInvoke(this Dispatcher dispatcher, Action action, TimeSpan timeout)
    {
        Task.Run(() =>
        {
            Thread.Sleep(timeout);
            dispatcher.Invoke(action);
        });
    }
}


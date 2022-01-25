using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FFmpeg.NET;
using MaterialDesignThemes.Wpf;
using VideoCompressorGUI.ffmpeg;
using VideoCompressorGUI.SettingsLoadables;
using Color = System.Windows.Media.Color;

namespace VideoCompressorGUI.Utils
{
    public static class ClassExtensions
    {
        private static int clickedTimes = 0;
    
        /// <summary>
        /// Returns <paramref name="str"/> with the minimal concatenation of <paramref name="ending"/> (starting from end) that
        /// results in satisfying .EndsWith(ending).
        /// </summary>
        /// <example>"hel".WithEnding("llo") returns "hello", which is the result of "hel" + "lo".</example>
        public static string WithEnding(this string str, string ending)
        {
            if (str == null)
                return ending;

            string result = str;

            // Right() is 1-indexed, so include these cases
            // * Append no characters
            // * Append up to N characters, where N is ending length
            for (int i = 0; i <= ending.Length; i++)
            {
                string tmp = result + ending.Right(i);
                if (tmp.EndsWith(ending))
                    return tmp;
            }

            return result;
        }
    
        /// <summary>Gets the rightmost <paramref name="length" /> characters from a string.</summary>
        /// <param name="value">The string to retrieve the substring from.</param>
        /// <param name="length">The number of characters to retrieve.</param>
        /// <returns>The substring.</returns>
        public static string Right(this string value, int length)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", length, "Length is less than zero");
            }

            return (length < value.Length) ? value.Substring(value.Length - length) : value;
        }
    
        public static string BuildNameFromString(this string name)
        {
            if (name.EndsWith(")"))
            {
                int index = name.Length - 2;
                string number = "";
			
                while (char.IsDigit(name[index]))
                {
                    number += name[index];
                    index--;
                }
			
		
                if(name[index] == '(' || (name[index] == '-' && (index - 1) >= 0 && name[index -1] == '(')) {
                    if (name[index] == '-') {
                        number += '-';
                    }
                    number = new string(number.Reverse().ToArray());
                    int pos = int.Parse(number);
                    return name.ReplaceLastOccurrence(number, (pos + 1).ToString());
                }
            }
		
            return name + "(1)";
        }
    
        public static string ReplaceLastOccurrence(this string source, string find, string replace)
        {
            int place = source.LastIndexOf(find);

            if(place == -1)
                return source;

            string result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }
    
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
        
        public static ImageSource ToImageSource(this Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        public static ConversionOptions BuildConversionOptions(this Engine ffmpeg, VideoFileMetaData file, CompressOptions compressOption, CompressPreset preset, TimeSpan snippetLength)
        {
            string extraArguments = compressOption.ExtensionOption.BuildExtraArguments();
            
            ConversionOptions options = new ConversionOptions
            {
                ExtraArguments = extraArguments,
                VideoBitRate = preset.UseTargetSizeCalculation ? preset.CalculateBitrateWithFixedTargetSize(snippetLength.TotalSeconds) : preset.Bitrate,
                VideoFps = (int) file.MetaData.VideoData.Fps,
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
}


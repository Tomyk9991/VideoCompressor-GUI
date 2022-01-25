using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using VideoCompressorGUI.Annotations;
using VideoCompressorGUI.Utils;

namespace ffmpegCompressor
{
    public class CompressData : INotifyPropertyChanged
    {
        [CanBeNull] public event PropertyChangedEventHandler PropertyChanged;
        
        private double _progress;
        private Color _progressColor;
        
        public bool IsCompressing { get; set; }
        
        public double Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(nameof(Progress)); }
        }

        public Color ProgressColor
        {
            get => _progressColor;
            set { _progressColor = value; OnPropertyChanged(nameof(ProgressColor)); }
        }

        public CompressData()
        {
            _progress = 1.2d;
        }
        

        [NotifyPropertyChangedInvocator]
        #nullable enable
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static Color FromPercentage(double percentage)
        {
            percentage = Math.Min(1.0d, Math.Max(percentage, 0.0d));
            Color[] colors = {
                Color.FromRgb(255, 76, 48),
                Color.FromRgb(241, 90, 34),
                Color.FromRgb(46, 204, 113)
            };

            int index = (int) (percentage * colors.Length - 1);
            index = Math.Min(1, Math.Max(index, 0)); // value can be 0, 1

            return colors[Math.Min(2, Math.Max(index, 0))].LerpTo(colors[index + 1], percentage);
        }
    }
}


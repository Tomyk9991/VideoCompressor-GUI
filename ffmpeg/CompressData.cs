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

        public CompressData()
        {
            _progress = 0.0d;
        }
        

        [NotifyPropertyChangedInvocator]
        #nullable enable
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


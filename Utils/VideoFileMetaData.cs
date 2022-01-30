using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FFmpeg.NET;
using ffmpegCompressor;
using VideoCompressorGUI.Annotations;
using VideoCompressorGUI.ffmpeg;
using VideoCompressorGUI.Utils.DataStructures;

namespace VideoCompressorGUI.Utils
{
    public class VideoFileMetaData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        private Bitfield8 _showButtonField;
        public string File { get; set; }
        public string ThumbnailPath { get; set; }
        public MetaData MetaData { get; set; }
        public DateTime CreatedOn { get; set; }
    
        public CompressData CompressData { get; set; }
        public CutStartEndParameter CutSeek { get; set; }

        public Bitfield8 ShowButtonField
        {
            get => _showButtonField;
            set { _showButtonField = value;
                OnPropertyChanged();
            }
        }

        public VideoFileMetaData(string file, string thumbnailPath, MetaData metaData, DateTime createdOn, Bitfield8 showButtonField)
        {
            File = file;
            ThumbnailPath = thumbnailPath;
            MetaData = metaData;
            CreatedOn = createdOn;
            CompressData = new CompressData();
            CutSeek = new CutStartEndParameter(TimeSpan.Zero, metaData.Duration);
            ShowButtonField = showButtonField;
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
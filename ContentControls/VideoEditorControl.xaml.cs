using System.Collections.Generic;
using System.Windows.Controls;
using VideoCompressorGUI.Utils;

namespace VideoCompressorGUI.ContentControls;


public partial class VideoEditorControl : UserControl
{
    private List<string> files;
    private ContentControl contentControl;

    private VideoFileMetaData currentlySelectedVideo;

    public VideoEditorControl(ContentControl contentControl, List<string> files)
    {
        InitializeComponent();
        
        this.contentControl = contentControl;
        this.files = files;

        this.videoBrowser.UpdateSource(files);
        this.videoBrowser.OnSelectionChanged += this.videoPlayer.UpdateSource;
    }
}
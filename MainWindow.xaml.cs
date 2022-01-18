using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.PerformanceData;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VideoCompressorGUI.ContentControls;
using VideoCompressorGUI.ContentControls.Settingspages;
using VideoCompressorGUI.Settings;

namespace VideoCompressorGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public event Action<KeyEventArgs, IInputElement> OnKeyPressed;
        public event Action<SizeChangedEventArgs> OnWindowSizeChanged;

        private Stack<ContentControl> controls = new();

        public MainWindow()
        {
            
            InitializeComponent();
            PushContentControl(new DragAndDropControl());
        }

        public void PushContentControl(ContentControl content)
        {
            controls.Push(content);
            this.contentControl.Content = content;
        }

        public void PopContentControl()
        {
            controls.Pop();
            ContentControl content = controls.Pop();
            PushContentControl(content);
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            IInputElement focusedControl = FocusManager.GetFocusedElement(this);
            OnKeyPressed?.Invoke(e, focusedControl);
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            OnWindowSizeChanged?.Invoke(e);
        }

        private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            GeneralSettingsData data = SettingsFolder.Load<GeneralSettingsData>();
            data.LatestTimeWatched = DateTime.Now;
            SettingsFolder.Save(data);
        }
    }
}

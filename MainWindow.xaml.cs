﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.PerformanceData;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VideoCompressorGUI.ContentControls;
using VideoCompressorGUI.ContentControls.Settingspages.InfoTab;
using VideoCompressorGUI.SettingsLoadables;

namespace VideoCompressorGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public event Action<KeyEventArgs, IInputElement> OnKeyPressed;
        public event Action<MouseButtonEventArgs, IInputElement> OnMousePressed;
        public event Action<SizeChangedEventArgs> OnWindowSizeChanged;
        public event Action<CancelEventArgs> OnWindowClosing;

        private Stack<ContentControl> controls = new();

        public MainWindow()
        {
            
            InitializeComponent();
            AboutSettings.DeleteOldFiles();
            PushContentControl(new DragAndDropWindowControl());
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

        #nullable enable
        private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            GeneralSettingsData data = SettingsFolder.Load<GeneralSettingsData>();
            data.LatestTimeWatched = DateTime.Now;
            SettingsFolder.Save(data);

            OnWindowClosing?.Invoke(e);
        }

        private void MainWindow_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            IInputElement focusedControl = FocusManager.GetFocusedElement(this);
            OnMousePressed?.Invoke(e, focusedControl);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.PerformanceData;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VideoCompressorGUI.ContentControls;
using VideoCompressorGUI.ContentControls.Settingspages;
using VideoCompressorGUI.ContentControls.Settingspages.InfoTab;
using VideoCompressorGUI.SettingsLoadables;
using VideoCompressorGUI.Utils;
using VideoCompressorGUI.Utils.Github;

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

        private Stack<(ContentControl, bool)> controls = new();

        public MainWindow()
        {
            InitializeComponent();
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            
            AboutSettings.DeleteOldFiles();
            
            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly()?.ManifestModule.Name);
            ni.Visible = true;

            iconImage.Source = ni.Icon.ToImageSource();
            
            
            Dispatcher.Invoke(async () =>
            {
                var ghReleaseChecker = new GithubReleaseCheck();
                GithubResponse githubResponse = await ghReleaseChecker.FetchData();


                if (githubResponse == null)
                {
                    hasUpdateNotification.Visibility = Visibility.Collapsed;
                    return;
                }
                
                hasUpdateNotification.Visibility = ghReleaseChecker.Check() ? Visibility.Visible : Visibility.Collapsed;
            });
            
            PushContentControl(new DragAndDropWindowControl(), true);
        }

        public void PushContentControl(ContentControl content, bool showMenubar)
        {
            controls.Push((content, showMenubar));
            this.contentControl.Content = content;

            menubarGrid.Visibility = showMenubar ? Visibility.Visible : Visibility.Collapsed;
        }

        public void PopContentControl()
        {
            controls.Pop();
            var (content, showMenubar) = controls.Pop();
            PushContentControl(content, showMenubar);
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            IInputElement focusedControl = FocusManager.GetFocusedElement(this);
            OnKeyPressed?.Invoke(e, focusedControl);
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.BorderThickness = WindowState == WindowState.Maximized ? new Thickness(6) : new Thickness(0);
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

        private void Minimize_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_OnClick(object sender, RoutedEventArgs e)
        {
            
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuBarSettings_OnClick(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).PushContentControl(new Settings(), false);
        }

        private void MenuBarExit_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuBarPresetsEdit_OnClick(object sender, RoutedEventArgs e)
        {
            ((MainWindow) Application.Current.MainWindow).PushContentControl(new PresetsEditor(), false);
        }

        private void MenuBarOnUpdateNotification_OnClick(object sender, MouseButtonEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).PushContentControl(new Settings(SettingsPage.About), false);
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using VideoCompressorGUI.Settings;

namespace VideoCompressorGUI.ContentControls.Settingspages.GeneralSettingsTab
{
    public partial class GeneralSettings : UserControl
    {
        public static DependencyProperty selectedPathProperty =
            DependencyProperty.Register("SelectedPath", typeof(string), typeof(GeneralSettings));

        public string SelectedPath
        {
            get => (string)GetValue(selectedPathProperty);
            set => SetValue(selectedPathProperty, value);
        }

        public GeneralSettings()
        {
            InitializeComponent();
            var settingsLoad = SettingsFolder.Load<GeneralSettingsData>();

            ApplySettingsLoad(settingsLoad);
        }

        private void ApplySettingsLoad(GeneralSettingsData settings)
        {
            this.collapsibleGroupBoxNewestVideo.IsVisibleContent = settings.AutomaticallyUseNewestVideos;
            this.SelectedPath = settings.PathToNewestVideos;
            this.openExplorerAfterCompressCheckbox.IsChecked = settings.OpenExplorerAfterCompress;
            this.deleteOriginalFileAfterCompressCheckbox.IsChecked = settings.DeleteOriginalFileAfterCompress;
            this.removeFromItemsListAfterCompressCheckbox.IsChecked = settings.RemoveFromItemsList;
            this.openExplorerAfterLastCompressCheckbox.IsChecked = settings.OpenExplorerAfterLastCompression;
        }

        private void SelectedPath_OnClick(object sender, MouseButtonEventArgs e)
        {
            SelectPath();
        }

        private void SelectPathButton_OnClick(object sender, RoutedEventArgs e)
        {
            SelectPath();
        }

        private void SelectPath()
        {
            var dialog = new VistaFolderBrowserDialog();

            if ((bool)dialog.ShowDialog(Window.GetWindow(this)))
            {
                var selectedFolder = dialog.SelectedPath;
                this.SelectedPath = selectedFolder;
            }
        }

        private void GeneralSettings_OnUnloaded(object sender, RoutedEventArgs e)
        {
            GeneralSettingsData data = new GeneralSettingsData
            {
                LatestTimeWatched = DateTime.Now,
                AutomaticallyUseNewestVideos =
                    collapsibleGroupBoxNewestVideo.IsVisibleContent && this.SelectedPath != "",
                PathToNewestVideos = collapsibleGroupBoxNewestVideo.IsVisibleContent ? this.SelectedPath : "",
                OpenExplorerAfterCompress = openExplorerAfterCompressCheckbox.IsChecked.Value,
                DeleteOriginalFileAfterCompress = deleteOriginalFileAfterCompressCheckbox.IsChecked.Value,
                RemoveFromItemsList = removeFromItemsListAfterCompressCheckbox.IsChecked.Value,
                OpenExplorerAfterLastCompression = openExplorerAfterLastCompressCheckbox.IsChecked.Value
            };

            SettingsFolder.Save(data);
        }

        private void DeletePathButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.SelectedPath = "";
        }
    }
}
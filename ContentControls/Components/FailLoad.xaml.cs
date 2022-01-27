using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using VideoCompressorGUI.ContentControls.Settingspages.GeneralSettingsTab;
using VideoCompressorGUI.SettingsLoadables;
using VideoCompressorGUI.Utils.Logger;

namespace VideoCompressorGUI.ContentControls.Components
{
    public partial class FailLoad : UserControl
    {
        private string lastHeader = "";
        public FailLoad(string header = "FFmpeg konnte nicht geladen werden")
        {
            InitializeComponent();
            lastHeader = header;
        }


        private void Settings_OnClick(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).PushContentControl(new Settings());
        }

        private void FailLoad_OnLoaded(object sender, RoutedEventArgs e)
        {
            Log.Warn("Fail Control loaded");
            headerTextBlock.Text = this.lastHeader;

            GeneralSettingsData settings = SettingsFolder.Load<GeneralSettingsData>();
            
            List<string> missingFiles = GeneralSettings.ValidateFFmpegPath(settings.FFmpegPath);
            failMessageTextBlock.Text = missingFiles.Count == 0 ? "" :
                missingFiles.Count == 1 ? "Es fehlt folgende Datei: " + Environment.NewLine + missingFiles[0] :
                "Es fehlen folgende Dateien: " + Environment.NewLine + "    - " + string.Join(Environment.NewLine + "    - ", missingFiles);

            failFirstTextBlock.Text = "Verzeichnis: \'" + settings.FFmpegPath + "'";
            
            if (missingFiles.Count == 0)
            {
                ((MainWindow)Application.Current.MainWindow).PopContentControl();
            }
        }
    }
}
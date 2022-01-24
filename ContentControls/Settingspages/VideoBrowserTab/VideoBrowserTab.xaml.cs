using System;
using System.Windows;
using System.Windows.Controls;
using VideoCompressorGUI.SettingsLoadables;
using VideoCompressorGUI.Utils.DataStructures;

namespace VideoCompressorGUI.ContentControls.Settingspages.VideoBrowserTab
{
    public partial class VideoBrowserTab : UserControl
    {
        private CheckBox[] checkboxes = null;
        public VideoBrowserTab()
        {
            InitializeComponent();
            this.checkboxes = new [] {
                openFolderCheckbox,
                deleteItemCheckbox
            };
            
            InitializeCheckBoxes();
            ((MainWindow)Application.Current.MainWindow).OnWindowClosing += _ => SaveStates();
        }

        private void SaveStates()
        {
            Bitfield8 field = new Bitfield8();
            for (int i = 0; i < checkboxes.Length; i++)
            {
                field[i] = checkboxes[i].IsChecked.Value;
            }
            
            SettingsFolder.Save(new VideoBrowserItemTemplate()
            {
                BitField = field
            });
        }

        private void InitializeCheckBoxes()
        {
            var videoBrowserTemplateItem = SettingsFolder.Load<VideoBrowserItemTemplate>();
            
            for (int i = 0; i < checkboxes.Length; i++)
            {
                checkboxes[i].IsChecked = videoBrowserTemplateItem.BitField[i];
            }
        }

        private void VideoBrowserTab_OnUnloaded(object sender, RoutedEventArgs e)
        {
            SaveStates();
        }

        private void VideoBrowserTab_OnLoaded(object sender, RoutedEventArgs e)
        {
            InitializeCheckBoxes();
        }
    }
}
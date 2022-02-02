using System;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using VideoCompressorGUI.ContentControls.Components;
using VideoCompressorGUI.SettingsLoadables;
using VideoCompressorGUI.Utils;

namespace VideoCompressorGUI.ContentControls.Settingspages
{
    public partial class PresetsEditor : UserControl
    {
        private bool isAdding = true;

        private TextBlock lastPresetTextBlock = null;
        private CompressPreset lastPreset = null;
        private Grid lastParentTextBlockElement = null;

        private Brush originalButtonNewPresetItemColor;

        private CompressPresetCollection compressPresetCollection;

        private CompressPreset lastPresetCopy = null;

        public PresetsEditor()
        {
            InitializeComponent();

            supportedCodecItem.IsEnabled = NvidiaNvencSupport.Supported(); 
            
            originalButtonNewPresetItemColor = buttonNewPresetItem.Background.Clone();
            collapsibleGroupboxPredefinedBitrate.IsVisibleContent = true;

            this.compressPresetCollection = SettingsFolder.Load<CompressPresetCollection>();
            this.nameNotEmptyValidationRule.collection = this.compressPresetCollection;

            for (int i = 0; i < compressPresetCollection.CompressPresets.Count; i++)
            {
                AddTab(compressPresetCollection.CompressPresets[i]);
            }
        }

        private bool ValidateCanUse()
        {
            nameNotEmptyValidationRule.IgnorePreset = lastPreset;

            var r1 = nameNotEmptyValidationRule.Validate(nameTextBox.Text, null);
            var r2 = isDigitValidationRule.Validate(bitrateTextBox.Text, null);
            var r3 = isDigitValidationRule.Validate(targetSizeTextBox.Text, null);

            bool nothingChanged = true;
            
            if (lastPresetCopy != null)
                nothingChanged = lastPresetCopy.Equals(lastPreset);

            bool everythingValid = r1.IsValid &&
                                   (!collapsibleGroupboxPredefinedBitrate.IsVisibleContent || r2.IsValid) &&
                                   (!collapsibleGroupboxCalculatedBitrate.IsVisibleContent || r3.IsValid ||
                                    (collapsibleGroupboxCalculatedBitrate.IsVisibleContent &&
                                     targetSizeAskLaterCheckBox.IsChecked.Value)) &&
                                   !nothingChanged;

            savingButton.IsEnabled = everythingValid;

            return everythingValid;
        }

        private void AddTab(CompressPreset preset, bool switchToTab = false)
        {
            TextBlock textBlock = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = preset.PresetName
            };

            Grid grid = new Grid
            {
                Cursor = Cursors.Hand,
                Height = 60
            };

            MouseButtonEventHandler gridOnMouseDown = (_, _) =>
            {
                if (lastPresetTextBlock != null)
                    lastPresetTextBlock.Foreground = Brushes.White;

                lastPresetTextBlock = textBlock;
                lastPreset = preset;
                lastParentTextBlockElement = grid;

                lastPresetTextBlock.Foreground = originalButtonNewPresetItemColor;

                OnPresetChanged(preset);
            };

            grid.MouseDown += gridOnMouseDown;


            grid.Children.Add(textBlock);
            presetsStackPanel.Children.Insert(0, grid);

            if (switchToTab)
            {
                gridOnMouseDown.Invoke(null, null);
            }
        }

        private void OnPresetChanged(CompressPreset newPreset)
        {
            isAdding = false;

            lastPresetCopy = newPreset.Copy();
            
            savingButtonsTextBox.Text = "Übernehmen";
            savingButtonsIcon.Kind = PackIconKind.Update;
            

            savingButton.IsEnabled = false;
            deleteButton.Visibility = Visibility.Visible;
            deleteButton.IsEnabled = this.compressPresetCollection.CompressPresets.Count > 1;

            buttonNewPresetItem.Background = Brushes.White;
            buttonNewPresetItem.BorderBrush = Brushes.White;
            buttonsNewPresetItemIcon.Foreground = Brushes.Black;

            nameTextBox.Text = newPreset.PresetName;
            selectedCodecComboBox.SelectedIndex = (int) newPreset.Codec;

            this.collapsibleGroupboxPredefinedBitrate.IsVisibleContent = newPreset.UseBitrate;
            this.collapsibleGroupboxCalculatedBitrate.IsVisibleContent = newPreset.UseTargetSizeCalculation;

            if (newPreset.UseBitrate)
                bitrateTextBox.Text = newPreset.Bitrate.Value.ToString();

            targetSizeAskLaterCheckBox.IsChecked = newPreset.AskLater;

            if (newPreset.UseTargetSizeCalculation && !newPreset.AskLater)
            {
                targetSizeTextBox.Text = !newPreset.AskLater ? newPreset.TargetSize.Value.ToString() : "";
            }
        }

        private void StackPanelPresetNew_OnClick(object sender, RoutedEventArgs e)
        {
            isAdding = true;

            lastPreset = null;
            lastPresetCopy = null;

            savingButtonsTextBox.Text = "Hinzufügen";
            savingButtonsIcon.Kind = PackIconKind.Add;

            savingButton.IsEnabled = false;
            deleteButton.Visibility = Visibility.Collapsed;

            if (lastPresetTextBlock != null)
                lastPresetTextBlock.Foreground = Brushes.White;

            buttonNewPresetItem.Background = originalButtonNewPresetItemColor;
            buttonNewPresetItem.BorderBrush = originalButtonNewPresetItemColor;
            buttonsNewPresetItemIcon.Foreground = Brushes.White;

            nameTextBox.Text = "";
            selectedCodecComboBox.SelectedIndex = 0;
            bitrateTextBox.Text = "";
            targetSizeTextBox.Text = "";

            this.collapsibleGroupboxPredefinedBitrate.IsVisibleContent = true;
            this.collapsibleGroupboxCalculatedBitrate.IsVisibleContent = false;

            this.targetSizeAskLaterCheckBox.IsChecked = false;

            bitrateTextBox.Text = "";
            targetSizeTextBox.Text = "";
        }

        private void OnName_Enter(object sender, TextChangedEventArgs e)
        {
            string originalName = lastPreset.PresetName;
            lastPreset.PresetName = nameTextBox.Text;
            ValidateCanUse();
            lastPreset.PresetName = originalName;
        }

        private void OnDigit_Enter(object sender, TextChangedEventArgs e)
        {
            bool wasDigit = false;
            int? oldValue = null;
            if (isDigitValidationRule.Validate(((TextBox)sender).Text, null).IsValid)
            {
                wasDigit = true;

                if (((TextBox)sender).Name == "bitrateTextBox")
                {
                    oldValue = lastPreset.Bitrate;
                    lastPreset.Bitrate = int.Parse(((TextBox)sender).Text);
                }
                else
                {
                    oldValue = lastPreset.TargetSize;
                    lastPreset.TargetSize = int.Parse(((TextBox)sender).Text);
                }
            }

            ValidateCanUse();
            
            if (wasDigit && ((TextBox)sender).Name == "bitrateTextBox")
            {
                lastPreset.Bitrate = oldValue;
            }
            else if (wasDigit && ((TextBox)sender).Name == "targetSizeTextBox")
            {
                lastPreset.TargetSize = oldValue;
            }
        }

        private void ClosePresets_OnClick(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).PopContentControl();
        }

        private void CollapsibleGroupboxCalculatedBitrate_OnIsVisibilityChanged(CollapsibleGroupBox sender,
            bool newValue)
            => HandleCollapsibleGroupBox(collapsibleGroupboxPredefinedBitrate, collapsibleGroupboxCalculatedBitrate,
                newValue);

        private void CollapsibleGroupboxPredefinedBitrate_OnIsVisibilityChanged(CollapsibleGroupBox sender,
            bool newValue)
            => HandleCollapsibleGroupBox(collapsibleGroupboxCalculatedBitrate, collapsibleGroupboxPredefinedBitrate,
                newValue);

        private void HandleCollapsibleGroupBox(CollapsibleGroupBox a, CollapsibleGroupBox b, bool newValue)
        {
            if (!a.IsVisibleContent && !newValue)
            {
                b.SetIsVisibleContentWithoutEventFire(true);
                return;
            }

            b.SetIsVisibleContentWithoutEventFire(newValue);
            a.SetIsVisibleContentWithoutEventFire(!newValue);

            ValidateCanUse();
        }

        private void SaveOrAdd_OnClick(object sender, RoutedEventArgs e)
        {
            string presetName = nameTextBox.Text;
            bool useBitrate = collapsibleGroupboxPredefinedBitrate.IsVisibleContent;
            bool useTargetSize = collapsibleGroupboxCalculatedBitrate.IsVisibleContent;
            bool askLater = useTargetSize ? targetSizeAskLaterCheckBox.IsChecked.Value : false;
            Console.WriteLine((CodecDTO)selectedCodecComboBox.SelectedIndex);
            CodecDTO codec = (CodecDTO)selectedCodecComboBox.SelectedIndex;

            int? bitrate = useBitrate ? int.Parse(bitrateTextBox.Text) : null;
            int? targetSize = useTargetSize ? (askLater ? null : int.Parse(targetSizeTextBox.Text)) : null;

            if (isAdding)
            {
                CompressPreset newPreset =
                    new CompressPreset(presetName, codec, useBitrate, bitrate, useTargetSize, askLater, targetSize);

                compressPresetCollection.CompressPresets.Add(newPreset);
                AddTab(newPreset, true);
            }
            else
            {
                lastPreset.PresetName = presetName;
                lastPreset.UseBitrate = useBitrate;
                lastPreset.UseTargetSizeCalculation = useTargetSize;
                lastPreset.AskLater = askLater;
                lastPreset.Codec = codec;

                lastPreset.TargetSize = targetSize;
                lastPreset.Bitrate = bitrate;


                lastPresetTextBlock.Text = presetName;

                savingButton.IsEnabled = false;

                lastPresetCopy = lastPreset.Copy();
            }

            SettingsFolder.Save(compressPresetCollection);
        }

        private void DeleteSelectedPreset_OnClick(object sender, RoutedEventArgs e)
        {
            compressPresetCollection.CompressPresets.Remove(lastPreset);

            if (lastParentTextBlockElement != null)
            {
                presetsStackPanel.Children.Remove(lastParentTextBlockElement);
                StackPanelPresetNew_OnClick(null, null);
            }

            SettingsFolder.Save(compressPresetCollection);
        }

        private void TargetSizeAskLaterCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            ValidateCanUse();
        }

        private void SelectedCodecComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lastPreset != null)
            {
                CodecDTO oldValue = lastPreset.Codec;
                lastPreset.Codec = (CodecDTO)selectedCodecComboBox.SelectedIndex;
                ValidateCanUse();
                lastPreset.Codec = oldValue;
            }
        }
    }
}
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using VideoCompressorGUI.CompressPresets;
using VideoCompressorGUI.ContentControls.Components;
using VideoCompressorGUI.Settings;

namespace VideoCompressorGUI.ContentControls.Settingspages;

public partial class PresetsEditor : UserControl
{
    // true, if the "hinzufügen" tab is open, otherwise false
    private bool isAdding = true;

    private TextBlock lastPresetTextBlock = null;
    private CompressPreset lastPreset = null;
    
    public PresetsEditor()
    {
        InitializeComponent();
        
        savingButtonsTextBox.Text = "Hinzufügen";
        this.collapsibleGroupboxPredefinedBitrate.IsVisibleContent = true;
        this.collapsibleGroupboxCalculatedBitrate.IsVisibleContent = false;

        var compressPresetCollection = SettingsFolder.Load<CompressPresetCollection>();
        
        for (int i = 0; i < compressPresetCollection.CompressPresets.Count; i++)
        {
            AddTab(compressPresetCollection.CompressPresets[i]);
        }
    }

    private bool ValidateCanUse()
    {
        var r1 = nameNotEmptyValidationRule.Validate(nameTextBox.Text, null);
        var r2 = isDigitValidationRule.Validate(bitrateTextBox.Text, null);
        var r3 = isDigitValidationRule.Validate(targetSizeTextBox.Text, null);
        
        bool everythingValid = r1.IsValid && (!collapsibleGroupboxPredefinedBitrate.IsVisibleContent || r2.IsValid) && (!collapsibleGroupboxCalculatedBitrate.IsVisibleContent || r3.IsValid);
        
        savingButton.IsEnabled = everythingValid;

        return everythingValid;
    }

    private void AddTab(CompressPreset preset)
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
        
        grid.MouseDown += (_, _) =>
        {
            lastPresetTextBlock = textBlock;
            lastPreset = preset;
            
            OnPresetChanged(preset);
        };
        

        grid.Children.Add(textBlock);
        presetsStackPanel.Children.Insert(0, grid);
    }

    private void OnPresetChanged(CompressPreset newPreset)
    {
        isAdding = true;

        savingButtonsTextBox.Text = "Übernehmen";
        savingButtonsIcon.Kind = PackIconKind.Update;

        savingButton.IsEnabled = true;
        deleteButton.Visibility = Visibility.Visible;

        nameTextBox.Text = newPreset.PresetName;

        this.collapsibleGroupboxPredefinedBitrate.IsVisibleContent = newPreset.UseBitrate;
        this.collapsibleGroupboxCalculatedBitrate.IsVisibleContent = newPreset.UseTargetSizeCalculation;
        
        if (newPreset.UseBitrate)
            bitrateTextBox.Text = newPreset.Bitrate.Value.ToString();

        if (newPreset.UseTargetSizeCalculation)
            targetSizeTextBox.Text = newPreset.TargetSize.Value.ToString();
    }
    
    private void StackPanelPresetNew_OnClick(object sender, RoutedEventArgs e)
    {
        isAdding = false;
        
        savingButtonsTextBox.Text = "Hinzufügen";
        savingButtonsIcon.Kind = PackIconKind.Add;

        savingButton.IsEnabled = false;
        deleteButton.Visibility = Visibility.Collapsed;

        nameTextBox.Text = "";
        
        this.collapsibleGroupboxPredefinedBitrate.IsVisibleContent = true;
        this.collapsibleGroupboxCalculatedBitrate.IsVisibleContent = false;
        
        bitrateTextBox.Text = "";
        targetSizeTextBox.Text = "";
    }

    private void OnName_Enter(object sender, TextChangedEventArgs e)
    {
        ValidateCanUse();
    }
    
    private void OnDigit_Enter(object sender, TextChangedEventArgs e)
    {
        ValidateCanUse();
    }

    private void ClosePresets_OnClick(object sender, RoutedEventArgs e)
    {
        ((MainWindow) Application.Current.MainWindow).PopContentControl();
    }

    private void CollapsibleGroupboxCalculatedBitrate_OnIsVisibilityChanged(CollapsibleGroupBox sender, bool newValue)
        => HandleCollapsibleGroupBox(collapsibleGroupboxPredefinedBitrate, collapsibleGroupboxCalculatedBitrate, newValue);

    private void CollapsibleGroupboxPredefinedBitrate_OnIsVisibilityChanged(CollapsibleGroupBox sender, bool newValue)
        => HandleCollapsibleGroupBox(collapsibleGroupboxCalculatedBitrate, collapsibleGroupboxPredefinedBitrate, newValue);

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
        if (isAdding)
        {
            // new preset. need for save
            bool everythingValid = ValidateCanUse();
            if (everythingValid)
            {
                // save it. display it on the left
                // navigate to it
            }
        }
        else
        {
            // preset is already in existence and needs to be updated
        }
    }
}
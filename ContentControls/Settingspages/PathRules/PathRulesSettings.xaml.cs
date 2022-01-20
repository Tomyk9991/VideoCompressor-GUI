using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using Ookii.Dialogs.Wpf;
using VideoCompressorGUI.Settings;

namespace VideoCompressorGUI.ContentControls.Settingspages;

public partial class PathRulesSettings : UserControl
{
    private static readonly string BUSY = "Bereits belegt";
    private PathRuleCollection rules;
    private Dictionary<PathRule, Grid> gridDict = new();

    public PathRulesSettings()
    {
        InitializeComponent();
        this.rules = SettingsFolder.Load<PathRuleCollection>();
        rulesItemsPanel.Children.Clear();
        gridDict.Clear();

        foreach (var pathRule in this.rules.Collection)
        {
            AddRow(pathRule);
        }
        
        Settings.OnClosingSettings += OnSettingsClosing;
        
        okCancelDialog.OnConfirm += () =>
        {
            ((MainWindow) Application.Current.MainWindow).PopContentControl();
        };

        okCancelDialog.OnCancel += () =>
        {
            okCancelDialog.Visibility = Visibility.Collapsed;
        };

    }

    private bool OnSettingsClosing()
    {
        if (!ValidateCanUse())
        {
            okCancelDialog.Visibility = Visibility.Visible;
            return true;
        }

        
        return false;
    }

    private string SelectPath()
    {
        var dialog = new VistaFolderBrowserDialog();

        if ((bool)dialog.ShowDialog(Window.GetWindow(this)))
        {
            string selectedPath = dialog.SelectedPath;
            return selectedPath;
        }

        return "";
    }

    private void AddRuleButton_OnClick(object sender, RoutedEventArgs e)
    {
        PathRule rule = new PathRule("", "");
        rules.Add(rule);
        AddRow(rule);
    }

    private void AddRow(PathRule rule)
    {
        Grid grid = new Grid
        {
            Margin = new Thickness(0, 0, 0, 10)
        };
        var col1 = new ColumnDefinition
        {
            Width = new GridLength(1, GridUnitType.Star)
        };
        var col2 = new ColumnDefinition
        {
            Width = GridLength.Auto
        };
        var col3 = new ColumnDefinition
        {
            Width = new GridLength(1, GridUnitType.Star)
        };
        var col4 = new ColumnDefinition
        {
            Width = GridLength.Auto
        };

        grid.ColumnDefinitions.Add(col1);
        grid.ColumnDefinitions.Add(col2);
        grid.ColumnDefinitions.Add(col3);
        grid.ColumnDefinitions.Add(col4);

        TextBox baseDirectoryTextBox = new TextBox
        {
            Margin = new Thickness(10, 0, 10, 0),
            IsReadOnly = true,
            Text = rule.Directory,
            VerticalAlignment = VerticalAlignment.Center
        };


        baseDirectoryTextBox.PreviewMouseUp += (s, a) =>
        {
            BaseDirectoryTextBox_OnMouseUp(baseDirectoryTextBox, rule);
        };

        Grid.SetColumn(baseDirectoryTextBox, 0);

        grid.Children.Add(baseDirectoryTextBox);

        PackIcon icon = new PackIcon
        {
            VerticalAlignment = VerticalAlignment.Center,
            Kind = PackIconKind.ArrowRight
        };
        Grid.SetColumn(icon, 1);

        grid.Children.Add(icon);

        TextBox mappedDirectoryTextBox = new TextBox
        {
            IsReadOnly = true,
            Text = rule.MappedDirectory,
            Margin = new Thickness(10, 0, 10, 0)
        };
        Grid.SetColumn(mappedDirectoryTextBox, 2);
        mappedDirectoryTextBox.PreviewMouseUp += (s, a) =>
        {
            MappedDirectoryTextBox_OnPreviewMouseUp(mappedDirectoryTextBox, rule);
        };

        grid.Children.Add(mappedDirectoryTextBox);

        Button removeButton = new Button
        {
            Margin = new Thickness(0, 0, 5, 0),
            Width = 45,
            Content = new PackIcon()
            {
                Kind = PackIconKind.Delete
            }
        };
        removeButton.Click += (sender, args) => { RemoveRow(rule, grid); };

        Grid.SetColumn(removeButton, 3);
        grid.Children.Add(removeButton);
        gridDict[rule] = grid;

        rulesItemsPanel.Children.Add(grid);

        ValidateCanUse();
    }

    private void RemoveRow(PathRule rule, Grid parent)
    {
        rulesItemsPanel.Children.Remove(parent);
        gridDict.Remove(rule);
        rules.Remove(rule);
    }

    private bool ValidateCanUse()
    {
        bool f = false;
        foreach (PathRule rule in rules.Collection)
        {
            if (rule.Directory == "" || rule.Directory == BUSY)
            {
                f = true;
                gridDict[rule].Background = new SolidColorBrush(Color.FromArgb(30, 255, 0, 0));
                continue;
            }

            if (rule.MappedDirectory == "")
            {
                f = true;
                gridDict[rule].Background  = new SolidColorBrush(Color.FromArgb(30, 255, 0, 0));
                continue;
            }

            if (gridDict.ContainsKey(rule))
            {
                gridDict[rule].Background  = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
        }

        return !f;
    }

    private void MappedDirectoryTextBox_OnPreviewMouseUp(TextBox sender, PathRule rule)
    {
        string mappedDirectory = SelectPath();

        if (mappedDirectory != "")
        {
            rule.MappedDirectory = mappedDirectory;
            sender.Text = rule.MappedDirectory;
        }

        ValidateCanUse();
    }

    private void BaseDirectoryTextBox_OnMouseUp(TextBox sender, PathRule rule)
    {
        string directory = SelectPath();

        
        if (directory != "")
        {
            if (rules.Collection.FirstOrDefault(t => t.Directory == directory) != null)
            {
                if (rule.Directory != directory)
                {
                    directory = BUSY;
                }
            }
            
            rule.Directory = directory;
            sender.Text = rule.Directory;
        }

        ValidateCanUse();
    }

    private void PathRulesSettings_OnLoaded(object sender, RoutedEventArgs e)
    {
        rulesItemsPanel.Children.Clear();
        gridDict.Clear();
        this.rules = SettingsFolder.Load<PathRuleCollection>();

        foreach (var pathRule in this.rules.Collection)
        {
            AddRow(pathRule);
        }
    }

    private void PathRulesSettings_OnUnloaded(object sender, RoutedEventArgs e)
    {
        FilterWrongRules();
        SettingsFolder.Save(this.rules);
    }

    private void FilterWrongRules()
    {
        for (int i = this.rules.Collection.Count - 1; i >= 0; i--)
        {
            PathRule pathRule = this.rules.Collection[i];
            
            if (pathRule.Directory == "" || pathRule.MappedDirectory == "" || pathRule.Directory == BUSY)
            {
                this.rules.Remove(pathRule);
            }
        }
    }
}
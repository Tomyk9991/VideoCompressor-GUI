using System.Collections.Generic;
using VideoCompressorGUI.Settings;

namespace VideoCompressorGUI.ContentControls.Settingspages;

[System.Serializable]
public class PathRule
{
    public string Directory { get; set; }
    public string MappedDirectory { get; set; }

    public PathRule(string directory, string mappedDirectory)
    {
        Directory = directory;
        MappedDirectory = mappedDirectory;
    }
}

[System.Serializable]
public class PathRuleCollection : ISettingsLoadable<PathRuleCollection>
{
    public List<PathRule> Collection { get; set; }

    public PathRuleCollection()
    {
        Collection = new List<PathRule>();
    }

    public PathRuleCollection CreateDefault() => new();

    public void Add(PathRule rule)
    {
        this.Collection.Add(rule);
    }
    
    public void Remove(PathRule rule)
    {
        this.Collection.Remove(rule);
    }
}
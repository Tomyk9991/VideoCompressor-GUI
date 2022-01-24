using System.Collections.Generic;
using VideoCompressorGUI.SettingsLoadables;

namespace VideoCompressorGUI.ContentControls.Settingspages.PathRulesSettingsTab
{
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

        public bool ContainsDirectory(string directory, out string s)
        {
            for (int i = 0; i < this.Collection.Count; i++)
            {
                if (this.Collection[i].Directory == directory)
                {
                    s = Collection[i].MappedDirectory;
                    return true;
                }
            }

            s = "";
            return false;
        }
    }
}
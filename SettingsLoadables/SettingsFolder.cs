using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using VideoCompressorGUI.ContentControls.Settingspages.PathRulesSettingsTab;
using VideoCompressorGUI.Utils.Logger;

namespace VideoCompressorGUI.SettingsLoadables
{
    public class SettingsFolder
    {
        private static string ROOT = "";
        private static Dictionary<Type, Dictionary<string, string>> filenameDictionary = new();


        static SettingsFolder()
        {
            ROOT = AppDomain.CurrentDomain.BaseDirectory + "/settings";

            filenameDictionary = new Dictionary<Type, Dictionary<string, string>>
            {
                {
                    typeof(CompressPresetCollection), new Dictionary<string, string>
                    {
                        { "", nameof(CompressPresetCollection) + ".json" }
                    }
                },
                {
                    typeof(GeneralSettingsData), new Dictionary<string, string>
                    {
                        { "", nameof(GeneralSettingsData) + ".json" }
                    }
                },
                {
                    typeof(VideoPlayerCache), new Dictionary<string, string>
                    {
                        { "", nameof(VideoPlayerCache) + ".json" }
                    }
                },
                {
                    typeof(PathRuleCollection), new Dictionary<string, string>
                    {
                        { "", nameof(PathRuleCollection) + ".json" }
                    }
                },
                {
                    typeof(VideoEditorCache), new Dictionary<string, string>
                    {
                        { "", nameof(VideoEditorCache) + ".json" }
                    }
                },
            };

            CheckFoldersAndFiles();
        }

        private static void CheckFoldersAndFiles()
        {
            Directory.CreateDirectory(ROOT);


            foreach (KeyValuePair<Type, Dictionary<string, string>> keyValuePair in filenameDictionary)
            {
                foreach (KeyValuePair<string, string> pair in keyValuePair.Value)
                {
                    string filePath = ROOT + "/" + pair.Value;
                    if (!File.Exists(filePath))
                    {
                        FileStream fs = File.Create(filePath);
                        fs.Close();
                    }
                }
            }
        }

        public static T Load<T>(string key = "") where T : class, ISettingsLoadable<T>, new()
        {
            T value = new T();

            string path = ROOT + "/" + filenameDictionary[typeof(T)][key];
            string content = File.ReadAllText(path);

            if (string.IsNullOrWhiteSpace(content))
            {
                Log.Info($"No configuration loaded for: {typeof(T)}. Load standard config");
                return value.CreateDefault();
            }

            return JsonConvert.DeserializeObject<T>(content);
        }

        public static void Save<T>(T value, string key = "")
        {
            string path = ROOT + "/" + filenameDictionary[typeof(T)][key];
            string json = JsonConvert.SerializeObject(value, Formatting.Indented);

            Console.ForegroundColor = ConsoleColor.Green;
            Log.Info($"Saving to: {path}");
            Log.Info(json);
            Console.ResetColor();

            File.WriteAllText(path, json);
        }
    }
}
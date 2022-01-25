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

            Type[] types =
            {
                typeof(CompressPresetCollection),
                typeof(GeneralSettingsData),
                typeof(VideoPlayerCache),
                typeof(PathRuleCollection),
                typeof(VideoEditorCache),
                typeof(VideoBrowserItemTemplate)
            };

            filenameDictionary = new Dictionary<Type, Dictionary<string, string>>();
            
            foreach (Type type in types)
            {
                var value = new Dictionary<string, string>()
                {
                    { "", type.Name + ".json" }
                };
                
                filenameDictionary.Add(type, value);
            }

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
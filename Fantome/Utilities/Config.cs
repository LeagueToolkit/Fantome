using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Utilities
{
    public abstract class Config
    {
        public const string FILE_PATH = "config.json";

        private static readonly Dictionary<string, object> _defaultConfig = new()
        {
            { "LeagueLocation", "" },
            { "LoggingPattern", "{Timestamp:dd-MM-yyyy HH:mm:ss.fff} | [{Level}] | {Message:lj}{NewLine}{Exception}" },
            { "GameHashtableChecksum", "" },
            { "LCUHashtableChecksum", "" },
            { "PackedBinRegex", @"^DATA/.*_(Skins_Skin|Tiers_Tier|(Skins|Tiers)_Root).*\.bin$" },
            { "BINPackedKeywords", new string[] { "Skins", "Tiers" } },
            { "GenerateHashesFromBIN", false },
            { "SyncHashes", true },
            { "ExtractInitialDirectory", "" }
        };
        private static Dictionary<string, object> _config = new();

        public static T Get<T>(string key)
        {
            if (!_config.ContainsKey(key))
            {
                return GetDefault<T>(key);
            }

            if (typeof(T).BaseType == typeof(Enum))
            {
                return (T)Enum.Parse(typeof(T), _config[key].ToString());
            }
            else if (typeof(T).BaseType == typeof(Array))
            {
                // C# array
                if (_config[key] is T csharpArray)
                {
                    return csharpArray;
                }
                // JSON array
                else if (_config[key] is JArray jsonArray)
                {
                    return jsonArray.ToObject<T>();
                }
            }

            return (T)Convert.ChangeType(_config[key], typeof(T));
        }
        public static T GetDefault<T>(string key)
        {
            return (T)_defaultConfig[key];
        }

        public static void Set(string key, object value)
        {
            if (_config.ContainsKey(key))
            {
                _config[key] = value;
            }
            else
            {
                _config.Add(key, value);
            }

            Write();
        }

        public static void Load(string fileLocation = FILE_PATH)
        {
            if (File.Exists(FILE_PATH))
            {
                Deserialize(File.ReadAllText(fileLocation));

                //Check if config is outdated
                foreach (KeyValuePair<string, object> configEntry in _defaultConfig)
                {
                    if (!_config.ContainsKey(configEntry.Key))
                    {
                        _config.Add(configEntry.Key, configEntry.Value);
                    }
                }
            }
            else
            {
                _config = _defaultConfig;
            }
        }
        public static void Write(string fileLocation = FILE_PATH)
        {
            File.WriteAllText(fileLocation, Serialize());
        }
        public static string Serialize()
        {
            return JsonConvert.SerializeObject(_config, Formatting.Indented);
        }
        public static void Deserialize(string json)
        {
            _config = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }
    }
}

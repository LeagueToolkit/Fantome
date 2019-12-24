using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fantome.Utilities
{
    public static class Config
    {
        public const string CONFIG_FILE = "CONFIG.json";

        private static readonly Dictionary<string, object> _defaultConfig = new Dictionary<string, object>
        {
            { "LeagueLocation", "" }
        };
        private static Dictionary<string, object> _config = new Dictionary<string, object>();

        public static T Get<T>(string key)
        {
            if(!_config.ContainsKey(key))
            {
                return default;
            }

            return (T)_config[key];
        }
        public static void Set(string key, object value)
        {
            if(_config.ContainsKey(key))
            {
                _config[key] = value;
            }
            else
            {
                _config.Add(key, value);
            }

            Write();
        }

        public static void Load(string fileLocation = CONFIG_FILE)
        {
            if(File.Exists(CONFIG_FILE))
            {
                Deserialize(File.ReadAllText(fileLocation));
            }
            else
            {
                _config = _defaultConfig;
            }
        }
        public static void Write(string fileLocation = CONFIG_FILE)
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

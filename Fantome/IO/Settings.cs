using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Fantome.IO
{
    public class Settings
    {
        public Dictionary<String, String> Entries { get; private set; } = new Dictionary<String, String>();

        public Settings() { }

        public Settings(String location)
        {
            this.Entries = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(location));
        }

        public void Save(String location)
        {
            File.WriteAllText(location, JsonConvert.SerializeObject(this.Entries, Formatting.Indented));
        }
    }
}

using Newtonsoft.Json;
using System;
using System.IO;

namespace Fantome.IO
{
    public class Settings
    {
        public String LeagueOfLegendsPath { get; private set; }
        public void Save(String location)
        {
            File.WriteAllText(location, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        public static Settings GetInstance(String location)
        {
            return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(location));
        }
    }
}

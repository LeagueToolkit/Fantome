using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Fantome.ModManagement.IO
{
    public class ModInfo
    {
        public string Name { get; private set; }
        public string Author { get; private set; }
        public Version Version { get; private set; }
        public string Description { get; private set; }

        public ModInfo(string name, string author, Version version, string description)
        {
            this.Name = name;
            this.Author = author;
            this.Version = version;
            this.Description = description;
        }

        public string CreateID()
        {
            return string.Format("{0} - {1} (by {2})", this.Name, this.Version.ToString(), this.Author);
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new VersionConverter());
        }
        public static ModInfo Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<ModInfo>(json, new VersionConverter());
        }
    }
}

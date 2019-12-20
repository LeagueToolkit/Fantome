using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fantome.ModManagement
{
    public class ModDatabase
    {
        public Dictionary<string, bool> Mods { get; set; } = new Dictionary<string, bool>();

        public ModDatabase()
        {

        }

        public ModDatabase(Dictionary<string, bool> mods)
        {
            this.Mods = mods;
        }

        public void AddMod(string id, bool isInstalled)
        {
            this.Mods.Add(id, isInstalled);
        }

        public void ChangeModState(string id, bool isInstalled)
        {
            this.Mods[id] = isInstalled;
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static ModDatabase Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<ModDatabase>(json);
        }
    }
}

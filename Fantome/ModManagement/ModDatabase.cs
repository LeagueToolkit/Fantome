using System.Collections.Generic;
using Fantome.ModManagement.IO;
using Newtonsoft.Json;


namespace Fantome.ModManagement
{
    public class ModDatabase
    {
        public Dictionary<string, bool> Mods { get; set; } = new Dictionary<string, bool>();
        private Dictionary<string, ModFile> _modFiles = new Dictionary<string, ModFile>();

        public ModDatabase()
        {

        }

        public ModDatabase(Dictionary<ModFile, bool> mods)
        {
            foreach (KeyValuePair<ModFile, bool> mod in mods)
            {
                string id = mod.Key.GetModIdentifier();

                this.Mods.Add(id, mod.Value);
                this._modFiles.Add(id, mod.Key);
            }
        }

        public void AddMod(ModFile mod, bool isInstalled)
        {
            this.Mods.Add(mod.GetModIdentifier(), isInstalled);
            this._modFiles.Add(mod.GetModIdentifier(), mod);
        }
        public void RemoveMod(string id)
        {
            this.Mods.Remove(id);
            this._modFiles.Remove(id);
        }
        public void ChangeModState(string id, bool isInstalled)
        {
            this.Mods[id] = isInstalled;
        }
        public ModFile GetMod(string id)
        {
            return this._modFiles[id];
        }
        public bool IsInstalled(ModFile mod)
        {
            string id = mod.GetModIdentifier();

            if(this.Mods.ContainsKey(id))
            {
                return this.Mods[id];
            }

            return false;
        }

        internal void SyncFileDictionary()
        {
            foreach (KeyValuePair<string, bool> mod in this.Mods)
            {
                this._modFiles.Add(mod.Key, new ModFile(string.Format(@"{0}\{1}.zip", ModManager.MOD_FOLDER, mod.Key)));
            }
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
        public static ModDatabase Deserialize(string json)
        {
            ModDatabase database = JsonConvert.DeserializeObject<ModDatabase>(json);
            database.SyncFileDictionary();
            return database;
        }
    }
}

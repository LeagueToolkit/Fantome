using System.Collections.Generic;
using System.IO;
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
            SyncFileDictionary();
        }

        public ModDatabase(Dictionary<ModFile, bool> mods)
        {
            foreach (KeyValuePair<ModFile, bool> mod in mods)
            {
                string id = mod.Key.GetID();

                this.Mods.Add(id, mod.Value);
                this._modFiles.Add(id, mod.Key);
            }
        }

        public void AddMod(ModFile mod, bool isInstalled)
        {
            this.Mods.Add(mod.GetID(), isInstalled);
            this._modFiles.Add(mod.GetID(), mod);

            Write();
        }
        public void RemoveMod(string id)
        {
            this.Mods.Remove(id);
            this._modFiles.Remove(id);

            Write();
        }
        public void ChangeModState(string id, bool isInstalled)
        {
            this.Mods[id] = isInstalled;

            Write();
        }
        public ModFile GetMod(string id)
        {
            return this._modFiles[id];
        }
        public bool IsInstalled(ModFile mod)
        {
            string id = mod.GetID();

            if (this.Mods.ContainsKey(id))
            {
                return this.Mods[id];
            }

            return false;
        }

        internal void SyncFileDictionary()
        {
            List<string> modsToRemove = new List<string>();

            foreach (KeyValuePair<string, bool> mod in this.Mods)
            {
                //Remove mods which are not present in the Mods folder anymore
                string modPath = string.Format(@"{0}\{1}.zip", ModManager.MOD_FOLDER, mod.Key);
                if (!File.Exists(modPath))
                {
                    modsToRemove.Add(mod.Key);
                }

                this._modFiles.Add(mod.Key, new ModFile(modPath));
            }

            //Scan Mod folder for mods which were potentially added by the user
            foreach (string modFilePath in Directory.EnumerateFiles(ModManager.MOD_FOLDER))
            {
                string modFileName = Path.GetFileNameWithoutExtension(modFilePath);

                if (!this.Mods.ContainsKey(modFileName))
                {
                    AddMod(new ModFile(modFilePath), false);
                }
            }

            Write();
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        public static ModDatabase Deserialize(string json)
        {
            ModDatabase database = JsonConvert.DeserializeObject<ModDatabase>(json);
            database.SyncFileDictionary();
            return database;
        }
        public void Write(string fileLocation = ModManager.DATABASE_FILE)
        {
            File.WriteAllText(fileLocation, Serialize());
        }
    }
}

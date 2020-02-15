using Fantome.ModManagement.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;


namespace Fantome.ModManagement
{
    public class ModDatabase
    {
        public Dictionary<string, bool> Mods { get; set; } = new Dictionary<string, bool>();

        private Dictionary<string, ModFile> _modFiles = new Dictionary<string, ModFile>();
        private ModManager _modManager;

        public ModDatabase()
        {

        }
        public ModDatabase(ModManager modManager)
        {
            this._modManager = modManager;
            SyncWithModFolder();
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

        public bool IsInstalled(string modID)
        {
            if (this.Mods.ContainsKey(modID))
            {
                return this.Mods[modID];
            }

            return false;
        }
        public bool ContainsMod(string modID)
        {
            return this.Mods.ContainsKey(modID);
        }

        public void SyncWithModFolder()
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

                this._modFiles.Add(mod.Key, new ModFile(this._modManager, modPath));
            }

            //Scan Mod folder for mods which were potentially added by the user
            foreach (string modFilePath in Directory.EnumerateFiles(ModManager.MOD_FOLDER))
            {
                string modFileName = Path.GetFileNameWithoutExtension(modFilePath);

                if (!this.Mods.ContainsKey(modFileName))
                {
                    AddMod(new ModFile(this._modManager, modFilePath), false);
                }
            }

            Write();
        }

        public void SetModManager(ModManager modManager)
        {
            this._modManager = modManager;
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        public static ModDatabase Deserialize(ModManager modManager, string json)
        {
            ModDatabase database = JsonConvert.DeserializeObject<ModDatabase>(json);
            database.SetModManager(modManager);
            database.SyncWithModFolder();

            return database;
        }
        public void Write(string fileLocation = ModManager.DATABASE_FILE)
        {
            File.WriteAllText(fileLocation, Serialize());
        }
    }
}

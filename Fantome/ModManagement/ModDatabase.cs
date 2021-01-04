using Fantome.ModManagement.IO;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;


namespace Fantome.ModManagement
{
    public class ModDatabase
    {
        public Dictionary<string, bool> Mods { get; set; } = new Dictionary<string, bool>();

        private Dictionary<string, ModFile> _modFiles = new Dictionary<string, ModFile>();

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
            ModFile mod = this._modFiles[id];

            if(!mod.IsOpen)
            {
                mod.Reopen();
            }

            return mod;
        }

        public void MountMods()
        {
            foreach (var mod in this.Mods)
            {
                string modPath = Path.Combine(ModManager.MOD_FOLDER, mod.Key + ".fantome");
                if(File.Exists(modPath))
                {
                    try
                    {
                        this._modFiles.Add(mod.Key, new ModFile(modPath));
                        Log.Information("Mounted Mod: {0}", modPath);
                    }
                    catch(Exception excepton)
                    {
                        Log.Warning("Failed to mount Mod: {0}", modPath);
                        Log.Warning("REASON: {0}", excepton);
                    }
                }
                else
                {
                    Log.Warning("Mod does not exist: {0}", modPath );
                }
            }
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

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        public static ModDatabase Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<ModDatabase>(json);
        }
        public void Write(string fileLocation = ModManager.DATABASE_FILE)
        {
            File.WriteAllText(fileLocation, Serialize());
        }
    }
}

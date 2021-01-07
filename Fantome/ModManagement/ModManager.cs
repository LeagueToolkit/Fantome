using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.Libraries.League.Helpers.Cryptography;
using Fantome.Libraries.League.IO.WAD;
using Fantome.ModManagement.IO;
using Fantome.MVVM.ViewModels;
using Fantome.MVVM.ModelViews.Dialogs;
using Fantome.Utilities;
using MaterialDesignThemes.Wpf;
using Serilog;
using Fantome.ModManagement.WAD;

namespace Fantome.ModManagement
{
    public class ModManager
    {
        public const string MOD_FOLDER = "Mods";
        public const string OVERLAY_FOLDER = "Overlay";
        public const string DATABASE_FILE = "MOD_DATABASE.json";
        public const string INDEX_FILE = "FILE_INDEX.json";

        public LeagueFileIndex Index { get; private set; }
        public string LeagueFolder { get; private set; }
        public ModDatabase Database { get; private set; }

        public ModManager()
        {
            Log.Information("Creating a new ModManager instance");
        }

        public void Initialize(string leagueFolder)
        {
            this.LeagueFolder = leagueFolder;

            ProcessModDatabase();
            LoadLeagueFileIndex(true);
            CheckIndexVersion();

            if(!VerifyOverlayIntegrity())
            {
                ForceReset(false);
            }
        }

        private async void LoadLeagueFileIndex(bool loadExisting)
        {
            if (File.Exists(INDEX_FILE) && loadExisting)
            {
                this.Index = LeagueFileIndex.Deserialize(File.ReadAllText(INDEX_FILE));
                Log.Information("Loaded File Index from: " + INDEX_FILE);

                CheckIndexVersion();
            }
            else
            {
                try
                {
                    this.Index = new LeagueFileIndex(this.LeagueFolder);
                    Log.Information("Created new Game Index from: " + this.LeagueFolder);
                }
                catch(Exception exception)
                {
                    await DialogHelper.ShowMessageDialog("Failed to create League File Index\n" + exception);
                    Log.Error("Failed to create League File Index");
                    Log.Error(exception.ToString());
                }
            }
        }
        private void ProcessModDatabase()
        {
            if (File.Exists(DATABASE_FILE))
            {
                this.Database = ModDatabase.Deserialize(File.ReadAllText(DATABASE_FILE));

                SyncWithModFolder();

                Log.Information("Loaded Mod Database: " + DATABASE_FILE);
            }
            else
            {
                this.Database = new ModDatabase();

                SyncWithModFolder();

                Log.Information("Created new Mod Database");
            }

            this.Database.MountMods();
        }

        private void CheckIndexVersion()
        {
            Version leagueVersion = GetLeagueVersion();
            if (this.Index.Version != leagueVersion)
            {
                Log.Information("Index is out of date");
                Log.Information("Current Index Version: {0}", this.Index.Version.ToString());
                Log.Information("League Version: {0}", leagueVersion.ToString());

                ForceReset(false);
            }
        }

        public void ForceReset(bool deleteMods)
        {
            Log.Information("Doing Force Reset");
            Log.Information("DELETE_MODS = " + deleteMods);

            //Delete everything in overlay folder
            foreach(string directory in Directory.EnumerateDirectories(OVERLAY_FOLDER))
            {
                Directory.Delete(directory, true);
            }

            if(deleteMods)
            {
                List<string> modsToDelete = this.Database.Mods.Keys.ToList();
                foreach (string modToDelete in modsToDelete)
                {
                    this.Database.RemoveMod(modToDelete);
                }

                foreach(string file in Directory.EnumerateFiles(MOD_FOLDER))
                {
                    File.Delete(file);
                }
            }
            else
            {
                foreach (string databaseMod in this.Database.Mods.Keys.ToList())
                {
                    this.Database.ChangeModState(databaseMod, false);
                }
            }

            LoadLeagueFileIndex(false);
        }

        public async void AddMod(ModFile mod, bool install = false)
        {
            Log.Information("Adding Mod: {0} to Mod Manager", mod.GetID());

            if (install)
            {
                await DialogHelper.ShowInstallModDialog(mod, this);
            }

            this.Database.AddMod(mod, install);
        }
        public void RemoveMod(ModFile mod)
        {
            string modId = mod.GetID();

            if (this.Database.IsInstalled(mod.GetID()))
            {
                UninstallMod(mod);
            }

            mod.Content.Dispose();
            this.Database.RemoveMod(modId);
            File.Delete(string.Format(@"{0}\{1}.fantome", MOD_FOLDER, modId));
        }

        public void InstallMod(ModFile mod)
        {
            Log.Information("Installing Mod: {0}", mod.GetID());

            this.Index.StartEdit();

            //Update the Index with our new mod and also check for asset collisions
            UpdateIndex(mod);

            //Write all the created WAD files to our overlay directory
            WriteModWADFiles(mod);

            AddModToDatabase(mod);

            this.Index.EndEdit();
        }
        private void UpdateIndex(ModFile mod)
        {
            var modWadFiles = mod.GetWadFiles(this.Index);
            this.Index.AddMod(mod.GetID(), modWadFiles.Keys.ToList());

            foreach (KeyValuePair<string, WADFile> modWadFile in modWadFiles)
            {
                foreach (WADEntry entry in modWadFile.Value.Entries)
                {
                    if (this.Index.Game.ContainsKey(entry.XXHash))
                    {
                        this.Index.AddModFile(entry.XXHash, mod.GetID(), this.Index.Game[entry.XXHash]);
                    }
                    else
                    {
                        this.Index.AddModFile(entry.XXHash, mod.GetID(), new List<string>() { modWadFile.Key });
                    }
                }
            }
        }
        private void WriteModWADFiles(ModFile mod)
        {
            Action<KeyValuePair<string, WADFile>> writeWadFileDelegate = new Action<KeyValuePair<string, WADFile>>(WriteWadFile);

            if (Config.Get<bool>("ParallelWadInstallation"))
            {
                ParallelOptions parallelOptions = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };

                Parallel.ForEach(mod.GetWadFiles(this.Index), parallelOptions, (modWadFile) =>
                {
                    writeWadFileDelegate.Invoke(modWadFile);
                });
            }
            else
            {
                foreach(KeyValuePair<string, WADFile> modWadFile in mod.GetWadFiles(this.Index))
                {
                    writeWadFileDelegate.Invoke(modWadFile);
                }
            }

            void WriteWadFile(KeyValuePair<string, WADFile> modWadFile)
            {
                string wadPath = this.Index.FindWADPath(modWadFile.Key);
                string overlayModWadPath = string.Format(@"{0}\{1}", OVERLAY_FOLDER, wadPath);
                string gameModWadPath = string.Format(@"{0}\{1}", this.LeagueFolder, wadPath);

                //Check if the WAD already exists, if it does, we need to merge the 2 WADs
                //if it doesnt, then we need to copy it from the game directory
                if (!File.Exists(overlayModWadPath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(overlayModWadPath));

                    WADFile baseWad = new WADFile(gameModWadPath);
                    bool returnedModdedWad = false;
                    using (WADFile mergedWad = WadMerger.Merge(baseWad, modWadFile.Value, out returnedModdedWad))
                    {
                        mergedWad.Write(overlayModWadPath);
                    }

                    if (returnedModdedWad)
                    {
                        baseWad.Dispose();
                    }
                    else
                    {
                        modWadFile.Value.Dispose();
                    }
                }
                else
                {
                    File.Move(overlayModWadPath, overlayModWadPath + ".temp");

                    using (WADFile mergedWad = WadMerger.Merge(new WADFile(overlayModWadPath + ".temp"), modWadFile.Value))
                    {
                        mergedWad.Write(overlayModWadPath);
                    }

                    //Delete temp wad file
                    File.Delete(overlayModWadPath + ".temp");
                    modWadFile.Value.Dispose();
                }
            }
        }

        public void UninstallMod(ModFile mod)
        {
            Log.Information("Uninstalling Mod: " + mod.GetID());
            List<ulong> modFiles = new List<ulong>();
            if (this.Index.ModEntryMap.TryGetValue(mod.GetID(), out List<ulong> _modFiles))
            {
                modFiles = new List<ulong>(_modFiles);
            }
            Dictionary<string, WADFile> wadFiles = new Dictionary<string, WADFile>();

            this.Index.StartEdit();

            //In this loop we remove the installed WAD entries
            foreach (ulong modFile in modFiles)
            {
                List<string> modFileWadFiles = this.Index.Mod[modFile];

                //Initialize WAD files for entry deletion
                foreach (string modFileWadFile in modFileWadFiles)
                {
                    if (!wadFiles.ContainsKey(modFileWadFile))
                    {
                        wadFiles.Add(modFileWadFile, new WADFile(string.Format(@"{0}\{1}", OVERLAY_FOLDER, modFileWadFile)));
                    }

                    WADFile wad = wadFiles[modFileWadFile];
                    wad.RemoveEntry(modFile);
                }

                this.Index.RemoveModFile(modFile, mod.GetID());
            }

            //Now we need to either delete empty WAD files or fill the ones from which we removed the entries with original files
            //if the modified ones are the same as original then we need to delete those too
            foreach (KeyValuePair<string, WADFile> wadFile in wadFiles)
            {
                //If the WAD isn't being used by any other mod or is empty we can delete it
                if (this.Index.WadModMap[wadFile.Key].All(x => x == mod.GetID()) ||
                    wadFile.Value.Entries.Count == 0)
                {
                    wadFile.Value.Dispose();
                    File.Delete(string.Format(@"{0}\{1}", OVERLAY_FOLDER, wadFile.Key));
                }
                //If it's used by some other mods we need to merge it into the original WAD
                else
                {
                    string gameWadPath = string.Format(@"{0}\{1}", this.LeagueFolder, wadFile.Key);
                    string overlayWadPath = string.Format(@"{0}\{1}", OVERLAY_FOLDER, wadFile.Key);
                    WADFile originalWad = new WADFile(gameWadPath);

                    using (WADFile mergedWad = WadMerger.Merge(originalWad, wadFile.Value))
                    {
                        mergedWad.Write(overlayWadPath + ".temp");
                    }

                    wadFile.Value.Dispose();
                    File.Delete(overlayWadPath);
                    File.Move(overlayWadPath + ".temp", overlayWadPath);
                }
            }

            this.Database.ChangeModState(mod.GetID(), false);
            this.Index.RemoveMod(mod.GetID());
            this.Index.EndEdit();
        }

        private void AddModToDatabase(ModFile mod)
        {
            string id = mod.GetID();
            if (this.Database.Mods.ContainsKey(id))
            {
                this.Database.ChangeModState(id, true);
            }
            else
            {
                this.Database.AddMod(mod, true);
            }
        }

        public void SyncWithModFolder()
        {
            foreach (KeyValuePair<string, bool> mod in this.Database.Mods)
            {
                //Remove mods which are not present in the Mods folder anymore
                string modPath = string.Format(@"{0}\{1}.fantome", MOD_FOLDER, mod.Key);
                if (!File.Exists(modPath))
                {
                    this.Database.RemoveMod(mod.Key);
                }
            }

            //Scan Mod folder for mods which were potentially added by the user
            foreach (string modFilePath in Directory.EnumerateFiles(MOD_FOLDER))
            {
                string modFileName = Path.GetFileNameWithoutExtension(modFilePath);

                if (!this.Database.ContainsMod(modFileName))
                {
                    AddMod(new ModFile(modFilePath), false);
                }
            }
        }

        public bool VerifyOverlayIntegrity()
        {
            Log.Information("Verifying Overlay integrity...");

            List<string> installedModIds = this.Database.Mods.Where(x => x.Value).Select(x => x.Key).ToList();

            foreach(string installedModId in installedModIds)
            {
                using ModFile mod = this.Database.GetMod(installedModId);
                Dictionary<string, WADFile> modWadFiles = mod.GetWadFiles(this.Index);

                // First verify that index is valid
                foreach(var modWadFile in modWadFiles)
                {
                    if(this.Index.WadModMap.TryGetValue(modWadFile.Key, out List<string> wadModIds))
                    {
                        if(!wadModIds.Any(x => x == installedModId))
                        {
                            Log.Warning("WAD: {0} is installed but Mod: {1} is not mapped to it in the index", modWadFile.Key, installedModId);
                            return false; // Mod is installed but one of its WAD files isn't in the index
                        }
                    }
                    else
                    {
                        Log.Warning("Mod: {0} is installed but its WAD: {1} is not mapped to it in the index", installedModId, modWadFile.Key);
                        return false; // WAD is installed but isn't in index
                    }

                    if(this.Index.ModEntryMap.TryGetValue(installedModId, out List<ulong> modEntries))
                    {
                        foreach(WADEntry entry in modWadFile.Value.Entries)
                        {
                            if(!modEntries.Any(x => x == entry.XXHash))
                            {
                                Log.Warning("Mod: {0} is installed but its WAD Entry: {1} is not mapped to it in the index", installedModId, entry.XXHash);
                                return false; // An installed WAD Entry is not present in the index
                            }
                        }
                    }
                    else
                    {
                        Log.Warning("Mod: {0} is installed but isn't present in the Mod-Entry index map", installedModId);
                        return false; // Mod is installed but doesn't have any entries registered
                    }
                }
            }

            Log.Information("Overlay Integrity: Good");
            return true;
        }

        private Version GetLeagueVersion()
        {
            return new Version(FileVersionInfo.GetVersionInfo(Path.Combine(this.LeagueFolder, "League of Legends.exe")).FileVersion);
        }
    }
}

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
using Fantome.UserControls.Dialogs;
using Fantome.Utilities;
using MaterialDesignThemes.Wpf;
using Serilog;

namespace Fantome.ModManagement
{
    public class ModManager
    {
        public const string MOD_FOLDER = "Mods";
        public const string OVERLAY_FOLDER = "Overlay";
        public const string DATABASE_FILE = "MOD_DATABASE.json";
        public const string INDEX_FILE = "FILE_INDEX.json";

        public LeagueFileIndex Index { get; private set; }
        public string LeagueFolder { get; set; }
        public ModDatabase Database { get; set; }

        public ModManager() { }

        public ModManager(string leagueFolder)
        {
            AssignLeague(leagueFolder);
        }

        public void AssignLeague(string leagueFolder)
        {
            if (!IsValidLeagueFolder(leagueFolder))
            {
                //Error
            }
            else
            {
                this.LeagueFolder = leagueFolder;

                ProcessDatabase();
                ProcessLeagueFileIndex();
            }
        }

        public void ProcessLeagueFileIndex()
        {
            if (File.Exists(INDEX_FILE))
            {
                LeagueFileIndex currentIndex = LeagueFileIndex.Deserialize(File.ReadAllText(INDEX_FILE));
                Version leagueVersion = GetLeagueVersion();
                if (currentIndex.Version != leagueVersion)
                {
                    Log.Information("Index is out of date, creating new Index");
                    Log.Information("Current Index Version: {0}", currentIndex.Version.ToString());
                    Log.Information("League Version: {0}", leagueVersion.ToString());

                    //Create new Index and copy Mod Data from old one
                    this.Index = new LeagueFileIndex(this.LeagueFolder);
                    currentIndex.CopyModData(this.Index);
                    Log.Information("Copied old Mod Index Data to new Index");

                    //We need to reinstall mods
                    foreach (KeyValuePair<string, bool> mod in this.Database.Mods)
                    {
                        if (mod.Value)
                        {
                            InstallMod(this.Database.GetMod(mod.Key));
                        }
                    }

                    Log.Information("Created new Index");
                }
                else
                {
                    this.Index = currentIndex;
                    Log.Information("Loaded File Index from: " + INDEX_FILE);
                }
            }
            else
            {
                this.Index = new LeagueFileIndex(this.LeagueFolder);
                Log.Information("Created new Game Index from: " + this.LeagueFolder);
            }
        }
        public void ProcessDatabase()
        {
            if (File.Exists(DATABASE_FILE))
            {
                this.Database = ModDatabase.Deserialize(File.ReadAllText(DATABASE_FILE));
                Log.Information("Loaded Mod Database: " + DATABASE_FILE);
            }
            else
            {
                this.Database = new ModDatabase();
                Log.Information("Created new Mod Database");
            }
        }

        public async void AddMod(ModFile mod, bool install = false)
        {
            Log.Information("Adding Mod: {0} to Mod Manager", mod.GetID());
            this.Database.AddMod(mod, install);

            if (install)
            {
                await Installation.Install(mod, this);
            }
        }
        public void RemoveMod(ModFile mod)
        {
            string modId = mod.GetID();

            if (this.Database.IsInstalled(mod))
            {
                UninstallMod(mod);
            }

            mod.Content.Dispose();
            this.Database.RemoveMod(modId);
            File.Delete(string.Format(@"{0}\{1}.zip", MOD_FOLDER, modId));
        }

        public void InstallMod(ModFile mod)
        {
            Log.Information("Installing Mod: {0}", mod.GetID());
            Dictionary<string, WADFile> modWadFiles = new Dictionary<string, WADFile>();

            this.Index.StartEdit();

            //Collect WAD files in WAD folder
            CollectWADFiles(mod, modWadFiles);

            //Pack WAD folders files into WAD files
            CollectWADFolders(mod, modWadFiles);

            //Collect files from the RAW folder
            CollectRAWFiles(mod, modWadFiles);

            //Update the Index with our new mod and also check for asset collisions
            UpdateIndex(mod, modWadFiles);

            //Write all the created WAD files to our overlay directory
            WriteModWADFiles(modWadFiles);

            AddModToDatabase(mod);

            this.Index.EndEdit();
        }
        private void CollectWADFiles(ModFile mod, Dictionary<string, WADFile> modWadFiles)
        {
            foreach (ZipArchiveEntry zipEntry in mod.GetEntries(@"WAD[\\/][\w.]+.wad.client(?![\\/])"))
            {
                char ps = Pathing.GetPathSeparator(zipEntry.FullName);
                string wadPath = this.Index.FindWADPath(zipEntry.FullName.Split(ps)[1]);

                zipEntry.ExtractToFile("wadtemp", true);
                modWadFiles.Add(wadPath, new WADFile(new MemoryStream(File.ReadAllBytes("wadtemp"))));
                File.Delete("wadtemp");

                //We need to check each entry to see if they're shared across any other WAD files
                //if they are, we need to also modify those WADs
                foreach (WADEntry entry in modWadFiles[wadPath].Entries)
                {
                    //Check if the entry is present in the game files or if it's new
                    if (this.Index.Game.ContainsKey(entry.XXHash))
                    {
                        foreach (string additionalWadPath in this.Index.Game[entry.XXHash].Where(x => x != wadPath))
                        {
                            if (!modWadFiles.ContainsKey(additionalWadPath))
                            {
                                modWadFiles.Add(additionalWadPath, new WADFile(3, 0));
                            }

                            if (entry.Type == EntryType.Uncompressed)
                            {
                                modWadFiles[additionalWadPath].AddEntry(entry.XXHash, entry.GetContent(false), false);
                            }
                            else if (entry.Type == EntryType.Compressed || entry.Type == EntryType.ZStandardCompressed)
                            {
                                modWadFiles[additionalWadPath].AddEntryCompressed(entry.XXHash, entry.GetContent(false), entry.UncompressedSize, entry.Type);
                            }
                        }
                    }
                }
            }
        }
        private void CollectWADFolders(ModFile mod, Dictionary<string, WADFile> modWadFiles)
        {
            List<string> wadPaths = new List<string>();

            foreach (ZipArchiveEntry zipEntry in mod.GetEntries(@"WAD[\\/][\w.]+.wad.client[\\/].*"))
            {
                char ps = Pathing.GetPathSeparator(zipEntry.FullName);
                string wadName = zipEntry.FullName.Split(ps)[1];
                string wadPath = this.Index.FindWADPath(wadName);
                string path = zipEntry.FullName.Replace(string.Format("WAD{0}{1}{0}", ps, wadName), "").Replace('\\', '/');
                ulong hash = XXHash.XXH64(Encoding.ASCII.GetBytes(path.ToLower()));

                MemoryStream memoryStream = new MemoryStream();
                zipEntry.Open().CopyTo(memoryStream);

                if (!modWadFiles.ContainsKey(wadPath))
                {
                    modWadFiles.Add(wadPath, new WADFile(3, 0));
                    wadPaths.Add(wadPath);
                }

                if (Path.GetExtension(path) == ".wpk")
                {
                    modWadFiles[wadPath].AddEntry(hash, memoryStream.ToArray(), false);
                }
                else
                {
                    modWadFiles[wadPath].AddEntry(hash, memoryStream.ToArray(), true);
                }
            }

            //Shared Entry Check
            foreach (string wadPath in wadPaths)
            {
                foreach (WADEntry entry in modWadFiles[wadPath].Entries)
                {
                    //Check if the entry is present in the game files or if it's new
                    if (this.Index.Game.ContainsKey(entry.XXHash))
                    {
                        foreach (string additionalWadPath in this.Index.Game[entry.XXHash].Where(x => x != wadPath))
                        {
                            if (!modWadFiles.ContainsKey(additionalWadPath))
                            {
                                modWadFiles.Add(additionalWadPath, new WADFile(3, 0));
                            }

                            modWadFiles[additionalWadPath].AddEntryCompressed(entry.XXHash, entry.GetContent(false), entry.UncompressedSize, EntryType.ZStandardCompressed);
                        }
                    }
                }
            }
        }
        private void CollectRAWFiles(ModFile mod, Dictionary<string, WADFile> modWadFiles)
        {
            foreach (ZipArchiveEntry zipEntry in mod.GetEntries(@"RAW[\\/].*"))
            {
                char ps = Pathing.GetPathSeparator(zipEntry.FullName);
                string path = zipEntry.FullName.Replace(@"RAW" + ps, "").Replace('\\', '/');
                ulong hash = XXHash.XXH64(Encoding.ASCII.GetBytes(path.ToLower()));
                List<string> fileWadFiles = new List<string>();

                //Check if file exists, if not, we discard it
                if (this.Index.Game.ContainsKey(hash))
                {
                    fileWadFiles = this.Index.Game[hash];
                    foreach (string wadFilePath in fileWadFiles)
                    {
                        if (!modWadFiles.ContainsKey(wadFilePath))
                        {
                            modWadFiles.Add(wadFilePath, new WADFile(3, 0));
                        }

                        MemoryStream memoryStream = new MemoryStream();
                        zipEntry.Open().CopyTo(memoryStream);

                        modWadFiles[wadFilePath].AddEntry(hash, memoryStream.ToArray(), true);
                    }
                }
            }
        }
        private void UpdateIndex(ModFile mod, Dictionary<string, WADFile> modWadFiles)
        {
            List<string> assetCollisions = this.Index.CheckForAssetCollisions(modWadFiles);

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
        private void WriteModWADFiles(Dictionary<string, WADFile> modWadFiles)
        {
            ParallelOptions parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.ForEach(modWadFiles, (modWadFile) =>
            {
                string wadPath = this.Index.FindWADPath(modWadFile.Key);
                string overlayModWadPath = string.Format(@"{0}\{1}", OVERLAY_FOLDER, wadPath);
                string gameModWadPath = string.Format(@"{0}\Game\{1}", this.LeagueFolder, wadPath);

                //Check if the WAD already exists, if it does, we need to merge the 2 WADs
                //if it doesnt, then we need to copy it from the game directory
                if (!File.Exists(overlayModWadPath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(overlayModWadPath));

                    using (WADFile mergedWad = WADMerger.Merge(new WADFile(gameModWadPath), modWadFile.Value))
                    {
                        mergedWad.Write(overlayModWadPath);
                    }

                    modWadFile.Value.Dispose();
                }
                else
                {
                    File.Move(overlayModWadPath, overlayModWadPath + ".temp");

                    using (WADFile mergedWad = WADMerger.Merge(new WADFile(overlayModWadPath + ".temp"), modWadFile.Value))
                    {
                        mergedWad.Write(overlayModWadPath);
                    }

                    //Delete temp wad file
                    File.Delete(overlayModWadPath + ".temp");
                    modWadFile.Value.Dispose();
                }
            });
        }

        public void UninstallMod(ModFile mod)
        {
            Log.Information("Uninstalling Mod: " + mod.GetID());
            List<ulong> modFiles = new List<ulong>(this.Index.ModEntryMap[mod.GetID()]);
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
            foreach(KeyValuePair<string, WADFile> wadFile in wadFiles)
            {
                //If the WAD isn't being used by any other mod or is empty we can delete it
                if (!this.Index.WadModMap[wadFile.Key].Any(x => x != mod.GetID()) ||
                    wadFile.Value.Entries.Count == 0)
                {
                    wadFile.Value.Dispose();
                    File.Delete(string.Format(@"{0}\{1}", OVERLAY_FOLDER, wadFile.Key));
                }
                //If it's used by some other mods we need to merge it into the original WAD
                else
                {
                    string gameWadPath = string.Format(@"{0}\Game\{1}", this.LeagueFolder, wadFile.Key);
                    string overlayWadPath = string.Format(@"{0}\{1}", OVERLAY_FOLDER, wadFile.Key);
                    WADFile originalWad = new WADFile(gameWadPath);

                    using (WADFile mergedWad = WADMerger.Merge(originalWad, wadFile.Value))
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

        private Version GetLeagueVersion()
        {
            return new Version(FileVersionInfo.GetVersionInfo(Path.Combine(this.LeagueFolder, "Game/League of Legends.exe")).FileVersion);
        }
        private bool IsValidLeagueFolder(string leagueFolder)
        {
            return File.Exists(Path.Combine(leagueFolder, "LeagueClient.exe"));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Fantome.Libraries.League.Helpers.Cryptography;
using Fantome.Libraries.League.IO.WAD;
using Fantome.ModManagement.IO;
using Fantome.Utilities;

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

            this.LeagueFolder = leagueFolder;

            ProcessDatabase();
            ProcessLeagueFileIndex();
        }

        public void ProcessLeagueFileIndex()
        {
            if (File.Exists(INDEX_FILE))
            {
                LeagueFileIndex currentIndex = LeagueFileIndex.Deserialize(File.ReadAllText(INDEX_FILE));
                if (currentIndex.Version != GetLeagueVersion())
                {
                    //Create new Index and copy Mod Data from old one
                    this.Index = new LeagueFileIndex(this.LeagueFolder);
                    currentIndex.CopyModData(this.Index);

                    //We need to reinstall mods
                    foreach (KeyValuePair<string, bool> mod in this.Database.Mods)
                    {
                        if (mod.Value)
                        {
                            InstallMod(this.Database.GetMod(mod.Key));
                        }
                    }
                }
                else
                {
                    this.Index = currentIndex;
                }
            }
            else
            {
                this.Index = new LeagueFileIndex(this.LeagueFolder);
            }
        }
        public void ProcessDatabase()
        {
            if (File.Exists(DATABASE_FILE))
            {
                this.Database = ModDatabase.Deserialize(File.ReadAllText(DATABASE_FILE));
            }
            else
            {
                this.Database = new ModDatabase();
            }
        }

        public void AddMod(ModFile mod, bool install = false)
        {
            this.Database.AddMod(mod, install);

            if (install)
            {
                InstallMod(mod);
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
            Dictionary<string, WADFile> modWadFiles = new Dictionary<string, WADFile>();

            this.Index.StartEdit();

            //Collect WAD files in WAD folder
            CollectWADFiles(mod, modWadFiles);

            //Pack WAD folders files into WAD files
            CollectWADFolders(mod, modWadFiles);

            //Collect files from the RAW folder
            CollectRAWFiles(mod, modWadFiles);

            //Write all the created WAD files to our overlay directory
            WriteModWADFiles(modWadFiles);

            AddModToDatabase(mod);

            //Clean up memory by disposing all WAD files
            foreach (KeyValuePair<string, WADFile> wad in modWadFiles)
            {
                wad.Value.Dispose();
            }

            this.Index.AssignWadsToMod(mod.GetID(), modWadFiles.Keys.ToList());
            this.Index.EndEdit();
        }
        private void CollectWADFiles(ModFile mod, Dictionary<string, WADFile> modWadFiles)
        {
            foreach (ZipArchiveEntry zipEntry in mod.GetEntries(@"WAD[\\/][\w.]+.wad.client(?![\\/])"))
            {
                char ps = Pathing.GetPathSeparator(zipEntry.FullName);
                string wadName = zipEntry.FullName.Split(ps)[1];

                zipEntry.ExtractToFile("wadtemp", true);
                modWadFiles.Add(wadName, new WADFile(new MemoryStream(File.ReadAllBytes("wadtemp"))));
                File.Delete("wadtemp");

                //We need to check each entry to see if they're shared across any other WAD files
                //if they are, we need to also modify those WADs
                foreach (WADEntry entry in modWadFiles[wadName].Entries)
                {
                    //Check if the entry is present in the game files or if it's new
                    if (this.Index.Game.ContainsKey(entry.XXHash))
                    {
                        foreach (string additionalWadPath in this.Index.Game[entry.XXHash].Where(x => Path.GetFileName(x) != wadName))
                        {
                            string additionalWadFile = Path.GetFileName(additionalWadPath);
                            if (!modWadFiles.ContainsKey(additionalWadFile))
                            {
                                modWadFiles.Add(additionalWadFile, new WADFile(3, 0));
                            }

                            if (entry.Type == EntryType.Uncompressed)
                            {
                                modWadFiles[additionalWadFile].AddEntry(entry.XXHash, entry.GetContent(false), false);
                            }
                            else if (entry.Type == EntryType.Compressed || entry.Type == EntryType.ZStandardCompressed)
                            {
                                modWadFiles[additionalWadFile].AddEntryCompressed(entry.XXHash, entry.GetContent(false), entry.UncompressedSize, entry.Type);
                            }
                        }
                    }

                    //Add file to Index
                    //TODO: ASSET COLLISION
                    if (this.Index.Game.ContainsKey(entry.XXHash))
                    {
                        this.Index.AddModFile(entry.XXHash, mod.GetID(), this.Index.Game[entry.XXHash]);
                    }
                    else
                    {
                        this.Index.AddModFile(entry.XXHash, mod.GetID(), new List<string>() { this.Index.FindWADPath(wadName) });
                    }
                }
            }
        }
        private void CollectWADFolders(ModFile mod, Dictionary<string, WADFile> modWadFiles)
        {
            List<string> wadNames = new List<string>();

            foreach (ZipArchiveEntry zipEntry in mod.GetEntries(@"WAD[\\/][\w.]+.wad.client[\\/].*"))
            {
                char ps = Pathing.GetPathSeparator(zipEntry.FullName);
                string wadName = zipEntry.FullName.Split(ps)[1];
                string path = zipEntry.FullName.Replace(string.Format("WAD{0}{1}{0}", ps, wadName), "").Replace('\\', '/');
                ulong hash = XXHash.XXH64(Encoding.ASCII.GetBytes(path.ToLower()));

                MemoryStream memoryStream = new MemoryStream();
                zipEntry.Open().CopyTo(memoryStream);

                if (!modWadFiles.ContainsKey(wadName))
                {
                    modWadFiles.Add(wadName, new WADFile(3, 0));
                    wadNames.Add(wadName);
                }

                if (Path.GetExtension(path) == ".wpk")
                {
                    modWadFiles[wadName].AddEntry(hash, memoryStream.ToArray(), false);
                }
                else
                {
                    modWadFiles[wadName].AddEntry(hash, memoryStream.ToArray(), true);
                }
            }

            //Shared Entry Check
            foreach (string wadName in wadNames)
            {
                foreach (WADEntry entry in modWadFiles[wadName].Entries)
                {
                    //Check if the entry is present in the game files or if it's new
                    if (this.Index.Game.ContainsKey(entry.XXHash))
                    {
                        foreach (string additionalWadPath in this.Index.Game[entry.XXHash].Where(x => Path.GetFileName(x) != wadName))
                        {
                            string additionalWadFile = Path.GetFileName(additionalWadPath);
                            if (!modWadFiles.ContainsKey(additionalWadFile))
                            {
                                modWadFiles.Add(additionalWadFile, new WADFile(3, 0));
                            }

                            modWadFiles[additionalWadFile].AddEntryCompressed(entry.XXHash, entry.GetContent(false), entry.UncompressedSize, EntryType.ZStandardCompressed);
                        }
                    }

                    //Add file to Index
                    //TODO: ASSET COLLISION
                    if (this.Index.Game.ContainsKey(entry.XXHash))
                    {
                        this.Index.AddModFile(entry.XXHash, mod.GetID(), this.Index.Game[entry.XXHash]);
                    }
                    else
                    {
                        this.Index.AddModFile(entry.XXHash, mod.GetID(), new List<string>() { this.Index.FindWADPath(wadName) });
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
                        string wadFile = Path.GetFileName(wadFilePath);

                        if (!modWadFiles.ContainsKey(wadFile))
                        {
                            modWadFiles.Add(wadFile, new WADFile(3, 0));
                        }

                        MemoryStream memoryStream = new MemoryStream();
                        zipEntry.Open().CopyTo(memoryStream);

                        modWadFiles[wadFile].AddEntry(hash, memoryStream.ToArray(), true);
                    }

                    this.Index.AddModFile(hash, mod.GetID(), this.Index.Game[hash]);
                }
            }
        }
        private void WriteModWADFiles(Dictionary<string, WADFile> modWadFiles)
        {
            foreach (KeyValuePair<string, WADFile> modWadFile in modWadFiles)
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
                }
            }
        }

        public void UninstallMod(ModFile mod)
        {
            List<ulong> modFiles = new List<ulong>(this.Index.ModFiles[mod.GetID()]);
            Dictionary<string, WADFile> wadFiles = new Dictionary<string, WADFile>();
            List<string> wadsToDelete = new List<string>();

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
                if (!this.Index.WadModAssignments[wadFile.Key].Any(x => x != mod.GetID()) ||
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
            this.Index.RemoveWadsFromMod(mod.GetID());
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

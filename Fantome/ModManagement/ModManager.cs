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
                    this.Index = new LeagueFileIndex(this.LeagueFolder);

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
        public void UninstallMod(ModFile mod)
        {
            Dictionary<string, WADFile> modWadFiles = new Dictionary<string, WADFile>();

            //Collect WAD file name from WAD folders
            foreach (ZipArchiveEntry zipEntry in mod.GetEntries(@"WAD\\\w*.wad.client\\[\s\S]*"))
            {
                string wadName = zipEntry.FullName.Split('\\')[1];

                if (!modWadFiles.ContainsKey(wadName))
                {
                    modWadFiles.Add(wadName, new WADFile(3, 0));
                }
            }

            //Collect WAD files
            foreach (ZipArchiveEntry zipEntry in mod.GetEntries(@"WAD\\\w*.wad.client(?![\\])"))
            {
                string wadName = zipEntry.FullName.Split('\\')[1];

                MemoryStream wadStream = new MemoryStream();

                zipEntry.Open().CopyTo(wadStream);
                modWadFiles.Add(wadName, new WADFile(wadStream));
            }

            //Process WAD files
            foreach (KeyValuePair<string, WADFile> modWadFile in modWadFiles)
            {
                Dictionary<string, Tuple<WADFile, WADFile>> sharedWadFiles = new Dictionary<string, Tuple<WADFile, WADFile>>();

                this.Index.StartEdit();

                //For WAD folders
                foreach (ZipArchiveEntry zipEntry in mod.Content.Entries
                    .Where(x => Regex.IsMatch(x.FullName, string.Format(@"WAD\\{0}\\[\s\S]", modWadFile.Key))) //get only WAD entries, files can be extensionless, thus next step is required
                    .Where(x => x.CompressedLength != 0)) //get only files
                {
                    string path = zipEntry.FullName.Replace(string.Format("WAD\\{0}\\", modWadFile.Key), "").Replace('\\', '/');
                    ulong hash = XXHash.XXH64(Encoding.ASCII.GetBytes(path.ToLower()));

                    this.Index.RemoveModFile(hash);
                }

                //For WAD files
                foreach (WADEntry entry in modWadFile.Value.Entries)
                {
                    List<string> entryWadFiles = new List<string>() { modWadFile.Key };

                    if (this.Index.Game.ContainsKey(entry.XXHash))
                    {
                        entryWadFiles = this.Index.Game[entry.XXHash];

                        //If we detect additional WAD files for this entry
                        if (entryWadFiles.Count > 1)
                        {
                            UninstallAdditionalWadFiles(entry, entryWadFiles, sharedWadFiles);
                        }
                    }

                    this.Index.RemoveModFile(entry.XXHash);
                }

                this.Index.EndEdit();

                string wadPath = this.Index.FindWADPath(modWadFile.Key);
                File.Delete(string.Format(@"{0}\{1}", OVERLAY_FOLDER, wadPath));
            }

            this.Database.ChangeModState(mod.GetModIdentifier(), false);
        }
        public void RemoveMod(ModFile mod)
        {
            if (this.Database.IsInstalled(mod))
            {
                UninstallMod(mod);
            }

            this.Database.RemoveMod(mod.GetModIdentifier());
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

            this.Index.EndEdit();

            //Clean up memory by disposing all WAD files
            foreach (KeyValuePair<string, WADFile> wad in modWadFiles)
            {
                wad.Value.Dispose();
            }
        }
        private void CollectWADFiles(ModFile mod, Dictionary<string, WADFile> modWadFiles)
        {
            foreach (ZipArchiveEntry zipEntry in mod.GetEntries(@"WAD\\\w*.wad.client(?![\\])"))
            {
                string wadName = zipEntry.FullName.Split('\\')[1];

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

                            modWadFiles[additionalWadFile].AddEntryCompressed(entry.XXHash, entry.GetContent(false), entry.UncompressedSize, EntryType.ZStandardCompressed);
                        }
                    }

                    //Add file to Index
                    //TODO: ASSET COLLISION
                    if (this.Index.Game.ContainsKey(entry.XXHash))
                    {
                        this.Index.AddModFile(entry.XXHash, this.Index.Game[entry.XXHash]);
                    }
                    else
                    {
                        this.Index.AddModFile(entry.XXHash, new List<string>() { this.Index.FindWADPath(wadName) });
                    }
                }
            }
        }
        private void CollectWADFolders(ModFile mod, Dictionary<string, WADFile> modWadFiles)
        {
            List<string> wadNames = new List<string>();

            foreach (ZipArchiveEntry zipEntry in mod.GetEntries(@"WAD\\\w*.wad.client\\[\s\S]*"))
            {
                string wadName = zipEntry.FullName.Split('\\')[1];
                string path = zipEntry.FullName.Replace(string.Format("WAD\\{0}\\", wadName), "").Replace('\\', '/');
                ulong hash = XXHash.XXH64(Encoding.ASCII.GetBytes(path.ToLower()));

                MemoryStream memoryStream = new MemoryStream();
                zipEntry.Open().CopyTo(memoryStream);

                if (!modWadFiles.ContainsKey(wadName))
                {
                    modWadFiles.Add(wadName, new WADFile(3, 0));
                }

                modWadFiles[wadName].AddEntry(hash, memoryStream.ToArray(), true);
                wadNames.Add(wadName);
            }

            //Shared Entry Check
            foreach (string wadName in wadNames)
            {
                foreach (WADEntry entry in modWadFiles[wadName].Entries)
                {
                    //Check if the entry is present in the game files or if it's new
                    if (this.Index.Game.ContainsKey(entry.XXHash))
                    {
                        foreach (string additionalWadPath in this.Index.Game[entry.XXHash].Where(x => x != wadName))
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
                        this.Index.AddModFile(entry.XXHash, this.Index.Game[entry.XXHash]);
                    }
                    else
                    {
                        this.Index.AddModFile(entry.XXHash, new List<string>() { this.Index.FindWADPath(wadName) });
                    }
                }
            }
        }
        private void CollectRAWFiles(ModFile mod, Dictionary<string, WADFile> modWadFiles)
        {
            foreach (ZipArchiveEntry zipEntry in mod.GetEntries(@"RAW\\[\s\S]*"))
            {
                string path = zipEntry.FullName.Replace(@"RAW\", "").Replace('\\', '/');
                ulong hash = XXHash.XXH64(Encoding.ASCII.GetBytes(path.ToLower()));
                List<string> fileWadFiles = new List<string>();

                //Check if file exists, if not, we discard it
                if (this.Index.Game.ContainsKey(hash))
                {
                    fileWadFiles = this.Index.Game[hash];
                }
                else
                {
                    continue;
                }

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

                    //Add file to Index
                    //TODO: ASSET COLLISION
                    if (this.Index.Game.ContainsKey(hash))
                    {
                        this.Index.AddModFile(hash, this.Index.Game[hash]);
                    }
                    else
                    {
                        this.Index.AddModFile(hash, new List<string>() { wadFilePath });
                    }
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
                    File.Copy(gameModWadPath, overlayModWadPath + ".temp");
                }
                else
                {
                    File.Move(overlayModWadPath, overlayModWadPath + ".temp");
                }

                using (WADFile mergedWad = WADMerger.Merge(new WADFile(overlayModWadPath + ".temp"), modWadFile.Value))
                {
                    mergedWad.Write(overlayModWadPath);
                }

                //Delete temp wad file
                File.Delete(overlayModWadPath + ".temp");
            }
        }

        private void AddModToDatabase(ModFile mod)
        {
            string id = mod.GetModIdentifier();
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

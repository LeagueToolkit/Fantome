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

        public ModManager(string leagueFolder)
        {
            if (!IsValidLeagueFolder(leagueFolder))
            {
                //Error
            }

            this.LeagueFolder = leagueFolder;

            ProcessLeagueFileIndex();
            ProcessDatabase();
        }

        public void ProcessLeagueFileIndex()
        {
            if (File.Exists(INDEX_FILE))
            {
                this.Index = LeagueFileIndex.Deserialize(File.ReadAllText(INDEX_FILE));
                if (this.Index.Version != GetLeagueVersion())
                {
                    this.Index = new LeagueFileIndex(this.LeagueFolder);
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


            foreach (KeyValuePair<string, bool> mod in this.Database.Mods)
            {
                if (mod.Value)
                {
                    InstallMod(this.Database.GetMod(mod.Key));
                }
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
        public void InstallMod(ModFile mod)
        {
            Dictionary<ulong, List<string>> modIndex = new Dictionary<ulong, List<string>>();
            Dictionary<string, WADFile> wadFiles = new Dictionary<string, WADFile>();

            //Collect WAD files
            foreach (ZipArchiveEntry zipEntry in mod.Content.Entries.Where(x => Regex.IsMatch(x.FullName, @"WAD\\\w*.wad.client\\[\s\S]*")))
            {
                string wadName = zipEntry.FullName.Split('\\')[1];

                if (!wadFiles.ContainsKey(wadName))
                {
                    wadFiles.Add(wadName, new WADFile(3, 0));
                }
            }

            //Collect WAD files
            foreach (ZipArchiveEntry zipEntry in mod.Content.Entries.Where(x => Regex.IsMatch(x.FullName, @"WAD\\\w*.wad.client(?![\\])")))
            {
                string wadName = zipEntry.FullName.Split('\\')[1];

                MemoryStream wadStream = new MemoryStream();

                zipEntry.Open().CopyTo(wadStream);
                wadFiles.Add(wadName, new WADFile(wadStream));
            }

            //Process WAD folder folders
            foreach (KeyValuePair<string, WADFile> wadFile in wadFiles)
            {
                foreach (ZipArchiveEntry zipEntry in mod.Content.Entries
                    .Where(x => Regex.IsMatch(x.FullName, string.Format(@"WAD\\{0}\\[\s\S]", wadFile.Key))) //get only WAD entries, files can be extensionless, thus next step is required
                    .Where(x => x.CompressedLength != 0)) //get only files
                {
                    string path = zipEntry.FullName.Replace(string.Format("WAD\\{0}\\", wadFile.Key), "").Replace('\\', '/');
                    ulong hash = XXHash.XXH64(Encoding.ASCII.GetBytes(path.ToLower()));

                    MemoryStream memoryStream = new MemoryStream();
                    zipEntry.Open().CopyTo(memoryStream);

                    wadFile.Value.AddEntry(hash, memoryStream.ToArray(), true);
                }
            }

            //Now we need to install the WAD files
            foreach (KeyValuePair<string, WADFile> wadFile in wadFiles)
            {
                //Write modded files to index
                foreach (WADEntry entry in wadFile.Value.Entries)
                {
                    this.Index.Mod.Add(entry.XXHash, new List<string>() { wadFile.Key });
                }

                //Load up the origianl WAD for merging
                string wadPath = this.Index.FindWADPath(wadFile.Key);
                WADFile originalWad = new WADFile(string.Format(@"{0}\Game\{1}", this.LeagueFolder, wadPath));

                //Create overlay directory for the mod
                Directory.CreateDirectory(string.Format(@"{0}\{1}", OVERLAY_FOLDER, Path.GetDirectoryName(wadPath)));

                //Merge the modded WAD with the original
                using (WADFile merged = WADMerger.Merge(originalWad, wadFile.Value))
                {
                    merged.Write(string.Format(@"{0}\{1}", OVERLAY_FOLDER, wadPath));
                }
            }

            //Add Mod to database
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
        public void UninstallMod(ModFile mod)
        {
            Dictionary<string, WADFile> wadFiles = new Dictionary<string, WADFile>();

            //Collect WAD files
            foreach (ZipArchiveEntry zipEntry in mod.Content.Entries.Where(x => Regex.IsMatch(x.FullName, @"WAD\\\w*.wad.client\\[\s\S]*")))
            {
                string wadName = zipEntry.FullName.Split('\\')[1];

                if (!wadFiles.ContainsKey(wadName))
                {
                    wadFiles.Add(wadName, new WADFile(3, 0));
                }
            }

            //Collect WAD files
            foreach (ZipArchiveEntry zipEntry in mod.Content.Entries.Where(x => Regex.IsMatch(x.FullName, @"WAD\\\w*.wad.client(?![\\])")))
            {
                string wadName = zipEntry.FullName.Split('\\')[1];

                MemoryStream wadStream = new MemoryStream();

                zipEntry.Open().CopyTo(wadStream);
                wadFiles.Add(wadName, new WADFile(wadStream));
            }

            //Process WAD folder folders
            foreach (KeyValuePair<string, WADFile> wadFile in wadFiles)
            {
                //For WAD folders
                foreach (ZipArchiveEntry zipEntry in mod.Content.Entries
                    .Where(x => Regex.IsMatch(x.FullName, string.Format(@"WAD\\{0}\\[\s\S]", wadFile.Key))) //get only WAD entries, files can be extensionless, thus next step is required
                    .Where(x => x.CompressedLength != 0)) //get only files
                {
                    string path = zipEntry.FullName.Replace(string.Format("WAD\\{0}\\", wadFile.Key), "").Replace('\\', '/');
                    ulong hash = XXHash.XXH64(Encoding.ASCII.GetBytes(path.ToLower()));

                    this.Index.Mod.Remove(hash);
                }

                //For WAD files
                foreach (WADEntry entry in wadFile.Value.Entries)
                {
                    this.Index.Mod.Remove(entry.XXHash);
                }

                string wadPath = this.Index.FindWADPath(wadFile.Key);
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

        private Dictionary<ulong, List<string>> GenerateWadIndex(WADFile wad, string wadName)
        {
            Dictionary<ulong, List<string>> index = new Dictionary<ulong, List<string>>(wad.Entries.Count);

            foreach (WADEntry entry in wad.Entries)
            {
                index.Add(entry.XXHash, new List<string>() { wadName });
            }

            return index;
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

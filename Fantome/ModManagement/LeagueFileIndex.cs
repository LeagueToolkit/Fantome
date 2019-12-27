using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Fantome.Libraries.League.IO.WAD;
using Newtonsoft.Json.Converters;

namespace Fantome.ModManagement
{
    public class LeagueFileIndex
    {
        public Version Version { get; set; } = new Version(0, 0, 0, 0);
        [JsonIgnore] public Dictionary<ulong, List<string>> Game => this._gameIndex;
        [JsonIgnore] public Dictionary<ulong, List<string>> Mod => this._modIndex;
        [JsonIgnore] public Dictionary<string, List<ulong>> ModFiles => this._modFiles;
        [JsonIgnore] public Dictionary<string, List<string>> WadModAssignments => this._wadModAssignments;

        [JsonProperty] private Dictionary<ulong, List<string>> _gameIndex { get; set; } = new Dictionary<ulong, List<string>>();
        [JsonProperty] private Dictionary<ulong, List<string>> _modIndex { get; set; } = new Dictionary<ulong, List<string>>();
        [JsonProperty] private Dictionary<string, List<ulong>> _modFiles { get; set; } = new Dictionary<string, List<ulong>>();
        [JsonProperty] private Dictionary<string, List<string>> _wadModAssignments { get; set; } = new Dictionary<string, List<string>>();

        private bool _shouldWrite = true;

        public LeagueFileIndex() { }
        public LeagueFileIndex(string leagueFolder)
        {
            string wadRootPath = Path.Combine(leagueFolder, @"Game\DATA\FINAL");
            this.Version = new Version(FileVersionInfo.GetVersionInfo(Path.Combine(leagueFolder, @"Game\League of Legends.exe")).FileVersion);

            StartEdit();

            foreach (string wadFile in Directory.GetFiles(wadRootPath, "*", SearchOption.AllDirectories).Where(x => x.Contains(".wad")))
            {
                using (WADFile wad = new WADFile(wadFile))
                {
                    List<ulong> fileHashes = new List<ulong>();
                    foreach (WADEntry entry in wad.Entries)
                    {
                        fileHashes.Add(entry.XXHash);

                        string gameWadPath = wadFile.Replace(Path.Combine(leagueFolder, @"Game\"), "");

                        if (this._gameIndex.ContainsKey(entry.XXHash))
                        {
                            this._gameIndex[entry.XXHash].Add(gameWadPath);
                        }
                        else
                        {
                            this._gameIndex.Add(entry.XXHash, new List<string>() { gameWadPath });
                        }
                    }
                }
            }

            EndEdit();
        }

        public void StartEdit()
        {
            this._shouldWrite = false;
        }
        public void EndEdit()
        {
            this._shouldWrite = true;
            Write();
        }

        public void AddModFile(ulong hash, string modId, List<string> wads)
        {
            //Add file to File-Wad map
            this._modIndex.Add(hash, wads);

            //Add file to Mod-File map
            if (!this._modFiles.ContainsKey(modId))
            {
                this._modFiles.Add(modId, new List<ulong>());
            }
            this._modFiles[modId].Add(hash);

            if (this._shouldWrite)
            {
                Write();
            }
        }
        public void RemoveModFile(ulong hash, string modId)
        {
            this._modIndex.Remove(hash);
            this._modFiles[modId].Remove(hash);

            //Remove Mod entry since it's empty
            if (this._modFiles[modId].Count == 0)
            {
                this._modFiles.Remove(modId);
            }

            if (this._shouldWrite)
            {
                Write();
            }
        }
        public void AssignWadsToMod(string modId, List<string> wads)
        {
            foreach (string wadName in wads)
            {
                string wadPath = FindWADPath(wadName);

                if (!this._wadModAssignments.ContainsKey(wadPath))
                {
                    this._wadModAssignments.Add(wadPath, new List<string>());
                }

                this._wadModAssignments[wadPath].Add(modId);
            }

            if (this._shouldWrite)
            {
                Write();
            }
        }
        public void RemoveWadsFromMod(string modId)
        {
            foreach (KeyValuePair<string, List<string>> wadFile in this._wadModAssignments)
            {
                wadFile.Value.RemoveAll(x => x == modId);
            }
        }

        public string FindWADPath(string wadName)
        {
            foreach (KeyValuePair<ulong, List<string>> file in this._gameIndex)
            {
                foreach (string wadFile in file.Value)
                {
                    if (wadFile.Contains(wadName))
                    {
                        return wadFile;
                    }
                }
            }

            return string.Empty;
        }
        public List<string> GetModWadFiles(string modId)
        {
            List<string> wadFiles = new List<string>();

            foreach (KeyValuePair<string, List<string>> wadFile in this._wadModAssignments)
            {
                if (wadFile.Value.Any(x => x == modId))
                {
                    wadFiles.Add(wadFile.Key);
                }
            }

            return wadFiles;
        }

        public void CopyModData(LeagueFileIndex targetIndex)
        {
            targetIndex.AssignModIndex(targetIndex.Mod);
            targetIndex.AssignModFiles(targetIndex.ModFiles);
            targetIndex.AssignWadModAssignments(targetIndex.WadModAssignments);
        }
        internal void AssignModIndex(Dictionary<ulong, List<string>> modIndex)
        {
            this._modIndex = modIndex;
        }
        internal void AssignModFiles(Dictionary<string, List<ulong>> modFiles)
        {
            this._modFiles = modFiles;
        }
        internal void AssignWadModAssignments(Dictionary<string, List<string>> wadModAssignments)
        {
            this._wadModAssignments = wadModAssignments;
        }

        public static LeagueFileIndex Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<LeagueFileIndex>(json, new VersionConverter());
        }
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new VersionConverter());
        }
        public void Write(string fileLocation = ModManager.INDEX_FILE)
        {
            File.WriteAllText(fileLocation, Serialize());
        }
    }
}

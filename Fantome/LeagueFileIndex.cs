using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Fantome.Libraries.League.IO.WAD;

namespace Fantome
{
    public class LeagueFileIndex
    {
        public Version Version { get; set; } = new Version(0, 0, 0, 0);
        [JsonProperty] private Dictionary<string, List<ulong>> _gameIndex { get; set; } = new Dictionary<string, List<ulong>>();
        [JsonProperty] private Dictionary<string, List<ulong>> _modIndex { get; set; } = new Dictionary<string, List<ulong>>();

        [JsonIgnore] public Dictionary<string, List<ulong>> Game { get => this._gameIndex; }
        [JsonIgnore] public Dictionary<string, List<ulong>> Mod { get => this._modIndex; }

        public LeagueFileIndex(string leagueFolder)
        {
            string wadRootPath = Path.Combine(leagueFolder, "Game/DATA/FINAL");

            this.Version = new Version(FileVersionInfo.GetVersionInfo(Path.Combine(leagueFolder, "Game/League of Legends.exe")).FileVersion);

            foreach (string wadFile in Directory.GetFiles(wadRootPath, "*", SearchOption.AllDirectories).Where(x => x.Contains(".wad")))
            {
                using (WADFile wad = new WADFile(wadFile))
                {
                    List<ulong> fileHashes = new List<ulong>();

                    foreach(WADEntry entry in wad.Entries)
                    {
                        fileHashes.Add(entry.XXHash);
                    }

                    AddGameWAD(wadFile, fileHashes);
                }
            }
        }

        public void AddGameWAD(string name, List<ulong> files)
        {
            this._gameIndex.Add(name, files);
        }
        public void AddGameFile(string name, ulong fileName)
        {
            this._gameIndex[name].Add(fileName);
        }
        public void RemoveGameWAD(string name)
        {
            this._gameIndex.Remove(name);
        }
        public void RemoveGameFile(string name, ulong fileName)
        {
            this._gameIndex[name].Remove(fileName);
        }

        public void AddModWAD(string name, List<ulong> files)
        {
            this._modIndex.Add(name, files);
        }
        public void AddModFile(string name, ulong fileName)
        {
            this._modIndex[name].Add(fileName);
        }
        public void RemoveModWAD(string name)
        {
            this._modIndex.Remove(name);
        }
        public void RemoveModFile(string name, ulong fileName)
        {
            this._modIndex[name].Remove(fileName);
        }

        public static LeagueFileIndex Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<LeagueFileIndex>(json);
        }
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}

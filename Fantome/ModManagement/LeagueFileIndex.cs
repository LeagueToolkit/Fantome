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
        [JsonIgnore] public Dictionary<ulong, List<string>> Game { get => this._gameIndex; }
        [JsonIgnore] public Dictionary<ulong, List<string>> Mod { get => this._modIndex; }

        [JsonProperty] private Dictionary<ulong, List<string>> _gameIndex { get; set; } = new Dictionary<ulong, List<string>>();
        [JsonProperty] private Dictionary<ulong, List<string>> _modIndex { get; set; } = new Dictionary<ulong, List<string>>();

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

        public void AddModFile(ulong hash, List<string> wads)
        {
            this._modIndex.Add(hash, wads);

            if (this._shouldWrite)
            {
                Write();
            }
        }
        public void RemoveModFile(ulong hash)
        {
            this._modIndex.Remove(hash);

            if (this._shouldWrite)
            {
                Write();
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

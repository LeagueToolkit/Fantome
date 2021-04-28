﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Fantome.ModManagement.IO;
using Serilog;
using Fantome.Utilities;
using LeagueToolkit.Helpers.Exceptions;
using LeagueToolkit.IO.WadFile;

namespace Fantome.ModManagement
{
    public class LeagueFileIndex
    {
        public Version Version { get; set; } = new Version(0, 0, 0, 0);
        [JsonIgnore] public Dictionary<ulong, List<string>> Game => this._gameIndex;
        [JsonIgnore] public Dictionary<ulong, List<string>> Mod => this._modIndex;
        [JsonIgnore] public Dictionary<string, List<ulong>> ModEntryMap => this._modEntryMap;
        [JsonIgnore] public Dictionary<ulong, string> EntryModMap => this._entryModMap;
        [JsonIgnore] public Dictionary<string, List<string>> WadModMap => this._wadModMap;

        [JsonProperty] private Dictionary<ulong, List<string>> _gameIndex = new Dictionary<ulong, List<string>>();
        [JsonProperty] private Dictionary<ulong, List<string>> _modIndex = new Dictionary<ulong, List<string>>();
        [JsonProperty] private Dictionary<string, List<ulong>> _modEntryMap = new Dictionary<string, List<ulong>>();
        [JsonProperty] private Dictionary<ulong, string> _entryModMap = new Dictionary<ulong, string>();
        [JsonProperty] private Dictionary<string, List<string>> _wadModMap = new Dictionary<string, List<string>>();

        private Dictionary<ulong, List<string>> _newModIndex = new Dictionary<ulong, List<string>>();
        private Dictionary<string, List<ulong>> _newModEntryMap = new Dictionary<string, List<ulong>>();
        private Dictionary<ulong, string> _newEntryModMap = new Dictionary<ulong, string>();
        private Dictionary<string, List<string>> _newWadModMap = new Dictionary<string, List<string>>();

        private bool _isEditing = true;

        public LeagueFileIndex() { }
        public LeagueFileIndex(string leagueFolder)
        {
            string wadRootPath = Path.Combine(leagueFolder, @"DATA\FINAL");
            this.Version = new Version(FileVersionInfo.GetVersionInfo(Path.Combine(leagueFolder, "League of Legends.exe")).FileVersion);

            foreach (string wadFile in Directory.GetFiles(wadRootPath, "*.wad.client", SearchOption.AllDirectories))
            {
                try
                {
                    using (Wad wad = Wad.Mount(wadFile, false))
                    {
                        List<ulong> fileHashes = new List<ulong>();
                        foreach (var entry in wad.Entries)
                        {
                            fileHashes.Add(entry.Key);

                            string gameWadPath = wadFile.Replace(leagueFolder + Pathing.GetPathSeparator(leagueFolder), "");

                            if (this._gameIndex.ContainsKey(entry.Key))
                            {
                                this._gameIndex[entry.Key].Add(gameWadPath);
                            }
                            else
                            {
                                this._gameIndex.Add(entry.Key, new List<string>() { gameWadPath });
                            }
                        }
                    }
                }
                catch(InvalidFileSignatureException)
                {
                    // Silent error, this can happen because the Riot patcher is broken
                    Log.Error("Found a orrupted/invalid game WAD file: " + wadFile);
                }
                catch(Exception exception)
                {
                    throw new Exception("Failed to index WAD file: " + wadFile, exception);
                }
            }

            Write();
        }

        public void StartEdit()
        {
            Log.Information("Starting Index Edit");
            this._isEditing = true;
        }
        public void EndEdit()
        {
            this._isEditing = false;

            foreach (KeyValuePair<ulong, List<string>> newModEntry in this._newModIndex)
            {
                this._modIndex.Add(newModEntry.Key, newModEntry.Value);
            }
            foreach (KeyValuePair<string, List<ulong>> newModEntry in this._newModEntryMap)
            {
                this._modEntryMap.Add(newModEntry.Key, newModEntry.Value);
            }
            foreach (KeyValuePair<ulong, string> newEntryMod in this._newEntryModMap)
            {
                this._entryModMap.Add(newEntryMod.Key, newEntryMod.Value);
            }
            foreach (KeyValuePair<string, List<string>> newWadMod in this._newWadModMap)
            {
                if(this._wadModMap.ContainsKey(newWadMod.Key))
                {
                    this._wadModMap[newWadMod.Key].AddRange(newWadMod.Value);
                }
                else
                {
                    this._wadModMap.Add(newWadMod.Key, newWadMod.Value);
                }
            }

            Log.Information("Commited Index changes");

            this._newModIndex = new Dictionary<ulong, List<string>>();
            this._newModEntryMap = new Dictionary<string, List<ulong>>();
            this._newEntryModMap = new Dictionary<ulong, string>();
            this._newWadModMap = new Dictionary<string, List<string>>();

            Write();
        }
        public void CancelEdit()
        {
            this._isEditing = false;
            this._newModIndex = new Dictionary<ulong, List<string>>();
            this._newModEntryMap = new Dictionary<string, List<ulong>>();
            this._newEntryModMap = new Dictionary<ulong, string>();
            this._newWadModMap = new Dictionary<string, List<string>>();
        }

        public void AddModFile(ulong hash, string modId, List<string> wads)
        {
            if (this._isEditing)
            {
                //Add to Entry - WAD map
                if (this._newModIndex.ContainsKey(hash))
                {
                    this._newModIndex[hash] = wads;
                }
                else
                {
                    this._newModIndex.Add(hash, wads);
                }

                //Add to Mod - Entry map
                if (this._newModEntryMap.ContainsKey(modId))
                {
                    if(!this._newModEntryMap[modId].Contains(hash))
                    {
                        this._newModEntryMap[modId].Add(hash);
                    }
                }
                else
                {
                    this._newModEntryMap.Add(modId, new List<ulong>() { hash });
                }

                //Add to Entry - Mod map
                if (!this._newEntryModMap.ContainsKey(hash))
                {
                    this._newEntryModMap.Add(hash, modId);
                }
            }
        }
        public void RemoveModdedEntry(ulong hash, string modId)
        {
            if (this._modIndex.ContainsKey(hash))
            {
                this._modIndex.Remove(hash);
            }
            else
            {
                Log.Warning("Unable to remove Entry: {0} installed by {1} in the Mod Index", hash, modId);
            }

            if (this._modEntryMap.ContainsKey(modId))
            {
                this._modEntryMap[modId].Remove(hash);
            }
            else
            {
                Log.Warning("Unable to remove Mod: {0} from Mod Entry Map", modId);
            }

            if (this._entryModMap.ContainsKey(hash))
            {
                this._entryModMap.Remove(hash);
            }
            else
            {
                Log.Warning("Unable to remove Entry: {0} from Entry Mod Map", hash);
            }

            //Remove Mod entry since it's empty
            if (this._modEntryMap.ContainsKey(modId) && this._modEntryMap[modId].Count == 0)
            {
                this._modEntryMap.Remove(modId);
            }

            if (!this._isEditing)
            {
                Write();
            }
        }
        public void AddMod(string modId, List<string> wads)
        {
            if (this._isEditing)
            {
                foreach (string wadPath in wads)
                {
                    if (this._newWadModMap.ContainsKey(wadPath))
                    {
                        this._newWadModMap[wadPath].Add(modId);
                    }
                    else
                    {
                        this._newWadModMap.Add(wadPath, new List<string>() { modId });
                    }

                    Log.Information("Added Mod: {0} to WAD: {1}", modId, wadPath);
                }
            }
        }
        public void RemoveMod(string modId)
        {
            List<string> wadsToRemove = new List<string>();

            foreach (KeyValuePair<string, List<string>> wadFile in this._wadModMap)
            {
                wadFile.Value.RemoveAll(x => x == modId);
                Log.Information("Removed Mod: {0} from WAD: {1} in WAD Mod Map", modId, wadFile.Key);

                if(wadFile.Value.Count == 0)
                {
                    wadsToRemove.Add(wadFile.Key);
                }
            }

            foreach(string wadToRemove in wadsToRemove)
            {
                this._wadModMap.Remove(wadToRemove);
            }
        }

        public List<string> CheckForAssetCollisions(Dictionary<string, WadBuilder> modWadFiles)
        {
            List<string> collidingMods = new List<string>();

            foreach (KeyValuePair<string, WadBuilder> modWadFile in modWadFiles)
            {
                foreach (var entry in modWadFile.Value.Entries)
                {
                    //Check whether the entry is already modded, if it is we add the colliding mod to the list
                    if (this._modIndex.ContainsKey(entry.Key))
                    {
                        string collidingMod = this._entryModMap[entry.Key];
                        if (!collidingMods.Contains(collidingMod))
                        {
                            collidingMods.Add(collidingMod);
                        }
                    }
                }
            }

            return collidingMods;
        }
        public string FindWADPath(string wadName)
        {
            foreach (KeyValuePair<ulong, List<string>> file in this._gameIndex)
            {
                foreach (string wadFile in file.Value)
                {
                    if (wadFile.Contains(wadName, StringComparison.OrdinalIgnoreCase))
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

            foreach (KeyValuePair<string, List<string>> wadFile in this._wadModMap)
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
            targetIndex.SetModIndex(this.Mod);
            targetIndex.SetModEntryMap(this.ModEntryMap);
            targetIndex.SetEntryModMap(this.EntryModMap);
            targetIndex.SetWadModMap(this.WadModMap);
        }
        internal void SetModIndex(Dictionary<ulong, List<string>> modIndex)
        {
            this._modIndex = modIndex;
        }
        internal void SetModEntryMap(Dictionary<string, List<ulong>> modEntryMap)
        {
            this._modEntryMap = modEntryMap;
        }
        internal void SetEntryModMap(Dictionary<ulong, string> entryModMap)
        {
            this._entryModMap = entryModMap;
        }
        internal void SetWadModMap(Dictionary<string, List<string>> wadModMap)
        {
            this._wadModMap = wadModMap;
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

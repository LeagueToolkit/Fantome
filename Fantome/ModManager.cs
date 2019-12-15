using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.Common.IO;

namespace Fantome
{
    public class ModManager
    {
        private const string MOD_FOLDER = "Mods";

        public LeagueFileIndex Index { get; private set; }
        public string LeagueFolder { get; set; }
        public string OverlayFolder { get; set; }
        public List<ModFile> InstalledMods { get; set; } = new List<ModFile>();

        public ModManager(string leagueFolder, string overlayFolder)
        {
            if(!IsValidLeagueFolder(leagueFolder))
            {

            }

            this.LeagueFolder = leagueFolder;
            this.OverlayFolder = overlayFolder;

            BuildLeagueFileIndex();
        }

        public void BuildLeagueFileIndex()
        {
            if (File.Exists("LeagueFileIndex.json"))
            {
                //Check difference between Index and the League executable version
                Version leagueVersion = GetLeagueVersion();
                this.Index = LeagueFileIndex.Deserialize(File.ReadAllText("LeagueFileIndex.json"));

                if(leagueVersion > this.Index.Version)
                {
                    LeagueFileIndex newIndex = new LeagueFileIndex(this.LeagueFolder);

                    //Find new WAD files and also check file changes (addition)
                    foreach (KeyValuePair<string, List<ulong>> wadFile in newIndex.Game)
                    {
                        if(!this.Index.Game.ContainsKey(wadFile.Key))
                        {
                            this.Index.AddGameWAD(wadFile.Key, wadFile.Value);
                        }
                        else
                        {
                            foreach(ulong fileHash in wadFile.Value)
                            {
                                //Check if a file is new
                                if(!this.Index.Game[wadFile.Key].Contains(fileHash))
                                {
                                    this.Index.AddGameFile(wadFile.Key, fileHash);
                                }
                            }
                        }
                    }

                    //Find removed WAD files and also check file changes (removal)
                    foreach (KeyValuePair<string, List<ulong>> wadFile in this.Index.Game)
                    {
                        if (!newIndex.Game.ContainsKey(wadFile.Key))
                        {
                            this.Index.RemoveGameWAD(wadFile.Key);
                        }
                        else
                        {
                            foreach (ulong fileHash in wadFile.Value)
                            {
                                //Check if a file is removed
                                if (!newIndex.Game[wadFile.Key].Contains(fileHash))
                                {
                                    this.Index.RemoveGameFile(wadFile.Key, fileHash);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                this.Index = new LeagueFileIndex(this.LeagueFolder);
            }
        }

        public void InstallMod(ModFile mod)
        {

        }

        public void UninstallMod(ModFile mod)
        {

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

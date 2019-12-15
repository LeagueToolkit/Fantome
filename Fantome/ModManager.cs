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

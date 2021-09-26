using Fluxor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Store.Modules.Config
{
    public class Feature : Feature<ConfigState>
    {
        public override string GetName() => "Config";
        protected override ConfigState GetInitialState() => new()
        {
            LeagueLocation = "",
            LoggingPattern = "{Timestamp:dd-MM-yyyy HH:mm:ss.fff} | [{Level}] | {Message:lj}{NewLine}{Exception}",
            GameHashtablePath = "GAME_HASHTABLE.json",
            LCUHashtablePath = "LCU_HASHTABLE.json",
            GameHashtableChecksum = "",
            LCUHashtableChecksum = "",
            PackedBinRegex = @"^DATA/.*_(Skins_Skin|Tiers_Tier|(Skins|Tiers)_Root).*\.bin$",
            PackedBinKeywords = new string[] { "Skins", "Tiers" },
            GenerateHashesFromBin = false,
            SyncHashes = true
        };
    }
}

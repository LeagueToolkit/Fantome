using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Store.Modules.Config
{
    public class ConfigState
    {
        public string LeagueLocation { get; init; }
        public string LoggingPattern { get; init; }
        public string GameHashtableChecksum {  get; init; }
        public string LCUHashtableChecksum { get; init; }
        public string PackedBinRegex {  get; init; }
        public string[] PackedBinKeywords { get; init; }
        public bool GenerateHashesFromBin {  get; init; }
        public bool SyncHashes { get; init;  }
    }
}

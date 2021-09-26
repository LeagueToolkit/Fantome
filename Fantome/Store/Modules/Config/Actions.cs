using System;

namespace Fantome.Store.Modules.Config
{
    public class FetchConfigAction
    {
        public class Request : AsyncActionRequest { }
        public class Success : AsyncActionSuccess
        {
            public ConfigState Config { get; set; }
        }
        public class Failure : AsyncActionFailure
        {
            public Exception Error { get; set; }
        }
    }

    public class SetLeagueLocationAction : ConfigAction
    {
        public string LeagueLocation { get; set; }
    }
    public class SetGameHashtableChecksumAction : ConfigAction 
    {
        public string GameHashtableChecksum { get; set; }
    }
    public class SetLCUHashtableChecksumAction : ConfigAction
    {
        public string LCUHashtableChecksum { get; set; }
    }
}

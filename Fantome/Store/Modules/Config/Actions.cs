using System;

namespace Fantome.Store.Modules.Config
{
    public class FetchConfigAction
    {
        public class Request : AsyncActionRequest { }
        public class Success : AsyncActionSuccess { }
        public class Failure : AsyncActionFailure
        {
            public Exception Error { get; set; }
        }
    }

    public class SetConfigAction : ConfigAction
    {
        public ConfigState Config { get; set; }
    }

    public class SetLeagueLocationAction : ConfigAction
    {
        public string LeagueLocation { get; set; }
    }
}

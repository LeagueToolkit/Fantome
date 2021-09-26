using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fluxor;

namespace Fantome.Store.Modules.Config
{
    public static class Reducers
    {
        [ReducerMethod]
        public static ConfigState HandleFetchConfigSuccess(ConfigState state, FetchConfigAction.Success action)
        {
            return action.Config;
        }

        [ReducerMethod]
        public static ConfigState HandleSetLeagueLocation(ConfigState state, SetLeagueLocationAction action)
        {
            return state with { LeagueLocation = action.LeagueLocation };
        }

        [ReducerMethod]
        public static ConfigState HandleSetGameHashtableChecksum(ConfigState state, SetGameHashtableChecksumAction action)
        {
            return state with { GameHashtableChecksum = action.GameHashtableChecksum };
        }
        [ReducerMethod]
        public static ConfigState HandleSetLCUHashtableChecksum(ConfigState state, SetLCUHashtableChecksumAction action)
        {
            return state with { LCUHashtableChecksum = action.LCUHashtableChecksum };
        }
    }
}

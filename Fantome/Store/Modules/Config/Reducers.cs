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
        public static ConfigState HandleSetConfig(ConfigState state, SetConfigAction action)
        {
            return action.Config;
        }

        [ReducerMethod]
        public static ConfigState HandleSetLeagueLocation(ConfigState state, SetLeagueLocationAction action)
        {
            return state with { LeagueLocation = action.LeagueLocation };
        }
    }
}

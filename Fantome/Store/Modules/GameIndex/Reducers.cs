using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fluxor;

namespace Fantome.Store.Modules.GameIndex
{
    public static class FetchGameIndexReducers
    {
        [ReducerMethod]
        public static GameIndexState HandleBuildGameIndexSuccess(GameIndexState state, BuildGameIndexAction.Success action)
        {
            return state with { WadToEntriesMap = new(action.GameIndex.WadToEntriesMap) };
        }
    }
}

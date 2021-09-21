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
        public static GameIndexState HandleFetchGameIndex(GameIndexState state, FetchGameIndexAction action)
        {
            return state with { IsLoading = true };
        }

        [ReducerMethod]
        public static GameIndexState HandleFetchGameIndexSuccess(GameIndexState state, FetchGameIndexSuccessAction action)
        {
            return state with { IsLoading = false, WadToEntriesMap = new(action.GameIndex.WadToEntriesMap) };
        }
    }
}

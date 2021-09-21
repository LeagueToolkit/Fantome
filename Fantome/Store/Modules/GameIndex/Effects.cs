using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.Core;
using Fantome.Store.Modules.Config;
using Fluxor;
using Microsoft.AspNetCore.Components;

namespace Fantome.Store.Modules.GameIndex
{
    public class FetchGameIndexEffects
    {
        [EffectMethod]
        public async Task HandleFetchGameIndex(FetchGameIndexAction action, IDispatcher dispatcher)
        {
            try
            {
                GameIndexStorage indexStorage = await Core.GameIndex.FetchAsync();
            }
            catch(FileNotFoundException)
            {
                GameIndexStorage indexStorage = Core.GameIndex.ScanGameLocation(action.GameLocation);

                dispatcher.Dispatch(new FetchGameIndexSuccessAction() { GameIndex = indexStorage });
            }
        }
    }

    public class Effects
    {
        [EffectMethod]
        public Task HandleSetLeagueLocation(SetLeagueLocationAction action, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new FetchGameIndexAction() { GameLocation = action.LeagueLocation });

            return Task.CompletedTask;
        }
    }
}

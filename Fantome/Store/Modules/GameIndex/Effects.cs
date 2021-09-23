using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.Core;
using Fantome.Core.Exceptions;
using Fantome.Store.Modules.Config;
using Fluxor;
using Microsoft.AspNetCore.Components;

namespace Fantome.Store.Modules.GameIndex
{
    public class BuildGameIndexEffects
    {
        private readonly IState<ConfigState> _config;

        public BuildGameIndexEffects(IState<ConfigState> config)
        {
            this._config = config;
        }

        [EffectMethod]
        public Task HandleBuildGameIndex(BuildGameIndexAction _, IDispatcher dispatcher)
        {
            try
            {
                GameIndexStorage indexStorage = Core.GameIndex.ScanGameLocation(this._config.Value.LeagueLocation);

                dispatcher.Dispatch(new BuildGameIndexSuccessAction() { GameIndex = indexStorage });
            }
            catch (Exception exception)
            {
                dispatcher.Dispatch(new BuildGameIndexErrorAction() { Exception = exception });
            }

            return Task.CompletedTask;
        }
    }

    public class Effects
    {
        [EffectMethod]
        public Task HandleSetLeagueLocation(SetLeagueLocationAction _, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new BuildGameIndexAction());

            return Task.CompletedTask;
        }
    }
}

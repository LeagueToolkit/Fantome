using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.Core;
using Fantome.Store.Modules.Config;
using Fantome.Store.Modules.GameIndex;
using Fluxor;

namespace Fantome.Services.WadRepository
{
    public interface IWadRepositoryService
    {
        Task Synchronize();
    }

    public class WadRepositoryService : IWadRepositoryService
    {
        private IState<GameIndexState> _gameIndex;
        private IState<ConfigState> _config;

        public WadRepositoryService(IState<GameIndexState> gameIndex, IState<ConfigState> config)
        {
            this._gameIndex = gameIndex;
            this._config = config;
        }

        public Task Synchronize()
        {


            return Task.CompletedTask;
        }
    }
}

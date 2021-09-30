using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.Services.WadRepository;
using Fantome.Store.Modules.GameIndex;
using Fantome.Store.Modules.Hashtable;
using Fluxor;
using LeagueToolkit.IO.WadFile;

namespace Fantome.Store.Modules.Explorer
{
    public class Effects
    {
        private readonly IWadRepositoryService _wadRepository;
        private readonly IState<GameIndexState> _gameIndex;
        private readonly IState<HashtableState> _hashtable;

        public Effects(IWadRepositoryService wadRepository, IState<GameIndexState> gameIndex, IState<HashtableState> hashtable)
        {
            this._wadRepository = wadRepository;
            this._gameIndex = gameIndex;
            this._hashtable = hashtable;
        }

        [EffectMethod]
        public async Task HandleAddWadRequest(AddWadAction.Request action, IDispatcher dispatcher)
        {
            try
            {
                using Wad wad = Wad.Mount(action.WadFileLocation, false);


            }
            catch (Exception exception)
            {
                dispatcher.Dispatch(new AddWadAction.Failure() { Error = exception });
            }
        }

        [EffectMethod]
        public async Task HandleSynchronizeWadRepositoryRequest(SynchronizeWadRepositoryAction.Request action, IDispatcher dispatcher)
        {
            WadFolder rootFolder = await this._wadRepository.Synchronize(this._gameIndex.Value, this._hashtable.Value);

            dispatcher.Dispatch(new SynchronizeWadRepositoryAction.Success() { RootFolder = rootFolder });
        }
    }
}

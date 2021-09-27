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
        private IWadRepositoryService _wadRepository;
        private IState<GameIndexState> _gameIndex;
        private IState<HashtableState> _hashtable;

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
            await this._wadRepository.Synchronize(this._gameIndex.Value, this._hashtable.Value);

            dispatcher.Dispatch(new SynchronizeWadRepositoryAction.Success());
        }
    }
}

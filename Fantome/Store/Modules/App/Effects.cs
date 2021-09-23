using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.Store.Modules.Config;
using Fluxor;

namespace Fantome.Store.Modules.App
{
    public class AppEffects
    {
        [EffectMethod]
        public Task HandleStoreInitializedAction(StoreInitializedAction action, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new FetchConfigAction.Request());

            return Task.CompletedTask;
        }
    }
}

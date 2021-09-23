using System;
using Fluxor;

namespace Fantome.Store.Modules.App
{
    public class Feature : Feature<AppState>
    {
        public override string GetName() => "App";
        protected override AppState GetInitialState() => new()
        { 
            ActionStates = new()
        };
    }
}

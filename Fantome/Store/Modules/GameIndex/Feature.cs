using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fluxor;

namespace Fantome.Store.Modules.GameIndex
{
    public class Feature : Feature<GameIndexState>
    {
        public override string GetName() => "GameIndex";
        protected override GameIndexState GetInitialState() => new()
        { 
            WadToEntriesMap = new(new Dictionary<string, List<ulong>>()),
            EntryToWadsMap = new(new Dictionary<ulong, List<string>>())
        };
    }
}

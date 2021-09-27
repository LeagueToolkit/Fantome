using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Store.Modules.GameIndex
{
    public record GameIndexState
    {
        public ReadOnlyDictionary<string, List<ulong>> WadToEntriesMap { get; init; }
        public ReadOnlyDictionary<ulong, List<string>> EntryToWadsMap { get; init; }
    }
}

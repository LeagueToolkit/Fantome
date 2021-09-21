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
        public bool IsLoading { get; init; }
        public ReadOnlyDictionary<string, List<ulong>> WadToEntriesMap { get; init; }
    }
}

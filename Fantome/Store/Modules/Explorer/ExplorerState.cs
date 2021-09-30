using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueToolkit.IO.WadFile;

namespace Fantome.Store.Modules.Explorer
{
    public record ExplorerState
    {
        public HashSet<ExplorerWadItem> IndexedItems { get; set; }
    }

    public record ExplorerWadItem
    {
        public bool IsExpanded { get; set; }

        public string Name { get; init; }
        public string Path { get; init; }

        public HashSet<ExplorerWadItem> Items { get; init; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueToolkit.IO.WadFile;

namespace Fantome.Store.Modules.Extractor
{
    public record ExtractorState
    {
        public Dictionary<string, ExtractorWadState> WadFiles { get; init; }


    }

    public record ExtractorWadState
    {

    }
}

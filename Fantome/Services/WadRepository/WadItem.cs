using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Services.WadRepository
{
    public abstract class WadItem
    {
        public WadItem Parent { get; init; }

        public string Name { get; init; }
        public string Path { get; init; }
    }
}

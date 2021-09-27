using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Services.WadRepository
{
    public class WadFile : WadItem
    {
        public WadFile(WadItem parent, string path)
        {
            this.Parent = parent;
            this.Path = path;
        }
    }
}

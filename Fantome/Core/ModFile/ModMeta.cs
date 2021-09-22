using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Core.ModFile
{
    public class ModMeta
    {
        public string Name { get; private set; }
        public string Author { get; private set; }
        public string Version { get; private set; }
        public string Description { get; private set; }
        public List<string> Tags { get; private set; }
        public List<string> Dependencies { get; private set; }
        public List<string> Provides { get; private set; }
    }
}

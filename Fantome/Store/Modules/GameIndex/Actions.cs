using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.Core;

namespace Fantome.Store.Modules.GameIndex
{
    public class FetchGameIndexAction
    {
        public string GameLocation { get; set; }
    }
    public class FetchGameIndexSuccessAction
    {
        public GameIndexStorage GameIndex { get; set; }
    }
}

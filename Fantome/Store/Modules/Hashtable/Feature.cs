using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fluxor;

namespace Fantome.Store.Modules.Hashtable
{
    public class Feature : Feature<HashtableState>
    {
        public override string GetName() => "Hashtable";
        protected override HashtableState GetInitialState() => new();
    }
}

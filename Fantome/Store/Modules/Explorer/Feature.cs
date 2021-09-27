using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fluxor;

namespace Fantome.Store.Modules.Explorer
{
    public class Feature : Feature<ExplorerState>
    {
        public override string GetName() => "Explorer";
        protected override ExplorerState GetInitialState() => new() { };
    }
}

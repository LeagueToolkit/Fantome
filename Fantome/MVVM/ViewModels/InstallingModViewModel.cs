using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.ModManagement;
using Fantome.ModManagement.IO;

namespace Fantome.MVVM.ViewModels
{
    public class InstallingModViewModel
    {
        public ModFile Mod { get; private set; }
        public ModManager ModManager { get; private set; }

        public InstallingModViewModel(ModFile mod, ModManager modManager)
        {
            this.Mod = mod;
            this.ModManager = modManager;
        }

        public void Install()
        {
            this.ModManager.InstallMod(this.Mod);
        }
    }
}

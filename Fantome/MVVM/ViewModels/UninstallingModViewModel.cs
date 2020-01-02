using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.ModManagement;
using Fantome.ModManagement.IO;

namespace Fantome.MVVM.ViewModels
{
    public class UninstallingModViewModel
    {
        public ModFile Mod { get; private set; }
        public ModManager ModManager { get; private set; }

        public string UninstallingString => "Uninstalling " + this.Mod.GetID();

        public UninstallingModViewModel(ModFile mod, ModManager modManager)
        {
            this.Mod = mod;
            this.ModManager = modManager;
        }

        public void Uninstall()
        {
            this.ModManager.UninstallMod(this.Mod);
        }
    }
}

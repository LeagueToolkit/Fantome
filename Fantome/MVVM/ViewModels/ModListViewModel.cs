using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Fantome.ModManagement;
using Fantome.ModManagement.IO;
using Fantome.Utilities;
using Serilog;

namespace Fantome.MVVM.ViewModels
{
    public class ModListViewModel : PropertyNotifier
    {
        public ObservableCollection<ModListItemViewModel> Items
        {
            get => this._items;
            set
            {
                this._items = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<ModListItemViewModel> _items = new ObservableCollection<ModListItemViewModel>();
        private ModManager _modManager;

        public ModListViewModel(ModManager modManager)
        {
            this._modManager = modManager;
        }

        public void SyncWithModManager()
        {
            //Remove non-existant mods
            List<ModListItemViewModel> toRemove = new List<ModListItemViewModel>();
            foreach (ModListItemViewModel modItem in this.Items)
            {
                if (!this._modManager.Database.ContainsMod(modItem.Mod.GetID()))
                {
                    toRemove.Add(modItem);
                }
            }
            foreach (ModListItemViewModel modItem in toRemove)
            {
                RemoveMod(modItem);
            }


            //Check for new mods
            foreach (KeyValuePair<string, bool> modEntry in this._modManager.Database.Mods)
            {
                this.Items.Add(new ModListItemViewModel(this._modManager.Database.GetMod(modEntry.Key), this._modManager, this));
                this.Items.Last().IsInstalled = modEntry.Value;
            }
        }

        public async Task AddMod(ModFile mod, bool install)
        {
            if (this.Items.Any(x => x.Mod == mod))
            {
                await DialogHelper.ShowMessageDialog("A Mod with the same ID has already been added");
                Log.Information("Cannot load Mod: {0} because it is already present in the databse", mod.GetID());
            }
            else
            {
                string validationError = mod.Validate(this._modManager.Index);
                if (!string.IsNullOrEmpty(validationError))
                {
                    await DialogHelper.ShowMessageDialog(validationError);
                }
                else
                {
                    ModListItemViewModel modListItem = new ModListItemViewModel(mod, this._modManager, this);
                    this.Items.Add(modListItem);

                    if (install)
                    {
                        await modListItem.Install(true);
                    }
                }
            }
        }
        public void RemoveMod(ModListItemViewModel mod)
        {
            this.Items.Remove(mod);
        }
    }
}

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
    public class ModListViewModel : INotifyPropertyChanged
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

        public event PropertyChangedEventHandler PropertyChanged;

        public ModListViewModel(ModManager modManager)
        {
            this._modManager = modManager;
        }

        public void Sync()
        {
            foreach (KeyValuePair<string, bool> modEntry in this._modManager.Database.Mods)
            {
                this.Items.Add(new ModListItemViewModel(this._modManager.Database.GetMod(modEntry.Key), modEntry.Value, this._modManager, this));
            }
        }

        public void AddMod(ModFile mod, bool install)
        {
            if (this.Items.Any(x => x.Mod == mod))
            {
                DialogHelper.ShowMessageDialog("A Mod with the same ID has already been added");
                Log.Information("Cannot load Mod: {0} because it is already present in the databse", mod.GetID());
            }
            else
            {
                this.Items.Add(new ModListItemViewModel(mod, install, this._modManager, this));
            }
        }
        public void RemoveMod(ModListItemViewModel mod)
        {
            this.Items.Remove(mod);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

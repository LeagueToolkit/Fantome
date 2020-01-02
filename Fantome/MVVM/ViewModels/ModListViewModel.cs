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
        public ObservableCollection<ModCardViewModel> Items
        {
            get => this._items;
            set
            {
                this._items = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<ModCardViewModel> _items = new ObservableCollection<ModCardViewModel>();
        private ModManager _modManager;

        public event PropertyChangedEventHandler PropertyChanged;

        public ModListViewModel(ModManager modManager)
        {
            this._modManager = modManager;
        }

        public void Sync()
        {
            foreach(KeyValuePair<string, bool> modEntry in this._modManager.Database.Mods)
            {
                this.Items.Add(new ModCardViewModel(this._modManager.Database.GetMod(modEntry.Key), modEntry.Value, this._modManager, this));
            }
        }

        public void AddMod(ModFile mod, bool install)
        {
            this.Items.Add(new ModCardViewModel(mod, install, this._modManager, this));
        }
        public void RemoveMod(ModCardViewModel mod)
        {
            this.Items.Remove(mod);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

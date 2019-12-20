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

        public void AddMod(ModFile mod)
        {
            this.Items.Add(new ModCardViewModel(mod, this._modManager));
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

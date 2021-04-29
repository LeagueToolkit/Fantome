using Fantome.ModManagement.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.MVVM.ViewModels
{
    public class ModListCategoryViewModel : PropertyNotifier
    {
        public ModCategory Category { get; private set; }
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

        public ModListCategoryViewModel(ModCategory category, ObservableCollection<ModListItemViewModel> items)
        {
            this.Category = category;
            this._items = items;
        }
    }
}

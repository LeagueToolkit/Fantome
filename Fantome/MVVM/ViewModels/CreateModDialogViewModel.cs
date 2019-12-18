using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Fantome.ModManagement.IO;

namespace Fantome.MVVM.ViewModels
{
    public class CreateModDialogViewModel : INotifyPropertyChanged
    {
        public string WadLocation
        {
            get => this._wadLocation;
            set => NotifyPropertyChanged();
        }
        public string RawLocation
        {
            get => this._rawLocation;
            set => NotifyPropertyChanged();
        }
        public ModInfo Info
        {
            get => this._info;
            set => NotifyPropertyChanged();
        }

        private string _wadLocation;
        private string _rawLocation;
        private ModInfo _info;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

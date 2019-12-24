using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Fantome.UserControls.Dialogs;
using Fantome.ModManagement;
using Fantome.ModManagement.IO;
using Fantome.MVVM.Commands;
using MaterialDesignThemes.Wpf;

using SImage = System.Drawing.Image;

namespace Fantome.MVVM.ViewModels
{
    public class CreateModDialogViewModel : INotifyPropertyChanged
    {
        public string WadLocation
        {
            get => this._wadLocation;
            set
            {
                this._wadLocation = value;
                NotifyPropertyChanged();
            }
        }
        public string RawLocation
        {
            get => this._rawLocation;
            set
            {
                this._rawLocation = value;
                NotifyPropertyChanged();
            }
        }
        public string Name
        {
            get => this._name;
            set
            {
                this._name = value;
                NotifyPropertyChanged();
            }
        }
        public string Author
        {
            get => this._author;
            set
            {
                this._author = value;
                NotifyPropertyChanged();
            }
        }
        public string Version
        {
            get => this._version;
            set
            {
                this._version = value;
                NotifyPropertyChanged();
            }
        }
        public BitmapImage Image 
        {
            get => this._image;
            set
            {
                this._image = value;
                NotifyPropertyChanged();
            }
        }

        private string _wadLocation;
        private string _rawLocation;
        private string _name;
        private string _author;
        private string _version;
        private BitmapImage _image;
        private ModListViewModel _modList;

        public ICommand RunDialogComdmand => new RelayCommand(ExecuteRunDialog);

        public event PropertyChangedEventHandler PropertyChanged;

        public CreateModDialogViewModel(ModListViewModel modList)
        {
            this._modList = modList;
        }

        private async void ExecuteRunDialog(object o)
        {
            CreateModDialog view = new CreateModDialog
            {
                DataContext = this
            };


            object result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);

            if((bool)result == true)
            {
                ModInfo info = new ModInfo(this._name, this._author, new Version(this._version), "");
                SImage image = null;
                if(this._image != null)
                {
                    image = SImage.FromStream(this._image.StreamSource);
                }

                ModFile mod = new ModFile(this._wadLocation, this._rawLocation, info, image);
                this._modList.AddMod(mod, false);
            }
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {

        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

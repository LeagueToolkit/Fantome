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
    public class CreateModDialogViewModel : PropertyNotifier
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

        public ModManager ModManager { get; private set; }

        private string _wadLocation;
        private string _rawLocation;
        private string _name;
        private string _author;
        private string _version = "1.0";
        private BitmapImage _image;
        private ModListViewModel _modList;

        public CreateModDialogViewModel(ModListViewModel modList, ModManager modManager)
        {
            this.ModManager = modManager;
            this._modList = modList;
        }

        public async void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter == true)
            {
                ModInfo info = new ModInfo(this._name, this._author, new Version(this._version), "");
                SImage image = null;
                if (this._image != null)
                {
                    image = SImage.FromStream(this._image.StreamSource);
                }

                ModFile mod = new ModFile(this.ModManager, this._wadLocation, this._rawLocation, info, image);
                await this._modList.AddMod(mod, false);
            }
        }
    }
}
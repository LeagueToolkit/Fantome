using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Fantome.ModManagement;
using Fantome.ModManagement.IO;

namespace Fantome.MVVM.ViewModels
{
    public class ModCardViewModel : INotifyPropertyChanged
    {
        public bool IsInstalled 
        {
            get => this._isInstalled;
            set
            {
                this._isInstalled = value;
                NotifyPropertyChanged();
            }
        }
        public string Name { get => this._mod.Info.Name; }
        public string Author { get => this._mod.Info.Author; }
        public BitmapImage Image { get => this._image; }

        private bool _isInstalled;
        private BitmapImage _image;
        private ModFile _mod;
        private ModManager _modManager;
        private ModListViewModel _modList;

        public event PropertyChangedEventHandler PropertyChanged;

        public ModCardViewModel(ModFile mod, ModManager _modManager, ModListViewModel modList)
        {
            this._mod = mod;
            this._modManager = _modManager;
            this._modList = modList;

            if(mod.Image != null)
            {
                MemoryStream memoryStream = new MemoryStream();
                BitmapImage bitmap = new BitmapImage();

                mod.Image.Save(memoryStream, ImageFormat.Png);
                bitmap.BeginInit();
                bitmap.StreamSource = memoryStream;
                bitmap.EndInit();

                this._image = bitmap;
            }
        }

        public void Install()
        {
            this._modManager.InstallMod(this._mod);
            this.IsInstalled = true;
        }
        public void Uninstall()
        {
            this._modManager.UninstallMod(this._mod);
            this.IsInstalled = false;
        }
        public void Remove()
        {
            this._modList.RemoveMod(this);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

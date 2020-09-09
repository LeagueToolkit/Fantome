using Fantome.ModManagement;
using Fantome.ModManagement.IO;
using Fantome.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Fantome.MVVM.ViewModels
{
    public class ModListItemViewModel : PropertyNotifier
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
        public string Name => this.Mod.Info.Name;
        public string Author => this.Mod.Info.Author;
        public string Version => this.Mod.Info.Version.ToString();
        public BitmapImage Image => this._image;
        public ModFile Mod { get; private set; }

        private bool _isInstalled;
        private BitmapImage _image;

        private ModListViewModel _modList;

        public ModListItemViewModel(ModFile mod, ModListViewModel modList)
        {
            this.Mod = mod;
            this._modList = modList;

            if (mod.Image != null)
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

        public async Task Install(bool forceInstall = false)
        {
            await this._modList.InstallMod(this, forceInstall);
        }
        public async Task Uninstall(bool forceUninstall = false)
        {
            await this._modList.UninstallMod(this, forceUninstall);
        }
        public void Remove()
        {
            this._modList.RemoveMod(this);
        }
    }
}

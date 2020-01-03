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
using Fantome.UserControls.Dialogs;
using Fantome.Utilities;
using MaterialDesignThemes.Wpf;

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
        public string Name => this.Mod.Info.Name;
        public string Author => this.Mod.Info.Author;
        public string Version => this.Mod.Info.Version.ToString();
        public BitmapImage Image => this._image;
        public ModFile Mod { get; private set; }

        private bool _isInstalled;
        private BitmapImage _image;
        private ModManager _modManager;
        private ModListViewModel _modList;

        public event PropertyChangedEventHandler PropertyChanged;

        public ModCardViewModel(ModFile mod, bool isInstalled, ModManager _modManager, ModListViewModel modList)
        {
            this.Mod = mod;
            this._modManager = _modManager;
            this._modList = modList;
            this._isInstalled = isInstalled;

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

        public async void Install()
        {
            if (this.IsInstalled && !this._modManager.Database.IsInstalled(this.Mod))
            {
                //Validate Mod before installation
                string validationError = this.Mod.Validate(this._modManager);
                if(!string.IsNullOrEmpty(validationError))
                {
                    DialogHelper.ShowMessageDialog(validationError);
                    this.IsInstalled = false;
                }
                else
                {
                    await Installation.Install(this.Mod, this._modManager);
                    this.IsInstalled = true;
                }
            }
        }
        public async void Uninstall()
        {
            if (!this.IsInstalled && this._modManager.Database.IsInstalled(this.Mod))
            {
                await Installation.Uninstall(this.Mod, this._modManager);
                this.IsInstalled = false;
            }
        }
        public void Remove()
        {
            this._modList.RemoveMod(this);
            this._modManager.RemoveMod(this.Mod);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

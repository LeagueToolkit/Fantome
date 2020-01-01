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
        public string Name { get => this._mod.Info.Name; }
        public string Author { get => this._mod.Info.Author; }
        public BitmapImage Image { get => this._image; }

        private bool _isInstalled;
        private BitmapImage _image;
        private ModFile _mod;
        private ModManager _modManager;
        private ModListViewModel _modList;

        public event PropertyChangedEventHandler PropertyChanged;

        public ModCardViewModel(ModFile mod, bool isInstalled, ModManager _modManager, ModListViewModel modList)
        {
            this._mod = mod;
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
            if (this.IsInstalled && !this._modManager.Database.IsInstalled(this._mod))
            {
                InstallingModDialog dialog = new InstallingModDialog()
                {
                    DataContext = new InstallingModViewModel(this._mod, this._modManager)
                };

                await DialogHost.Show(dialog, "RootDialog", dialog.StartInstallation, null);
                this.IsInstalled = true;
            }
        }
        public void Uninstall()
        {
            if (!this.IsInstalled && this._modManager.Database.IsInstalled(this._mod))
            {
                this._modManager.UninstallMod(this._mod);
                this.IsInstalled = false;
            }
        }
        public void Remove()
        {
            this._modList.RemoveMod(this);
            this._modManager.RemoveMod(this._mod);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

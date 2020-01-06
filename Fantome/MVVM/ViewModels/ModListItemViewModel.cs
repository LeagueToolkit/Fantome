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
    public class ModListItemViewModel : INotifyPropertyChanged
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

        public ModListItemViewModel(ModFile mod, bool isInstalled, ModManager _modManager, ModListViewModel modList)
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
                if (!string.IsNullOrEmpty(validationError))
                {
                    DialogHelper.ShowMessageDialog(validationError);
                    this.IsInstalled = false;
                }
                else
                {
                    //Now we need to check for asset collisions
                    List<string> collisions = this._modManager.Index.CheckForAssetCollisions(this.Mod.WadFiles);
                    if (collisions.Count != 0)
                    {
                        object result = await DialogHelper.ShowAssetConflictDialog(this.Mod, collisions);
                        if ((bool)result == true)
                        {
                            foreach (string collision in collisions)
                            {
                                ModFile collisionMod = this._modManager.Database.GetMod(collision);
                                ModListItemViewModel modViewModel = this._modList.Items.FirstOrDefault(x => x.Mod == collisionMod);
                                
                                await modViewModel.Uninstall(true);
                            }
                        }
                        else
                        {
                            this.IsInstalled = false;
                            return;
                        }
                    }

                    await DialogHelper.InstallMod(this.Mod, this._modManager);
                    this.IsInstalled = true;
                }
            }
        }
        public async Task<object> Uninstall(bool forceUninstall = false)
        {
            if ((!this.IsInstalled && this._modManager.Database.IsInstalled(this.Mod)) || forceUninstall)
            {
                object result = await DialogHelper.UninstallMod(this.Mod, this._modManager);
                this.IsInstalled = false;

                return result;
            }

            return null;
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

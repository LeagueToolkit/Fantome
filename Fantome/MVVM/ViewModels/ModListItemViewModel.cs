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
        private ModManager _modManager;
        private ModListViewModel _modList;

        public ModListItemViewModel(ModFile mod, ModManager modManager, ModListViewModel modList)
        {
            this.Mod = mod;
            this._modManager = modManager;
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
            if ((this.IsInstalled && !this._modManager.Database.IsInstalled(this.Mod)) || forceInstall)
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
                    //Generate WAD files for the Mod
                    await DialogHelper.ShowGenerateWadFilesDialog(this.Mod);

                    //Now we need to check for asset collisions
                    List<string> collisions = this._modManager.Index.CheckForAssetCollisions(this.Mod.WadFiles);
                    if (collisions.Count != 0)
                    {
                        object uninstallCollisions = await DialogHelper.ShowAssetConflictDialog(this.Mod, collisions);
                        if ((bool)uninstallCollisions == true)
                        {
                            foreach (string collision in collisions)
                            {
                                ModFile collisionMod = this._modManager.Database.GetMod(collision);
                                ModListItemViewModel collisionModViewModel = this._modList.Items.FirstOrDefault(x => x.Mod == collisionMod);

                                await collisionModViewModel.Uninstall(true);
                            }
                        }
                        else
                        {
                            this.IsInstalled = false;
                            return;
                        }
                    }

                    await DialogHelper.ShowInstallModDialog(this.Mod, this._modManager);
                    this.IsInstalled = true;
                }

                //Do this to release all memory allocated by the ZipArchive
                this.Mod.DisposeReopen();
            }
        }
        public async Task Uninstall(bool forceUninstall = false)
        {
            if ((!this.IsInstalled && this._modManager.Database.IsInstalled(this.Mod)) || forceUninstall)
            {
                await DialogHelper.ShowUninstallModDialog(this.Mod, this._modManager);
                this.IsInstalled = false;

                //Do this to release all memory allocated by the ZipArchive
                this.Mod.DisposeReopen();
            }
        }
        public void Remove()
        {
            this._modList.RemoveMod(this);
            this._modManager.RemoveMod(this.Mod);
        }
    }
}

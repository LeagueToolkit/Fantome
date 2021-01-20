using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Fantome.ModManagement;
using Fantome.ModManagement.IO;
using Fantome.Utilities;
using Serilog;

namespace Fantome.MVVM.ViewModels
{
    public class ModListViewModel : PropertyNotifier
    {
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

        public ModManager ModManager { get; private set; }

        public ModListViewModel()
        {
            Log.Information("Creating a new ModListViewModel instance");

            this.ModManager = new ModManager();
        }

        public void Initialize(string leagueLocation)
        {
            Log.Information("Initializing ModListViewModel");

            this.ModManager.Initialize(leagueLocation);

            SyncWithModManager();
        }

        private void SyncWithModManager()
        {
            Log.Information("Syncing with Mod Manager");

            //Remove non-existent mods
            List<ModListItemViewModel> toRemove = new List<ModListItemViewModel>();
            foreach (ModListItemViewModel modItem in this.Items)
            {
                if (!this.ModManager.Database.ContainsMod(modItem.Mod.GetID()))
                {
                    toRemove.Add(modItem);
                }
            }
            foreach (ModListItemViewModel modItem in toRemove)
            {
                RemoveMod(modItem);
            }

            //Check for new mods
            foreach (KeyValuePair<string, bool> modEntry in this.ModManager.Database.Mods)
            {
                this.Items.Add(new ModListItemViewModel(this.ModManager.Database.GetMod(modEntry.Key), this));
                this.Items.Last().IsInstalled = modEntry.Value;
            }
        }

        public async Task AddMod(ModManager modManager, ModFile mod, bool install)
        {
            if (this.Items.Any(x => x.Mod == mod))
            {
                await DialogHelper.ShowMessageDialog("A Mod with the same ID has already been added");
                Log.Information("Cannot load Mod: {0} because it is already present in the database", mod.GetID());
            }
            else
            {
                string validationError = mod.Validate(modManager.Index);
                if (!string.IsNullOrEmpty(validationError))
                {
                    await DialogHelper.ShowMessageDialog(validationError);
                }
                else
                {
                    ModListItemViewModel modListItem = new ModListItemViewModel(mod, this);
                    this.Items.Add(modListItem);

                    if (install)
                    {
                        await InstallMod(modListItem, true);
                    }
                }
            }
        }
        public void RemoveMod(ModListItemViewModel mod)
        {
            this.ModManager.RemoveMod(mod.Mod);

            this.Items.Remove(mod);
        }

        public async Task InstallMod(ModListItemViewModel modItem, bool forceInstall = false)
        {
            if ((modItem.IsInstalled && !this.ModManager.Database.IsInstalled(modItem.Mod.GetID())) || forceInstall)
            {
                //Validate Mod before installation
                string validationError = modItem.Mod.Validate(this.ModManager.Index);
                if (!string.IsNullOrEmpty(validationError))
                {
                    await DialogHelper.ShowMessageDialog(validationError);
                    modItem.IsInstalled = false;
                }
                else
                {
                    //Generate WAD files for the Mod
                    await DialogHelper.ShowGenerateWadFilesDialog(modItem.Mod, this.ModManager.Index);

                    //Now we need to check for asset collisions
                    List<string> collisions = this.ModManager.Index.CheckForAssetCollisions(modItem.Mod.GetWadFiles(this.ModManager.Index));
                    if (collisions.Count != 0)
                    {
                        object uninstallCollisions = await DialogHelper.ShowAssetConflictDialog(modItem.Mod, collisions);
                        if ((bool)uninstallCollisions == true)
                        {
                            foreach (string collision in collisions)
                            {
                                ModFile collisionMod = this.ModManager.Database.GetMod(collision);
                                ModListItemViewModel collisionModViewModel = this.Items.FirstOrDefault(x => x.Mod == collisionMod);

                                await UninstallMod(collisionModViewModel, true);
                            }
                        }
                        else
                        {
                            modItem.IsInstalled = false;
                            return;
                        }
                    }

                    await DialogHelper.ShowInstallModDialog(modItem.Mod, this.ModManager);
                    modItem.IsInstalled = true;
                }

                //Do this to release all memory allocated by the ZipArchive
                modItem.Mod.DisposeReopen();
            }
        }

        public async Task UninstallMod(ModListItemViewModel modItem, bool forceUninstall = false)
        {
            if ((!modItem.IsInstalled && this.ModManager.Database.IsInstalled(modItem.Mod.GetID())) || forceUninstall)
            {
                await DialogHelper.ShowUninstallModDialog(modItem.Mod, this.ModManager);
                modItem.IsInstalled = false;

                //Do this to release all memory allocated by the ZipArchive
                modItem.Mod.DisposeReopen();
            }
        }
    }
}

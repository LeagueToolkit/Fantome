using Fantome.ModManagement;
using Fantome.ModManagement.IO;
using Fantome.MVVM.Commands;
using Fantome.MVVM.ModelViews.Dialogs;
using Fantome.Utilities;
using LoLCustomSharp;
using MaterialDesignThemes.Wpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Newtonsoft.Json;

namespace Fantome.MVVM.ViewModels
{
    public class MainWindowViewModel: PropertyNotifier
    {
        // ---------- BINDINGS ----------- \\
        public ModListViewModel ModList
        {
            get => this._modList;
            set
            {
                this._modList = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsModListTypeCard
        {
            get => Config.Get<int>("ModListType") == 0;
            set
            {
                if (value)
                {
                    Config.Set("ModListType", 0);
                }

                NotifyPropertyChanged();
            }
        }
        public bool IsModListTypeRow
        {
            get => Config.Get<int>("ModListType") == 1;
            set
            {
                if (value)
                {
                    Config.Set("ModListType", 1);
                }

                NotifyPropertyChanged();
            }
        }
        public bool IsUpdateAvailable
        {
            get => _isUpdateAvailable;
            set
            {
                this._isUpdateAvailable = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isUpdateAvailable;
        private ModListViewModel _modList;

        // ---------- CORE COMPONENTS ----------- \\
        private OverlayPatcher _patcher;

        public NotifyIcon TrayIcon { get; private set; }

        // ---------- COMMANDS ----------- \\
        public ICommand AddModCommand => new RelayCommand(AddMod);
        public ICommand RunSettingsDialogCommand => new RelayCommand(RunSettingsDialog);
        public ICommand RunCreateModDialogCommand => new RelayCommand(RunCreateModDialog);
        public ICommand OpenGithubCommand => new RelayCommand(OpenGithub);
        public ICommand OpenGithubReleasesCommand => new RelayCommand(OpenGithubReleases);

        public MainWindowViewModel()
        {
            Log.Information("Creating a new MainWindowViewModel instance");
        }

        // ---------- INITIALIZATION ----------- \\
        public async void Initialize()
        {
            Log.Information("Initializing MainWindowViewModel");

            InitializeTrayIcon();

            string leagueLocation = Config.Get<string>("LeagueLocation");
            if (string.IsNullOrEmpty(leagueLocation) || !LeagueLocationValidator.Validate(leagueLocation))
            {
                Log.Information("Detected empty LeagueLocation in config");

                leagueLocation = await DialogHelper.ShowLeagueLocationSelectionDialog();
                Config.Set("LeagueLocation", leagueLocation);
            }

            this._modList = new ModListViewModel();

            this._modList.Initialize(leagueLocation);

            InitializePatcher();
        }
        private void InitializeTrayIcon()
        {
            Log.Information("Initializing Tray Icon");

            string[] arguments = Environment.GetCommandLineArgs();
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();

            //Add Close button
            contextMenuStrip.Items.Add(new ToolStripMenuItem("Close", null, delegate (object sender, EventArgs args)
            {
                System.Windows.Application.Current.Shutdown();
            }));

            //Create Icon
            this.TrayIcon = new NotifyIcon()
            {
                Visible = false,
                Icon = Properties.Resources.icon,
                ContextMenuStrip = contextMenuStrip
            };
        }
        private void InitializePatcher()
        {
            Log.Information("Initializing League Patcher");

            string overlayDirectory = (Directory.GetCurrentDirectory() + @"\" + ModManager.OVERLAY_FOLDER + @"\").Replace('\\', '/');
            this._patcher = new OverlayPatcher();
            this._patcher.Start(overlayDirectory, OnPatcherMessage, OnPatcherError);
        }

        // ---------- MOD OPERATIONS ----------- \\
        private async void AddMod(object o) => await AddMod();
        public async Task AddMod()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog("Choose the ZIP file of your mod")
            {
                Multiselect = false
            };

            dialog.Filters.Add(new CommonFileDialogFilter("Fantome mod Files", "*.fantome;*.zip"));

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                await AddMod(dialog.FileName);
            }
        }
        public async Task AddMod(string modOriginalPath)
        {
            string modPath = "";
            string currentModInfo;

            try
            {
                using (ModFile originalMod = new ModFile(modOriginalPath))
                {
                    modPath = string.Format(@"{0}\{1}.fantome", ModManager.MOD_FOLDER, originalMod.GetID());
                    currentModInfo = JsonConvert.SerializeObject(originalMod.Info);
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to read mod file: \"{ModOriginalPath}\"", modOriginalPath);
                await DialogHelper.ShowMessageDialog($"Failed to read mod file: \"{modOriginalPath}\"\n\n{exception.Message}");
                return;
            }

            //If mod is not in Mods folder then we copy it
            if (!File.Exists(modPath))
            {
                Log.Information("Copying Mod: {0} to {1}", modOriginalPath, modPath);

                try
                {
                    File.Copy(modOriginalPath, modPath, true);
                    using ZipArchive modZip = ZipFile.Open(modPath, ZipArchiveMode.Update);
                    ZipArchiveEntry infoJsonEntry = modZip.GetEntry(@"META\info.json") ?? modZip.GetEntry("META/info.json");
                    await using StreamWriter writer = new StreamWriter(infoJsonEntry.Open());
                    writer.BaseStream.SetLength(0);
                    writer.Write(currentModInfo);
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "Failed to copy mod from: {ModOriginalPath} to {ModPath}", modOriginalPath, modPath);
                    await DialogHelper.ShowMessageDialog($"Failed to copy mod from: {modOriginalPath} to {modPath}\n\n{exception.Message}");
                    return;
                }
            }

            Log.Information("Loading Mod: {0}", modPath);

            bool installAddedMods = Config.Get<bool>("InstallAddedMods");
            ModFile mod = new ModFile(modPath);
            await this.ModList.AddMod(this._modList.ModManager, mod, installAddedMods);
        }

        // ---------- DIALOGS ----------- \\
        private async void RunSettingsDialog(object o)
        {
            await DialogHelper.ShowSettingsDialog();
        }
        private async void RunCreateModDialog(object o)
        {
            ModFile createdMod = await DialogHelper.ShowCreateModDialog(this._modList.ModManager.Index);

            if (createdMod != null)
            {
                await this.ModList.AddMod(this._modList.ModManager, createdMod, false);
            }
        }

        // ---------- GITHUB ----------- \\
        private void OpenGithub(object o)
        {
            Process.Start("cmd", "/C start https://github.com/LoL-Fantome/Fantome");
        }
        private void OpenGithubReleases(object o)
        {
            Process.Start("cmd", "/C start https://github.com/LoL-Fantome/Fantome/releases");
        }

        // ---------- PATCHER MESSAGE PIPELINE ----------- \\
        private void OnPatcherError(Exception exception)
        {
            Log.Error("PATCHER: {0}", exception);
        }
        private void OnPatcherMessage(string message)
        {
            Log.Information("PATCHER: {0}", message);
        }

        // ---------- UPDATES ----------- \\
        public async Task CheckForUpdate()
        {
            UpdateInfo updateInfo = await UpdateHelper.CheckForUpdate();

            this.IsUpdateAvailable = updateInfo.IsUpdateAvailable;
        }
    }
}

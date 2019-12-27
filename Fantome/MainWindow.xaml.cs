using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Fantome.ModManagement;
using Fantome.ModManagement.IO;
using Fantome.MVVM.Commands;
using Fantome.MVVM.ViewModels;
using Fantome.UserControls.Dialogs;
using Fantome.Utilities;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using LoLCustomSharp;
using System.Windows.Forms;

using Application = System.Windows.Application;

namespace Fantome
{
    public partial class MainWindow : Window
    {
        public ModListViewModel ModList { get => this.ModsListBox.DataContext as ModListViewModel; }

        private ModManager _modManager;
        private OverlayPatcher _patcher;
        private NotifyIcon _notifyIcon;

        public ICommand RunSettingsDialogCommand => new RelayCommand(RunSettingsDialog);
        public ICommand RunCreateModDialogCommand => new RelayCommand(RunCreateModDialog);

        public MainWindow()
        {
            Config.Load();
            CreateWorkFolders();
            StartPatcher();
            InitializeComponent();
            InitializeModManager();
            BindMVVM();
            InitializeTrayIcon();
        }

        private void InitializeModManager()
        {
            this._modManager = new ModManager();
        }
        private void StartPatcher()
        {
            string overlayDirectory = (Directory.GetCurrentDirectory() + @"\" + ModManager.OVERLAY_FOLDER + @"\").Replace('\\', '/');
            this._patcher = new OverlayPatcher();
            this._patcher.Start(overlayDirectory);
        }
        private void CreateWorkFolders()
        {
            Directory.CreateDirectory(ModManager.MOD_FOLDER);
            Directory.CreateDirectory(ModManager.OVERLAY_FOLDER);
        }
        private void BindMVVM()
        {
            this.ModsListBox.DataContext = new ModListViewModel(this._modManager);
            this.ButtonCreateMod.DataContext = this;
            this.SettingsButton.DataContext = this;
        }
        private void InitializeTrayIcon()
        {
            Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/Fantome;component/Resources/fantome.ico")).Stream;
            this._notifyIcon = new NotifyIcon()
            {
                Visible = true,
                Icon = new System.Drawing.Icon(iconStream)
            };

            this._notifyIcon.DoubleClick += delegate (object sender, EventArgs args)
            {
                Show();
                this.WindowState = WindowState.Normal;
            };
        }

        private void AddMod(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog("Choose the ZIP file of your mod")
            {
                Multiselect = false
            };

            dialog.Filters.Add(new CommonFileDialogFilter("ZIP Files", ".zip"));

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string modName = Path.GetFileName(dialog.FileName);
                string modPath = string.Format(@"{0}\{1}", ModManager.MOD_FOLDER, modName);

                if (!File.Exists(modPath))
                {
                    File.Copy(dialog.FileName, modPath, true);
                }

                ModFile mod = new ModFile(modPath);
                this.ModList.AddMod(mod, true);
            }
        }

        private async void RunSettingsDialog(object o)
        {
            SettingsDialog dialog = new SettingsDialog
            {
                DataContext = new SettingsViewModel()
            };


            object result = await DialogHost.Show(dialog, "RootDialog", (dialog.DataContext as SettingsViewModel).ClosingEventHandler);
        }
        private async void RunCreateModDialog(object o)
        {
            CreateModDialog dialog = new CreateModDialog
            {
                DataContext = new CreateModDialogViewModel(this.ModList)
            };


            object result = await DialogHost.Show(dialog, "RootDialog", (dialog.DataContext as CreateModDialogViewModel).ClosingEventHandler);
        }
        private async void DialogHost_Loaded(object sender, EventArgs e)
        {
            string leagueLocation = Config.Get<string>("LeagueLocation");
            if (string.IsNullOrEmpty(leagueLocation))
            {
                await GetLeagueLocation();
                Config.Set("LeagueLocation", leagueLocation);
                this._modManager.AssignLeague(leagueLocation);
            }

            this._modManager.AssignLeague(leagueLocation);
            this.ModList.Sync();

            async Task GetLeagueLocation()
            {
                LeagueLocationDialog dialog = new LeagueLocationDialog()
                {
                    DataContext = new LeagueLocationDialogViewModel()
                };

                object result = await DialogHost.Show(dialog, "RootDialog");

                if ((bool)result == true)
                {
                    leagueLocation = dialog.ViewModel.LeagueLocation;
                }
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if(this.WindowState == WindowState.Minimized)
            {
                Hide();
            }

            base.OnStateChanged(e);
        }
    }
}

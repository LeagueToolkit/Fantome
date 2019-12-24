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
using Fantome.JobManagement;
using Fantome.ModManagement;
using Fantome.ModManagement.IO;
using Fantome.MVVM.Commands;
using Fantome.MVVM.ViewModels;
using Fantome.UserControls.Dialogs;
using Fantome.Utilities;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Fantome
{
    public partial class MainWindow : Window
    {
        public ModListViewModel ModList { get => this.ModsListBox.DataContext as ModListViewModel; }

        private ModManager _modManager;
        private Thread _patcher;

        public ICommand RunSettingsDialogCommand => new RelayCommand(RunSettingsDialog);

        public MainWindow()
        {
            Config.Load();
            CreateWorkFolders();
            StartPatcher();
            InitializeComponent();
            InitializeModManager();
            BindMVVM();
        }

        private void InitializeModManager()
        {
            this._modManager = new ModManager();
        }
        private void StartPatcher()
        {
            string arguments = string.Format("{0} -r", Directory.GetCurrentDirectory() + @"\" + ModManager.OVERLAY_FOLDER + @"\").Replace('\\', '/');

            this._patcher = new Thread(delegate ()
            {
                using (Job job = new Job())
                {
                    ProcessStartInfo info = new ProcessStartInfo()
                    {
                        FileName = "lolcustomskin.exe",
                        Arguments = arguments,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = false,
                    };

                    using (Process patcher = Process.Start(info))
                    {
                        job.AddProcess(patcher.Id);

                        patcher.EnableRaisingEvents = true;

                        patcher.WaitForExit();
                    }

                    job.Close();
                }
            });

            this._patcher.Start();
        }
        private void CreateWorkFolders()
        {
            Directory.CreateDirectory(ModManager.MOD_FOLDER);
            Directory.CreateDirectory(ModManager.OVERLAY_FOLDER);
        }
        private void BindMVVM()
        {
            this.ModsListBox.DataContext = new ModListViewModel(this._modManager);
            this.PopupMain.DataContext = new CreateModDialogViewModel(this.ModList);
            this.SettingsButton.DataContext = this;
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
                ModFile mod = new ModFile(dialog.FileName);
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
    }
}

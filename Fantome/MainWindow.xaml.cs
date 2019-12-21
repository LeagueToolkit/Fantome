using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Fantome.ModManagement;
using Fantome.ModManagement.IO;
using Fantome.MVVM.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Fantome
{
    public partial class MainWindow : Window
    {
        public ModListViewModel ModList { get => this.ModsListBox.DataContext as ModListViewModel; }
        
        private ModManager _modManager;
        private Process _patcher;

        public MainWindow()
        {
            Application.Current.Exit += new ExitEventHandler(OnApplicationClose);

            CreateWorkFolders();
            StartPatcher();

            this._modManager = new ModManager("C:/Riot Games/League of Legends");
            File.WriteAllText("LeagueFileIndex.json", this._modManager.Index.Serialize());
            //this.ModManager.InstallMod(new ModFile("Mods/WAD", "", new ModInfo("Project Evelynn", "Some Random Feeling", new Version(1, 0), ""), System.Drawing.Image.FromFile("preview-1536x864.png")));
            //this.ModManager.InstallMod(new ModFile("Mods/WAD", "", new ModInfo("Project Evelynnn", "Some Random Feeling", new Version(1, 0), ""), System.Drawing.Image.FromFile("preview-1536x864.png")));
            //this.ModManager.InstallMod(new ModFile("Mods/WAD", "", new ModInfo("Project Evelynnnn", "Some Random Feeling", new Version(1, 0), ""), System.Drawing.Image.FromFile("preview-1536x864.png")));
            //this.ModManager.InstallMod(new ModFile("Mods/WAD", "", new ModInfo("Project Evelynnnnn", "Some Random Feeling", new Version(1, 0), ""), System.Drawing.Image.FromFile("preview-1536x864.png")));
            //this.ModManager.InstallMod(new ModFile("Mods/WAD", "", new ModInfo("Project Evelynnnnnn", "Some Random Feeling", new Version(1, 0), ""), System.Drawing.Image.FromFile("preview-1536x864.png")));

            //File.WriteAllText("info.json", new ModInfo("Project Evelynn", "Some Random Feeling", new Version(1, 0), "").Serialize());

            InitializeComponent();
            BindMVVM();
        }

        private void StartPatcher()
        {
            string arguments = string.Format("{0} -r", Directory.GetCurrentDirectory() + @"\" + ModManager.OVERLAY_FOLDER + @"\").Replace('\\', '/');

            this._patcher = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "lolcustomskin.exe",
                    Arguments = arguments,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                },

            };

            this._patcher.Start();
        }
        private void CreateWorkFolders()
        {
            Directory.CreateDirectory(ModManager.MOD_FOLDER);
            Directory.CreateDirectory(ModManager.OVERLAY_FOLDER);
        }
        private void BindMVVM()
        {
            this.PopupMain.DataContext = new CreateModDialogViewModel(this._modManager);
            this.ModsListBox.DataContext = new ModListViewModel(this._modManager);
        }

        private void OnApplicationClose(object sender, EventArgs e)
        {
            this._patcher.Kill();
        }

        private void AddMod(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog("Choose the ZIP file of your mod")
            {
                Multiselect = false
            };

            dialog.Filters.Add(new CommonFileDialogFilter("ZIP Files", ".zip"));

            if(dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ModFile mod = new ModFile(dialog.FileName);
                this.ModList.AddMod(mod);
            }
        }
    }
}

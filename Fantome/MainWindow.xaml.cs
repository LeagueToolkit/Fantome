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

namespace Fantome
{
    public partial class MainWindow : Window
    {
        public ModManager ModManager { get; private set; }

        private Process _patcher;

        public MainWindow()
        {
            Directory.CreateDirectory("Overlay");
            StartPatcher();

            this.ModManager = new ModManager("C:/Riot Games/League of Legends");
            File.WriteAllText("LeagueFileIndex.json", this.ModManager.Index.Serialize());
            this.ModManager.InstallMod(new ModFile("Mods/WAD", "", new ModInfo("Project Evelynn", "Some Random Feeling", new Version(1, 0), ""), System.Drawing.Image.FromFile("preview-1536x864.png")));

            //File.WriteAllText("info.json", new ModInfo("Project Evelynn", "Some Random Feeling", new Version(1, 0), "").Serialize());

            InitializeComponent();

            this.PopupMain.DataContext = new CreateModDialogViewModel(this.ModManager);
            this.Card.DataContext = this.ModManager.InstalledMods[0];

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
                    CreateNoWindow = false
                },

            };

            this._patcher.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this._patcher.Kill();
        }
    }
}

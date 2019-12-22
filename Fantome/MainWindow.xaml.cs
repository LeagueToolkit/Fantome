using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Fantome.JobManagement;
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
        private Thread _patcher;

        public MainWindow()
        {
            this._modManager = new ModManager("C:/Riot Games/League of Legends");

            CreateWorkFolders();
            StartPatcher();
            InitializeComponent();
            BindMVVM();
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
                this.ModList.AddMod(mod);
            }
        }
    }
}

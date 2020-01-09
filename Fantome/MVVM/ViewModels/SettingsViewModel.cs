using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Fantome.Utilities;
using MaterialDesignThemes.Wpf;

namespace Fantome.MVVM.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public string LeagueLocation
        {
            get => this._leagueLocation;
            set
            {
                this._leagueLocation = value;
                NotifyPropertyChanged();
            }
        }
        public bool ParallelWadInstallation
        {
            get => this._parallelWadInstallation;
            set
            {
                this._parallelWadInstallation = value;
                NotifyPropertyChanged();
            }
        }
        public bool PackWadFolders
        {
            get => this._packWadFolders;
            set
            {
                this._packWadFolders = value;
                NotifyPropertyChanged();
            }
        }
        public bool InstallAddedMods
        {
            get => this._installAddedMods;
            set
            {
                this._installAddedMods = value;
                NotifyPropertyChanged();
            }
        }

        private string _leagueLocation;
        private bool _parallelWadInstallation;
        private bool _packWadFolders;
        private bool _installAddedMods;

        private bool _needsRestart;

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsViewModel()
        {
            this._leagueLocation = Config.Get<string>("LeagueLocation");
            this._parallelWadInstallation = Config.Get<bool>("ParallelWadInstallation");
            this._packWadFolders = Config.Get<bool>("PackWadFolders");
            this._installAddedMods = Config.Get<bool>("InstallAddedMods");
        }

        public void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter)
            {
                if (this._leagueLocation != Config.Get<string>("LeagueLocation"))
                {
                    this._needsRestart = true;
                    Config.Set("LeagueLocation", this._leagueLocation);
                }

                Config.Set("ParallelWadInstallation", this._parallelWadInstallation);
                Config.Set("PackWadFolders", this._packWadFolders);
                Config.Set("InstallAddedMods", this._installAddedMods);

                if (this._needsRestart)
                {
                    Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

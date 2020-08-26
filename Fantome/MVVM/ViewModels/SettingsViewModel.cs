using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Fantome.Utilities;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System.Reflection;

namespace Fantome.MVVM.ViewModels
{
    public class SettingsViewModel : PropertyNotifier
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
        public bool MinimizeToTray
        {
            get => this._minimizeToTray;
            set
            {
                this._minimizeToTray = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsDarkTheme
        {
            get => this._isDarkTheme;
            set
            {
                this._isDarkTheme = value;
                ChangeTheme();
                NotifyPropertyChanged();
            }
        }
        public PrimaryColor SelectedPrimaryColor
        {
            get => this._selectedPrimaryColor;
            set
            {
                this._selectedPrimaryColor = value;
                ChangeTheme();
                NotifyPropertyChanged();
            }
        }
        public IEnumerable<PrimaryColor> PrimaryColors => Enum.GetValues(typeof(PrimaryColor)).Cast<PrimaryColor>();
        public SecondaryColor SelectedSecondaryColor
        {
            get => this._selectedSecondaryColor;
            set
            {
                this._selectedSecondaryColor = value;
                ChangeTheme();
                NotifyPropertyChanged();
            }
        }
        public IEnumerable<SecondaryColor> SecondaryColors => Enum.GetValues(typeof(SecondaryColor)).Cast<SecondaryColor>();

        private string _leagueLocation;
        private bool _parallelWadInstallation;
        private bool _packWadFolders;
        private bool _installAddedMods;
        private bool _isDarkTheme;
        private PrimaryColor _selectedPrimaryColor;
        private SecondaryColor _selectedSecondaryColor;
        private bool _minimizeToTray;

        private bool _needsRestart;
        private bool _forceNewIndex;

        public SettingsViewModel()
        {
            this._leagueLocation = Config.Get<string>("LeagueLocation");
            this._parallelWadInstallation = Config.Get<bool>("ParallelWadInstallation");
            this._packWadFolders = Config.Get<bool>("PackWadFolders");
            this._installAddedMods = Config.Get<bool>("InstallAddedMods");
            this._isDarkTheme = Config.Get<bool>("IsDarkTheme");
            this._selectedPrimaryColor = Config.Get<PrimaryColor>("PrimaryColor");
            this._selectedSecondaryColor = Config.Get<SecondaryColor>("SecondaryColor");
            this._minimizeToTray = Config.Get<bool>("MinimizeToTray");
        }

        public void OnSaveSettings(object sender, DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter)
            {
                if (this._leagueLocation != Config.Get<string>("LeagueLocation"))
                {
                    this._needsRestart = true;
                    this._forceNewIndex = true;
                    Config.Set("LeagueLocation", this._leagueLocation);
                }

                Config.Set("ParallelWadInstallation", this._parallelWadInstallation);
                Config.Set("PackWadFolders", this._packWadFolders);
                Config.Set("InstallAddedMods", this._installAddedMods);
                Config.Set("IsDarkTheme", this._isDarkTheme);
                Config.Set("PrimaryColor", this._selectedPrimaryColor);
                Config.Set("SecondaryColor", this._selectedSecondaryColor);
                Config.Set("MinimizeToTray", this._minimizeToTray);

                if (this._needsRestart)
                {
                    string arguments = string.Empty;

                    if(this._forceNewIndex)
                    {
                        arguments += "-forceNewIndex ";
                    }

                    Process.Start(Application.ResourceAssembly.Location, arguments);
                    Application.Current.Shutdown();
                }
            }
            else
            {
                //Reset to theme that is set with settings.json
                ThemeHelper.LoadTheme();
            }
        }

        private void ChangeTheme()
        {
            ThemeHelper.ChangeTheme(this._isDarkTheme ? Theme.Dark : Theme.Light, GetPrimaryColor(), GetSecondaryColor());
        }
        private Color GetPrimaryColor()
        {
            return ThemeHelper.ConvertPrimaryColor(this._selectedPrimaryColor);
        }
        private Color GetSecondaryColor()
        {
            return ThemeHelper.ConvertSecondaryColor(this._selectedSecondaryColor);
        }
    }
}

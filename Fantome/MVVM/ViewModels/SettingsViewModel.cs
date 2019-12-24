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

        private string _leagueLocation;

        private bool _needsRestart;

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsViewModel()
        {
            this._leagueLocation = Config.Get<string>("LeagueLocation");
        }

        public void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            //No changes to config needed
            if((bool)eventArgs.Parameter == false)
            {
                return;
            }

            if(this._leagueLocation != Config.Get<string>("LeagueLocation"))
            {
                this._needsRestart = true;
                Config.Set("LeagueLocation", this._leagueLocation);
            }

            if(this._needsRestart)
            {
                Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Fantome.MVVM.ViewModels
{
    public class LeagueLocationDialogViewModel : INotifyPropertyChanged
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

        public event PropertyChangedEventHandler PropertyChanged;

        public LeagueLocationDialogViewModel()
        {
            //This will only work for old League installations
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Riot Games, Inc\League of Legends"))
            {
                if (key != null)
                {
                    object leagueLocationObject = key.GetValue("Location");
                    if (leagueLocationObject != null)
                    {
                        this._leagueLocation = (leagueLocationObject as string) + @"\Game";
                    }
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

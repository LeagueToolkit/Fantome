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
                this.IsLeagueSelected = true;
                NotifyPropertyChanged();
            }
        }
        public bool IsLeagueSelected
        {
            get => this._isLeagueSelected;
            set
            {
                this._isLeagueSelected = value;
                NotifyPropertyChanged();
            }
        }

        private string _leagueLocation;
        private bool _isLeagueSelected;

        public event PropertyChangedEventHandler PropertyChanged;

        public LeagueLocationDialogViewModel()
        {

        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

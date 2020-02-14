using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Fantome.MVVM
{
    public abstract class PropertyNotifier : INotifyPropertyChanged
    {
        public PropertyNotifier() : base() { }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

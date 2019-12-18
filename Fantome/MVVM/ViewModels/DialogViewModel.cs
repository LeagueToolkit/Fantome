using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Fantome.Dialogs;
using Fantome.MVVM.Commands;
using MaterialDesignThemes.Wpf;

namespace Fantome.MVVM.ViewModels
{
    public class DialogViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand RunDialogCommand => new RelayCommand(ExecuteRunDialog);

        public DialogViewModel()
        {

        }

        private async void ExecuteRunDialog(object o)
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            CreateModDialog view = new CreateModDialog
            {
                DataContext = new CreateModDialogViewModel()
            };


            object result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);

            Console.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            Console.WriteLine("You can intercept the closing event, and cancel here.");
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

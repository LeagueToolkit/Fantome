using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Fantome.MVVM.ViewModels;
using Fantome.Utilities;

namespace Fantome.UserControls.Dialogs
{
    /// <summary>
    /// Interaction logic for UninstallingModDialog.xaml
    /// </summary>
    public partial class UninstallingModDialog : UserControl
    {
        public UninstallingModViewModel ViewModel => this.DataContext as UninstallingModViewModel;

        public UninstallingModDialog()
        {
            InitializeComponent();
        }

        public void StartUninstallation(object sender, EventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += InstallMod;
            worker.RunWorkerCompleted += CloseDialog;
            worker.WorkerSupportsCancellation = true;

            worker.RunWorkerAsync(new Tuple<BackgroundWorker, UninstallingModViewModel>(worker, this.ViewModel));
        }

        private void CloseDialog(object sender, RunWorkerCompletedEventArgs e)
        {
            DialogHelper.RootDialog.IsOpen = false;
        }

        private void InstallMod(object sender, DoWorkEventArgs e)
        {
            Tuple<BackgroundWorker, UninstallingModViewModel> argument = e.Argument as Tuple<BackgroundWorker, UninstallingModViewModel>;

            argument.Item2.Uninstall();
            argument.Item1.CancelAsync();
        }
    }
}

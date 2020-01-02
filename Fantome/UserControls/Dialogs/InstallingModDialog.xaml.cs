using System;
using System.ComponentModel;
using System.Windows.Controls;
using Fantome.MVVM.ViewModels;
using Fantome.Utilities;
using MaterialDesignThemes.Wpf;

namespace Fantome.UserControls.Dialogs
{
    /// <summary>
    /// Interaction logic for InstallingModDialog.xaml
    /// </summary>
    public partial class InstallingModDialog : UserControl
    {
        public InstallingModViewModel ViewModel => this.DataContext as InstallingModViewModel;

        public InstallingModDialog()
        {
            InitializeComponent();
        }

        public void StartInstallation(object sender, EventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += InstallMod;
            worker.RunWorkerCompleted += CloseDialog;
            worker.WorkerSupportsCancellation = true;

            worker.RunWorkerAsync(new Tuple<BackgroundWorker, InstallingModViewModel>(worker, this.ViewModel));
        }

        private void CloseDialog(object sender, RunWorkerCompletedEventArgs e)
        {
            DialogHelper.RootDialog.IsOpen = false;
        }

        private void InstallMod(object sender, DoWorkEventArgs e)
        {
            Tuple<BackgroundWorker, InstallingModViewModel> argument = e.Argument as Tuple<BackgroundWorker, InstallingModViewModel>;

            argument.Item2.Install();
            argument.Item1.CancelAsync();
        }
    }
}

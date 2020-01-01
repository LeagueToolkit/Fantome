using System;
using System.ComponentModel;
using System.Windows.Controls;
using Fantome.MVVM.ViewModels;
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
            using (BackgroundWorker worker = new BackgroundWorker())
            {
                worker.DoWork += InstallMod;
                worker.RunWorkerCompleted += CloseDialog;

                worker.RunWorkerAsync(this.ViewModel);
            }
        }

        private void CloseDialog(object sender, RunWorkerCompletedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }

        private void InstallMod(object sender, DoWorkEventArgs e)
        {
            (e.Argument as InstallingModViewModel).Install();
        }
    }
}

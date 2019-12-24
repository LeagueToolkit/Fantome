using System;
using System.Collections.Generic;
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
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Fantome.UserControls.Dialogs
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : UserControl
    {
        public SettingsViewModel ViewModel { get => this.DataContext as SettingsViewModel; }

        public SettingsDialog()
        {
            InitializeComponent();
        }

        private void SelectLeagueLocationButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.ViewModel.LeagueLocation = dialog.FileName;
            }
        }
    }
}
